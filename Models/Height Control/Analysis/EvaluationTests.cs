﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2017, Institute for Software & Systems Engineering
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetySharp.CaseStudies.HeightControl.Analysis
{
	using FluentAssertions;
	using ISSE.SafetyChecking.Modeling;
	using Modeling;
	using NUnit.Framework;
	using SafetySharp.Analysis;
	using SafetySharp.Modeling;
	using System.Collections;
	using ISSE.SafetyChecking;
	using ISSE.SafetyChecking.DiscreteTimeMarkovChain;
	using ISSE.SafetyChecking.Formula;
	using Modeling.Controllers;
	using Modeling.Sensors;
	using Newtonsoft.Json;
	using Runtime;

	class EvaluationTests
	{
		public class ModelProbabilities
		{
#pragma warning disable 0169
#pragma warning disable 0649
			public double LightBarrierFalseDetection;
			public double LightBarrierMisdetection;
			public double OverheadDetectorFalseDetection;
			public double OverheadDetectorMisdetection;
			public double SmallLightBarrierFalseDetection;
			public double SmallLightBarrierMisdetection;
			public double LeftHV;
			public double LeftOHV;
			public double SlowTraffic;
#pragma warning restore 0169
#pragma warning restore 0649
		}

		public static void SetProbabilities(Model model)
		{
			var probabilities = JsonConvert.DeserializeObject<ModelProbabilities>(System.IO.File.ReadAllText("Analysis/heightcontrol_probabilities.json"));

			foreach (var detector in model.Components.OfType<LightBarrier>())
			{
				detector.FalseDetection.ProbabilityOfOccurrence = new Probability(probabilities.LightBarrierFalseDetection);
				detector.Misdetection.ProbabilityOfOccurrence = new Probability(probabilities.LightBarrierMisdetection);
			}

			foreach (var detector in model.Components.OfType<OverheadDetector>())
			{
				detector.FalseDetection.ProbabilityOfOccurrence = new Probability(probabilities.OverheadDetectorFalseDetection);
				detector.Misdetection.ProbabilityOfOccurrence = new Probability(probabilities.OverheadDetectorMisdetection);
			}
			foreach (var detector in model.Components.OfType<SmallLightBarrier>())
			{
				detector.FalseDetection.ProbabilityOfOccurrence = new Probability(probabilities.SmallLightBarrierFalseDetection);
				detector.Misdetection.ProbabilityOfOccurrence = new Probability(probabilities.SmallLightBarrierMisdetection);
			}

			model.VehicleSet.LeftHV.ProbabilityOfOccurrence = new Probability(probabilities.LeftHV);
			model.VehicleSet.LeftOHV.ProbabilityOfOccurrence = new Probability(probabilities.LeftOHV);
			model.VehicleSet.SlowTraffic.ProbabilityOfOccurrence = new Probability(probabilities.SlowTraffic);
		}
		
		[Test]
		public void CreateMarkovChainWithFalseFormula()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.AddFormulaToCheck(new ExecutableStateFormula(()=> false));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateMarkovChainWithBothHazards()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.Collision, UnaryOperator.Finally, 50));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}


		[Test]
		public void CreateFaultAwareMarkovChainForFalseAlarm1()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.EndControl.LeftDetector.FalseDetection));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateFaultAwareMarkovChainForFalseAlarm2()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.EndControl.LeftDetector.Misdetection));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateFaultAwareMarkovChainForFalseAlarm3()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.PreControl.PositionDetector.FalseDetection));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateFaultAwareMarkovChainForFalseAlarm4()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.PreControl.PositionDetector.Misdetection));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateFaultAwareMarkovChainForFalseAlarm5()
		{
			var model = Model.CreateOriginal();
			SetProbabilities(model);

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.EndControl.LeftDetector.FalseDetection));
			markovChainGenerator.AddFormulaToCheck(new FaultFormula(model.HeightControl.EndControl.LeftDetector.Misdetection));
			markovChainGenerator.AddFormulaToCheck(new BoundedUnaryFormula(model.FalseAlarm, UnaryOperator.Finally, 50));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}
	}
}