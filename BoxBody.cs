using System;
using MathematicsX;
using Bubbles;

namespace Bubbles
{
	public class BoxBody : Body
    {
		public Vec3 extends { get; set; }
		public override Bounds bounds { get { return new Bounds(position, extends.magnitude); } }

		internal BoxBody(BoxBodyDef def)
			: base(def.mass, def.resilience, def.position, def.rotation)
		{
			this.extends = def.extents;
		}

		internal override void UpdatePair(Body other)
		{
			SphereBody sphere = other as SphereBody;
			if (sphere != null)
			{
				
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
