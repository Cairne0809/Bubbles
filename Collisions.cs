using System;
using System.Collections.Generic;
using MathematicsX;

namespace Bubbles
{
	public class Collisions
	{
		private Simplex2D m_simplex;

		public Collisions()
		{
			m_simplex = new Simplex2D();
		}

		public bool Distance(Body body1, Body body2, out Vec3 distNorm)
		{
			m_simplex.Clear();

			distNorm = body1.Position - body2.Position;
			//distNorm = Vec3.GetRandom();

			// get the first Minkowski Difference point
			m_simplex.Add(Support(body1, body2, distNorm));
			// negate d for the next point
			distNorm = -distNorm;
			// start looping
			while (true)
			{
				// add a new point to the simplex because we haven't terminated yet
				m_simplex.Add(Support(body1, body2, distNorm));
				// otherwise we need to determine if the origin is in
				// the current simplex

				if (VecX.Dot(distNorm, m_simplex.Last) <= 0)
				{
					//Out of Voronoi
					return false;
				}
				else
				{
					Vec2 d2;
					if (m_simplex.ContainsOrigin(out d2))
					{
						distNorm.xy = d2;
						// if it does then we know there is a collision
						return true;
					}
					distNorm.xy = d2;
				}
			}

		}

		//Returns one of the Minkowski Difference
		private Vec3 Support(Body body1, Body body2, Vec3 direction)
		{
			Vec3 p1 = body1.GetFarthestPoint(direction);
			Vec3 p2 = body2.GetFarthestPoint(-direction);
			Vec3 d = p1 - p2;
			return d;
		}




		private double CombineBounce(double lhs, double rhs)
		{
			return Math.Max(lhs, rhs);
		}
		private double CombineFriction(double lhs, double rhs)
		{
			return Math.Min(lhs, rhs);
		}

		public bool Collide(Body lhs, Body rhs)
		{
			if (lhs.Mass + rhs.Mass > 0 || lhs.IsKinematic || rhs.IsKinematic)
			{
				//球与球
				if (lhs.Shape.IsSphere && rhs.Shape.IsSphere)
				{
					return Collide_Sphere_Sphere(lhs, lhs.Shape.AsSphere, rhs, rhs.Shape.AsSphere);
				}
				//盒子与盒子
				else if (lhs.Shape.IsBox && rhs.Shape.IsBox)
				{
					return Collide_Box_Box(lhs, rhs);
				}
				//盒子与球
				else if (lhs.Shape.IsBox && rhs.Shape.IsSphere)
				{
					return Collide_Box_Sphere(lhs, lhs.Shape.AsBox, rhs, rhs.Shape.AsSphere);
				}
				else if (lhs.Shape.IsSphere && rhs.Shape.IsBox)
				{
					return Collide_Box_Sphere(rhs, rhs.Shape.AsBox, lhs, lhs.Shape.AsSphere);
				}
				//粒子与球
				else if (lhs.Shape.IsParticle && rhs.Shape.IsSphere)
				{
					return Collide_Particle_Sphere(lhs, rhs, rhs.Shape.AsSphere);
				}
				else if (lhs.Shape.IsSphere && rhs.Shape.IsParticle)
				{
					return Collide_Particle_Sphere(rhs, lhs, lhs.Shape.AsSphere);
				}
				//盒子与粒子
				else if (lhs.Shape.IsBox && rhs.Shape.IsParticle)
				{
					return Collide_Box_Particle(lhs, lhs.Shape.AsBox, rhs);
				}
				else if (lhs.Shape.IsParticle && rhs.Shape.IsBox)
				{
					return Collide_Box_Particle(rhs, rhs.Shape.AsBox, lhs);
				}
			}
			return false;
		}

		private void CalcPosVel(Body lhs, Body rhs, Vec3 distNorm)
		{
			double bounceMul = CombineBounce(lhs.Bounce, rhs.Bounce) + 1;
			Vec3 lnv = VecX.Project(lhs.Velocity, distNorm);
			Vec3 rnv = VecX.Project(rhs.Velocity, distNorm);
			if (lhs.IsKinematic)
			{
				rhs.SetTrim(-distNorm, -bounceMul * rnv);
			}
			else if (rhs.IsKinematic)
			{
				lhs.SetTrim(distNorm, -bounceMul * lnv);
			}
			else
			{
				double sumMass = lhs.Mass + rhs.Mass;
				Vec3 lhsTrimPos = rhs.Mass / sumMass * distNorm;
				Vec3 rhsTrimPos = -lhs.Mass / sumMass * distNorm;
				Vec3 lhsTrimVel = rhs.Mass / sumMass * bounceMul * (rnv - lnv);
				Vec3 rhsTrimVel = lhs.Mass / sumMass * bounceMul * (lnv - rnv);
				lhs.SetTrim(lhsTrimPos, lhsTrimVel);
				rhs.SetTrim(rhsTrimPos, rhsTrimVel);
			}
		}

