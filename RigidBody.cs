﻿using System;
using MathematicsX;

namespace Bubbles
{
	public class RigidBody : WorldObject
    {
		public Vec3 velocity;
		public Vec3 angularVelocity;

		internal RigidBody(World world) : base(world, 1)
		{

		}

		internal void Update(double deltaTime)
		{
			if (velocity != Vec3.zero)
			{
				transform.position += velocity * deltaTime;
				transform.ForEach((Transform child) =>
				{
					Collider collider = child.worldObject as Collider;
					world.m_broadPhase.MoveProxy(collider.proxyId, collider.bounds);
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
				transform.RemoveAll((Transform child) =>
				{
					(child.worldObject as Collider).Destroy();
				});
			}
		}

		public void ForEachCollider(Action<Collider> forEach)
		{
			transform.ForEach((Transform child) =>
			{
				forEach(child.worldObject as Collider);
			});
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
