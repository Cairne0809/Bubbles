using MathematicsX;

namespace Bubbles
{
	public struct Ray3D
	{
		private Vec3 m_origin;
		private Vec3 m_direction;

		public Ray3D(Vec3 origin, Vec3 direction)
		{
			m_origin = origin;
			m_direction = direction.Normalize();
		}

		public Vec3 Origin
		{
			get { return m_origin; }
			set { m_origin = value; }
		}
		public Vec3 Direction
		{
			get { return m_direction; }
			set { m_direction = value.Normalize(); }
		}

		public Vec3 GetPoint(double distance)
		{
			return m_origin + m_direction * distance;
		}
	}

	public struct RayCast3D
	{
		private Ray3D m_ray;
		public double maxDistance;
		public double currentDistance;

		public RayCast3D(Ray3D ray, double maxDistance = double.MaxValue)
		{
			m_ray = ray;
			this.maxDistance = maxDistance;
			currentDistance = 0;
		}

		public Ray3D Ray
		{
			get { return m_ray; }
		}
		
	}
}
