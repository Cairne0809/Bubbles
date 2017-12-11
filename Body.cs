using System;
using MathematicsX;

namespace Bubbles
{
	public class Body : WorldObject
    {
		public Vec3 velocity;
		public Vec3 angularVelocity;

		internal Body(World world) : base(world, 1)
		{

		}

		internal void Update(double deltaTime)
		{
			if (velocity != Vec3.zero)
			{
				Vec3 dv = velocity * deltaTime;
				transform.position += dv;
				double delta = dv.magnitude;
				transform.ForEachObject((Collider collider) =>
				{
					world.m_broadPhase.MoveProxy(collider.proxyId, collider.bounds, delta);
				});
			}
		}

		public void Destroy()
		{
			if (transform.parent != null)
			{
				world.DestroyBody(this);
			}
			else
			{
				transform.ForEachObject((Collider collider) =>
				{
					collider.Destroy();
				});
				transform.RemoveAll();
			}
		}

		public void ForEachCollider(Action<Collider> forEach)
		{
			transform.ForEachObject(forEach);
		}

		public void DestroyCollider(Collider collider)
		{
			if (transform.Remove(collider.transform))
			{
				world.m_broadPhase.DestroyProxy(collider.proxyId);
				collider.Destroy();
			}
		}

		public SphereCollider CreateSphereCollider(Vec3 offset, double radius)
		{
			SphereCollider sphere = new SphereCollider(world, radius);
			sphere.transform.localPosition = offset;
			transform.Add(sphere.transform);
			sphere.proxyId = world.m_broadPhase.CreateProxy(sphere.bounds, sphere);
			return sphere;
		}
		public SphereCollider CreateSphereCollider(double radius)
		{
			return CreateSphereCollider(Vec3.zero, radius);
		}

    }
}
