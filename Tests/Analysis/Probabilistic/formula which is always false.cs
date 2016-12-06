﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Analysis.Probabilistic
{
	using SafetySharp.Analysis;
	using SafetySharp.Modeling;
	using Shouldly;
	using Utilities;

	internal class FormulaWhichIsAlwaysFalse : ProbabilisticAnalysisTestObject
	{


		protected override void Check()
		{
			var c = new C();
			Probability probabilityOfFalse;
			
			Formula falseFormula = false;
			var probabilityFinallyFalseFormula = new CalculateProbabilityToReachStateFormula(falseFormula);
			
			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator(TestModel.InitializeModel(c));
			markovChainGenerator.AddFormulaToCheck(probabilityFinallyFalseFormula);
			var dtmc = markovChainGenerator.GenerateMarkovChain();
			var typeOfModelChecker = (Type)Arguments[0];
			var modelChecker = (DtmcModelChecker)Activator.CreateInstance(typeOfModelChecker, dtmc);
			probabilityOfFalse = modelChecker.CalculateProbability(probabilityFinallyFalseFormula);

			probabilityOfFalse.Is(0.0, 0.001).ShouldBe(true);
		}

		private class C : Component
		{
			[Range(0, 4, OverflowBehavior.Clamp)]
			private int _value;

			protected internal override void Initialize()
			{
				_value = Choose(0, 1);
			}


			public override void Update()
			{
				_value++;
			}

		}
	}
}