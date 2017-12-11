namespace Bubbles
{
	public class SphereCollider : Collider
	{
		public double radius;

		override public Bounds bounds { get { return new Bounds(transform.position, radius); } }

		internal SphereCollider(World world, double radius) : base(world)
		{
			this.radius = radius;
		}

		internal void OnCollision()
		{

		}
	}
}
