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

		public double mass { get; set; }
		public double resilience { get; set; }
		public virtual Vec3 position { get; set; }
		public virtual Quat rotation { get; set; }
		public virtual Vec3 velocity { get; set; }
		public virtual Vec3 acceleration { get; set; }
		public virtual Vec3 angularVelocity { get; set; }
		public virtual Vec3 angularAcceleration { get; set; }
		public virtual Bounds bounds { get { return new Bounds(); } }

		Vec3 m_trimPos;
		Vec3 m_trimVel;

		protected Body(BodyDef def)
		{
			mass = def.mass;
			resilience = def.resilience;
			position = def.position;
			rotation = def.rotation.sqrMagnitude == 0 ? Quat.identity : def.rotation;
		}
		protected Body(double mass, double resilience, Vec3 position, Quat rotation)
		{
			this.mass = mass;
			this.resilience = resilience;
			this.position = position;
			this.rotation = rotation.sqrMagnitude == 0 ? Quat.identity : rotation;
		}

		public Body Next()
		{
			return m_next;
		}

		internal bool PrimaryUpdate(double deltaTime)
		{
			if (deltaTime > 0)
			{
				velocity += acceleration * deltaTime;
				if (velocity.sqrMagnitude > 0)
				{
					position += velocity * deltaTime;
					return true;
				}
			}
			return false;
		}

		internal virtual void UpdatePair(Body other)
		{
			
		}

		internal bool FinalUpdate()
		{
			if (m_trimVel.sqrMagnitude > 0)
			{
				velocity += m_trimVel;
				m_trimVel = Vec3.zero;
			}
			if (m_trimPos.sqrMagnitude > 0)
			{
				position += m_trimPos;
				m_trimPos = Vec3.zero;
				return true;
			}
			return false;
		}

		internal void SetTrimVel(Vec3 trim)
		{
			if (trim.sqrMagnitude > m_trimVel.sqrMagnitude)
			{
				m_trimVel = trim;
			}
		}

		internal void SetTrimPos(Vec3 trim)
		{
			if (trim.sqrMagnitude > m_trimPos.sqrMagnitude)
			{
				m_trimPos = trim;
			}
		}


		static uint biasIndex = 0;
		internal static Vec3 GetBias()
		{
			Vec3 bias = new Vec3();
			switch (biasIndex % 6)
			{
				case 0: bias.x = 1e-100 * biasIndex; break;
				case 1: bias.x = -1e-100 * biasIndex; break;
				case 2: bias.y = 1e-100 * biasIndex; break;
				case 3: bias.y = -1e-100 * biasIndex; break;
				case 4: bias.z = 1e-100 * biasIndex; break;
				case 5: bias.z = -1e-100 * biasIndex; break;
			}
			biasIndex++;
			if (biasIndex == uint.MaxValue) biasIndex = 0;
			return bias;
		}
    }
}
