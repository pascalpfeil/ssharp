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

namespace SafetySharp.CaseStudies.ProductionCell.Modeling.Controllers
{
	using System.Collections.Generic;
	using SafetySharp.Modeling;

	internal class Agent : Component
	{
		public Agent(params Capability[] capabilities)
		{
			AvailableCapabilites.AddRange(capabilities);
			AllocatedRoles.Capacity = capabilities.Length;
		}

		public List<Capability> AvailableCapabilites { get; } = new List<Capability>();
		public List<Role> AllocatedRoles { get; } = new List<Role>();

		[Hidden(HideElements = true)]
		public List<Agent> Outputs { get; } = new List<Agent>();

		[Hidden(HideElements = true)]
		public List<Agent> Inputs { get; } = new List<Agent>();

		public extern void TriggerReconfiguration();

		public static void Connect(Agent from, Agent to)
		{
			from.Outputs.Add(to);
			to.Inputs.Add(from);
		}
	}
}