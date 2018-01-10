using System;
using MathematicsX;

namespace Bubbles
{
    public class World
    {
		DynamicTree<Body> m_tree;
		internal BroadPhase<Body> m_broadPhase;

		internal int m_bodyCount;
		internal Body m_activeList;
		internal Body m_inactiveList;
		internal Body m_particleList;

		public World()
		{
			m_tree = new DynamicTree<Body>();
			m_broadPhase = new BroadPhase<Body>(m_tree);
			
			m_bodyCount = 0;
		}

		public void Destroy()
		{
			for (Body body = m_activeList; body != null; body = body.Next())
			{
				DestroyBody(body);
			}
			for (Body body = m_particleList; body != null; body = body.Next())
			{
				DestroyBody(body);
			}
			for (Body body = m_inactiveList; body != null; body = body.Next())
			{
				DestroyBody(body);
			}
		}

		public void ForEachAssistantBounds(Action<Bounds> forEach)
		{
			m_tree.ForEachAssistantBounds(forEach);
		}

		public void ForEachBody(Action<Body> forEach)
		{
			for (Body b = m_activeList; b != null; b = b.Next())
			{
				forEach(b);
			}
			for (Body b = m_particleList; b != null; b = b.Next())
			{
				forEach(b);
			}
			for (Body b = m_inactiveList; b != null; b = b.Next())
			{
				forEach(b);
			}
		}

		public int GetBodyCount()
		{
			return m_bodyCount;
		}

		public Body CreateBody(BodyDef def)
		{
			Body body = new Body(def);
			if (body.shape.IsParticle())
			{
				body.AddToList(ref m_particleList);
			}
			else
			{
				body.AddToList(ref m_activeList);
				body.m_proxyId = m_broadPhase.CreateProxy(body.GetBounds(), body);
			}
			m_bodyCount++;
			return body;
		}

		public void DestroyBody(Body body)
		{
			if (body.m_next != null || body.m_prev != null)
			{
				if (body.isAsleep || body.isStatic)
				{
					body.RemoveFromList(ref m_inactiveList);
				}
				else
				{
					if (body.shape.IsParticle())
					{
						body.RemoveFromList(ref m_particleList);
					}
					else
					{
						body.RemoveFromList(ref m_activeList);
					}
				}
				if (body.m_proxyId != BroadPhase<Body>.NULL_PROXY)
				{
					m_broadPhase.DestroyProxy(body.m_proxyId);
					body.m_proxyId = BroadPhase<Body>.NULL_PROXY;
				}
				--m_bodyCount;
			}
		}

		public void MakeStatic(Body body, bool b)
		{
			if (body.m_isStatic == b) return;
			body.m_isStatic = b;
			if (b)
			{
				body.m_isAsleep = true;
				if (body.shape.IsParticle())
				{
					body.RemoveFromList(ref m_particleList);
				}
				else
				{
					body.RemoveFromList(ref m_activeList);
				}
				body.AddToList(ref m_inactiveList);
			}
			else
			{
				body.RemoveFromList(ref m_inactiveList);
				if (body.shape.IsParticle())
				{
					body.AddToList(ref m_particleList);
				}
				else
				{
					body.AddToList(ref m_activeList);
				}
			}
		}

		public void Update(double deltaTime)
		{
			PrimaryUpdate(deltaTime, false);
			for (int i = 0; i < 1; ++i)
			{
				FinalUpdate();
			}
		}
		public void Update()
		{
			PrimaryUpdate(0, true);
			for (int i = 0; i < 1; ++i)
			{
				FinalUpdate();
			}
		}

		public void WhenUpdatePairs(Action<Body, Body> Callback)
		{
			this.WhenUpdatePairsCallback = Callback;
		}
		Action<Body, Body> WhenUpdatePairsCallback;

		void PrimaryUpdate(double deltaTime, bool imperative)
		{
			for (Body body = m_activeList; body != null; body = body.Next())
			{
				if (body.PrimaryUpdate(deltaTime) || imperative)
				{
					m_broadPhase.MoveProxy(body.m_proxyId, body.GetBounds());
				}
			}
			for (Body body = m_particleList; body != null; body = body.Next())
			{
				body.PrimaryUpdate(deltaTime);
			}
		}

		void FinalUpdate()
		{
			m_broadPhase.UpdatePairs(UpdatePairsCallback);

			for (Body body = m_activeList; body != null; body = body.Next())
			{
				if (body.FinalUpdate())
				{
					m_broadPhase.MoveProxy(body.m_proxyId, body.GetBounds());
				}
			}
			for (Body body = m_particleList; body != null; body = body.Next())
			{
				m_broadPhase.Query(body.GetBounds(), (int proxyId) =>
				{
					Body other = m_tree.GetUserData(proxyId);
					UpdatePairsCallback(body, other);
					return true;
				});
				body.FinalUpdate();
			}
		}

		void UpdatePairsCallback(Body A, Body B)
		{
			Collisions.Collide(A, B);

			if (WhenUpdatePairsCallback != null)
				WhenUpdatePairsCallback(A, B);
		}

		public void QueryBounds(Bounds bounds, Func<Bounds, bool> QueryCallback)
		{
			m_broadPhase.Query(bounds, (int proxyId) =>
			{
				Bounds other = m_tree.GetBounds(proxyId);
				return QueryCallback(other);
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
