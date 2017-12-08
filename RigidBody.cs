using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathX;

namespace Bubbles
{
	public class RigidBody
    {
		Transform m_transform;

		public Transform transform { get { return m_transform; } }
		public World world { get { return m_transform.parent == null ? null : m_transform.parent.userData as World; } }

		public Vec3 velocity;
		public Vec3 angularVelocity;

		public RigidBody()
		{
			m_transform = new Transform(this, 1);
		}

		internal void Update(double deltaTime)
		{
			if (velocity != Vec3.zero)
			{
				m_transform.position += velocity * deltaTime;
				m_transform.ForEach((Transform child) =>
				{
					Collider collider = child.userData as Collider;
					world.m_broadPhase.MoveProxy(collider.proxyId, collider.bounds);
				});
			}
		}

		public void Destroy()
		{
			if (m_transform.parent != null)
			{
				world.DestroyBody(this);
			}
			else
			{
				m_transform.RemoveAll((Transform child) =>
				{
					(child.userData as Collider).Destroy();
				});
			}
		}

		public void ForEachCollider(Action<Collider> forEach)
		{
			m_transform.ForEach((Transform child) =>
			{
				forEach(child.userData as Collider);
			});
		}

		public void DestroyCollider(Collider collider)
		{
			if (m_transform.Remove(collider.transform))
			{
				world.m_broadPhase.DestroyProxy(collider.proxyId);
				collider.Destroy();
			}
		}

		public SphereCollider CreateSphereCollider(Vec3 offset, double radius)
		{
			SphereCollider sphere = new SphereCollider(radius);
			sphere.transform.localPosition = offset;
			m_transform.Add(sphere.transform);
			sphere.proxyId = world.m_broadPhase.CreateProxy(sphere.bounds, sphere);
			return sphere;
		}
		public SphereCollider CreateSphereCollider(double radius)
		{
			return CreateSphereCollider(Vec3.zero, radius);
		}

    }
}
