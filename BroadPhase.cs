﻿using System;

namespace Bubbles
{
	class BroadPhase<T>
	{
		//pair定义
		struct Pair : IComparable<Pair>
		{
			//代理a
			public int proxyIdA;
			//代理b
			public int proxyIdB;

			public int CompareTo(Pair other)
			{
				int deltaA = proxyIdA - other.proxyIdA;
				if (deltaA == 0)
				{
					return proxyIdB - other.proxyIdB;
				}
				return deltaA;
			}
		}

		public const int NULL_PROXY = -1;

		//动态树声明
		DynamicTree<T> m_tree;
		//移动的缓冲区
		int[] m_moveBuffer;
		//需要移动的代理数量
		int m_moveCount;
		//pair缓冲区
		Pair[] m_pairBuffer;
		//pair数量
		int m_pairCount;
		//查询代理id
		int m_queryProxyId;

		internal BroadPhase(DynamicTree<T> tree)
		{
			m_tree = tree;

			m_moveCount = 0;
			m_moveBuffer = new int[16];

			m_pairCount = 0;
			m_pairBuffer = new Pair[16];
		}

		void BufferMove(int proxyId)
		{
			//移动缓冲区过小，增容
			if (m_moveCount == m_moveBuffer.Length)
			{
				//获取移动缓冲区
				Array.Resize(ref m_moveBuffer, m_moveBuffer.Length * 2);
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

		internal int CreateProxy(Bounds bounds, T userData)
		{
			//获取代理id
			int proxyId = m_tree.CreateProxy(bounds, userData);
			//添加代理到移动缓冲区中
			BufferMove(proxyId);
			return proxyId;
		}

		internal void DestroyProxy(int proxyId)
		{
			UnBufferMove(proxyId);
			m_tree.DestroyProxy(proxyId);
		}

		internal void MoveProxy(int proxyId, Bounds bounds)
		{
			bool buffer = m_tree.MoveProxy(proxyId, bounds);
			if (buffer)
			{
				BufferMove(proxyId);
			}
		}

		internal void TouchProxy(int proxyId)
		{
			BufferMove(proxyId);
		}

		internal void UpdatePairs(Action<T, T> UpdatePairsCallback)
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
				Bounds bounds = m_tree.GetBounds(m_queryProxyId);
				// 查询树，创建多个pair并将他们添加到pair缓冲区中
				m_tree.Query(bounds, QueryCallback);
			}
			//重置移动缓冲区
			m_moveCount = 0;
			// 排序pair缓冲区
			Array.Sort(m_pairBuffer, 0, m_pairCount);
			// 发送pair到客户端
			int index = 0;
			while (index < m_pairCount)
			{
				//在pair缓冲区中获取当前的pair
				Pair primaryPair = m_pairBuffer[index];
				//根据相交记录
				T userDataA = m_tree.GetUserData(primaryPair.proxyIdA);
				T userDataB = m_tree.GetUserData(primaryPair.proxyIdB);

				UpdatePairsCallback(userDataA, userDataB);
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
		bool QueryCallback(int proxyId)
		{
			// 一个代理不需要自己pair更新自己的pair  
			if (proxyId == m_queryProxyId)
			{
				return true;
			}
			// 如果需要增加pair缓冲区  
			if (m_pairCount == m_pairBuffer.Length)
			{
				//获取旧的pair缓冲区，并增加容量
				Array.Resize(ref m_pairBuffer, m_pairBuffer.Length * 2);
			}
			//设置最新的pair
			//并自增pair数量
			m_pairBuffer[m_pairCount].proxyIdA = Math.Min(proxyId, m_queryProxyId);
			m_pairBuffer[m_pairCount].proxyIdB = Math.Max(proxyId, m_queryProxyId);
			++m_pairCount;

			return true;
		}

		internal void Query(Bounds bounds, Func<int, bool> QueryCallback)
		{
			m_tree.Query(bounds, QueryCallback);
		}

		internal void RayCast(RayCastInput input, Func<int, double, double> RayCastCallback)
		{
			m_tree.RayCast(input, RayCastCallback);
		}

		internal int RayCastClosest(RayCastInput input)
		{
			int closest = -1;
			double min = input.maxDistance;
			m_tree.RayCast(input, (int proxyId, double distance) =>
			{
				double absDistance = Math.Abs(distance);
				if (absDistance <= min)
				{
					closest = proxyId;
					min = absDistance;
				}
				return min;
			});
			return closest;
		}

	}
}
