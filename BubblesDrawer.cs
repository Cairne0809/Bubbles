using System;
using System.Drawing;
using MathematicsX;

namespace Bubbles
{
	public class BubblesDrawer
	{
		Pen pen = new Pen(Color.FromArgb(255, 255, 0));

		public Camera camera = new Camera();

		public void DrawPoint(Graphics g, Vec3 pos)
		{
			if (camera.WorldToScreen(pos, out pos))
			{
				int x = Round(pos.x);
				int y = Round(pos.y);
				try { g.DrawEllipse(pen, x, y, 1, 1); }
				catch (Exception e) { }
			}
		}

		public void DrawSegment(Graphics g, Vec3 p1, Vec3 p2)
		{
			if (camera.WorldToScreen(p1, out p1) && camera.WorldToScreen(p2, out p2))
			{
				int x1 = Round(p1.x);
				int y1 = Round(p1.y);
				int x2 = Round(p2.x);
				int y2 = Round(p2.y);
				try { g.DrawLine(pen, x1, y1, x2, y2); }
				catch (Exception e) { }
			}
		}

		public void DrawSphere(Graphics g, Vec3 pos, double radius)
		{
			Vec3 delta = camera.position - pos;
			double magnitude = delta.magnitude;
			if (magnitude <= radius) return;
			pos += radius * radius / magnitude / magnitude * delta;
			radius *= Math.Sin(Math.Acos(radius / magnitude));
			
			if (camera.WorldToScreen(radius, pos, out radius) && camera.WorldToScreen(pos, out pos))
			{
				int x = Round(pos.x - radius);
				int y = Round(pos.y - radius);
				int r = Ceiling(radius * 2);
				try { g.DrawEllipse(pen, x, y, r, r); }
				catch (Exception e) { }
			}
		}

		public void DrawCube(Graphics g, Vec3 pos, Quat rot, Vec3 extends)
		{
			BoxShape box = new BoxShape(extends);
			Segment<Vec3>[] edges = box.GetEdges();
			for (int i = 0; i < edges.Length; i++)
			{
				Vec3 p0 = rot * edges[i].p0 + pos;
				Vec3 p1 = rot * edges[i].p1 + pos;
				DrawSegment(g, p0, p1);
			}
		}

		public void DrawWorld(Graphics g, World world)
		{
			world.ForEachBody((Body body) =>
			{
				if (body.shape.IsParticle())
				{
					DrawPoint(g, body.position);
				}
				else if (body.shape.IsSphere())
				{
					DrawSphere(g, body.position, body.shape.AsSphere().radius);
				}
				else if (body.shape.IsBox())
				{
					DrawCube(g, body.position, body.rotation, body.shape.AsBox().extends);
				}
			});
		}

		int Round(double v)
		{
			return (int)Math.Round(v);
		}
		int Ceiling(double v)
		{
			return (int)Math.Ceiling(v);
		}

		public void DrawFunction(Graphics g, double scale, Func<double, double> func)
		{
			RectangleF rect = g.ClipBounds;
			
			int lastX = Round(0);
			int lastY = Round(rect.Bottom - func(0) * scale);

			for (double x = rect.X + 1; x < rect.Right; x++)
			{
				double y = func(x / scale) * scale;
				g.DrawLine(pen, lastX, lastY, Round(x), Round(rect.Bottom - y));
				lastX = Round(x);
				lastY = Round(rect.Bottom - y);
			}
		}
	}
}
