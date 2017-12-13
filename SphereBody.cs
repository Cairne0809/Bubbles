using System;
using MathematicsX;
using Bubbles;

namespace Bubbles
{
	public class SphereBody : Body
    {
		public double radius { get; set; }
		public override Bounds bounds { get { return new Bounds(position, radius); } }

		internal SphereBody(SphereBodyDef def)
			: base(def.mass, def.resilience, def.position, def.rotation)
		{
			radius = def.radius;
		}

		internal override void UpdatePair(Body other)
		{
			ParticleBody particle = other as ParticleBody;
			if (particle != null)
			{
				Collisions.Collide(this, particle);
				return;
			}
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
