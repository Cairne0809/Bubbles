using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathX;
using System.Collections;

namespace Bubbles
{
	class BroadPhase<T>
	{
		//pair定义
		struct Pair : IComparable<Pair>
		{
			//链表的next指针
			public int next;
			//代理a
			public int proxyIdA;
			//代理b
			public int proxyIdB;

			public int CompareTo(Pair other)
			{
				if (proxyIdA < other.proxyIdA)
				{
					return -1;
				}
				else if (proxyIdA > other.proxyIdA)
				{
					return 1;
				}
				else
				{
					if (proxyIdB < other.proxyIdB)
					{
						return -1;
					}
					return 1;
				}
			}
		}

		public const int NULL_PROXY = -1;

		//动态树声明
		DynamicTree<T> m_tree;
		//代理数量
		int m_proxyCount;
		//移动的缓冲区
		int[] m_moveBuffer;
		//移动缓冲区的总容量
		int m_moveCapacity;
		//需要移动的代理数量
		int m_moveCount;
		//pair缓冲区
		Pair[] m_pairBuffer;
		//pair缓冲区中的总容量
		int m_pairCapacity;
		//pair数量
		int m_pairCount;
		//查询代理id
		int m_queryProxyId;

		public BroadPhase(DynamicTree<T> tree)
		{
			m_tree = tree;

			m_proxyCount = 0;

			m_pairCapacity = 16;
			m_pairCount = 0;
			m_pairBuffer = new Pair[m_pairCapacity];

			m_moveCapacity = 16;
			m_moveCount = 0;
			m_moveBuffer = new int[m_moveCapacity];
		}

		void BufferMove(int proxyId)
		{
			//移动缓冲区过小，增容
			if (m_moveCount == m_moveCapacity)
			{
				//获取移动缓冲区
				int[] old = m_moveBuffer;
				m_moveCapacity *= 2;
				m_moveBuffer = new int[m_moveCapacity];
				Array.Copy(old, m_moveBuffer, old.Length);
			}
			//添加代理id到移动缓冲区中  
			m_moveBuffer[m_moveCount] = proxyId;
			//自增  
			++m_moveCount;
		}

		void UnBufferMove(int proxyId)
		{
			//查找相应的代理
			for (int i = 0; i < m_moveCount; ++i)
			{
				//找到代理，并置空
				if (m_moveBuffer[i] == proxyId)
				{
					m_moveBuffer[i] = NULL_PROXY;
					return;
				}
			}
		}

		bool QueryCallback(int proxyId)
		{
			// 一个代理不需要自己pair更新自己的pair  
			if (proxyId == m_queryProxyId)
			{
				return true;
			}
			// 如果需要增加pair缓冲区  
			if (m_pairCount == m_pairCapacity)
			{
				//获取旧的pair缓冲区，并增加容量
				Pair[] old = m_pairBuffer;
				m_pairCapacity *= 2;
				m_pairBuffer = new Pair[m_pairCapacity];
				Array.Copy(old, m_pairBuffer, old.Length);
			}
			//设置最新的pair
			//并自增pair数量
			m_pairBuffer[m_pairCount].proxyIdA = Math.Min(proxyId, m_queryProxyId);
			m_pairBuffer[m_pairCount].proxyIdB = Math.Max(proxyId, m_queryProxyId);
			++m_pairCount;

			return true;
		}

		public int CreateProxy(Bounds bounds, T userData)
		{
			//获取代理id
			int proxyId = m_tree.CreateProxy(bounds, userData);
			//代理数量自增
			++m_proxyCount;
			//添加代理到移动缓冲区中
			BufferMove(proxyId);
			return proxyId;
		}

		public void DestroyProxy(int proxyId)
		{
			UnBufferMove(proxyId);
			--m_proxyCount;
			m_tree.DestroyProxy(proxyId);
		}

		public void MoveProxy(int proxyId, Bounds bounds)
		{
			bool buffer = m_tree.MoveProxy(proxyId, bounds);
			if (buffer)
			{
				BufferMove(proxyId);
			}
		}

		public void TouchProxy(int proxyId)
		{
			BufferMove(proxyId);
		}

		public void UpdatePairs(Action<object, object> AddPair)
		{
			//重置pair缓存区
			m_pairCount = 0;
			//执行查询树上所有需要移动代理
			for (int i = 0; i < m_moveCount; ++i)
			{
				m_queryProxyId = m_moveBuffer[i];
				if (m_queryProxyId == NULL_PROXY)
				{
					continue;
				}
				// 我们需要查询树的宽大的AABB，以便当我们创建pair失败时，可以再次创建
				Bounds bb = m_tree.GetBounds(m_queryProxyId);
				// 查询树，创建多个pair并将他们添加到pair缓冲区中
				m_tree.Query(QueryCallback, bb);
			}
			//重置移动缓冲区
			m_moveCount = 0;
			// 排序pair缓冲区
			Array.Sort<Pair>(m_pairBuffer);
			// 发送pair到客户端
			int index = 0;
			while (index < m_pairCount)
			{
				//在pair缓冲区中获取当前的pair
				Pair primaryPair = m_pairBuffer[index];
				//根据相交记录
				object userDataA = m_tree.GetUserData(primaryPair.proxyIdA);
				object userDataB = m_tree.GetUserData(primaryPair.proxyIdB);

				AddPair(userDataA, userDataB);
				++index;

				//跳过重复的pair
				while (index < m_pairCount)
				{
					Pair pair = m_pairBuffer[index];
					if (pair.proxyIdA != primaryPair.proxyIdA || pair.proxyIdB != primaryPair.proxyIdB)
					{
						break;
					}
					++index;
				}
			}
		}

		void Query(Func<int, bool> QueryCallback, Bounds bb)
		{
			m_tree.Query(QueryCallback, bb);
		}

		void Query(Func<int, double, double> RayCastCallback, RayCastInput input)
		{
			m_tree.RayCast(RayCastCallback, input);
		}

	}
}
