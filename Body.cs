using System;
using System.Collections.Generic;
using MathematicsX;

namespace Bubbles
{
	public class Body : IComparable<Body>
	{
		internal int m_proxyId;
		internal Body m_prev;
		internal Body m_next;

		private Shape m_shape;
		private bool m_isKinematic;
		private double m_mass;
		private double m_bounce;
		//private double m_friction;
		private Quat m_rotation;
		private Vec3 m_position;
		private Vec3 m_velocity;
		private Vec3 m_angularVelocity;
		private Vec3 m_acceleration;
		private Vec3 m_angularAcceleration;
		private Bounds m_bounds;
		
		private Vec3 m_trimPos;
		private Vec3 m_trimVel;

		internal Body(BodyDef def)
		{
			m_shape = def.shape;
			m_isKinematic = def.isKinematic;
			m_mass = def.mass;
			m_bounce = def.bounce;
			//m_friction = def.friction;
			m_rotation = def.rotation.Equals(Quat.zero) ? Quat.identity : def.rotation;
			m_position = def.position;
			m_bounds = new Bounds(this, m_position, m_shape.Radius);
		}

		public Body Next
		{
			get { return m_next; }
		}

		public Shape Shape
		{
			get { return m_shape; }
		}

		public bool IsKinematic
		{
			get { return m_isKinematic; }
			set { m_isKinematic = value; }
		}

		public double Mass
		{
			get { return m_mass; }
			set { m_mass = value; }
		}
		public double Bounce
		{
			get { return m_bounce; }
			set { m_bounce = value; }
		}
		/*public double Friction
		{
			get { return m_friction; }
			set { m_friction = value; }
		}*/

		public Quat Rotation
		{
			get { return m_rotation; }
			set { m_rotation = value; }
		}
		public Vec3 Position
		{
			get { return m_position; }
			set { m_bounds.Center = m_position = value; }
		}
		public Vec3 Velocity
		{
			get { return m_velocity; }
			set { m_velocity = value; }
		}
		public Vec3 AngularVelocity
		{
			get { return m_angularVelocity; }
			set { m_angularVelocity = value; }
		}
		public Vec3 Acceleration
		{
			get { return m_acceleration; }
			set { m_acceleration = value; }
		}
		public Vec3 AngularAcceleration
		{
			get { return m_angularAcceleration; }
			set { m_angularAcceleration = value; }
		}

		public Bounds Bounds
		{
			get { return m_bounds; }
		}

		public Vec3 GetFarthestPoint(Vec3 direction)
		{
			return m_position + m_rotation * m_shape.GetFarthestPoint(~m_rotation * direction);
		}


		internal void PrimaryUpdate(double deltaTime, bool clearForce)
		{
			if (deltaTime > 0)
			{
				if (!m_isKinematic)
				{
					m_velocity += m_acceleration * deltaTime;
					m_angularVelocity += m_angularAcceleration * deltaTime;
					if (clearForce)
					{
						m_acceleration = m_angularAcceleration = Vec3.zero;
					}
				}
				if (!m_velocity.Equals(Vec3.zero))
				{
					Position = m_position + m_velocity * deltaTime;
				}
			}
		}
		
		internal void FinalUpdate()
		{
			if (!m_trimVel.Equals(Vec3.zero))
			{
				m_velocity += m_trimVel;
				m_trimVel = Vec3.zero;
			}
			if (!m_trimPos.Equals(Vec3.zero))
			{
				Position = m_position + m_trimPos;
				m_trimPos = Vec3.zero;
			}
		}

		internal void SetTrim(Vec3 trimPos, Vec3 trimVel)
		{
			if (VecX.SqrLength(trimPos) > VecX.SqrLength(m_trimPos))
			{
				m_trimPos = trimPos;
				m_trimVel = trimVel;
			}
		}

		public int CompareTo(Body other)
		{
			int dp = m_proxyId - other.m_proxyId;
			if (dp == 0)
			{
				return GetHashCode() - other.GetHashCode();
			}
			return dp;
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
