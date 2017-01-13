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

namespace SafetySharp.Analysis
{
	using System;
	using System.Diagnostics;
	using ModelChecking;
	using Modeling;
	using Runtime;
	using Runtime.Serialization;
	using Utilities;

	/// <summary>
	///   Represents a model checker specifically created to check S# models.
	/// </summary>
	public class QualitativeChecker<TExecutableModel> where TExecutableModel : ExecutableModel<TExecutableModel>
	{
		/// <summary>
		///   The model checker's configuration that determines certain model checker settings.
		/// </summary>
		public AnalysisConfiguration Configuration = AnalysisConfiguration.Default;

		/// <summary>
		///   Raised when the model checker has written an output. The output is always written to the console by default.
		/// </summary>
		public event Action<string> OutputWritten = Console.WriteLine;
		

		/// <summary>
		///   Checks the invariant encoded into the model created by <paramref name="createModel" />.
		/// </summary>
		internal AnalysisResult<TExecutableModel> CheckInvariant(Func<TExecutableModel> createModel, int formulaIndex)
		{
			// We have to track the state vector layout here; this will nondeterministically store some model instance of
			// one of the workers; but since all state vectors are the same, we don't care
			ExecutedModel<TExecutableModel> model = null;
			Func<AnalysisModel<TExecutableModel>> createAnalysisModel = () =>
				model = new ActivationMinimalExecutedModel<TExecutableModel>(createModel, Configuration.SuccessorCapacity);

			using (var checker = new InvariantChecker<TExecutableModel>(createAnalysisModel, OutputWritten, Configuration, formulaIndex))
			{
				var result = checker.Check();
				return result;
			}
		}

		/// <summary>
		///   Checks the invariant encoded into the model created by <paramref name="createModel" />.
		/// <param name="invariant">The invariant that should be checked.</param>
		/// </summary>
		public AnalysisResult<TExecutableModel> CheckInvariant(Func<Formula[], Func<TExecutableModel>> createModel, Formula invariant)
		{
			// We have to track the state vector layout here; this will nondeterministically store some model instance of
			// one of the workers; but since all state vectors are the same, we don't care
			ExecutedModel<TExecutableModel> model = null;
			var formulas = new [] { invariant };
			Func<AnalysisModel<TExecutableModel>> createAnalysisModel = () =>
				model = new ActivationMinimalExecutedModel<TExecutableModel>(createModel(formulas), Configuration.SuccessorCapacity);

			using (var checker = new InvariantChecker<TExecutableModel>(createAnalysisModel, OutputWritten, Configuration, 0))
			{
				var result = checker.Check();
				return result;
			}
		}

		/// <summary>
		///   Generates a <see cref="StateGraph" /> for the model created by <paramref name="createModel" />.
		/// </summary>
		internal StateGraph<TExecutableModel> GenerateStateGraph(Func<TExecutableModel> createModel, params Formula[] stateFormulas)
		{
			// We have to track the state vector layout here; this will nondeterministically store some model instance of
			// one of the workers; but since all state vectors are the same, we don't care
			ExecutedModel<TExecutableModel> model = null;
			Func<AnalysisModel<TExecutableModel>> createAnalysisModel = () =>
				model = new ActivationMinimalExecutedModel<TExecutableModel>(createModel, Configuration.SuccessorCapacity);

			using (var checker = new StateGraphGenerator<TExecutableModel>(createAnalysisModel, stateFormulas, OutputWritten, Configuration))
			{
				var stateGraph = checker.GenerateStateGraph();
				return stateGraph;
			}
		}
		

		/// <summary>
		///   Checks whether the <paramref name="invariants" /> holds in all states of the <paramref name="model" />.
		/// </summary>
		/// <param name="model">The model that should be checked.</param>
		/// <param name="invariants">The invariants that should be checked.</param>
		public AnalysisResult<TExecutableModel>[] CheckInvariants(Func<Formula[], Func<TExecutableModel>> createModel, params Formula[] invariants)
		{
			Requires.NotNull(createModel, nameof(createModel));
			Requires.NotNull(invariants, nameof(invariants));
			Requires.That(invariants.Length > 0, nameof(invariants), "Expected at least one invariant.");

			var modelGenerator = createModel(invariants);

			var stateGraph = GenerateStateGraph(modelGenerator, invariants);
			var results = new AnalysisResult<TExecutableModel>[invariants.Length];

			for (var i = 0; i < invariants.Length; ++i)
				results[i] = CheckInvariant(stateGraph, invariants[i]);

			return results;
		}

		/// <summary>
		///   Checks whether the <paramref name="invariant" /> holds in all states of the <paramref name="stateGraph" />.
		/// </summary>
		/// <param name="stateGraph">The state graph that should be checked.</param>
		/// <param name="invariant">The invariant that should be checked.</param>
		internal AnalysisResult<TExecutableModel> CheckInvariant(StateGraph<TExecutableModel> stateGraph, Formula invariant)
		{
			Requires.NotNull(stateGraph, nameof(stateGraph));
			Requires.NotNull(invariant, nameof(invariant));

			var formulaIndex = Array.IndexOf(stateGraph.StateFormulas, invariant);

			Requires.That(formulaIndex != -1, nameof(invariant),
				"The invariant cannot be analyzed over the state graph. Use the same " +
				$"'{typeof(Formula).FullName}' instance as during the construction of the state graph.");


			// We have to track the state vector layout here; this will nondeterministically store some model instance of
			// one of the workers; but since all state vectors are the same, we don't care
			AnalysisModel<TExecutableModel> model = null;
			Func<AnalysisModel<TExecutableModel>> createAnalysisModel = () =>
					model = new StateGraphModel<TExecutableModel>(stateGraph, Configuration.SuccessorCapacity);

			using (var checker = new InvariantChecker<TExecutableModel>(createAnalysisModel, OutputWritten, Configuration, formulaIndex))
			{
				var result = checker.Check();
				return result;
			}
		}
	}
}