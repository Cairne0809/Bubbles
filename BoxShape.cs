using MathematicsX;

namespace Bubbles
{
	public class BoxShape : Shape
	{
		Vec3 m_extends;

		public Vec3 extends { get { return m_extends; } set { m_extends = VecX.Abs(value); } }

		public Vec3 max { get { return m_extends; } }
		public Vec3 min { get { return -m_extends; } }

		public BoxShape(Vec3 extends)
		{
			this.extends = extends;
		}

		public override double GetBoundsRadius() { return VecX.Length(extends); }

		public override bool IsParticle() { return false; }
		public override bool IsSphere() { return false; }
		public override bool IsBox() { return true; }

		public override ParticleShape AsParticle() { return null; }
		public override SphereShape AsSphere() { return null; }
		public override BoxShape AsBox() { return this; }
		
		public Vec3[] GetVertices()
		{
			Vec3[] vertices = new Vec3[8];
			vertices[0] = new Vec3(extends.x, extends.y, extends.z);
			vertices[1] = new Vec3(-extends.x, extends.y, extends.z);
			vertices[2] = new Vec3(-extends.x, -extends.y, extends.z);
			vertices[3] = new Vec3(extends.x, -extends.y, extends.z);
			vertices[4] = new Vec3(extends.x, extends.y, -extends.z);
			vertices[5] = new Vec3(-extends.x, extends.y, -extends.z);
			vertices[6] = new Vec3(-extends.x, -extends.y, -extends.z);
			vertices[7] = new Vec3(extends.x, -extends.y, -extends.z);
			return vertices;
		}

		public Segment<Vec3>[] GetEdges()
		{
			Vec3[] vertices = GetVertices();
			Segment<Vec3>[] edges = new Segment<Vec3>[12];
			edges[0] = new Segment<Vec3>(vertices[0], vertices[1]);
			edges[1] = new Segment<Vec3>(vertices[1], vertices[2]);
			edges[2] = new Segment<Vec3>(vertices[2], vertices[3]);
			edges[3] = new Segment<Vec3>(vertices[3], vertices[0]);
			edges[4] = new Segment<Vec3>(vertices[4], vertices[5]);
			edges[5] = new Segment<Vec3>(vertices[5], vertices[6]);
			edges[6] = new Segment<Vec3>(vertices[6], vertices[7]);
			edges[7] = new Segment<Vec3>(vertices[7], vertices[4]);
			edges[8] = new Segment<Vec3>(vertices[0], vertices[4]);
			edges[9] = new Segment<Vec3>(vertices[1], vertices[5]);
			edges[10] = new Segment<Vec3>(vertices[2], vertices[6]);
			edges[11] = new Segment<Vec3>(vertices[3], vertices[7]);
			return edges;
		}

	}
}
