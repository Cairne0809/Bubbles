﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles
{
	public struct SphereShape : IShape
	{
		public double radius;

		public SphereShape(double radius)
		{
			this.radius = radius;
		}

		public double GetBoundsRadius()
		{
			return radius;
		}
	}
}
