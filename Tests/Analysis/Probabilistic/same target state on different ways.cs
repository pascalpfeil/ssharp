﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2016, Institute for Software & Systems Engineering
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


namespace Tests.Analysis.Probabilistic
{
	using System;
	using SafetySharp.Analysis;
	using SafetySharp.Modeling;
	using Shouldly;
	using Utilities;

	internal class SameTargetStateOnDifferentWays : ProbabilisticAnalysisTestObject
	{
		protected override void Check()
		{
			var c = new C();
			Probability probabilityOfFinal1;

			using (var probabilityChecker = new ProbabilityChecker(TestModel.InitializeModel(c)))
			{
				var typeOfModelChecker = (Type)Arguments[0];
				var modelChecker = (ProbabilisticModelChecker)Activator.CreateInstance(typeOfModelChecker, probabilityChecker);

				Formula final1 = c.Result==1;
				var checkProbabilityOfFinal1 = probabilityChecker.CalculateProbabilityToReachStates(final1);
				probabilityChecker.CreateProbabilityMatrix();
				probabilityChecker.DefaultChecker = modelChecker;
				probabilityOfFinal1 = checkProbabilityOfFinal1.Check();
			}
			
			probabilityOfFinal1.Be(0.65, 0.000001).ShouldBe(true);
		}

		private class C : Component
		{
			public int Result;
			
			public override void Update()
			{
				if (Result == 0)
				{
					if (Choose(new Option<bool>(new Probability(0.1), true),
							   new Option<bool>(new Probability(0.9), false)))
					{
						if (Choose(new Option<bool>(new Probability(0.2), true),
							   new Option<bool>(new Probability(0.8), false)))
						{
							//way 1
							Result = 1;
						}
						else
						{
							Result = 2;
						}
					}
					else
					{
						if (Choose(new Option<bool>(new Probability(0.3), true),
							   new Option<bool>(new Probability(0.7), false)))
						{
							Result = 3;
						}
						else
						{
							//way 2
							Result = 1;
						}
					}
				}
			}
		}
	}
}
