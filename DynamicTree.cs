using System;
using System.Collections;
using System.Collections.Generic;

namespace Bubbles
{
	public abstract class DynamicTree<Value>
	{
		protected enum SplitProcess
		{
			Current = 0,
			ToChild1 = 1,
			ToChild2 = 2,
		}

		protected enum QueryProcess
		{
			Break = 0,
			Continue = 1,
			QueryChildren = 2,
		}

		private struct Node
		{
			public int parent;//父节点指针(同时为空节点的next指针)
			public int child1;//子节点指针1(同时为叶节点的next指针)
			public int child2;//子节点指针2(同时为叶节点的prev指针)
			public int height;//高度 = 1 + max(c1.h, c2.h)，叶节点为0
			public Value value;
		}

		protected const int NULL_NODE = 0;

		private Node[] m_nodes;//树的真正的头指针，也是一块连续的内存池的首地址
		private int m_nodeCapacity;
		private int m_nodeCount;
		private int m_root;
		private int m_leafList;
		private int m_freeList;

		private object m_writeLock;//写入锁

		public DynamicTree(int capacity)
		{
			m_nodeCapacity = Math.Max(4, capacity);
			m_nodes = new Node[m_nodeCapacity];
			m_nodeCount = NULL_NODE + 1;
			m_root = NULL_NODE;
			m_leafList = NULL_NODE;
			InitNodes(m_nodeCount);

			m_writeLock = new object();
		}

		#region private

		private void InitNodes(int start)
		{
			int last = m_nodeCapacity - 1;
			for (int i = start; i < last; i++)
			{
				m_nodes[i].parent = i + 1;
				m_nodes[i].height = -1;
				m_nodes[i].value = default(Value);
			}
			m_nodes[last].parent = NULL_NODE;
			m_freeList = start;
		}

		private int AllocateNode()
		{
			if (m_freeList == NULL_NODE)
			{
				m_nodeCapacity <<= 1;
				Array.Resize(ref m_nodes, m_nodeCapacity);
				InitNodes(m_nodeCount);
			}
			int nodeId = m_freeList;
			m_freeList = m_nodes[nodeId].parent;
			m_nodes[nodeId].parent = NULL_NODE;
			m_nodes[nodeId].child1 = NULL_NODE;
			m_nodes[nodeId].child2 = NULL_NODE;
			m_nodes[nodeId].height = 0;
			++m_nodeCount;
			return nodeId;
		}

		private void FreeNode(int nodeId)
		{
			m_nodes[nodeId].parent = m_freeList;
			m_nodes[nodeId].height = -1;
			m_nodes[nodeId].value = default(Value);
			m_freeList = nodeId;
			--m_nodeCount;
		}

		private void InsertLeaf(int leaf)
		{
			if (m_root == NULL_NODE)
			{
				m_root = leaf;
				m_nodes[m_root].parent = NULL_NODE;
				return;
			}

			int index = m_root;

			while (IsInner(index))
			{
				int child1 = m_nodes[index].child1;
				int child2 = m_nodes[index].child2;

				SplitProcess process = SplitTest(leaf, index, child1, child2);

				if (process == SplitProcess.ToChild1)
				{
					index = child1;
				}
				else if (process == SplitProcess.ToChild2)
				{
					index = child2;
				}
				else
				{
					break;
				}
			}

			int sibling = index;//兄弟节点
			int oldParent = m_nodes[sibling].parent;
			int newParent = AllocateNode();
			m_nodes[newParent].parent = oldParent;
			m_nodes[newParent].height = m_nodes[sibling].height + 1;
			InitInnerValue(newParent, leaf, sibling);

			if (oldParent != NULL_NODE)
			{
				// 兄弟节点不是根节点
				if (m_nodes[oldParent].child1 == sibling)
				{
					m_nodes[oldParent].child1 = newParent;
				}
				else
				{
					m_nodes[oldParent].child2 = newParent;
				}
				m_nodes[newParent].child1 = sibling;
				m_nodes[newParent].child2 = leaf;
				m_nodes[sibling].parent = newParent;
				m_nodes[leaf].parent = newParent;
			}
			else
			{
				// 兄弟节点是根节点
				m_nodes[newParent].child1 = sibling;
				m_nodes[newParent].child2 = leaf;
				m_nodes[sibling].parent = newParent;
				m_nodes[leaf].parent = newParent;
				m_root = newParent;
			}

			// 向后走修复树
			UpdateUpward(leaf);
		}

