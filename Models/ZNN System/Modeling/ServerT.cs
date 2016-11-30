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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.ZNNSystem.Modeling
{
	/// <summary>
	/// Represents the Server of the ZNN.com News System
	/// </summary>
	public class ServerT : Component
	{
		/// <summary>
		/// Content fidelity level
		/// </summary>
		public enum EFidelity
		{
			/// <summary>
			/// Static Text
			/// </summary>
			Low = 1,

			/// <summary>
			/// Text and a bit multimedia
			/// </summary>
			Medium =2,

			/// <summary>
			/// Full multimedia
			/// </summary>
			High = 3
		}

		/// <summary>
		/// Currently available units of the server. 0 Means the server is inactive.
		/// </summary>
		private int _AvailableServerUnits;

		/// <summary>
		/// Currently units under use of the server.
		/// </summary>
		private int _UsedServerUnits;

		/// <summary>
		/// Maximum available units of the server.
		/// </summary>
		private int _MaxServerUnits;

		/// <summary>
		/// Current costs of the Server. 0 means the server is inactive.
		/// </summary>
		public int Cost => _AvailableServerUnits * Model.ServerUnitCost;

		/// <summary>
		/// Currently units under use of the server.
		/// </summary>
		public int UsedServerUnits
		{
			get { return _UsedServerUnits; }
			internal set { _UsedServerUnits = (value <= _MaxServerUnits) ? value : _MaxServerUnits; }
		}

		/// <summary>
		/// Current load of the Server in %.
		/// </summary>
		[Range(0, 100, OverflowBehavior.Clamp)]
		public int Load => UsedServerUnits / _AvailableServerUnits * 100;

		/// <summary>
		/// Current content fidelity level of the server.
		/// </summary>
		public StateMachine<EFidelity> FidelityStateMachine = EFidelity.High;

		/// <summary>
		/// The current executing queries
		/// </summary>
		private List<Query> ExecutingQueries { get; set; }

		/// <summary>
		/// Initialize a new server instance
		/// </summary>
		/// <param name="maxServerUnits">Max Server capacity units</param>
		private ServerT(int maxServerUnits)
		{
			_MaxServerUnits = maxServerUnits;
		}

		/// <summary>
		/// Gets a new server
		/// </summary>
		/// <returns>New Server Instance or null if errors occurs</returns>
		public static ServerT GetNewServer()
		{
			ServerT server = null;
			try
			{
				//server = new ServerT(new Random().Next(20, 50));
				server = new ServerT(Model.DefaultAvailableServerUnits);
				server._AvailableServerUnits = server._MaxServerUnits;
			}
			catch { }
			return server;
		}

		///// <summary>
		///// Deactivates the server
		///// </summary>
		///// <param name="serverList">The server pool the server is inside</param>
		///// <returns>True if successfull</returns>
		//public bool Deactivate(List<ServerT> serverList)
		//{
		//	return serverList.Remove(this);
		//}

		/// <summary>
		/// Sets the content fidelity level of the server
		/// </summary>
		public void SetFidelity(EFidelity level)
		{
			FidelityStateMachine.Transition(
					from: new[] { EFidelity.Medium, EFidelity.Low },
					to: EFidelity.High,
					guard: level == EFidelity.High)
				.Transition(
					from: new[] { EFidelity.High, EFidelity.Low },
					to: EFidelity.Medium,
					guard: level == EFidelity.Medium)
				.Transition(
					from: new[] { EFidelity.Medium, EFidelity.High },
					to: EFidelity.Low,
					guard: level == EFidelity.Low);
		}

		/// <summary>
		/// Adds the query to the list to be executing
		/// </summary>
		/// <param name="query">The query</param>
		public void AddQuery(Query query)
		{
			ExecutingQueries.Add(query);
		}

		/// <summary>
		/// Simulates an execution step of the query and returns true if the query was executed
		/// </summary>
		/// <param name="query">The query</param>
		/// <returns>True if the query was executed</returns>
		public bool ExecuteQueryStep(Query query)
		{
			return ExecutingQueries.IndexOf(query) < _AvailableServerUnits;
		}

		/// <summary>
		/// Remove a completly executed query from the list
		/// </summary>
		/// <param name="query"></param>
		public void QueryComplete(Query query)
		{
			ExecutingQueries.Remove(query);
		}
		
	}
}
