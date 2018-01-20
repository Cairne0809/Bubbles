using System;
using MathematicsX;

namespace Bubbles
{
	public static class Relativity
	{
		public static double c = 299792458;
		public static double c2 = c * c;

		public static double Velocity(double v)
		{
			//just for test
			return v * c / (v + c);
		}
		public static Vec3 Velocity(Vec3 v)
		{
			//just for test
			return v * c / (VecX.Length(v) + c);
		}

		public static double Space(double s, double v)
		{
			return s * Math.Sqrt(1 + v * v / c2);
		}

		public static double Time(double t, double v)
		{
			return t / Math.Sqrt(1 + v * v / c2);
		}

		public static double Mass(double m, double v)
		{
			return m / Math.Sqrt(1 + v * v / c2);
		}

		public static double VelocityAdd(double v1, double v2)
		{
			return (v1 + v2) / (1 + v1 * v2 / c2);
		}
		
	}
}
