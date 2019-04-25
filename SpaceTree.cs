using System;
using System.Collections.Generic;

namespace Bubbles
{
	public class SpaceTree : DynamicTree<Bounds>
	{
		//pair定义
		private struct Pair : IComparable<Pair>
		{
			public int proxyA;
			public int proxyB;

			public int CompareTo(Pair other)
			{
				int deltaA = proxyA - other.proxyA;
				if (deltaA == 0)
				{
					return proxyB - other.proxyB;
				}
				return deltaA;
			}
		}

		//待检测碰撞缓冲区
		private HashSet<int> m_checkBuffer;
		//碰撞对缓冲区
		private SortedSet<Pair> m_pairBuffer;

		public SpaceTree(int capacity) : base(capacity)
		{
			m_checkBuffer = new HashSet<int>();
			m_pairBuffer = new SortedSet<Pair>();
		}

		sealed override protected void InitInnerValue(int current, int child1, int child2)
		{
			SetValue(current, GetValue(child1).Union(GetValue(child2)));
		}

		sealed override protected void UpdateInnerValue(int current, int child1, int child2)
		{
			SetValue(current, GetValue(child1).Union(GetValue(child2)));
		}

		sealed override protected SplitProcess SplitTest(int leaf, int current, int child1, int child2)
		{
			//选出合并分裂因子最小的一对
			double v0 = GetValue(current).Value;
			double v1 = GetValue(leaf).UnionValue(GetValue(child1));
			double v2 = GetValue(leaf).UnionValue(GetValue(child2));
			if (v0 <= v1 && v0 <= v2) return SplitProcess.Current;
			if (v1 <= v2) return SplitProcess.ToChild1;
			return SplitProcess.ToChild2;
		}

		sealed override protected bool OverlapsTest(int nodeId, Bounds space)
		{
			return GetValue(nodeId).Overlaps(space);
		}

		public int CreateProxy(Bounds value, bool bufferCheck)
		{
			int proxyId = CreateNode(value);
			//添加代理到待检测碰撞缓冲区
			if (bufferCheck)
			{
				m_checkBuffer.Add(proxyId);
			}
			return proxyId;
		}

		public void DestroyProxy(int proxyId)
		{
			//从待检测碰撞缓冲区中移除代理
			m_checkBuffer.Remove(proxyId);
			DestroyNode(proxyId);
		}

		public void TouchProxy(int proxyId)
		{
			m_checkBuffer.Add(proxyId);
		}

		public void UpdatePairs(Action<Bounds, Bounds> UpdatePairsCallback)
		{
			//执行查询树上所有需要移动代理
			foreach (int queryProxyId in m_checkBuffer)
			{
				Bounds queryBounds = GetValue(queryProxyId);
				// 查询树，创建多个pair并将他们添加到pair缓冲区中
				Query(queryBounds, (int nodeId) => 
				{
					Pair newPair;
					newPair.proxyA = Math.Min(nodeId, queryProxyId);
					newPair.proxyB = Math.Max(nodeId, queryProxyId);
					m_pairBuffer.Add(newPair);
				});
			}
			//重置移动缓冲区
			m_checkBuffer.Clear();
			
			foreach (Pair pair in m_pairBuffer)
			{
				//回调
				Bounds a = GetValue(pair.proxyA);
				Bounds b = GetValue(pair.proxyB);
				UpdatePairsCallback(a, b);
			}
			//重置pair缓存区
			m_pairBuffer.Clear();
		}
		
		public void RayCast(RayCast3D rayCast, Action<Bounds> RayCastCallback)
		{
			Query((int nodeId) =>
			{
				Bounds bounds = GetValue(nodeId);
				if (bounds.Overlaps(ref rayCast))
				{
					if (IsInner(nodeId))
					{
						return QueryProcess.QueryChildren;
					}
					RayCastCallback(bounds);
				}
				return QueryProcess.Continue;
			});
		}

		public bool RayCastClosest(RayCast3D rayCast, out Bounds bounds)
		{
			int closest = NULL_NODE;
			Query((int nodeId) =>
			{
				if (GetValue(nodeId).Overlaps(ref rayCast))
				{
					if (IsInner(nodeId))
					{
						return QueryProcess.QueryChildren;
					}
					closest = nodeId;
					if (rayCast.currentDistance < 0)
					{
						return QueryProcess.Break;
					}
					rayCast.maxDistance = rayCast.currentDistance;
				}
				return QueryProcess.Continue;
			});
			if (closest != NULL_NODE)
			{
				bounds = GetValue(closest);
				return true;
			}
			bounds = default(Bounds);
			return false;
		}

	}
}
