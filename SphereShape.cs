using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles
{
	public class SphereShape : Shape
	{
		public double radius;

		public override double GetBoundsRadius()
		{
			return radius;
		}
	}
}
