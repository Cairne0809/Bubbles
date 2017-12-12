using System;
using MathematicsX;

namespace Bubbles
{
    public class World
    {
		DynamicTree<Body> m_tree;
		internal BroadPhase<Body> m_broadPhase;

		internal Body m_bodyList;
		internal int m_bodyCount;

		public World()
		{
			m_tree = new DynamicTree<Body>();
			m_broadPhase = new BroadPhase<Body>(m_tree);

			m_bodyList = null;
			m_bodyCount = 0;
		}

		public void Destroy()
		{
			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				DestroyBody(body);
			}
		}

		public void ForEachAssistantBounds(Action<Bounds> forEach)
		{
			m_tree.ForEachAssistantFatBounds(forEach);
		}

		public void ForEachBody(Action<Body> forEach)
		{
			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				forEach(body);
			}
		}

		public Body GetBodyList()
		{
			return m_bodyList;
		}

		public int GetBodyCount()
		{
			return m_bodyCount;
		}

		public Body CreateBody()
		{
			Body body = new Body();
			body.m_proxyId = m_broadPhase.CreateProxy(body.bounds, body);

			if (m_bodyList != null)
			{
				body.m_next = m_bodyList;
				m_bodyList.m_prev = body;
			}
			m_bodyList = body;
			m_bodyCount++;

			return body;
		}

		public void DestroyBody(Body body)
		{
			if (body.m_proxyId == -1)
			{
				return;
			}

			m_broadPhase.DestroyProxy(body.m_proxyId);
			body.m_proxyId = -1;

			if (m_bodyList == body)
			{
				m_bodyList = body.m_next;
			}
			if (body.m_next != null)
			{
				body.m_next.m_prev = body.m_prev;
				body.m_next = null;
			}
			if (body.m_prev != null)
			{
				body.m_prev.m_next = body.m_next;
				body.m_prev = null;
			}
			--m_bodyCount;
		}

		public void Update(double deltaTime)
		{
			PrimaryUpdate(deltaTime);
			m_broadPhase.UpdatePairs(UpdatePairsCallback);
			FinalUpdate();
		}

		void PrimaryUpdate(double deltaTime)
		{
			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				Vec3 lastPosition = body.position;
				if (body.PrimaryUpdate(deltaTime))
				{
					m_broadPhase.MoveProxy(body.m_proxyId, body.bounds, Vec3.Distance(body.position, lastPosition));
				}
			}
		}

		void UpdatePairsCallback(Body A, Body B)
		{
			if (Collisions.TestOverlap(A, B))
			{
				A.UpdatePair(B, true);
				B.UpdatePair(A, false);
			}
		}

		void FinalUpdate()
		{
			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				if (body.FinalUpdate())
				{
					m_broadPhase.MoveProxy(body.m_proxyId, body.bounds, 0);
				}
			}
		}

		public void QueryBounds(Bounds bounds, Func<Bounds, bool> QueryCallback)
		{
			m_broadPhase.Query(bounds, (int proxyId) =>
			{
				Bounds other = m_tree.GetBounds(proxyId);
				if (QueryCallback(other))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
		}
		public void RayCastBounds(RayCastInput input, Func<Bounds, bool> RayCastCallback)
		{
			m_broadPhase.RayCast(input, (int proxyId, double distance) =>
			{
				Bounds bounds = m_tree.GetBounds(proxyId);
				if (RayCastCallback(bounds))
				{
					return input.maxDistance;
				}
				else
				{
					return -1;
				}
			});
		}
		public bool RayCastClosestBounds(RayCastInput input, out Bounds closest)
		{
			int proxyId = m_broadPhase.RayCastClosest(input);
			if (proxyId >= 0)
			{
				closest = m_tree.GetBounds(proxyId);
				return true;
			}
			closest = Bounds.NaB;
			return false;
		}

    }
}
