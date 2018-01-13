namespace Bubbles
{
	public struct RayCastInput
	{
		public Ray ray;
		public double currentDistance;
		public double maxDistance;

		public RayCastInput(Ray ray)
		{
			this.ray = ray;
			this.currentDistance = 0;
			this.maxDistance = double.MaxValue;
		}
		public RayCastInput(Ray ray, double maxDistance)
		{
			this.ray = ray;
			this.currentDistance = 0;
			this.maxDistance = maxDistance;
		}
	}
}
