
namespace Bubbles
{
	public class SphereShape : Shape
	{
		public double radius;

		public override double GetBoundsRadius() { return radius; }

		public override bool IsParticle() { return false; }
		public override bool IsSphere() { return true; }
		public override bool IsBox() { return false; }

		public override ParticleShape AsParticle() { return null; }
		public override SphereShape AsSphere() { return this; }
		public override BoxShape AsBox() { return null; }

		public SphereShape(double radius)
		{
			this.radius = radius;
		}
		
	}
}
