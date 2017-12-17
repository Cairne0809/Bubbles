using System;
using System.Collections.Generic;

namespace Bubbles
{
	public class DynamicTree<T>
	{
		//一个动态树的子节点，不于外包直接交换
		struct Node
		{
			//父节点指针(同时为链表的next指针)
			public int parent;
			//子节点指针
			public int child1;
			public int child2;
			//高度 叶子高度为0，空闲节点高度为-1
			public int height;
			//包围球
			public Bounds bounds;

			public T userData;

			//是否是叶子节点
			public bool IsLeaf()
			{
				return child1 == NULL;
			}

			public const int NULL = -1;
		}

		//树的根指针(也是索引)
		int m_root;
		//树的真正的头指针，也是一块连续的内存池的首地址
		Node[] m_nodes;
		//树节点的个数
		int m_nodeCount;
		//空闲链表指针
		int m_freeList;

		public DynamicTree()
		{
			m_root = Node.NULL;

			m_nodeCount = 0;
			//申请一块内存,创建m_nodeCapacity子节点,并清空内存中的内容
			m_nodes = new Node[16];
			// 创建一个空闲链表
			for (int i = 0; i < m_nodes.Length; ++i)
			{
				m_nodes[i].parent = i + 1;
				m_nodes[i].height = -1;
			}
			//链表的最后一个子节点的孩子指针、高度都置为初始值  
			m_nodes[m_nodes.Length - 1].parent = Node.NULL;
			m_freeList = 0;
		}

		int AllocateNode()
		{
			// 如果需要扩大节点内存池
			if (m_freeList == Node.NULL)
			{
				//空闲链表为空，重新创建一个更大的内存池
				Array.Resize(ref m_nodes, m_nodes.Length * 2);
				// 创建一个空闲链表。父节点成为下一个指针
				// 注意:这次是从m_nodeCount开始的
				for (int i = m_nodeCount; i < m_nodes.Length; ++i)
				{
					m_nodes[i].parent = i + 1;
					m_nodes[i].height = -1;
				}
				m_nodes[m_nodes.Length - 1].parent = Node.NULL;
				m_freeList = m_nodeCount;
			}
			//从空闲链表中去下一个节点，初始化该节点，
			//同时将空闲链表头指针m_freeList指向下一个
			int nodeId = m_freeList;
			m_freeList = m_nodes[nodeId].parent;
			m_nodes[nodeId].parent = Node.NULL;
			m_nodes[nodeId].child1 = Node.NULL;
			m_nodes[nodeId].child2 = Node.NULL;
			m_nodes[nodeId].height = 0;
			m_nodes[nodeId].userData = default(T);
			//增加节点数量
			++m_nodeCount;
			//返回节点id
			return nodeId;
		}

		void FreeNode(int nodeId)
		{
			if (nodeId < 0 || nodeId >= m_nodes.Length || m_nodeCount == 0) return;

			//将当前节点以头插入的方式放到空闲链表中  
			m_nodes[nodeId].parent = m_freeList;
			m_nodes[nodeId].height = -1;
			m_freeList = nodeId;
			//将节点个数减1  
			//注意此处并没有释放  
			--m_nodeCount;
		}

