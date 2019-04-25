using System;
using System.Collections.Generic;
using MathematicsX;

namespace Bubbles
{
	public abstract class Shape
	{
		public virtual double Radius { get { return 0; } }
		public virtual bool IsParticle { get { return false; } }
		public virtual bool IsSphere { get { return false; } }
		public virtual bool IsCapsule { get { return false; } }
		public virtual bool IsBox { get { return false; } }
		public virtual bool IsPolyhedron { get { return false; } }
		public virtual ParticleShape AsParticle { get { return null; } }
		public virtual SphereShape AsSphere { get { return null; } }
		public virtual CapsuleShape AsCapsule { get { return null; } }
		public virtual BoxShape AsBox { get { return null; } }
		public virtual PolyhedronShape AsPolyhedron { get { return null; } }

		public virtual Vec3 GetFarthestPoint(Vec3 localDirection)
		{
			return Vec3.zero;
		}
	}

	public class ParticleShape : Shape
	{
		public override bool IsParticle { get { return true; } }
		public override ParticleShape AsParticle { get { return this; } }
	}

	public class SphereShape : Shape
	{
		private double m_radius;

		public SphereShape(double radius)
		{
			m_radius = radius;
		}

		public override double Radius { get { return m_radius; } }
		public override bool IsSphere { get { return true; } }
		public override SphereShape AsSphere { get { return this; } }

		public override Vec3 GetFarthestPoint(Vec3 lDir)
		{
			return m_radius * lDir.Normalize();
		}
	}

	public class CapsuleShape : Shape
	{
		private double m_radius;
		private double m_height;

		public CapsuleShape(double radius, double height)
		{
			m_radius = radius;
			m_height = height;
		}

		public double Height { get { return m_height; } }

		public override double Radius { get { return Math.Max(m_radius, m_height * 0.5); } }
		public override bool IsCapsule { get { return true; } }
		public override CapsuleShape AsCapsule { get { return this; } }

		public override Vec3 GetFarthestPoint(Vec3 lDir)
		{
			double hh = Math.Max(0, m_height * 0.5 - m_radius);
			double t = Math.Sqrt((hh * hh + m_radius * m_radius) / lDir.SqrLength());
			double s;
			Vec3 sC = Math.Sign(lDir.z) * hh * Vec3.forward;
			if (GeometryX.LineSphereIntersection(lDir, sC, m_radius, out s, out s) >= 1)
				t = Math.Max(t, s);
			return t * lDir;
		}
	}

	public class BoxShape : Shape
	{
		private Vec3 m_extends;
		private double m_radius;

		public BoxShape(Vec3 extends)
		{
			m_extends = extends.Abs();
			m_radius = m_extends.Length();
		}

		public Vec3 Extends { get { return m_extends; } }
		public Vec3 Max { get { return m_extends; } }
		public Vec3 Min { get { return -m_extends; } }

		public override double Radius { get { return m_radius; } }
		public override bool IsBox { get { return true; } }
		public override BoxShape AsBox { get { return this; } }

		public override Vec3 GetFarthestPoint(Vec3 lDir)
		{
			Vec3 p;
			p.x = lDir.x >= 0 ? m_extends.x : -m_extends.x;
			p.y = lDir.y >= 0 ? m_extends.y : -m_extends.y;
			p.z = lDir.z >= 0 ? m_extends.z : -m_extends.z;
			return p;
		}

		public void GetVertices(IList<Vec3> vertices)
		{
			vertices[0] = new Vec3(m_extends.x, m_extends.y, m_extends.z);
			vertices[1] = new Vec3(-m_extends.x, m_extends.y, m_extends.z);
			vertices[2] = new Vec3(-m_extends.x, -m_extends.y, m_extends.z);
			vertices[3] = new Vec3(m_extends.x, -m_extends.y, m_extends.z);
			vertices[4] = new Vec3(m_extends.x, m_extends.y, -m_extends.z);
			vertices[5] = new Vec3(-m_extends.x, m_extends.y, -m_extends.z);
			vertices[6] = new Vec3(-m_extends.x, -m_extends.y, -m_extends.z);
			vertices[7] = new Vec3(m_extends.x, -m_extends.y, -m_extends.z);
		}

		public void GetEdges(IList<LineSeg<Vec3>> edges)
		{
			Vec3[] vertices = new Vec3[8];
			GetVertices(vertices);
			edges[0] = new LineSeg<Vec3>(vertices[0], vertices[1]);
			edges[1] = new LineSeg<Vec3>(vertices[1], vertices[2]);
			edges[2] = new LineSeg<Vec3>(vertices[2], vertices[3]);
			edges[3] = new LineSeg<Vec3>(vertices[3], vertices[0]);
			edges[4] = new LineSeg<Vec3>(vertices[4], vertices[5]);
			edges[5] = new LineSeg<Vec3>(vertices[5], vertices[6]);
			edges[6] = new LineSeg<Vec3>(vertices[6], vertices[7]);
			edges[7] = new LineSeg<Vec3>(vertices[7], vertices[4]);
			edges[8] = new LineSeg<Vec3>(vertices[0], vertices[4]);
			edges[9] = new LineSeg<Vec3>(vertices[1], vertices[5]);
			edges[10] = new LineSeg<Vec3>(vertices[2], vertices[6]);
			edges[11] = new LineSeg<Vec3>(vertices[3], vertices[7]);
		}
	}

	public class PolyhedronShape : Shape
	{
		private Vec3[] m_vertices;
		private double m_radius;

		public PolyhedronShape(IList<Vec3> vertices)
		{
			m_vertices = new Vec3[vertices.Count];
			m_radius = 0;
			for (int i = 0; i < m_vertices.Length; i++)
			{
				m_vertices[i] = vertices[i];
				double sqrLen = m_vertices[i].SqrLength();
				if (m_radius < sqrLen) m_radius = sqrLen;
			}
			m_radius = Math.Sqrt(m_radius);
		}

		public int VertexCount { get { return m_vertices.Length; } }

		public override double Radius { get { return m_radius; } }
		public override bool IsPolyhedron { get { return true; } }
		public override PolyhedronShape AsPolyhedron { get { return this; } }

		public override Vec3 GetFarthestPoint(Vec3 lDir)
		{
			Vec3 farthest = default(Vec3);
			double maxp = double.MinValue;
			foreach (Vec3 v in m_vertices)
			{
				double p = VecX.Dot(v, lDir);
				if (maxp < p)
				{
					maxp = p;
					farthest = v;
				}
			}
			return farthest;
		}

		public void GetVertices(IList<Vec3> vertices)
		{
			for (int i = 0; i < m_vertices.Length; i++)
			{
				vertices[i] = m_vertices[i];
			}
		}
	}

}
