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
			m_tree.doBalance = false;
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

		public void ForEachBody<T>(Action<T> forEach) where T : Body
		{
			for (Body b = m_bodyList; b != null; b = b.Next())
			{
				T body = b as T;
				if (body != null)
				{
					forEach(body);
				}
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

		public ParticleBody CreateParticleBody(BodyDef def)
		{
			ParticleBody body = new ParticleBody(def);
			InitiateBody(body);
			return body;
		}
		public SphereBody CreateSphereBody(SphereBodyDef def)
		{
			SphereBody body = new SphereBody(def);
			body.m_proxyId = m_broadPhase.CreateProxy(body.bounds, body);
			InitiateBody(body);
			return body;
		}
		public BoxBody CreateBoxBody(BoxBodyDef def)
		{
			BoxBody body = new BoxBody(def);
			body.m_proxyId = m_broadPhase.CreateProxy(body.bounds, body);
			InitiateBody(body);
			return body;
		}

		void InitiateBody(Body body)
		{
			if (m_bodyList != null)
			{
				body.m_next = m_bodyList;
				m_bodyList.m_prev = body;
			}
			m_bodyList = body;
			m_bodyCount++;
		}

		public void DestroyBody(Body body)
		{
			if (body.m_next != null || body.m_prev != null)
			{
				if (body.m_proxyId != BroadPhase<Body>.NULL_PROXY)
				{
					m_broadPhase.DestroyProxy(body.m_proxyId);
					body.m_proxyId = -1;
				}

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
			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				if (body is ParticleBody)
				{
					body.PrimaryUpdate(deltaTime);
				}
				else
				{
					if (body.PrimaryUpdate(deltaTime) || imperative)
					{
						m_broadPhase.MoveProxy(body.m_proxyId, body.bounds, 0);
					}
				}
			}
		}

		void FinalUpdate()
		{
			m_broadPhase.UpdatePairs(UpdatePairsCallback);

			for (Body body = m_bodyList; body != null; body = body.Next())
			{
				if (body is ParticleBody)
				{
					m_broadPhase.Query(body.bounds, (int proxyId) =>
					{
						Body other = m_tree.GetUserData(proxyId);
						UpdatePairsCallback(body, other);
						return true;
					});
					body.FinalUpdate();
				}
				else
				{
					if (body.FinalUpdate())
					{
						m_broadPhase.MoveProxy(body.m_proxyId, body.bounds, 0);
					}
				}
			}
		}

		void UpdatePairsCallback(Body A, Body B)
		{
			A.UpdatePair(B);
			B.UpdatePair(A);
			//if (Collisions.TestOverlap(A.bounds, B.bounds))
			//{
			//	A.UpdatePair(B);
			//	B.UpdatePair(A);
			//}
			//if (WhenUpdatePairsCallback != null)
			//{
			//	WhenUpdatePairsCallback(A, B);
			//}
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
