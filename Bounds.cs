using System;
using MathematicsX;

namespace Bubbles
{
	public struct Bounds : ISplitFactor
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

		public Bounds Combine(Vec3 point)
		{
			Vec3 vrl = center - point;
			double dist = vrl.magnitude;
			if (radius >= dist) return this;
			Vec3 p0 = point + vrl * (1 + radius / dist);
			return new Bounds((p0 + point) * 0.5, Vec3.Distance(p0, point) * 0.5);
		}
		public Bounds Combine(Bounds bounds)
		{
			Vec3 vrl = center - bounds.center;
			double dist = vrl.magnitude;
			if (radius >= dist + bounds.radius) return this;
			if (bounds.radius >= dist + radius) return bounds;
			Vec3 p0 = bounds.center + vrl * (1 + radius / dist);
			Vec3 p1 = center - vrl * (1 + bounds.radius / dist);
			return new Bounds((p0 + p1) * 0.5, Vec3.Distance(p0, p1) * 0.5);
		}

		public bool Intersects(Vec3 point)
		{
			return radius >= Vec3.Distance(center, point);
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
			double negB = Vec3.Dot(voc, dir) * 2;
			double C = voc.sqrMagnitude - radius * radius;
			double delta = negB * negB - 4 * C;
			if (delta >= 0)
			{
				delta = Math.Sqrt(delta);
				if (negB + delta >= 0)
				{
					distance = (negB - delta) * 0.5;
					return true;
				}
			}
			distance = 0;
			return false;
		}

		public bool RayCast(RayCastInput input)
		{
			double distance;
			bool result = IntersectRay(input.ray, out distance);
			if (result) result = distance <= input.maxDistance;
			return result;
		}

		public double GetValue()
		{
			return radius;
		}

		public ISplitFactor Combine(ISplitFactor splitf)
		{
			if (splitf is Bounds) return Combine((Bounds)splitf);
			return this;
		}

		public bool QueryCheck(object input, int type)
		{
			if (type == QueryCheck_Intersects) return Intersects((Bounds)input);
			if (type == QueryCheck_RayCast) return RayCast((RayCastInput)input);
			return false;
		}


		public const int QueryCheck_Intersects = 0;
		public const int QueryCheck_RayCast = 1;

		public static Bounds FromDiameter(Vec3 p0, Vec3 p1)
		{
			return new Bounds((p0 + p1) / 2, Vec3.Distance(p0, p1) / 2);
		}

		public static Bounds NaB { get { return new Bounds(Vec3.NaV, double.NaN); } }
		
    }
}
