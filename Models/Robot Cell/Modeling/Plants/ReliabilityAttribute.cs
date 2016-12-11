﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetySharp.CaseStudies.RobotCell.Modeling.Plants
{
    [AttributeUsage(AttributeTargets.Field)]
    class ReliabilityAttribute : Attribute
    {
        /// <summary>
        /// Mean time to failure in simulation steps
        /// </summary>
        public double MTTF { get; set; }
        public double Lambda { get; }


        /// <summary>Initializes a new instance of the <see cref="T:System.Attribute" /> class.</summary>
        public ReliabilityAttribute(double mttf)
        {
            if (mttf <= 0)
                throw new ArgumentOutOfRangeException();
            this.MTTF = mttf;
            Lambda = 1 / mttf;
        }

    }
}