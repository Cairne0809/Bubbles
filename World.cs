using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles
{
    public class World
    {
		Transform m_transform;
		DynamicTree<Collider> m_tree;
		internal BroadPhase<Collider> m_broadPhase;

		public World()
		{
			m_transform = new Transform(this, 16);
			m_tree = new DynamicTree<Collider>();
			m_broadPhase = new BroadPhase<Collider>(m_tree);
		}

		public void Destroy()
		{
			m_transform.RemoveAll((Transform child) =>
			{
				(child.userData as RigidBody).Destroy();
			});
		}

		public void ForEachBody(Action<RigidBody> forEach)
		{
			m_transform.ForEach((Transform child) =>
			{
				forEach(child.userData as RigidBody);
			});
		}

		public RigidBody CreateBody()
		{
			RigidBody body = new RigidBody();
			m_transform.Add(body.transform);
			return body;
		}

		public void DestroyBody(RigidBody body)
		{
			if (m_transform.Remove(body.transform))
			{
				body.Destroy();
			}
		}

		public void Update(double deltaTime)
		{
			m_transform.ForEach((Transform child) =>
			{
				RigidBody body = child.userData as RigidBody;
				body.Update(deltaTime);
			});
		}

    }
}
