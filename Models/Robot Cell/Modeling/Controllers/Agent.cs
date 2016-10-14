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

namespace SafetySharp.CaseStudies.RobotCell.Modeling.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using SafetySharp.Modeling;
	using Odp;

	internal abstract class Agent : BaseAgent<Agent, Task>
	{
		public readonly Fault ConfigurationUpdateFailed = new TransientFault();

		public Agent(params ICapability[] capabilities)
		{
			if (HasDuplicates(capabilities))
				throw new InvalidOperationException("Duplicate capabilities have no effect.");
			_availableCapabilities = new List<ICapability>(capabilities);
			ConfigurationUpdateFailed.Name = $"{Name}.{nameof(ConfigurationUpdateFailed)}";
		}

		/* TODO: agents cannot have duplicate capabilities
		 *
		 * Adding duplicate capabilities to agents (RobotAgents) has no effect:
		 * a robot may have multiple tools that perform the same action (e.g. multiple drills),
		 * but the agent has just one corresponding capability (as long as any of the tools are
		 * functioning).
		 *
		 * In the future, multiple capabilities should be supported, each associated with one tool.
		 * This would allow for the selection of less-used tools etc.
		 * To support this, we need to distinguish between functional equivalence and reference equality
		 * of capabilities in SafetySharp.Odp. For example, add IsEquivalentTo(ICapability) to the
		 * ICapability interface. Adjust all configuration mechanisms, agents etc. to
		 * use the appropriate comparison.
		 *
		 * */
		private bool HasDuplicates(ICapability[] capabilities)
		{
			var set = new HashSet<ICapability>();
			foreach (var cap in capabilities)
			{
				if (set.Contains(cap))
					return true;
				set.Add(cap);
			}
			return false;
		}

		protected readonly List<ICapability> _availableCapabilities;
		public override IEnumerable<ICapability> AvailableCapabilities => _availableCapabilities;

		public abstract string Name { get; }

		public bool HasResource => Resource != null;

		public override void Update()
		{
			CheckAllocatedCapabilities();
			base.Update();
		}

		protected override void DropResource()
		{
			Resource.Task.IsResourceInProduction = false;
			base.DropResource();
		}

		public virtual void Produce(ProduceCapability capability)
		{
		}

		public virtual void Process(ProcessCapability capability)
		{
		}

		public virtual void Consume(ConsumeCapability capability)
		{
		}

		// TODO: switch to original robot cell approach (Capability.Execute()) in ODP core?
		public override void ApplyCapability(ICapability capability)
		{
			if (capability is ProduceCapability)
				Produce(capability as ProduceCapability);
			else if (capability is ProcessCapability)
				Process(capability as ProcessCapability);
			else if (capability is ConsumeCapability)
				Consume(capability as ConsumeCapability);
			throw new InvalidOperationException();
		}

		public void CheckAllocatedCapabilities()
		{
			// We ignore faults for unused capabilities that are currently not used to improve general model checking efficiency
			// For DCCA efficiency, it would be beneficial, however, to check for faults of all capabilities and I/O relations;
			// this is also how the ODP seems to work

			// Using ToArray() to prevent modifications of the list during iteration...
			foreach (var capability in AvailableCapabilities.ToArray())
			{
				if (!CheckAllocatedCapability(capability))
					_availableCapabilities.Remove(capability);
			}

			foreach (var input in Inputs.ToArray())
			{
				if (!CheckInput(input))
					input.Disconnect(this);
			}

			foreach (var output in Outputs.ToArray())
			{
				if (!CheckOutput(output))
					this.Disconnect(output);
			}
		}

		protected virtual bool CheckAllocatedCapability(ICapability capability)
		{
			return true;
		}

		protected virtual bool CheckInput(Agent agent)
		{
			return true;
		}

		protected virtual bool CheckOutput(Agent agent)
		{
			return true;
		}

		[FaultEffect(Fault = nameof(ConfigurationUpdateFailed))]
		public abstract class ConfigurationUpdateFailedEffect : Agent
		{
			public ConfigurationUpdateFailedEffect(params ICapability[] capabilities)
				: base(capabilities) { }

			public override void RemoveAllocatedRoles(Task task) { }
			public override void AllocateRoles(params Role<Agent, Task>[] roles) { }
		}
	}
}