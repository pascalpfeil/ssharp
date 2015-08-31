﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace Elbtunnel.Controllers
{
	using SafetySharp.Modeling;
	using Sensors;
	using SharedComponents;

	/// <summary>
	///   Represents the original design of the end-control.
	/// </summary>
	public class EndControlAdditionalLightBarrier : Component, IEndControl
    {
        /// <summary>
        ///   The sensor that is used to detect vehicles in the end-control area on the left lane.
        /// </summary>
        private readonly IVehicleDetector _leftLaneOd;

        /// <summary>
        ///   The sensor that is used to detect over-height vehicles in the end-control area on the right lane.
        /// </summary>
        private readonly IVehicleDetector _rightLaneLb;

        /// <summary>
        ///   The timer that is used to deactivate the end-control automatically.
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        ///   The number of high vehicles currently in the main-control area.
        /// </summary>
        // TODO: OverflowBehavior.Error
        private int _count;

        /// <summary>
        ///   Initializes a new instance.
        /// </summary>
        /// <param name="leftLaneOd">The sensor that should be used to detect vehicles in the end-control area on the left lane.</param>
		/// <param name="rightLaneLb">The sensor that should be used to detect over-height vehicles in the end-control area on the right lane.</param>
        /// <param name="timeout">The amount of time after which the end-control is deactivated.</param>
        public EndControlAdditionalLightBarrier(IVehicleDetector leftLaneOd, IVehicleDetector rightLaneLb, int timeout)
		{
			_timer = new Timer(timeout);
            _leftLaneOd = leftLaneOd;
            _rightLaneLb = rightLaneLb;
        }

		/// <summary>
		///   Gets a value indicating whether a crash is potentially imminent.
		/// </summary>
		public bool IsCrashPotentiallyImminent()
		{
			return (_count>0) && _leftLaneOd.IsVehicleDetected();
		}

        /// <summary>
        ///   Gets the number of vehicles that entered the area in front of the end control during the current system step.
        /// </summary>
        public extern bool VehicleEntering();

		/// <summary>
		///   Updates the internal state of the component.
		/// </summary>
		public override void Update()
		{
			if (VehicleEntering())
			{
                _count++;
                _timer.Start();
			}

		    if (_timer.HasElapsed())
		        _count=0;

		    if (_rightLaneLb.IsVehicleDetected() && _count > 0)
		        _count--;

            if (_count == 0)
                _timer.Stop();

		    if (_count < 0)
		        _count = 0;
		}
	}
}