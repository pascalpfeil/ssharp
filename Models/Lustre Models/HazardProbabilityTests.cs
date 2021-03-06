﻿using ISSE.SafetyChecking.DiscreteTimeMarkovChain;
using ISSE.SafetyChecking.Formula;
using ISSE.SafetyChecking.Modeling;
using NUnit.Framework;
using SafetyLustre;
using System;
using System.Collections.Generic;
using System.IO;
using static Lustre_Models.TestUtil;
using static SafetyLustre.LustreSafetyAnalysis;

namespace Lustre_Models
{
    public class HazardProbabilityTests
    {
        [Test]
        public void TankRupture()
        {
            Formula invariant = new LustrePressureBelowThreshold();
            Formula hazard = new UnaryFormula(invariant, UnaryOperator.Not);
            LustrePressureBelowThreshold.threshold = 60;
            var faults = new List<Fault>
            {
                new TransientFault() { Name = "fault_switch", Identifier = 0, ProbabilityOfOccurrence = new Probability(3.0E-6) },
                new PermanentFault() { Name = "fault_k1", Identifier = 1, ProbabilityOfOccurrence = new Probability(3.0E-6) },
                new PermanentFault() { Name = "fault_k2", Identifier = 2, ProbabilityOfOccurrence = new Probability(3.0E-6) },
                new PermanentFault() { Name = "fault_timer", Identifier = 3, ProbabilityOfOccurrence = new Probability(1.0E-5) },
                new PermanentFault() { Name = "fault_sensor", Identifier = 4, ProbabilityOfOccurrence = new Probability(1.0E-5) }
            };
            var modelChecker = new LustreMarkovChainFromExecutableModelGenerator(Path.Combine(AssemblyDirectory, "pressureTank.lus"), "TANK", faults);
            modelChecker.AddFormulaToCheck(hazard);
            modelChecker.Configuration.UseCompactStateStorage = true;
            var lmc = modelChecker.GenerateLabeledMarkovChain();

            var ltmcModelChecker = new BuiltinLtmcModelChecker(lmc, Console.Out);
            var finallyHazard = new BoundedUnaryFormula(hazard, UnaryOperator.Finally, 200);
            var result = ltmcModelChecker.CalculateProbability(finallyHazard);

            Console.Write($"Probability of hazard: {result}");
        }

        [Test]
        public void Parametric()
        {
            Formula invariant = new LustrePressureBelowThreshold();
            Formula hazard = new UnaryFormula(invariant, UnaryOperator.Not);
            LustrePressureBelowThreshold.threshold = 60;
            var faultK1 = new PermanentFault() { Name = "fault_k1", Identifier = 1, ProbabilityOfOccurrence = new Probability(3.0E-6) };
            var faultK2 = new PermanentFault() { Name = "fault_k2", Identifier = 2, ProbabilityOfOccurrence = new Probability(3.0E-6) };
            var faultSensor = new PermanentFault() { Name = "fault_sensor", Identifier = 4, ProbabilityOfOccurrence = new Probability(1.0E-5) };
            var faults = new[] { faultK1, faultK2, faultSensor };

            var parameter = new QuantitativeParametricAnalysisParameter
            {
                StateFormula = hazard,
                Bound = null,
                From = 3.0E-7,
                To = 3.0E-5,
                Steps = 25,
            };

            using (var fileWriter = new StreamWriter(Path.Combine(AssemblyDirectory, "pressureTank_varyK1.csv"), append: false))
            {
                parameter.UpdateParameterInModel = value => { faultK1.ProbabilityOfOccurrence = new Probability(value); };
                var result = LustreModelChecker.ConductQuantitativeParametricAnalysis(Path.Combine(AssemblyDirectory, "pressureTank.lus"), "TANK", faults, parameter);
                result.ToCsv(fileWriter);
            }

            using (var fileWriter = new StreamWriter(Path.Combine(AssemblyDirectory, "pressureTank_varyK2.csv"), append: false))
            {
                parameter.UpdateParameterInModel = value => { faultK2.ProbabilityOfOccurrence = new Probability(value); };
                var result = LustreModelChecker.ConductQuantitativeParametricAnalysis(Path.Combine(AssemblyDirectory, "pressureTank.lus"), "TANK", faults, parameter);
                result.ToCsv(fileWriter);
            }

            using (var fileWriter = new StreamWriter(Path.Combine(AssemblyDirectory, "pressureTank_varySensor.csv"), append: false))
            {
                parameter.UpdateParameterInModel = value => { faultSensor.ProbabilityOfOccurrence = new Probability(value); };
                var result = LustreModelChecker.ConductQuantitativeParametricAnalysis(Path.Combine(AssemblyDirectory, "pressureTank.lus"), "TANK", faults, parameter);
                result.ToCsv(fileWriter);
            }
        }
    }
}
