using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mathx;

namespace Bubbles
{
	public class SphereCollider : Collider
	{
		double m_radius;

		override public Bounds bounds { get { return new Bounds(m_transform.position, m_radius); } }

		public SphereCollider(double radius)
		{
			m_radius = radius;
		}

		internal void OnCollision()
		{

		}
	}
}