		private void RemoveLeaf(int leaf)
		{
			//只有一个节点
			if (leaf == m_root)
			{
				m_root = NULL_NODE;
				return;
			}
			//获取父节点和祖父节点  
			int parent = m_nodes[leaf].parent;
			int grandParent = m_nodes[parent].parent;
			//选找兄弟节点  
			int sibling;
			if (m_nodes[parent].child1 == leaf)
			{
				sibling = m_nodes[parent].child2;
			}
			else
			{
				sibling = m_nodes[parent].child1;
			}
			// 祖父节点不为空，即父节点不是根节点  
			if (grandParent != NULL_NODE)
			{
				// Destroy parent and connect sibling to grandParent.  
				// 销毁父节点和将兄弟节点链接到祖父节点中  
				if (m_nodes[grandParent].child1 == parent)
				{
					m_nodes[grandParent].child1 = sibling;
				}
				else
				{
					m_nodes[grandParent].child2 = sibling;
				}
				m_nodes[sibling].parent = grandParent;
				//释放节点到内存池中  
				FreeNode(parent);

				// 向后走修复树
				UpdateUpward(sibling);
			}
			else
			{
				//获取根节点  
				m_root = sibling;
				m_nodes[sibling].parent = NULL_NODE;
				//释放父节点  
				FreeNode(parent);
			}
		}

		private void UpdateUpward(int index)
		{
			index = m_nodes[index].parent;
			while (index != NULL_NODE)
			{
				//平衡
				index = Balance(index);
				//左右孩子节点
				int child1 = m_nodes[index].child1;
				int child2 = m_nodes[index].child2;
				//高度和aabb
				m_nodes[index].height = 1 + Math.Max(m_nodes[child1].height, m_nodes[child2].height);
				UpdateInnerValue(index, child1, child2);
				//获取父节点
				index = m_nodes[index].parent;
			}
		}

