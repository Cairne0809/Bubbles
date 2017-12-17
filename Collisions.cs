using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathematicsX;

namespace Bubbles
{
	public static class Collisions
	{
		public static bool TestOverlap(Bounds lhs, Bounds rhs)
		{
			return Vec3.Distance(lhs.center, rhs.center) < lhs.radius + rhs.radius;
		}

		public static double Distance(Bounds lhs, Bounds rhs)
		{
			return Vec3.Distance(lhs.center, rhs.center) - lhs.radius - rhs.radius;
		}

		public static double CombineBounce(double lhs, double rhs)
		{
			return (lhs + rhs) / 2;
		}
		public static double CombineFriction(double lhs, double rhs)
		{
			return lhs * rhs;
		}

		public static void Collide(Body lhs, Body rhs)
		{
			if (lhs.mass + rhs.mass > 0)
			{
				if (lhs.shape.IsSphere() && rhs.shape.IsSphere())
				{
					Collide_Sphere_Sphere(lhs, lhs.shape.AsSphere(), rhs, rhs.shape.AsSphere());
				}
				else if (lhs.shape.IsParticle() && rhs.shape.IsSphere())
				{
					Collide_Particle_Sphere(lhs, rhs, rhs.shape.AsSphere());
				}
				else if (lhs.shape.IsSphere() && rhs.shape.IsParticle())
				{
					Collide_Particle_Sphere(rhs, lhs, lhs.shape.AsSphere());
				}
			}
		}

		static void CalculatePositionVelocity(Body lhs, Body rhs, Vec3 distanceNormal)
		{
			double bounceMul = CombineBounce(lhs.bounce, rhs.bounce) + 1;
			Vec3 lhsVerticalVelocity = Vec3.Project(lhs.velocity, distanceNormal);
			Vec3 rhsVerticalVelocity = Vec3.Project(rhs.velocity, distanceNormal);
			if (lhs.isStatic)
			{
				rhs.SetTrimPos(-distanceNormal);
				rhs.SetTrimVel(-bounceMul * rhsVerticalVelocity);
			}
			else if (rhs.isStatic)
			{
				lhs.SetTrimPos(distanceNormal);
				lhs.SetTrimVel(-bounceMul * lhsVerticalVelocity);
			}
			else
			{
				double sumMass = lhs.mass + rhs.mass;
				lhs.SetTrimPos(rhs.mass / sumMass * distanceNormal);
				rhs.SetTrimPos(-lhs.mass / sumMass * distanceNormal);
				lhs.SetTrimVel(bounceMul * rhs.mass / sumMass * (rhsVerticalVelocity - lhsVerticalVelocity));
				rhs.SetTrimVel(bounceMul * lhs.mass / sumMass * (lhsVerticalVelocity - rhsVerticalVelocity));
			}
		}

		static void Collide_Sphere_Sphere(Body lhs, SphereShape lhsShp, Body rhs, SphereShape rhsShp)
		{
			if (Vec3.Distance(lhs.position, rhs.position) > lhsShp.radius + rhsShp.radius)
			{
				return;
			}
			
			Vec3 distanceNormal = lhs.position - rhs.position;
			if (distanceNormal.sqrMagnitude == 0) distanceNormal = Body.GetBias();
			distanceNormal *= (lhsShp.radius + rhsShp.radius) / distanceNormal.magnitude - 1;
			CalculatePositionVelocity(lhs, rhs, distanceNormal);
		}

		static void Collide_Particle_Sphere(Body lhs, Body rhs, SphereShape rhsShp)
		{
			if (Vec3.Distance(lhs.position, rhs.position) > rhsShp.radius)
			{
				return;
			}
			
			Vec3 distanceNormal = lhs.position - rhs.position;
			if (distanceNormal.sqrMagnitude == 0) distanceNormal = Body.GetBias();
			distanceNormal *= rhsShp.radius / distanceNormal.magnitude - 1;
			CalculatePositionVelocity(lhs, rhs, distanceNormal);
		}

	}
}