		void InsertLeaf(int leaf)
		{
			//判断该树是否为空
			if (m_root == Node.NULL)
			{
				m_root = leaf;
				m_nodes[m_root].parent = Node.NULL;
				return;
			}
			//为该节点找到最好的兄弟（姐妹）节点
			//获取leaf的aabb
			Bounds leafBB = m_nodes[leaf].bounds;
			//获取根节点
			int index = m_root;
			//不是叶子节点
			while (m_nodes[index].IsLeaf() == false)
			{
				int child1 = m_nodes[index].child1;
				int child2 = m_nodes[index].child2;

				//选出合并包围球最小的一对
				Bounds[] bbs = new Bounds[3];
				bbs[0] = m_nodes[index].bounds;
				bbs[1] = m_nodes[child1].bounds + m_nodes[leaf].bounds;
				bbs[2] = m_nodes[child2].bounds + m_nodes[leaf].bounds;
				int min = 0;
				min = bbs[min].radius < bbs[1].radius ? min : 1;
				min = bbs[min].radius < bbs[2].radius ? min : 2;

				// 获取最小成本
				if (min == 0)
				{
					break;
				}
				//下降到最小cost
				if (min == 1)
				{
					index = child1;
				}
				else
				{
					index = child2;
				}
			}
			int sibling = index;
			//创建一个新的父节点
			//初始化
			int oldParent = m_nodes[sibling].parent;
			int newParent = AllocateNode();
			m_nodes[newParent].parent = oldParent;
			m_nodes[newParent].bounds = leafBB + m_nodes[sibling].bounds;
			m_nodes[newParent].height = m_nodes[sibling].height + 1;
			m_nodes[newParent].userData = default(T);

			if (oldParent != Node.NULL)
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
			// 向后走修复树的高度和aabb
			index = m_nodes[leaf].parent;
			while (index != Node.NULL)
			{
				//平衡
				index = Balance(index);

				//左右孩子节点
				int child1 = m_nodes[index].child1;
				int child2 = m_nodes[index].child2;

				//获取高度和aabb
				m_nodes[index].height = 1 + Math.Max(m_nodes[child1].height, m_nodes[child2].height);
				m_nodes[index].bounds = m_nodes[child1].bounds + m_nodes[child2].bounds;
				//获取parent节点
				index = m_nodes[index].parent;
			}
		}

		void RemoveLeaf(int leaf)
		{
			//只有一个节点
			if (leaf == m_root)
			{
				m_root = Node.NULL;
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
			if (grandParent != Node.NULL)
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
				// 调整祖先界限  
				int index = grandParent;
				while (index != Node.NULL)
				{
					//平衡
					index = Balance(index);

					//获取左右孩子  
					int child1 = m_nodes[index].child1;
					int child2 = m_nodes[index].child2;
					//合并aabb  
					//高度  
					m_nodes[index].bounds = m_nodes[child1].bounds + m_nodes[child2].bounds;
					m_nodes[index].height = 1 + Math.Max(m_nodes[child1].height, m_nodes[child2].height);
					//更新index  
					index = m_nodes[index].parent;
				}
			}
			else
			{
				//获取根节点  
				m_root = sibling;
				m_nodes[sibling].parent = Node.NULL;
				//释放父节点  
				FreeNode(parent);
			}
		}

