
namespace Bubbles
{
	public abstract class Shape
	{
		public abstract double GetBoundsRadius();

		public abstract bool IsParticle();
		public abstract bool IsSphere();
		public abstract bool IsBox();

		public abstract ParticleShape AsParticle();
		public abstract SphereShape AsSphere();
		public abstract BoxShape AsBox();

	}
}
