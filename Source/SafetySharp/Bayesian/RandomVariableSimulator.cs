﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2017, Institute for Software & Systems Engineering
// Copyright (c) 2017, Stefan Fritsch
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SafetySharp.Bayesian
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Analysis;
    using ISSE.SafetyChecking.Formula;
    using Modeling;

    /// <summary>
    /// Class for generating simulation data from a model.
    /// </summary>
    internal class RandomVariableSimulator
    {
        private readonly ModelBase _model;
        private readonly int _stepBounds;
        private int _numberOfSimulations;
        private readonly string _pathForGeneratedFiles;

        private ICollection<FaultRandomVariable> _faults;
        private ICollection<McsRandomVariable> _mcs;
        private ICollection<BooleanRandomVariable> _states;
        private BooleanRandomVariable _hazard;
        private readonly BayesianLearningConfiguration _config;

        private string DataPath => _pathForGeneratedFiles + "simulationData.csv";

        /// <summary>
        /// Creates a new RandomVariableSimulator instance
        /// </summary>
        /// <param name="model">The model for which a bayesian network will be learned</param>
        /// <param name="stepBounds">The maximal number of steps for a trace in a simulation step</param>
        /// <param name="pathForGeneratedFiles">Path to use for generated file creation</param>
        /// <param name="config">BayesianLearningConfiguration for further optional settings</param>
        public RandomVariableSimulator(ModelBase model, int stepBounds, string pathForGeneratedFiles, BayesianLearningConfiguration? config = null)
        {
            _config = config ?? BayesianLearningConfiguration.Default;
            _model = model;
            _stepBounds = stepBounds;
            _pathForGeneratedFiles = pathForGeneratedFiles;
        }

        public string GenerateSimulationData(ICollection<FaultRandomVariable> faults, ICollection<McsRandomVariable> mcs,
                                                    ICollection<BooleanRandomVariable> states, BooleanRandomVariable hazard, int numberOfSimulations)
        {
            _faults = faults;
            _mcs = mcs;
            _hazard = hazard;
            _states = states;
            _numberOfSimulations = numberOfSimulations;

            Console.Out.WriteLine("Create simulation data...");

            var allVariables = AllRandomVariables();
            var allFormulas = CreateAllFormulas();
            SafetySharpProbabilisticSimulator.Configuration.UseOptionProbabilitiesInSimulation = _config.UseRealProbabilitiesForSimulation;
            var simulator = new SafetySharpProbabilisticSimulator(_model, allFormulas.Values.ToArray());

            using (var w = new StreamWriter(DataPath))
            {
                w.WriteLine(string.Join(",", allVariables.Select(randomVariable => randomVariable.Name)));
                for (var currentStep = 0; currentStep < _numberOfSimulations; currentStep++)
                {
                    if (_numberOfSimulations > 100 && currentStep % (_numberOfSimulations / 100) == 0)
                    {
                        Console.Out.WriteLine($"{(double)currentStep / _numberOfSimulations:P0} done.");
                    }

                    simulator.SimulateSteps(_stepBounds);
                    var results = new bool[allVariables.Count];
                    for (var varIndex = 0; varIndex < allVariables.Count; varIndex++)
                    {
                        var currentVariable = allVariables[varIndex];
                        // cut sets cannot be checked for a given state, so check the occurence of its faults
                        if (currentVariable is McsRandomVariable)
                        {
                            var cutSet = (McsRandomVariable)currentVariable;
                            results[varIndex] =
                                cutSet.FaultVariables.All(fault => simulator.GetCountOfSatisfiedOnTrace(allFormulas[fault]) > 0);
                        }
                        // check the occurence of the random variable
                        else
                        {
                            results[varIndex] = simulator.GetCountOfSatisfiedOnTrace(allFormulas[currentVariable]) > 0;
                        }
                    }
                    w.WriteLine(string.Join(",", results.Select(res => res ? 'T' : 'F')));
                }
                w.Flush();
            }
            return DataPath;
        }

        private IList<RandomVariable> AllRandomVariables()
        {
            var allVariables = new List<RandomVariable>();
            allVariables.AddRange(_faults);
            allVariables.AddRange(_mcs);
            allVariables.AddRange(_states);
            allVariables.Add(_hazard);
            return allVariables;
        }

        private Dictionary<RandomVariable, Formula> CreateAllFormulas()
        {
            var allFormulas = new Dictionary<RandomVariable, Formula>();
            foreach (var faultVar in _faults)
            {
                allFormulas[faultVar] = faultVar.ToFormula();
            }
            foreach (var state in _states)
            {
                allFormulas[state] = state.ToFormula();
            }
            allFormulas[_hazard] = _hazard.ToFormula();
            return allFormulas;
        }
    }
}