
namespace Bubbles
{
	public class Collider : WorldObject
	{
		internal int proxyId;
		
		public Body rigidBody { get { return transform.parent == null ? null : transform.parent.worldObject as Body; } }
		virtual public Bounds bounds { get { return new Bounds(transform.position, 0); } }

		internal Collider(World world) : base(world)
		{

		}

		public void Destroy()
		{
			if (transform.parent != null)
			{
				rigidBody.DestroyCollider(this);
			}
		}

	}
}
