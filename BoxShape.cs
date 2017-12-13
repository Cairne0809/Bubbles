using MathematicsX;

namespace Bubbles
{
	public class BoxShape : Shape
	{
		public Vec3 extends;

		public override double GetBoundsRadius()
		{
			return extends.magnitude;
		}
	}
}
