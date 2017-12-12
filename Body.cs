using System;
using MathematicsX;
using Bubbles;

namespace Bubbles
{
	public class Body
    {
		internal int m_proxyId = -1;
		internal Body m_next = null;
		internal Body m_prev = null;

		public virtual double radius { get; set; }
		public virtual Vec3 position { get; set; }
		public virtual Quat rotation { get; set; }
		public virtual Vec3 lineVelocity { get; set; }
		public virtual Vec3 angularVelocity { get; set; }
		public virtual Bounds bounds { get { return new Bounds(position, radius); } }

		Vec3 m_trimPosition;

		internal Body()
		{
			rotation = Quat.identity;
		}

		public Body Next()
		{
			return m_next;
		}

		internal bool PrimaryUpdate(double deltaTime)
		{
			m_trimPosition = Vec3.zero;

			if (deltaTime > 0 && lineVelocity != Vec3.zero)
			{
				position += lineVelocity * deltaTime;
				return true;
			}
			return false;
		}

		internal void UpdatePair(Body other, bool jo)
		{
			Vec3 delta = position - other.position;
			if (delta == Vec3.zero) delta = Vec3.right * (jo ? 1 : -1);
			double mul = (radius + other.radius) / delta.magnitude - 1;
			m_trimPosition += delta * mul / 2;
		}

		internal bool FinalUpdate()
		{
			if (m_trimPosition != Vec3.zero)
			{
				position += m_trimPosition;
				return true;
			}
			return false;
		}

    }
}
