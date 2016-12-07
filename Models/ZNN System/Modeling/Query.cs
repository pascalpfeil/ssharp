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

using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.ZNNSystem.Modeling
{
	/// <summary>
	/// Class for Query and Response
	/// </summary>
	public class Query : Component
	{
		/// <summary>
		/// States for query states
		/// </summary>
		public enum State
		{
			Idle,
			QueryToProxy,
			QueryToServer,
			OnServer,
			LowFidelityComplete,
			MediumFidelityComplete,
			HighFidelityComplete,
			ResToProxy,
			ResToClient
		}

		/// <summary>
		/// State machine for query states
		/// </summary>
		public StateMachine<State> StateMachine = State.Idle;

		/// <summary>
		/// The server to execute the query
		/// </summary>
		public ServerT SelectedServer { get; set; }

		/// <summary>
		/// Creates a new query instance
		/// </summary>
		/// <param name="client">The client to request the query</param>
		public Query(ClientT client)
		{
			Client = client;
			SelectedServer = null;
		}

		public override void Update() { }
	}
}
