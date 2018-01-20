using MathematicsX;

namespace Bubbles
{
	public class Body
    {
		internal int m_proxyId = -1;
		internal Body m_next = null;
		internal Body m_prev = null;
		
		internal bool m_isStatic = false;
		internal bool m_isAsleep = false;

		public double mass { get; set; }
		public double bounce { get; set; }
		public double friction { get; set; }
		public Shape shape { get; set; }

		public bool isStatic { get { return m_isStatic; } }
		public bool isAsleep { get { return m_isAsleep; } }
		public Vec3 position { get; set; }
		public Quat rotation { get; set; }
		public Vec3 eulerAngles { get { return Quat.ToEuler(rotation); } set { rotation = Quat.FromEuler(value); } }
		public Vec3 velocity { get; set; }
		public Vec3 acceleration { get; set; }
		public Vec3 angularVelocity { get; set; }
		public Vec3 angularAcceleration { get; set; }
		
		Vec3 m_trimPos;
		Vec3 m_trimVel;

		internal Body(BodyDef def)
		{
			mass = def.mass;
			bounce = def.bounce;
			friction = def.friction;
			position = def.position;
			rotation = Quat.SqrLength(def.rotation) == 0 ? Quat.identity : def.rotation;
			shape = def.shape;
		}

		public Body Next()
		{
			return m_next;
		}

		public Bounds GetBounds()
		{
			return new Bounds(position, shape.GetBoundsRadius());
		}

		internal void AddToList(ref Body list)
		{
			if (list != null)
			{
				m_next = list;
				list.m_prev = this;
			}
			list = this;
		}
		internal void RemoveFromList(ref Body list)
		{
			if (list == this)
			{
				list = m_next;
			}
			if (m_next != null)
			{
				m_next.m_prev = m_prev;
			}
			if (m_prev != null)
			{
				m_prev.m_next = m_next;
			}
			m_prev = m_next = null;
		}

		internal bool PrimaryUpdate(double deltaTime)
		{
			if (deltaTime > 0)
			{
				velocity += acceleration * deltaTime;
				if (VecX.SqrLength(velocity) > 0)
				{
					position += velocity * deltaTime;
					//position += Relativity.Velocity(velocity) * deltaTime;
					acceleration = Vec3.zero;
					return true;
				}
			}
			acceleration = Vec3.zero;
			return false;
		}

		internal bool FinalUpdate()
		{
			if (VecX.SqrLength(m_trimVel) > 0)
			{
				velocity += m_trimVel;
				m_trimVel = Vec3.zero;
			}
			if (VecX.SqrLength(m_trimPos) > 0)
			{
				position += m_trimPos;
				m_trimPos = Vec3.zero;
				return true;
			}
			return false;
		}

		internal void SetTrim(Vec3 trimPos, Vec3 trimVel)
		{
			if (VecX.SqrLength(trimPos) > VecX.SqrLength(m_trimPos))
			{
				m_trimPos = trimPos;
				m_trimVel = trimVel;
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