		private int Balance(int iA)
		{
			Assert(iA != NULL_NODE);
			//已是平衡树，不需要调整
			if (m_nodes[iA].height < 2)
			{
				return iA;
			}
			// 获取A的左右孩子
			int iB = m_nodes[iA].child1;
			int iC = m_nodes[iA].child2;
			// 获得平衡值
			int balance = m_nodes[iC].height - m_nodes[iB].height;

			// 上旋C
			if (balance > 1)
			{
				//获取C的左右孩子iF、iG和子树F、G
				int iF = m_nodes[iC].child1;
				int iG = m_nodes[iC].child2;
				// 交换A和C
				m_nodes[iC].child1 = iA;
				m_nodes[iC].parent = m_nodes[iA].parent;
				m_nodes[iA].parent = iC;
				// A的父指针应该指向c
				// A不是头节点
				if (m_nodes[iC].parent != NULL_NODE)
				{
					if (m_nodes[m_nodes[iC].parent].child1 == iA)
					{
						m_nodes[m_nodes[iC].parent].child1 = iC;
					}
					else
					{
						m_nodes[m_nodes[iC].parent].child2 = iC;
					}
				}
				else
				{
					//A是头节点
					m_root = iC;
				}
				// 旋转
				// 如果f的高度大，则旋转F
				if (m_nodes[iF].height > m_nodes[iG].height)
				{
					m_nodes[iC].child2 = iF;
					m_nodes[iA].child2 = iG;
					m_nodes[iG].parent = iA;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iB].height, m_nodes[iG].height);
					m_nodes[iC].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iF].height);
					UpdateInnerValue(iA, iB, iG);
					UpdateInnerValue(iC, iA, iF);
				}
				else
				{
					// 旋转G
					m_nodes[iC].child2 = iG;
					m_nodes[iA].child2 = iF;
					m_nodes[iF].parent = iA;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iB].height, m_nodes[iF].height);
					m_nodes[iC].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iG].height);
					UpdateInnerValue(iA, iB, iF);
					UpdateInnerValue(iC, iA, iG);
				}
				return iC;
			}

			// 上旋B
			if (balance < -1)
			{
				int iD = m_nodes[iB].child1;
				int iE = m_nodes[iB].child2;
				//交换A和B
				m_nodes[iB].child1 = iA;
				m_nodes[iB].parent = m_nodes[iA].parent;
				m_nodes[iA].parent = iB;
				// A的旧父指针指向B
				if (m_nodes[iB].parent != NULL_NODE)
				{
					if (m_nodes[m_nodes[iB].parent].child1 == iA)
					{
						m_nodes[m_nodes[iB].parent].child1 = iB;
					}
					else
					{
						m_nodes[m_nodes[iB].parent].child2 = iB;
					}
				}
				else
				{
					m_root = iB;
				}
				//旋转
				if (m_nodes[iD].height > m_nodes[iE].height)
				{
					// 旋转D
					m_nodes[iB].child2 = iD;
					m_nodes[iA].child1 = iE;
					m_nodes[iE].parent = iA;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iC].height, m_nodes[iE].height);
					m_nodes[iB].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iD].height);
					UpdateInnerValue(iA, iC, iE);
					UpdateInnerValue(iB, iA, iD);
				}
				else
				{
					// 旋转E
					m_nodes[iB].child2 = iE;
					m_nodes[iA].child1 = iD;
					m_nodes[iD].parent = iA;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iC].height, m_nodes[iD].height);
					m_nodes[iB].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iE].height);
					UpdateInnerValue(iA, iC, iD);
					UpdateInnerValue(iB, iA, iE);
				}
				return iB;
			}

			return iA;
		}

		private void LeafListAdd(int nodeId)
		{
			if (m_leafList != NULL_NODE)
			{
				m_nodes[m_leafList].child2 = nodeId;
				m_nodes[nodeId].child1 = m_leafList;
			}
			m_leafList = nodeId;
		}

		private void LeafListRemove(int nodeId)
		{
			int next = m_nodes[nodeId].child1;
			int prev = m_nodes[nodeId].child2;
			if (prev == NULL_NODE)
			{
				m_leafList = next;
			}
			else
			{
				m_nodes[prev].child1 = next;
			}
			if (next != NULL_NODE)
			{
				m_nodes[next].child2 = prev;
			}
		}

		#endregion
		
		public bool IsFree(int nodeId)
		{
			return m_nodes[nodeId].height == -1;
		}
		public bool IsLeaf(int nodeId)
		{
			return m_nodes[nodeId].height == 0;
		}
		public bool IsInner(int nodeId)
		{
			return m_nodes[nodeId].height >= 1;
		}
		public bool IsRoot(int nodeId)
		{
			return nodeId == m_root;
		}

		protected int GetRoot()
		{
			return m_root;
		}
		protected int GetParent(int nodeId)
		{
			return m_nodes[nodeId].parent;
		}
		protected int GetChild1(int nodeId)
		{
			return m_nodes[nodeId].child1;
		}
		protected int GetChild2(int nodeId)
		{
			return m_nodes[nodeId].child2;
		}
		protected int GetHeight(int nodeId)
		{
			return m_nodes[nodeId].height;
		}

		public int NodeCapacity
		{
			get { return m_nodeCapacity; }
		}
		public int NodeCount
		{
			get { return m_nodeCount; }
		}
		public int LeafCount
		{
			get { return (m_nodeCount + 1) >> 1; }
		}
		public int Height
		{
			get { return m_nodes[m_root].height; }
		}
		public int MaxQueryDepth
		{
			get { return m_nodes[m_root].height + 1; }
		}
		public int LeafList
		{
			get { return m_leafList; }
		}
		public int NextLeaf(int leaf)
		{
			return m_nodes[leaf].child1;
		}

		protected abstract void InitInnerValue(int current, int child1, int child2);

		protected abstract void UpdateInnerValue(int current, int child1, int child2);

		protected abstract SplitProcess SplitTest(int leaf, int current, int child1, int child2);

		protected abstract bool OverlapsTest(int nodeId, Value value);

		protected void Query(Func<int, QueryProcess> QueryCallback)
		{
			if (m_root == NULL_NODE)
			{
				return;
			}
			int pointer = -1;
			int[] stack = new int[MaxQueryDepth];
			stack[++pointer] = m_root;
			while (pointer >= 0)
			{
				int nodeId = stack[pointer--];
				QueryProcess process = QueryCallback(nodeId);
				if (process == QueryProcess.Break)
				{
					return;
				}
				if (process == QueryProcess.QueryChildren)
				{
					stack[++pointer] = m_nodes[nodeId].child1;
					stack[++pointer] = m_nodes[nodeId].child2;
				}
			}
		}

		public void Query(Value value, Action<int> QueryCallback)
		{
			if (m_root == NULL_NODE)
			{
				return;
			}
			int pointer = -1;
			int[] stack = new int[MaxQueryDepth];
			stack[++pointer] = m_root;
			while (pointer >= 0)
			{
				int nodeId = stack[pointer--];
				if (OverlapsTest(nodeId, value))
				{
					if (IsInner(nodeId))
					{
						stack[++pointer] = m_nodes[nodeId].child1;
						stack[++pointer] = m_nodes[nodeId].child2;
					}
					else
					{
						QueryCallback(nodeId);
					}
				}
			}
		}

		protected int CreateNode(Value value)
		{
			int nodeId = AllocateNode();
			m_nodes[nodeId].value = value;
			InsertLeaf(nodeId);
			LeafListAdd(nodeId);
			return nodeId;
		}

		protected void DestroyNode(int nodeId)
		{
			Assert(nodeId >= 0 && nodeId < m_nodeCapacity);
			Assert(IsLeaf(nodeId));
			RemoveLeaf(nodeId);
			LeafListRemove(nodeId);
			FreeNode(nodeId);
		}

		public void Clear()
		{
			m_nodeCount = NULL_NODE + 1;
			m_root = NULL_NODE;
			m_leafList = NULL_NODE;
			InitNodes(m_nodeCount);
		}

		public Value GetRootValue()
		{
			return m_nodes[m_root].value;
		}
		
		protected void SetValue(int proxyId, Value value)
		{
			m_nodes[proxyId].value = value;
		}
		public Value GetValue(int proxyId)
		{
			Assert(proxyId >= 0 && proxyId < m_nodeCapacity);
			return m_nodes[proxyId].value;
		}


		protected void Assert(bool b)
		{
			if (!b) throw new Exception();
		}
		

		public IEnumerable<int> LeafCollection()
		{
			int leaf = m_leafList;
			while (leaf != NULL_NODE)
			{
				yield return leaf;
				leaf = m_nodes[leaf].child1;
			}
		}
		public void LeavesCopyTo(int[] array, int index)
		{
			int leaf = m_leafList;
			while (leaf != NULL_NODE)
			{
				array[index++] = leaf;
				leaf = m_nodes[leaf].child1;
			}
		}

		public IEnumerable<Value> ValueCollection()
		{
			int leaf = m_leafList;
			while (leaf != NULL_NODE)
			{
				yield return m_nodes[leaf].value;
				leaf = m_nodes[leaf].child1;
			}
		}
		public void ValuesCopyTo(Value[] array, int index)
		{
			int leaf = m_leafList;
			while (leaf != NULL_NODE)
			{
				array[index++] = m_nodes[leaf].value;
				leaf = m_nodes[leaf].child1;
			}
		}

		public IEnumerable<Value> OverlapsCollection(Value value)
		{
			if (m_root == NULL_NODE)
			{
				yield break;
			}
			int pointer = -1;
			int[] stack = new int[MaxQueryDepth];
			stack[++pointer] = m_root;
			while (pointer >= 0)
			{
				int nodeId = stack[pointer--];
				if (OverlapsTest(nodeId, value))
				{
					if (IsInner(nodeId))
					{
						stack[++pointer] = m_nodes[nodeId].child1;
						stack[++pointer] = m_nodes[nodeId].child2;
					}
					else
					{
						yield return m_nodes[nodeId].value;
					}
				}
			}
		}
		public void OverlapsCopyTo(Value value, Value[] array, int index)
		{
			if (m_root == NULL_NODE)
			{
				return;
			}
			int pointer = -1;
			int[] stack = new int[MaxQueryDepth];
			stack[++pointer] = m_root;
			while (pointer >= 0)
			{
				int nodeId = stack[pointer--];
				if (OverlapsTest(nodeId, value))
				{
					if (IsInner(nodeId))
					{
						stack[++pointer] = m_nodes[nodeId].child1;
						stack[++pointer] = m_nodes[nodeId].child2;
					}
					else
					{
						array[index++] = m_nodes[nodeId].value;
					}
				}
			}
		}

	}
	
}
