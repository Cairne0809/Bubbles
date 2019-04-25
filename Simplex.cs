using System;
using System.Collections.Generic;
using MathematicsX;

namespace Bubbles
{
	public abstract class SimplexBase<T> where T : struct, IVector
	{
		protected T[] m_points;
		protected int m_pointCount;
		protected int m_lastIndex;
		protected int[] m_otherIndices;
		protected int m_lowest;

		public SimplexBase()
		{
			int dim = default(T).Dimension;
			m_points = new T[dim + 1];
			m_pointCount = 0;
			m_lowest = -1;
			m_otherIndices = new int[dim];
			for (int i = 0; i < dim; i++)
			{
				m_otherIndices[i] = i;
			}
		}

		public int Capacity { get { return m_points.Length; } }
		public int PointCount { get { return m_pointCount; } }
		public T Last { get { return m_points[m_lastIndex]; } }

		public void Clear()
		{
			m_pointCount = 0;
			m_lastIndex = -1;
			for (int i = 0; i < m_otherIndices.Length; i++)
			{
				m_otherIndices[i] = i;
			}
			m_lowest = -1;
		}
		
		public void Add(T point)
		{
			if (m_pointCount < m_points.Length)
			{
				m_lastIndex = m_pointCount;
				m_points[m_pointCount++] = point;
			}
			else if (m_lowest != -1)
			{
				int repl = m_otherIndices[m_lowest];
				m_otherIndices[m_lowest] = m_lastIndex;
				m_lastIndex = repl;
				m_points[repl] = point;
				m_lowest = -1;
			}
			else
			{
				throw new Exception("Call GetOriginDirection first when PointCount == Capacity !");
			}
		}

		public abstract bool ContainsOrigin(out T dn);
	}

	public class Simplex2D : SimplexBase<Vec2>
	{
		public void Add(Vec3 point)
		{
			base.Add((Vec2)point);
		}
		
		public override bool ContainsOrigin(out Vec2 dn)
		{
			dn = default(Vec2);
			if (m_pointCount == 3)
			{
				Vec2 v0 = m_points[m_lastIndex];
				Vec2 v1 = m_points[m_otherIndices[0]];
				Vec2 v2 = m_points[m_otherIndices[1]];
				Vec2 n0 = VecX.Orthogonalize(v0 - v1, v2 - v1);
				Vec2 d1 = v1 - v0;
				Vec2 d2 = v2 - v0;
				Vec2 n1 = VecX.Cross(d2, d1, d1);
				Vec2 n2 = VecX.Cross(d1, d2, d2);
				if (VecX.Dot(n1, -v0) >= 0)
				{
					m_lowest = 1;
					dn = n1;
				}
				else if (VecX.Dot(n2, -v0) >= 0)
				{
					m_lowest = 0;
					dn = n2;
				}
				else
				{
					Vec2 dn0 = VecX.Orthogonalize(-v0, v1 - v0);
					Vec2 dn1 = VecX.Orthogonalize(-v1, v2 - v1);
					Vec2 dn2 = VecX.Orthogonalize(-v2, v0 - v2);
					double ll0 = dn0.SqrLength();
					double ll1 = dn1.SqrLength();
					double ll2 = dn2.SqrLength();
					if (ll0 < ll1 && ll0 < ll2) dn = dn0;
					else if (ll1 < ll2) dn = dn1;
					else dn = dn2;
					return true;
				}
			}
			else if (m_pointCount == 2)
			{
				Vec2 v0 = m_points[0];
				Vec2 d1 = m_points[1] - v0;
				dn = VecX.Cross(v0, d1, d1);
			}
			return false;
		}
	}
	
}
