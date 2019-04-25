using System;
using MathematicsX;

namespace Bubbles
{
	public struct Bounds
	{
		internal Body m_body;

        private Vec3 m_center;
		private double m_radius;
		private bool m_changeFlag;
		
		public Bounds(Body body)
		{
			m_body = body;
			m_center = default(Vec3);
			m_radius = default(double);
			m_changeFlag = true;
		}
		public Bounds(Vec3 center, double radius)
		{
			m_body = null;
			m_center = center;
			m_radius = radius;
			m_changeFlag = true;
		}
		public Bounds(Body body, Vec3 center, double radius)
		{
			m_body = body;
			m_center = center;
			m_radius = radius;
			m_changeFlag = true;
		}

		public Body Body
		{
			get { return m_body; }
		}

		public Vec3 Center
		{
			get { return m_center; }
			set { m_center = value; m_changeFlag = true; }
		}

		public double Radius
		{
			get { return m_radius; }
			set { m_radius = value; m_changeFlag = true; }
		}

		public double Value
		{
			get { return m_radius; }
		}

		internal bool ReadChangeFlag()
		{
			bool flag = m_changeFlag;
			m_changeFlag = false;
			return flag;
		}
		
		public Bounds Union(Bounds b)
		{
			Vec3 dp = m_center - b.m_center;
			double dist = VecX.Length(dp);
			if (m_radius >= dist + b.m_radius) return this;
			if (b.m_radius >= dist + m_radius) return b;
			Vec3 p0 = b.m_center + dp * (1.0 + m_radius / dist);
			Vec3 p1 = m_center - dp * (1.0 + b.m_radius / dist);
			return new Bounds((p0 + p1) * 0.5, VecX.Distance(p0, p1) * 0.5);
		}

		public double UnionValue(Bounds b)
		{
			Vec3 dp = m_center - b.m_center;
			double dist = VecX.Length(dp);
			if (m_radius >= dist + b.m_radius) return m_radius;
			if (b.m_radius >= dist + m_radius) return b.m_radius;
			Vec3 p0 = b.m_center + dp * (1.0 + m_radius / dist);
			Vec3 p1 = m_center - dp * (1.0 + b.m_radius / dist);
			return VecX.Distance(p0, p1) * 0.5;
		}

		public bool Overlaps(Bounds bounds)
		{
			double sumR = m_radius + bounds.m_radius;
			return sumR * sumR >= VecX.SqrDistance(m_center, bounds.m_center);
		}

		public bool Overlaps(Ray3D ray, out double distance)
		{
			//SqrDistance(C, O+D*t) = R^2
			//(D*D) * t^2 + (-2*OC*D) * t + (OC*OC-R^2) = 0
			Vec3 dir = ray.Direction;
			Vec3 voc = m_center - ray.Origin;
			double negB = VecX.Dot(voc, dir) * 2;
			double C = VecX.SqrLength(voc) - m_radius * m_radius;
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

		public bool Overlaps(ref RayCast3D rayCast)
		{
			double dist;
			if (Overlaps(rayCast.Ray, out dist) && dist <= rayCast.maxDistance)
			{
				rayCast.currentDistance = dist;
				return true;
			}
			return false;
		}

		
		public static Bounds FromDiameter(Vec3 p0, Vec3 p1)
		{
			return new Bounds((p0 + p1) / 2, VecX.Distance(p0, p1) / 2);
		}
		
    }
}