		int Balance(int iA)
		{
			//iA不是根节点
			if (iA == Node.NULL) return iA;
			//已是平衡树，不需要调整
			if (m_nodes[iA].IsLeaf() || m_nodes[iA].height < 2)
			{
				return iA;
			}
			// 获取A的左右孩子
			int iB = m_nodes[iA].child1;
			int iC = m_nodes[iA].child2;
			// iB、iC是否有效
			if (iB < 0 || iB >= m_nodes.Length) return iA;
			if (iC < 0 || iC >= m_nodes.Length) return iA;
			// 获取子树B、C
			// 获得平衡值
			int balance = m_nodes[iC].height - m_nodes[iB].height;

			// 上旋C
			if (balance > 1)
			{
				//获取C的左右孩子iF、iG和子树F、G
				int iF = m_nodes[iC].child1;
				int iG = m_nodes[iC].child2;
				// 验证iF、iG是否有效
				if (iF < 0 || iF >= m_nodes.Length) return iA;
				if (iG < 0 || iG >= m_nodes.Length) return iA;
				// 交换A和C
				m_nodes[iC].child1 = iA;
				m_nodes[iC].parent = m_nodes[iA].parent;
				m_nodes[iA].parent = iC;
				// A的父指针应该指向c  
				// A不是头节点  
				if (m_nodes[iC].parent != Node.NULL)
				{
					if (m_nodes[m_nodes[iC].parent].child1 == iA)
					{
						m_nodes[m_nodes[iC].parent].child1 = iC;
					}
					else
					{
						if (m_nodes[m_nodes[iC].parent].child2 != iA) return iA;
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
					m_nodes[iA].bounds = m_nodes[iB].bounds + m_nodes[iG].bounds;
					m_nodes[iC].bounds = m_nodes[iA].bounds + m_nodes[iF].bounds;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iB].height, m_nodes[iG].height);
					m_nodes[iC].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iF].height);
				}
				else
				{
					// 旋转G  
					m_nodes[iC].child2 = iG;
					m_nodes[iA].child2 = iF;
					m_nodes[iF].parent = iA;
					m_nodes[iA].bounds = m_nodes[iB].bounds + m_nodes[iF].bounds;
					m_nodes[iC].bounds = m_nodes[iA].bounds + m_nodes[iG].bounds;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iB].height, m_nodes[iF].height);
					m_nodes[iC].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iG].height);
				}
				return iC;
			}
			// 上旋B  
			if (balance < -1)
			{
				int iD = m_nodes[iB].child1;
				int iE = m_nodes[iB].child2;
				// 验证iD、iE是否有效
				if (iD < 0 || iD >= m_nodes.Length) return iA;
				if (iE < 0 || iE >= m_nodes.Length) return iA;
				//交换A和B  
				m_nodes[iB].child1 = iA;
				m_nodes[iB].parent = m_nodes[iA].parent;
				m_nodes[iA].parent = iB;
				// A的旧父指针指向B  
				if (m_nodes[iB].parent != Node.NULL)
				{
					if (m_nodes[m_nodes[iB].parent].child1 == iA)
					{
						m_nodes[m_nodes[iB].parent].child1 = iB;
					}
					else
					{
						if (m_nodes[m_nodes[iB].parent].child2 != iA) return iA;
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
					m_nodes[iA].bounds = m_nodes[iC].bounds + m_nodes[iE].bounds;
					m_nodes[iB].bounds = m_nodes[iA].bounds + m_nodes[iD].bounds;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iC].height, m_nodes[iE].height);
					m_nodes[iB].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iD].height);
				}
				else
				{
					// 旋转E  
					m_nodes[iB].child2 = iE;
					m_nodes[iA].child1 = iD;
					m_nodes[iD].parent = iA;
					m_nodes[iA].bounds = m_nodes[iC].bounds + m_nodes[iD].bounds;
					m_nodes[iB].bounds = m_nodes[iA].bounds + m_nodes[iE].bounds;
					m_nodes[iA].height = 1 + Math.Max(m_nodes[iC].height, m_nodes[iD].height);
					m_nodes[iB].height = 1 + Math.Max(m_nodes[iA].height, m_nodes[iE].height);
				}
				return iB;
			}
			return iA;
		}

		public int GetNodeCount()
		{
			return m_nodeCount;
		}

		public Bounds GetBounds(int proxyId)
		{
			return m_nodes[proxyId].bounds;
		}

		public void ForEachLeafFatBounds(Action<Bounds> forEach)
		{
			if (m_nodeCount == 0) return;
			Stack<int> stack = new Stack<int>(m_nodeCount);
			stack.Push(m_root);
			while (stack.Count > 0)
			{
				int proxyId = stack.Pop();
				Node node = m_nodes[proxyId];
				if (node.IsLeaf())
				{
					forEach(node.bounds);
				}
				else
				{
					stack.Push(node.child1);
					stack.Push(node.child2);
				}
			}
		}
		public void ForEachAssistantFatBounds(Action<Bounds> forEach)
		{
			if (m_nodeCount == 0) return;
			Stack<int> stack = new Stack<int>(m_nodeCount);
			stack.Push(m_root);
			while (stack.Count > 0)
			{
				int proxyId = stack.Pop();
				Node node = m_nodes[proxyId];
				if (!node.IsLeaf())
				{
					forEach(node.bounds);
					stack.Push(node.child1);
					stack.Push(node.child2);
				}
			}
		}

		public T GetUserData(int proxyId)
		{
			return m_nodes[proxyId].userData;
		}
		public void ForEachUserData(Action<T> forEach)
		{
			if (m_nodeCount == 0) return;
			Stack<int> stack = new Stack<int>(m_nodeCount);
			stack.Push(m_root);
			while (stack.Count > 0)
			{
				int proxyId = stack.Pop();
				Node node = m_nodes[proxyId];
				if (node.IsLeaf())
				{
					forEach(node.userData);
				}
				else
				{
					stack.Push(node.child1);
					stack.Push(node.child2);
				}
			}
		}

		public double GetAreaRatio()
		{
			//空树  
			if (m_root == Node.NULL)
			{
				return 0;
			}
			//获取根子树
			Node root = m_nodes[m_root];
			double rootArea = root.bounds.radius;
			//获取所有节点的总“体积”，其实是半径
			double totalArea = 0;
			for (int i = 0; i < m_nodes.Length; ++i)
			{
				Node node = m_nodes[i];
				if (node.height < 0)
				{
					//内存池内的空闲节点  
					continue;
				}
				totalArea += node.bounds.radius;
			}
			//获取比率  
			return totalArea / rootArea;
		}

		public int GetHeight()
		{
			if (m_root == Node.NULL)
			{
				return 0;
			}
			return m_nodes[m_root].height;
		}
		public int GetHeight(int nodeId)
		{
			if (nodeId < 0 || nodeId >= m_nodeCount) return 0;
			return m_nodes[nodeId].height;
		}

		public int ComputeHeight(int nodeId)
		{
			if (nodeId < 0 || nodeId >= m_nodes.Length) return 0;
			//获取子树头节点
			Node node = m_nodes[nodeId];
			// 是否是叶子
			if (node.IsLeaf())
			{  
				return 0;  
			}  
			//递给调用，返回高度  
			int height1 = ComputeHeight(node.child1);  
			int height2 = ComputeHeight(node.child2);  
			return 1 + Math.Max(height1, height2);
		}

		public int ComputeHeight()
		{
			return ComputeHeight(m_root);
		}

		public int GetMaxBalance()
		{
			int maxBalance = 0;
			for (int i = 0; i < m_nodes.Length; ++i)
			{
				Node node = m_nodes[i];
				// 内存池中的空闲节点  
				if (node.height <= 1)
				{
					continue;
				}
				if (node.IsLeaf()) continue;
				//获取最大平衡值  
				int child1 = node.child1;
				int child2 = node.child2;
				int balance = Math.Abs(m_nodes[child2].height - m_nodes[child1].height);
				maxBalance = Math.Max(maxBalance, balance);
			}
			return maxBalance;
		}

		public void Rebuild()
		{
			//从系统堆中申请一段内存
			int[] nodes = new int[m_nodeCount];
			int count = 0;
			//创建空闲的叶子数组。其余是空闲的
			for (int i = 0; i < m_nodes.Length; ++i)
			{
				if (m_nodes[i].height < 0)
				{
					// 内存池中空闲的节点
					continue;
				}
				// 是否是叶子节点
				if (m_nodes[i].IsLeaf())
				{
					m_nodes[i].parent = Node.NULL;
					nodes[count] = i;
					++count;
				}
				else
				{
					// 不是则释放到内存池中
					FreeNode(i);
				}
			}
			// 叶子节点的个数  
			while (count > 1)
			{
				//最小包围球
				double min = double.MaxValue;
				int iMin = -1, jMin = -1;
				//获取最小(j)和第二小(i)的包围球
				for (int i = 0; i < count; ++i)
				{
					Bounds aabbi = m_nodes[nodes[i]].bounds;
					for (int j = i + 1; j < count; ++j)
					{
						Bounds aabbj = m_nodes[nodes[j]].bounds;
						Bounds b = aabbi + aabbj;
						//获取最小的包围球
						if (b.radius < min)
						{
							iMin = i;
							jMin = j;
							min = b.radius;
						}
					}
				}
				//获取左右孩子节点和左右子树  
				int index1 = nodes[iMin];
				int index2 = nodes[jMin];
				//申请父子树索引
				int parentIndex = AllocateNode();
				//获取父子树节点
				m_nodes[parentIndex].child1 = index1;
				m_nodes[parentIndex].child2 = index2;
				m_nodes[parentIndex].height = 1 + Math.Max(m_nodes[index1].height, m_nodes[index2].height);
				m_nodes[parentIndex].bounds = m_nodes[index1].bounds + m_nodes[index2].bounds;
				m_nodes[parentIndex].parent = Node.NULL;
				//
				m_nodes[index1].parent = parentIndex;
				m_nodes[index2].parent = parentIndex;
				//覆盖最小aabb节点
				nodes[jMin] = nodes[count - 1];
				//将第二小aabb节点用父节点覆盖
				nodes[iMin] = parentIndex;
				--count;
			}
			//获取跟节点
			m_root = nodes[0];
		}

		internal void Query(Bounds bounds, Func<int, bool> QueryCallback)
		{
			//申请临时栈，根节点进栈
			Stack<int> stack = new Stack<int>(m_nodeCount);
			stack.Push(m_root);
			//判断栈的个数
			while (stack.Count > 0)
			{
				//获取节点id
				int nodeId = stack.Pop();
				if (nodeId == Node.NULL)
				{
					//节点内存池中的空闲节点
					continue;
				}
				//获取节点
				Node node = m_nodes[nodeId];
				//测试重叠
				if (node.bounds.Intersects(bounds))
				{
					//是否是叶子节点
					if (node.IsLeaf())
					{
						//是否继续
						bool proceed = QueryCallback(nodeId);
						if (proceed == false)
						{
							return;
						}
					}
					else
					{
						//左右孩子节点进栈
						stack.Push(node.child1);
						stack.Push(node.child2);
					}
				}
			}
		}

		internal void RayCast(RayCastInput input, Func<int, double, double> RayCastCallback)
		{
			if (input.maxDistance < 0)
			{
				return;
			}
			//创建一个临时栈，并将根节点进栈
			Stack<int> stack = new Stack<int>(m_nodeCount);
			stack.Push(m_root);
			//栈不为空
			while (stack.Count > 0)
			{
				//出栈
				int nodeId = stack.Pop();
				if (nodeId == Node.NULL)
				{
					//节点内存池中的空闲节点
					continue;
				}
				//根据节点索引获取节点
				Node node = m_nodes[nodeId];
				//判断包围球
				double distance;
				if (node.bounds.IntersectRay(input.ray, out distance) && distance <= input.maxDistance)
				{
					//是否是叶子节点
					if (node.IsLeaf())
					{
						//是否继续
						input.maxDistance = RayCastCallback(nodeId, distance);
						if (input.maxDistance < 0)
						{
							return;
						}
					}
					else
					{
						//左右孩子节点进栈
						stack.Push(node.child1);
						stack.Push(node.child2);
					}
				}
			}
		}

		public int CreateProxy(Bounds bounds, T userData)
		{
			//申请代理节点id
			int proxyId = AllocateNode();

			//填充aabb，为节点赋值
			m_nodes[proxyId].bounds = bounds;
			m_nodes[proxyId].height = 0;
			m_nodes[proxyId].userData = userData;
			//插入叶子节点
			InsertLeaf(proxyId);

			return proxyId;
		}

		public void DestroyProxy(int proxyId)
		{
			if (proxyId < 0 || proxyId >= m_nodes.Length || !m_nodes[proxyId].IsLeaf()) return;

			//删除叶子节点
			RemoveLeaf(proxyId);
			//是否子节点
			FreeNode(proxyId);
		}

		public bool MoveProxy(int proxyId, Bounds bounds, double delta)
		{
			if (proxyId < 0 || proxyId >= m_nodes.Length || !m_nodes[proxyId].IsLeaf()) return false;

			if (m_nodes[proxyId].bounds.Contains(bounds)) return false;
			//根据proxyId移除叶子
			RemoveLeaf(proxyId);
			//重新设置包围球
			bounds.radius += delta;
			m_nodes[proxyId].bounds = bounds;
			//插入叶子节点
			InsertLeaf(proxyId);
			return true;
		}

	}
}
