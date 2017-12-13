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

		public static double Distance(Body lhs, Body rhs)
		{
			return 0;
		}

		public static void Collide(ParticleBody me, SphereBody other)
		{
			double sumMass = me.mass + other.mass;
			if (sumMass > 0)
			{
				double mul;

				Vec3 norm = me.position - other.position;
				Vec3 meV = Vec3.Project(me.velocity, norm);
				Vec3 otherV = Vec3.Project(other.velocity, norm);
				double res = (me.resilience + other.resilience) / 2;
				mul = other.mass / sumMass * (res + 1);
				me.SetTrimVel((otherV - meV) * mul);

				Vec3 delta = me.position - other.position;
				double sqrMag = delta.sqrMagnitude;
				if (sqrMag == 0)
				{
					me.SetTrimPos(Body.GetBias());
				}
				else
				{
					mul = other.radius / sqrMag;
					me.SetTrimPos(delta * mul);
				}
			}
		}

		public static void Collide(SphereBody me, ParticleBody other)
		{
			double sumMass = me.mass + other.mass;
			if (sumMass > 0)
			{
				double mul;

				Vec3 norm = me.position - other.position;
				Vec3 meV = Vec3.Project(me.velocity, norm);
				Vec3 otherV = Vec3.Project(other.velocity, norm);
				double res = (me.resilience + other.resilience) / 2;
				mul = other.mass / sumMass * (res + 1);
				me.SetTrimVel((otherV - meV) * mul);

				//Vec3 delta = me.position - other.position;
				//double sqrMag = delta.sqrMagnitude;
				//if (sqrMag == 0)
				//{
				//	me.SetTrimPos(Body.GetBias());
				//}
				//else
				//{
				//	mul = me.radius / sqrMag;
				//	me.SetTrimPos(delta * mul);
				//}
			}
		}

		public static void Collide(SphereBody me, SphereBody other)
		{
			double sumMass = me.mass + other.mass;
			if (sumMass > 0)
			{
				double mul;

				Vec3 normal = me.position - other.position;
				Vec3 meV = Vec3.Project(me.velocity, normal);
				Vec3 otherV = Vec3.Project(other.velocity, normal);
				double res = (me.resilience + other.resilience) / 2;
				mul = other.mass / sumMass * (res + 1);
				me.SetTrimVel((otherV - meV) * mul);

				Vec3 delta = me.position - other.position;
				double sqrMag = delta.sqrMagnitude;
				if (sqrMag == 0)
				{
					me.SetTrimPos(Body.GetBias());
				}
				else
				{
					mul = (me.radius + other.radius) / sqrMag;
					me.SetTrimPos(delta * mul);
				}
			}
		}

	}
}
