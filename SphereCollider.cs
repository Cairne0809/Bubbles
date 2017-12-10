namespace Bubbles
{
	public class SphereCollider : Collider
	{
		double m_radius;

		override public Bounds bounds { get { return new Bounds(transform.position, m_radius); } }

		internal SphereCollider(World world, double radius) : base(world)
		{
			m_radius = radius;
		}

		internal void OnCollision()
		{

		}
	}
}
