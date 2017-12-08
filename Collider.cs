using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mathx;

namespace Bubbles
{
	public class Collider
	{
		internal int proxyId;

		protected Transform m_transform;

		public Transform transform { get { return m_transform; } }
		public RigidBody rigidBody { get { return m_transform.parent == null ? null : m_transform.parent.userData as RigidBody; } }
		virtual public Bounds bounds { get { return new Bounds(m_transform.position, 0); } }

		public Collider()
		{
			m_transform = new Transform(this, 0);
		}

		public void Destroy()
		{
			if (m_transform.parent != null)
			{
				rigidBody.DestroyCollider(this);
			}
		}

	}
}
