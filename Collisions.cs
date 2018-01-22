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
			return VecX.Distance(lhs.center, rhs.center) < lhs.radius + rhs.radius;
		}

		public static double Distance(Bounds lhs, Bounds rhs)
		{
			return VecX.Distance(lhs.center, rhs.center) - lhs.radius - rhs.radius;
		}

		public static double CombineBounce(double lhs, double rhs)
		{
			return (lhs + rhs) / 2;
		}
		public static double CombineFriction(double lhs, double rhs)
		{
			return lhs * rhs;
		}

		public static bool Collide(Body lhs, Body rhs)
		{
			if (lhs.mass + rhs.mass > 0 || lhs.isStatic || rhs.isStatic)
			{
				//球与球
				if (lhs.shape.IsSphere() && rhs.shape.IsSphere())
				{
					return Collide_Sphere_Sphere(lhs, lhs.shape.AsSphere(), rhs, rhs.shape.AsSphere());
				}
				//盒子与球
				else if (lhs.shape.IsBox() && rhs.shape.IsSphere())
				{
					return Collide_Box_Sphere(lhs, lhs.shape.AsBox(), rhs, rhs.shape.AsSphere());
				}
				else if (lhs.shape.IsSphere() && rhs.shape.IsBox())
				{
					return Collide_Box_Sphere(rhs, rhs.shape.AsBox(), lhs, lhs.shape.AsSphere());
				}
				//粒子与球
				else if (lhs.shape.IsParticle() && rhs.shape.IsSphere())
				{
					return Collide_Particle_Sphere(lhs, rhs, rhs.shape.AsSphere());
				}
				else if (lhs.shape.IsSphere() && rhs.shape.IsParticle())
				{
					return Collide_Particle_Sphere(rhs, lhs, lhs.shape.AsSphere());
				}
				//盒子与粒子
				else if (lhs.shape.IsBox() && rhs.shape.IsParticle())
				{
					return Collide_Box_Particle(lhs, lhs.shape.AsBox(), rhs);
				}
				else if (lhs.shape.IsParticle() && rhs.shape.IsBox())
				{
					return Collide_Box_Particle(rhs, rhs.shape.AsBox(), lhs);
				}
			}
			return false;
		}

		static void CalculatePositionVelocity(Body lhs, Body rhs, Vec3 distanceNormal)
		{
			double bounceMul = CombineBounce(lhs.bounce, rhs.bounce) + 1;
			Vec3 norm = VecX.Normalize(distanceNormal);
			Vec3 lhsVerticalVelocity = VecX.Project(lhs.velocity, norm);
			Vec3 rhsVerticalVelocity = VecX.Project(rhs.velocity, norm);
			if (lhs.isStatic)
			{
				rhs.SetTrim(-distanceNormal, -bounceMul * rhsVerticalVelocity);
			}
			else if (rhs.isStatic)
			{
				lhs.SetTrim(distanceNormal, -bounceMul * lhsVerticalVelocity);
			}
			else
			{
				double sumMass = lhs.mass + rhs.mass;
				Vec3 lhsTrimPos = rhs.mass / sumMass * distanceNormal;
				Vec3 rhsTrimPos = -lhs.mass / sumMass * distanceNormal;
				Vec3 lhsTrimVel = rhs.mass / sumMass * bounceMul * (rhsVerticalVelocity - lhsVerticalVelocity);
				Vec3 rhsTrimVel = lhs.mass / sumMass * bounceMul * (lhsVerticalVelocity - rhsVerticalVelocity);
				lhs.SetTrim(lhsTrimPos, lhsTrimVel);
				rhs.SetTrim(rhsTrimPos, rhsTrimVel);
			}
		}

		static bool Collide_Sphere_Sphere(Body lhs, SphereShape lhsShp, Body rhs, SphereShape rhsShp)
		{
			if (VecX.Distance(lhs.position, rhs.position) > lhsShp.radius + rhsShp.radius) return false;
			
			Vec3 distanceNormal = lhs.position - rhs.position;
			if (VecX.SqrLength(distanceNormal) == 0) distanceNormal = Body.GetBias();
			distanceNormal *= (lhsShp.radius + rhsShp.radius) / VecX.Length(distanceNormal) - 1;

			CalculatePositionVelocity(lhs, rhs, distanceNormal);

			return true;
		}

		static bool Collide_Particle_Sphere(Body lhs, Body rhs, SphereShape rhsShp)
		{
			if (VecX.Distance(lhs.position, rhs.position) > rhsShp.radius) return false;
			
			Vec3 distanceNormal = lhs.position - rhs.position;
			if (VecX.SqrLength(distanceNormal) == 0) distanceNormal = Body.GetBias();
			distanceNormal *= rhsShp.radius / VecX.Length(distanceNormal) - 1;

			CalculatePositionVelocity(lhs, rhs, distanceNormal);

			return true;
		}

		static bool Collide_Box_Sphere(Body lhs, BoxShape lhsShp, Body rhs, SphereShape rhsShp)
		{
			Vec3 ext = lhsShp.extends;
			Vec3 pos = ~lhs.rotation * (rhs.position - lhs.position);
			double r = rhsShp.radius;

			double dx = 0;
			double dy = 0;
			double dz = 0;
			if (pos.x < -ext.x) dx = pos.x + ext.x;
			else if (pos.x > ext.x) dx = pos.x - ext.x;
			if (pos.y < -ext.y) dy = pos.y + ext.y;
			else if (pos.y > ext.y) dy = pos.y - ext.y;
			if (pos.z < -ext.z) dz = pos.z + ext.z;
			else if (pos.z > ext.z) dz = pos.z - ext.z;
			if (dx * dx + dy * dy + dz * dz > r * r) return false;
			
			Vec3 distanceNormal;
			int wx = MathX.WeightI(-ext.x, ext.x, pos.x);
			int wy = MathX.WeightI(-ext.y, ext.y, pos.y);
			int wz = MathX.WeightI(-ext.z, ext.z, pos.z);
			if (wx == 0 && wy == 0 || wy == 0 && wz == 0 || wx == 0 && wz == 0)
			{
				double n1;
				double n2;
				Vec3 temp = new Vec3();
				n1 = pos.x - r - ext.x;
				n2 = pos.x + r + ext.x;
				temp.x = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				n1 = pos.y - r - ext.y;
				n2 = pos.y + r + ext.y;
				temp.y = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				n1 = pos.z - r - ext.z;
				n2 = pos.z + r + ext.z;
				temp.z = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				int minAxis = VecX.MinI(VecX.Abs(temp));
				distanceNormal = new Vec3();
				distanceNormal[minAxis] = temp[minAxis];
			}
			else
			{
				Vec3 pext = new Vec3(ext.x * wx, ext.y * wy, ext.z * wz);
				Vec3 ppos = new Vec3(wx == 0 ? 0 : pos.x, wy == 0 ? 0 : pos.y, wz == 0 ? 0 : pos.z);
				Vec3 delta = pext - ppos;
				double mag = VecX.Length(delta);
				distanceNormal = mag > 0 ? (r / mag - 1) * delta : r * new Vec3(wx, wy, wz);
			}

			CalculatePositionVelocity(lhs, rhs, lhs.rotation * distanceNormal);

			return true;
		}

		static bool Collide_Box_Particle(Body lhs, BoxShape lhsShp, Body rhs)
		{
			Vec3 ext = lhsShp.extends;
			Vec3 pos = ~lhs.rotation * (rhs.position - lhs.position);

			if (pos.x < -ext.x) return false;
			if (pos.x > ext.x) return false;
			if (pos.y < -ext.y) return false;
			if (pos.y > ext.y) return false;
			if (pos.z < -ext.z) return false;
			if (pos.z > ext.z) return false;

			double n1;
			double n2;
			Vec3 temp = new Vec3();
			n1 = pos.x - ext.x;
			n2 = pos.x + ext.x;
			temp.x = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			n1 = pos.y - ext.y;
			n2 = pos.y + ext.y;
			temp.y = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			n1 = pos.z - ext.z;
			n2 = pos.z + ext.z;
			temp.z = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			int minAxis = VecX.MinI(VecX.Abs(temp));
			Vec3 distanceNormal = new Vec3();
			distanceNormal[minAxis] = temp[minAxis];

			CalculatePositionVelocity(lhs, rhs, lhs.rotation * distanceNormal);

			return true;
		}

	}
}
