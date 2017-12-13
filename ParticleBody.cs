using MathematicsX;

namespace Bubbles
{
	public class ParticleBody : Body
	{
		public override Bounds bounds { get { return new Bounds(position, 0); } }

		internal ParticleBody(BodyDef def)
			: base(def)
		{

		}

		internal override void UpdatePair(Body other)
		{
			SphereBody sphere = other as SphereBody;
			if (sphere != null)
			{
				Collisions.Collide(this, sphere);
				return;
			}
			BoxBody box = other as BoxBody;
			if (box != null)
			{

				return;
			}
		}

	}
}
