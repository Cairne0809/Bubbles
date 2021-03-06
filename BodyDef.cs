﻿using MathematicsX;

namespace Bubbles
{
	public struct BodyDef
	{
		public bool isKinematic;
		public double mass;
		public double bounce;
		//public double friction;
		public Vec3 position;
		public Quat rotation;
		public Shape shape;
	}
}
