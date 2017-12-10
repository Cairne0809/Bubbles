using System;

namespace Bubbles
{
    public class World
    {
		Transform m_root;
		DynamicTree<Collider> m_tree;
		internal BroadPhase<Collider> m_broadPhase;

		public World()
		{
			m_root = new Transform(null, 16);
			m_tree = new DynamicTree<Collider>();
			m_broadPhase = new BroadPhase<Collider>(m_tree);
		}

		public void Destroy()
		{
			m_root.RemoveAll((Transform child) =>
			{
				(child.worldObject as RigidBody).Destroy();
			});
		}

		public void ForEachBody(Action<RigidBody> forEach)
		{
			m_root.ForEach((Transform child) =>
			{
				forEach(child.worldObject as RigidBody);
			});
		}

		public RigidBody CreateBody()
		{
			RigidBody body = new RigidBody(this);
			m_root.Add(body.transform);
			return body;
		}

		public void DestroyBody(RigidBody body)
		{
			if (m_root.Remove(body.transform))
			{
				body.Destroy();
			}
		}

		public void Update(double deltaTime)
		{
			m_root.ForEach((Transform child) =>
			{
				RigidBody body = child.worldObject as RigidBody;
				body.Update(deltaTime);
			});
		}

    }
}
