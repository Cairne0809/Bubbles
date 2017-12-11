namespace Bubbles
{
	public struct RayCastInput
	{
		public Ray ray;
		public double maxDistance;

		public RayCastInput(Ray ray)
		{
			this.ray = ray;
			this.maxDistance = double.MaxValue;
		}
		public RayCastInput(Ray ray, double maxDistance)
		{
			this.ray = ray;
			this.maxDistance = maxDistance;
		}
	}
}
