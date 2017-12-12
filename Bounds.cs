using System;
using MathematicsX;

namespace Bubbles
{
    public struct Bounds
    {
        public Vec3 center;
        public double radius;

		public Bounds(Vec3 center, double radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public bool isNaB { get { return center.isNaV || double.IsNaN(radius); } }

		public bool Contains(Vec3 point)
		{
			return radius >= Vec3.Distance(center, point);
		}

		public bool Contains(Bounds bounds)
		{
			return radius >= Vec3.Distance(center, bounds.center) + bounds.radius;
		}

		public bool Intersects(Bounds bounds)
		{
			return radius + bounds.radius >= Vec3.Distance(center, bounds.center);
		}

		public bool IntersectRay(Ray ray, out double distance)
		{
			//SqrDistance(C, O+D*t) = R^2
			//(D*D) * t^2 + (-2*OC*D) * t + (OC*OC-R^2) = 0
			Vec3 orig = ray.origin;
			Vec3 dir = ray.direction;
			Vec3 voc = center - orig;
			double negB = voc * dir * 2;
			double C = voc.sqrMagnitude - radius * radius;
			double delta = negB * negB - 4 * C;
			if (delta >= 0)
			{
				delta = Math.Sqrt(delta);
				if (negB + delta >= 0)
				{
					distance = (negB - delta) / 2;
					return true;
				}
			}
			distance = 0;
			return false;
		}

		public static Bounds FromDiameter(Vec3 p0, Vec3 p1)
		{
			return new Bounds((p0 + p1) / 2, Vec3.Distance(p0, p1) / 2);
		}

		public static Bounds operator +(Bounds lhs, Bounds rhs)
		{
			Vec3 vrl = lhs.center - rhs.center;
			double dist = vrl.magnitude;
			if (lhs.radius >= dist + rhs.radius) return lhs;
			if (rhs.radius >= dist + lhs.radius) return rhs;
			Vec3 p0 = rhs.center + vrl * (1 + lhs.radius / dist);
			Vec3 p1 = lhs.center - vrl * (1 + rhs.radius / dist);
			return new Bounds((p0 + p1) / 2, Vec3.Distance(p0, p1) / 2);
		}

		public static Bounds NaB { get { return new Bounds(Vec3.NaV, double.NaN); } }
		
    }
}
