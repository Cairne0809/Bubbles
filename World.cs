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
			m_root.ForEachObject((Body body) =>
			{
				body.Destroy();
			});
			m_root.RemoveAll();
		}

		public void ForEachAssistantBounds(Action<Bounds> forEach)
		{
			m_tree.ForEachAssistantFatBounds(forEach);
		}

		public void ForEachCollider(Action<Collider> forEach)
		{
			m_tree.ForEachUserData(forEach);
		}

		public void ForEachBody(Action<Body> forEach)
		{
			m_root.ForEachObject(forEach);
		}

		public Body CreateBody()
		{
			Body body = new Body(this);
			m_root.Add(body.transform);
			return body;
		}

		public void DestroyBody(Body body)
		{
			if (m_root.Remove(body.transform))
			{
				body.Destroy();
			}
		}

		public void Update(double deltaTime)
		{
			m_root.ForEachObject((Body body) =>
			{
				body.Update(deltaTime);
			});
		}

		public void RayCastBounds(Func<Bounds, bool> RayCastCallback, RayCastInput input)
		{
			m_broadPhase.RayCast((int proxyId, double distance) =>
			{
				Bounds bounds = m_tree.GetFatBounds(proxyId);
				if (RayCastCallback(bounds))
				{
					return input.maxDistance;
				}
				else
				{
					return -1;
				}
			}, input);
		}

    }
}
