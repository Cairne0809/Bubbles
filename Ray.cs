using MathematicsX;

namespace Bubbles
{
	public struct Ray
	{
		Vec3 m_origin;
		Vec3 m_direction;

		public Vec3 origin { get { return m_origin; } set { m_origin = value; } }
		public Vec3 direction { get { return m_direction; } set { m_direction = VecX.Normalize(value); } }

		public Ray(Vec3 origin, Vec3 direction)
		{
			m_origin = origin;
			m_direction = VecX.Normalize(direction);
		}

		public Vec3 GetPoint(double distance)
		{
			return origin + direction * distance;
		}
	}
}
