using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MathX;

namespace Bubbles
{
	public class Transform
	{
		object m_userData;
		List<Transform> m_children;
		Transform m_parent;
		Vec3 m_position = Vec3.zero;
		Quat m_rotation = Quat.identity;

		public object userData { get { return m_userData; } }

		public int childrenCount { get { return m_children.Count; } }

		public Vec3 localPosition { get { return m_position; } set { m_position = value; } }
		public Vec3 position
		{
			get
			{
				if (m_parent == null) return m_position;
				else return m_parent.position + m_position;
			}
			set
			{
				if (m_parent == null) m_position = value;
				else m_position = value - m_parent.position;
			}
		}

		public Quat localRotation { get { return m_rotation; } set { m_rotation = value; } }
		public Quat rotation
		{
			get
			{
				if (m_parent == null) return m_rotation;
				else return m_parent.rotation * m_rotation;
			}
			set
			{
				if (m_parent == null) m_rotation = value;
				else m_rotation = value * ~m_parent.rotation;
			}
		}

		public Transform(object userData, int childrenCapacity)
		{
			m_userData = userData;
			m_children = new List<Transform>(childrenCapacity);
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
					m_parent.Remove(this);
				}
				if (value != null)
				{
					value.Add(this);
				}
				m_parent = value;
			}
		}

		public Transform Get(int index)
		{
			if (index < 0 || index >= m_children.Count) return null;
			return m_children[index];
		}
		public int GetIndex(Transform child)
		{
			return m_children.IndexOf(child);
		}

		public void ForEach(Action<Transform> forEach)
		{
			m_children.ForEach(forEach);
		}

		
		public bool AddAt(int index, Transform child)
		{
			if (index < 0 || index > m_children.Count || m_children.IndexOf(child) >= 0) return false;
			m_children.Insert(index, child);
			child.m_parent = this;
			return true;
		}
		public bool Add(Transform child)
		{
			return AddAt(m_children.Count, child);
		}

		public bool Remove(Transform child)
		{
			if (m_children.Remove(child))
			{
				child.m_parent = null;
				return true;
			}
			return false;
		}
		public bool RemoveAt(int index)
		{
			if (index < 0 || index >= m_children.Count) return false;
			m_children[index].m_parent = null;
			m_children.RemoveAt(index);
			return true;
		}

		public void RemoveAll(Action<Transform> forEach = null)
		{
			if (forEach == null)
			{
				m_children.ForEach((Transform child) =>
				{
					child.m_parent = null;
				});
			}
			else
			{
				m_children.ForEach((Transform child) =>
				{
					child.m_parent = null;
					forEach(child);
				});
			}
			m_children.Clear();
		}

	}
}
