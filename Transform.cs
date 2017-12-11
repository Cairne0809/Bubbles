using System;
using MathematicsX;

namespace Bubbles
{
	public class Transform
	{
		Transform m_parent;
		Transform[] m_children;
		int m_childCount;
		public int childCount { get { return m_childCount; } }

		WorldObject m_worldObject;
		public WorldObject worldObject { get { return m_worldObject; } }

		Vec3 m_localPosition = Vec3.zero;
		Vec3 m_position = Vec3.zero;
		bool m_positionDirty = false;

        Quat m_localRotation = Quat.identity;
		Quat m_rotation = Quat.identity;
		bool m_rotationDirty = false;
		
		internal Transform(WorldObject worldObject, int childCapacity)
		{
			m_children = new Transform[childCapacity];
			m_worldObject = worldObject;
		}

		void DirtPosition()
		{
			if (m_positionDirty) return;
			m_positionDirty = true;
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].DirtPosition();
			}
		}
		public Vec3 localPosition
		{
			get
			{
				return m_localPosition;
			}
			set
			{
				m_localPosition = value;
				if (m_parent == null) m_position = value;
				DirtPosition();
			}
		}
		public Vec3 position
		{
			get
			{
				if (m_positionDirty && m_parent != null)
				{
					m_positionDirty = false;
					m_position = m_parent.position + m_localPosition;
				}
				return m_position;
			}
			set
			{
				m_position = value;
				if (m_parent == null) m_localPosition = value;
				else m_localPosition = value - m_parent.position;
				DirtPosition();
			}
		}

		void DirtRotation()
		{
			if (m_rotationDirty) return;
			m_rotationDirty = true;
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].DirtRotation();
			}
		}
		public Quat localRotation
		{
			get
			{
				return m_localRotation;
			}
			set
			{
				m_localRotation = value;
				if (m_parent == null) m_rotation = value;
				DirtRotation();
			}
		}
		public Quat rotation
		{
			get
			{
				if (m_rotationDirty && m_parent != null)
				{
					m_rotationDirty = false;
					m_rotation = m_parent.rotation * m_localRotation;
				}
				return m_rotation;
			}
			set
			{
				m_rotation = value;
				if (m_parent == null) m_localRotation = value;
				else m_localRotation = value * ~m_parent.rotation;
				DirtRotation();
			}
		}

		void OnAdd(Transform parent)
		{
			m_parent = parent;
			position = m_position;
			rotation = m_rotation;
		}
		void OnRemove()
		{
			m_localPosition = position;
			m_localRotation = rotation;
			m_parent = null;
		}

		void _Add(Transform child)
		{
			if (m_childCount ==  m_children.Length)
			{
				Array.Resize(ref m_children, m_childCount == 0 ? 1 : m_childCount * 2);
			}
			m_children[m_childCount++] = child;
		}
		bool _AddAt(int index, Transform child)
		{
			if (index < 0 || index > m_childCount) return false;
			if (m_childCount == m_children.Length)
			{
				Array.Resize(ref m_children, m_childCount == 0 ? 1 : m_childCount * 2);
			}
			m_children[m_childCount] = child;
			Move(m_childCount++, index);
			return true;
		}
		bool _Remove(Transform child)
		{
			int index = Array.IndexOf(m_children, child);
			if (index != -1)
			{
				m_children[index] = null;
				Move(index, --m_childCount);
				return true;
			}
			return false;
		}
		bool _RemoveAt(int index)
		{
			if (index < 0 || index >= m_childCount) return false;
			m_children[index] = null;
			Move(index, --m_childCount);
			return true;
		}
		void _Clear()
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i] = null;
			}
			m_childCount = 0;
		}

		public Transform parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				if (value == m_parent) return;
				if (m_parent != null)
				{
					m_parent._Remove(this);
					OnRemove();
				}
				if (value != null)
				{
					OnAdd(value);
					m_parent._Add(this);
				}
			}
		}

		public void Swap(int index1, int index2)
		{
			Transform temp = m_children[index1];
			m_children[index1] = m_children[index2];
			m_children[index2] = temp;
		}

		public void Move(int from, int to)
		{
			Transform temp = m_children[from];
			if (from < to)
			{
				for (; from < to; ++from)
				{
					m_children[from] = m_children[from + 1];
				}
			}
			else if (from > to)
			{
				for (; from > to; --from)
				{
					m_children[from] = m_children[from - 1];
				}
			}
			m_children[to] = temp;
		}

		public bool AddAt(int index, Transform child)
		{
			if (child == null || index < 0 || index > m_childCount) return false;
			if (child.m_parent != null)
			{
				child.m_parent._Remove(child);
				child.OnRemove();
			}
			_AddAt(index, child);
			child.OnAdd(this);
			return true;
		}
		public bool Add(Transform child)
		{
			if (child == null) return false;
			if (child.m_parent != null)
			{
				child.m_parent._Remove(child);
				child.OnRemove();
			}
			_Add(child);
			child.OnAdd(this);
			return true;
		}
		
		public bool RemoveAt(int index)
		{
			if (index < 0 || index >= m_childCount) return false;
			m_children[index].OnRemove();
			_RemoveAt(index);
			return true;
		}
		public bool Remove(Transform child)
		{
			if (_Remove(child))
			{
				child.OnRemove();
				return true;
			}
			return false;
		}

		public void RemoveAll()
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].OnRemove();
			}
			_Clear();
		}

		public Transform Get(int index)
		{
			if (index < 0 || index >= m_childCount) return null;
			return m_children[index];
		}
		public int GetIndex(Transform child)
		{
			return Array.IndexOf(m_children, child);
		}

		public void ForEach(Action<Transform> forEach)
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				forEach(m_children[i]);
			}
		}
		public void ForEachObject(Action<WorldObject> forEach)
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				forEach(m_children[i].m_worldObject);
			}
		}
		public void ForEachObject<T>(Action<T> forEach) where T : WorldObject
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				T obj = m_children[i].m_worldObject as T;
				if (obj != null)
				{
					forEach(obj);
				}
			}
		}

	}
}
