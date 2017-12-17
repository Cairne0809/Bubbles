
namespace Bubbles
{
	public class ParticleShape : Shape
	{
		public override double GetBoundsRadius() { return 0; }

		public override bool IsParticle() { return true; }
		public override bool IsSphere() { return false; }
		public override bool IsBox() { return false; }

		public override ParticleShape AsParticle() { return this; }
		public override SphereShape AsSphere() { return null; }
		public override BoxShape AsBox() { return null; }

	}
}
