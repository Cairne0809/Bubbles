namespace Bubbles
{
	public class WorldObject
	{
		World m_world;
		public World world { get { return m_world; } }

		Transform m_transform;
		public Transform transform { get { return m_transform; } }

		internal WorldObject(World world)
		{
			m_world = world;
			m_transform = new Transform(this, 0);
		}
		internal WorldObject(World world, int childCapacity)
		{
			m_world = world;
			m_transform = new Transform(this, childCapacity);
		}
	}
}
