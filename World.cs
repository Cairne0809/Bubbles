using System;
using System.Collections.Generic;
using MathematicsX;

namespace Bubbles
{
	public class World
	{
		private static object m_syncRoot = new object();
		private static World m_instance;
		public static World Instance
		{
			get
			{
				if (m_instance == null)
				{
					lock (m_syncRoot)
					{
						if (m_instance == null)
						{
							m_instance = new World();
						}
					}
				}
				return m_instance;
			}
		}


		private int m_bodyCount;
		private Body m_bodyList;
		private int m_particleCount;
		private Body m_particleList;

		private SpaceTree m_tree;
		private HashSet<Body> m_tempSet;

		private Action<Body, Body> m_WhenUpdatePairsCallback;

		private Collisions m_collisions;

		private World()
		{
			m_bodyCount = 0;
			m_bodyList = null;
			m_particleCount = 0;
			m_particleList = null;

			m_tree = new SpaceTree(16);
			m_tempSet = new HashSet<Body>();

			m_collisions = new Collisions();
		}

		public int BodyCount
		{
			get { return m_bodyCount; }
		}
		public Body BodyList
		{
			get { return m_bodyList; }
		}

		public int ParticleCount
		{
			get { return m_particleCount; }
		}
		public Body ParticleList
		{
			get { return m_particleList; }
		}

		public Body CreateBody(BodyDef def)
		{
			lock (this)
			{
				Body body = new Body(def);
				if (body.Shape.IsParticle)
				{
					m_particleCount++;
					ListAdd(body, ref m_particleList);
				}
				else
				{
					m_bodyCount++;
					ListAdd(body, ref m_bodyList);
					body.m_proxyId = m_tree.CreateProxy(body.Bounds, def.isKinematic);
				}
				return body;
			}
		}

		public void DestroyBody(Body body)
		{
			lock (this)
			{
				if (body.Shape.IsParticle)
				{
					m_particleCount--;
					ListRemove(body, ref m_particleList);
				}
				else
				{
					m_bodyCount--;
					ListRemove(body, ref m_bodyList);
					m_tree.DestroyProxy(body.m_proxyId);
				}
			}
		}

		public void Update(double deltaTime, int iterations, bool clearForce)
		{
			lock (this)
			{
				PrimaryUpdate(deltaTime, clearForce);
				for (int i = 0; i < iterations; ++i)
				{
					FinalUpdate();
				}
			}
		}
		public void Update(double deltaTime)
		{
			Update(deltaTime, 1, true);
		}

		public void WhenUpdatePairs(Action<Body, Body> Callback)
		{
			m_WhenUpdatePairsCallback = Callback;
		}
		
		
		public bool RayCastBounds(RayCast3D input, out Bounds closest)
		{
			return m_tree.RayCastClosest(input, out closest);
		}

		public void ForEachBody(Action<Body> Callback)
		{
			for (Body b = m_bodyList; b != null; b = b.Next)
			{
				Callback(b);
			}
		}

		public void ForEachParticle(Action<Body> Callback)
		{
			for (Body b = m_particleList; b != null; b = b.Next)
			{
				Callback(b);
			}
		}


		private void ListAdd(Body body, ref Body list)
		{
			if (list != null) list.m_prev = body;
			body.m_next = list;
			list = body;
		}

		private void ListRemove(Body body, ref Body list)
		{
			Body next = body.m_next;
			Body prev = body.m_prev;
			if (body == list) list = next;
			if (next != null) next.m_prev = prev;
			if (prev != null) prev.m_next = next;
		}

		private void PrimaryUpdate(double deltaTime, bool clearForce)
		{
			for (Body b = m_bodyList; b != null; b = b.Next)
			{
				b.PrimaryUpdate(deltaTime, clearForce);
				if (b.Bounds.ReadChangeFlag())
				{
					m_tempSet.Add(b);
					m_tree.DestroyProxy(b.m_proxyId);
				}
			}
			foreach (Body b in m_tempSet)
			{
				b.m_proxyId = m_tree.CreateProxy(b.Bounds, !b.IsKinematic);
			}
			m_tempSet.Clear();

			for (Body b = m_particleList; b != null; b = b.Next)
			{
				b.PrimaryUpdate(deltaTime, clearForce);
			}
		}

		private void FinalUpdate()
		{
			m_tree.UpdatePairs(UpdatePairsCallback);

			for (Body b = m_bodyList; b != null; b = b.Next)
			{
				b.FinalUpdate();
				if (b.Bounds.ReadChangeFlag())
				{
					m_tempSet.Add(b);
					m_tree.DestroyProxy(b.m_proxyId);
				}
			}
			foreach (Body b in m_tempSet)
			{
				b.m_proxyId = m_tree.CreateProxy(b.Bounds, !b.IsKinematic);
			}
			m_tempSet.Clear();
			
			for (Body b = m_particleList; b != null; b = b.Next)
			{
				foreach (Bounds other in m_tree.OverlapsCollection(b.Bounds))
				{
					UpdatePairsCallback(b.Bounds, other);
				}
				b.FinalUpdate();
			}
		}

		private void UpdatePairsCallback(Bounds a, Bounds b)
		{
			if (m_collisions.Collide(a.Body, b.Body))
			{
				m_WhenUpdatePairsCallback?.Invoke(a.Body, b.Body);
			}
		}

	}
}