		private bool Collide_Sphere_Sphere(Body lhs, SphereShape lhsShp, Body rhs, SphereShape rhsShp)
		{
			if (VecX.Distance(lhs.Position, rhs.Position) > lhsShp.Radius + rhsShp.Radius) return false;
			
			Vec3 distNorm = lhs.Position - rhs.Position;
			distNorm *= (lhsShp.Radius + rhsShp.Radius) / VecX.Length(distNorm) - 1;

			CalcPosVel(lhs, rhs, distNorm);

			return true;
		}

		private bool Collide_Particle_Sphere(Body lhs, Body rhs, SphereShape rhsShp)
		{
			if (VecX.Distance(lhs.Position, rhs.Position) > rhsShp.Radius) return false;
			
			Vec3 distNorm = lhs.Position - rhs.Position;
			distNorm *= rhsShp.Radius / distNorm.Length() - 1;

			CalcPosVel(lhs, rhs, distNorm);

			return true;
		}

		private bool Collide_Box_Sphere(Body lhs, BoxShape lhsShp, Body rhs, SphereShape rhsShp)
		{
			Vec3 ext = lhsShp.Extends;
			Vec3 pos = ~lhs.Rotation * (rhs.Position - lhs.Position);
			double r = rhsShp.Radius;
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
			
			Vec3 distNorm;
			int wx = MathX.WeightI(-ext.x, ext.x, pos.x);
			int wy = MathX.WeightI(-ext.y, ext.y, pos.y);
			int wz = MathX.WeightI(-ext.z, ext.z, pos.z);
			if (wx == 0 && wy == 0 || wy == 0 && wz == 0 || wx == 0 && wz == 0)
			{
				double n1;
				double n2;
				Vec3 temp;
				n1 = pos.x - r - ext.x;
				n2 = pos.x + r + ext.x;
				temp.x = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				n1 = pos.y - r - ext.y;
				n2 = pos.y + r + ext.y;
				temp.y = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				n1 = pos.z - r - ext.z;
				n2 = pos.z + r + ext.z;
				temp.z = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
				int minAxis = temp.Abs().MinAxis();
				distNorm = Vec3.zero;
				distNorm[minAxis] = temp[minAxis];
			}
			else
			{
				Vec3 pext = new Vec3(ext.x * wx, ext.y * wy, ext.z * wz);
				Vec3 ppos = new Vec3(wx == 0 ? 0 : pos.x, wy == 0 ? 0 : pos.y, wz == 0 ? 0 : pos.z);
				Vec3 delta = pext - ppos;
				double mag = delta.Length();
				distNorm = mag > 0 ? (r / mag - 1) * delta : r * new Vec3(wx, wy, wz);
			}

			CalcPosVel(lhs, rhs, lhs.Rotation * distNorm);

			return true;
		}

		private bool Collide_Box_Particle(Body lhs, BoxShape lhsShp, Body rhs)
		{
			Vec3 ext = lhsShp.Extends;
			Vec3 pos = ~lhs.Rotation * (rhs.Position - lhs.Position);

			if (pos.x < -ext.x) return false;
			if (pos.x > ext.x) return false;
			if (pos.y < -ext.y) return false;
			if (pos.y > ext.y) return false;
			if (pos.z < -ext.z) return false;
			if (pos.z > ext.z) return false;

			double n1;
			double n2;
			Vec3 temp;
			n1 = pos.x - ext.x;
			n2 = pos.x + ext.x;
			temp.x = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			n1 = pos.y - ext.y;
			n2 = pos.y + ext.y;
			temp.y = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			n1 = pos.z - ext.z;
			n2 = pos.z + ext.z;
			temp.z = Math.Abs(n1) < Math.Abs(n2) ? n1 : n2;
			int minAxis = temp.Abs().MinAxis();
			Vec3 distNorm = Vec3.zero;
			distNorm[minAxis] = temp[minAxis];

			CalcPosVel(lhs, rhs, lhs.Rotation * distNorm);

			return true;
		}

		private bool Collide_Box_Box(Body lhs, Body rhs)
		{
			Vec3 distNorm;
			if (Distance(lhs, rhs, out distNorm))
			{
				CalcPosVel(lhs, rhs, distNorm);
			}
			return false;
		}

	}
}
