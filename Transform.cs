﻿using System;
using MathematicsX;

namespace Bubbles
{
	public class Transform
	{
		private WorldObject m_worldObject;

		private Transform m_parent;
		private Transform[] m_children;
		private int m_childCount;

		private Vec3 m_localPosition = Vec3.zero;
		private Vec3 m_position = Vec3.zero;
		private bool m_positionDirty = false;

		private Quat m_localRotation = Quat.identity;
		private Quat m_rotation = Quat.identity;
		private bool m_rotationDirty = false;
		
		internal Transform(WorldObject worldObject, int childCapacity)
		{
			m_children = new Transform[childCapacity];
			m_worldObject = worldObject;
		}
		
		private void DirtPosition()
		{
			if (m_positionDirty) return;
			m_positionDirty = true;
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].DirtPosition();
			}
		}

		private void DirtRotation()
		{
			if (m_rotationDirty) return;
			m_rotationDirty = true;
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].DirtRotation();
			}
		}

		private void OnAdd(Transform parent)
		{
			m_parent = parent;
			Position = m_position;
			Rotation = m_rotation;
		}
		private void OnRemove()
		{
			m_localPosition = Position;
			m_localRotation = Rotation;
			m_parent = null;
		}

		private void _Add(Transform child)
		{
			if (m_childCount ==  m_children.Length)
			{
				Array.Resize(ref m_children, m_childCount == 0 ? 1 : m_childCount * 2);
			}
			m_children[m_childCount++] = child;
		}
		private bool _AddAt(int index, Transform child)
		{
			if (index < 0 || index > m_childCount) return false;
			if (m_childCount == m_children.Length)
			{
				Array.Resize(ref m_children, m_childCount == 0 ? 1 : m_childCount * 2);
			}
			m_children[m_childCount] = child;
			MoveChild(m_childCount++, index);
			return true;
		}
		private bool _Remove(Transform child)
		{
			int index = Array.IndexOf(m_children, child);
			if (index != -1)
			{
				m_children[index] = null;
				MoveChild(index, --m_childCount);
				return true;
			}
			return false;
		}
		private bool _RemoveAt(int index)
		{
			if (index < 0 || index >= m_childCount) return false;
			m_children[index] = null;
			MoveChild(index, --m_childCount);
			return true;
		}
		private void _Clear()
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i] = null;
			}
			m_childCount = 0;
		}

		public Vec3 LocalPosition
		{
			get { return m_localPosition; }
			set
			{
				m_localPosition = value;
				if (m_parent == null) m_position = value;
				DirtPosition();
			}
		}

		public Vec3 Position
		{
			get
			{
				if (m_positionDirty && m_parent != null)
				{
					m_positionDirty = false;
					m_position = m_parent.Position + m_localPosition;
				}
				return m_position;
			}
			set
			{
				m_position = value;
				if (m_parent == null) m_localPosition = value;
				else m_localPosition = value - m_parent.Position;
				DirtPosition();
			}
		}

		public Quat LocalRotation
		{
			get { return m_localRotation; }
			set
			{
				m_localRotation = value;
				if (m_parent == null) m_rotation = value;
				DirtRotation();
			}
		}

		public Quat Rotation
		{
			get
			{
				if (m_rotationDirty && m_parent != null)
				{
					m_rotationDirty = false;
					m_rotation = m_parent.Rotation * m_localRotation;
				}
				return m_rotation;
			}
			set
			{
				m_rotation = value;
				if (m_parent == null) m_localRotation = value;
				else m_localRotation = value * ~m_parent.Rotation;
				DirtRotation();
			}
		}

		public Transform Parent
		{
			get { return m_parent; }
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

		public int ChildCount
		{
			get { return m_childCount; }
		}

		public void SwapChildren(int index1, int index2)
		{
			Transform temp = m_children[index1];
			m_children[index1] = m_children[index2];
			m_children[index2] = temp;
		}

		public void MoveChild(int from, int to)
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

		public bool AddChild(int index, Transform child)
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
		public bool AddChild(Transform child)
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

		public bool RemoveChild(int index)
		{
			if (index < 0 || index >= m_childCount) return false;
			m_children[index].OnRemove();
			_RemoveAt(index);
			return true;
		}
		public bool RemoveChild(Transform child)
		{
			if (_Remove(child))
			{
				child.OnRemove();
				return true;
			}
			return false;
		}

		public void RemoveAllChildren()
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				m_children[i].OnRemove();
			}
			_Clear();
		}

		public Transform GetChild(int index)
		{
			if (index < 0 || index >= m_childCount) return null;
			return m_children[index];
		}
		public int GetChildIndex(Transform child)
		{
			return Array.IndexOf(m_children, child);
		}

		public void ForEachChild(Action<Transform> forEach)
		{
			for (int i = 0; i < m_childCount; ++i)
			{
				forEach(m_children[i]);
			}
		}

		public T GetObject<T>() where T : WorldObject
		{
			return m_worldObject as T;
		}

		public void ForEachChildObject<T>(Action<T> forEach) where T : WorldObject
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
