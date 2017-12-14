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

		public static double CombineResilience(double lhs, double rhs)
		{
			return (lhs + rhs) / 2;
		}

		public static void Collide(Body subjectBody, Body objectBody)
		{
			if (subjectBody.mass + objectBody.mass > 0)
			{
				if (subjectBody.shape is ParticleShape)
				{
					if (objectBody.shape is SphereShape)
					{
						Collide_Particle_Sphere(subjectBody, objectBody, (SphereShape)objectBody.shape);
					}
				}
				else if (subjectBody.shape is SphereShape)
				{
					if (objectBody.shape is ParticleShape)
					{
						Collide_Sphere_Particle(subjectBody, (SphereShape)subjectBody.shape, objectBody);
					}
					else if (objectBody.shape is SphereShape)
					{
						Collide_Sphere_Sphere(subjectBody, (SphereShape)subjectBody.shape, objectBody, (SphereShape)objectBody.shape);
					}
				}
			}
		}

		static void Collide_Particle_Sphere(Body s, Body o, SphereShape os)
		{
			if (Vec3.Distance(s.position, o.position) > os.radius)
			{
				return;
			}

			double mul;

			//velocity
			Vec3 norm = s.position - o.position;
			Vec3 meV = Vec3.Project(s.velocity, norm);
			Vec3 otherV = Vec3.Project(o.velocity, norm);
			double resilience = CombineResilience(s.resilience, o.resilience);
			mul = o.mass / (s.mass + o.mass) * (resilience + 1);
			s.SetTrimVel((otherV - meV) * mul);

			//position
			Vec3 delta = s.position - o.position;
			double sqrMag = delta.sqrMagnitude;
			if (sqrMag == 0)
			{
				s.SetTrimPos(Body.GetBias());
			}
			else
			{
				mul = os.radius / sqrMag;
				s.SetTrimPos(delta * mul);
			}
		}

		static void Collide_Sphere_Particle(Body s, SphereShape ss, Body o)
		{
			if (Vec3.Distance(s.position, o.position) > ss.radius)
			{
				return;
			}

			double mul;

			//velocity
			Vec3 norm = s.position - o.position;
			Vec3 meV = Vec3.Project(s.velocity, norm);
			Vec3 otherV = Vec3.Project(o.velocity, norm);
			double resilience = CombineResilience(s.resilience, o.resilience);
			mul = o.mass / (s.mass + o.mass) * (resilience + 1);
			s.SetTrimVel((otherV - meV) * mul);
		}

		static void Collide_Sphere_Sphere(Body s, SphereShape ss, Body o, SphereShape os)
		{
			if (Vec3.Distance(s.position, o.position) > ss.radius + os.radius)
			{
				return;
			}

			double mul;

			//velocity
			Vec3 normal = s.position - o.position;
			Vec3 meV = Vec3.Project(s.velocity, normal);
			Vec3 otherV = Vec3.Project(o.velocity, normal);
			double resilience = CombineResilience(s.resilience, o.resilience);
			mul = o.mass / (s.mass + o.mass) * (resilience + 1);
			s.SetTrimVel((otherV - meV) * mul);

			//position
			Vec3 delta = s.position - o.position;
			double sqrMag = delta.sqrMagnitude;
			if (sqrMag == 0)
			{
				s.SetTrimPos(Body.GetBias());
			}
			else
			{
				//double meVelMag = s.velocity.magnitude;
				//double otherVelMag = o.velocity.magnitude;
				//double sumVelMag = meVelMag + otherVelMag;
				//if (sumVelMag == 0)
				//{
				//	mul = (ss.radius + os.radius) / sqrMag * meVelMag / sumVelMag;
				//}
				//else
				//{
				//	mul = (ss.radius + os.radius) / sqrMag;
				//}
				mul = (ss.radius + os.radius) / sqrMag;
				s.SetTrimPos(delta * mul);
			}
		}

	}
}
