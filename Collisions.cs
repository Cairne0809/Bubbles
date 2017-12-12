using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathematicsX;

namespace Bubbles
{
	public static class Collisions
	{
		public static bool TestOverlap(Body A, Body B)
		{
			return Vec3.Distance(A.position, B.position) < A.radius + B.radius;
		}
	}
}
