﻿using System;
using System.Drawing;
using MathematicsX;

namespace Bubbles
{
	public class BubblesDrawer
	{
		private Pen pen = new Pen(Color.FromArgb(255, 255, 0));
		private SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 0));

		public SimpleCamera camera = new SimpleCamera();

		public void DrawPoint(Graphics g, Vec3 pos, Color color)
		{
			if (camera.WorldToScreen(pos, out pos))
			{
				int x = ToScreenX(pos.x);
				int y = ToScreenY(pos.y);
				pen.Color = color;
				g.DrawEllipse(pen, x, y, 1, 1);
			}
		}
		public void DrawPoint(Graphics g, Vec3 pos)
		{
			DrawPoint(g, pos, Color.FromArgb(0, 255, 0));
		}

		public void DrawSegment(Graphics g, Vec3 p1, Vec3 p2, Color color)
		{
			if (camera.WorldToScreen(p1, out p1) && camera.WorldToScreen(p2, out p2))
			{
				int x1 = ToScreenX(p1.x);
				int y1 = ToScreenY(p1.y);
				int x2 = ToScreenX(p2.x);
				int y2 = ToScreenY(p2.y);
				pen.Color = color;
				g.DrawLine(pen, x1, y1, x2, y2);
			}
		}
		public void DrawSegment(Graphics g, Vec3 p1, Vec3 p2)
		{
			DrawSegment(g, p1, p2, Color.FromArgb(0, 255, 0));
		}

		public void DrawSphere(Graphics g, Vec3 pos, double radius, Color color)
		{
			Vec3 delta = camera.Position - pos;
			double magnitude = VecX.Length(delta);
			if (magnitude <= radius) return;
			pos += radius * radius / magnitude / magnitude * delta;
			radius *= Math.Sin(Math.Acos(radius / magnitude));
			
			if (camera.WorldToScreen(radius, pos, out radius) && camera.WorldToScreen(pos, out pos))
			{
				int x = ToScreenX(pos.x - radius);
				int y = ToScreenY(pos.y + radius);
				int r = Ceiling(radius * 2);
				pen.Color = color;
				g.DrawEllipse(pen, x, y, r, r);
			}
		}
		public void DrawSphere(Graphics g, Vec3 pos, double radius)
		{
			DrawSphere(g, pos, radius, Color.FromArgb(0, 255, 0));
		}

		public void DrawCube(Graphics g, Vec3 pos, Quat rot, Vec3 extends, Color color)
		{
			BoxShape box = new BoxShape(extends);
			LineSeg<Vec3>[] edges = new LineSeg<Vec3>[12];
			box.GetEdges(edges);
			for (int i = 0; i < edges.Length; i++)
			{
				Vec3 p0 = rot * edges[i].p0 + pos;
				Vec3 p1 = rot * edges[i].p1 + pos;
				DrawSegment(g, p0, p1, color);
			}
		}
		public void DrawCube(Graphics g, Vec3 pos, Quat rot, Vec3 extends)
		{
			DrawCube(g, pos, rot, extends, Color.FromArgb(0, 255, 0));
		}

		public void DrawText(Graphics g, string text, float size, Color color)
		{
			brush.Color = color;
			Font font = new Font("Consolas", size);
			g.DrawString(text, font, brush, new PointF(0, 0));
		}
		public void DrawText(Graphics g, string text, float size)
		{
			DrawText(g, text, size, Color.White);
		}

		public void DrawWorld(Graphics g, World world)
		{
			world.ForEachParticle((Body body) =>
			{
				DrawPoint(g, body.Position);
			});
			world.ForEachBody((Body body) =>
			{
				if (body.Shape.IsSphere)
				{
					DrawSphere(g, body.Position, (double)body.Shape.AsSphere.Radius);
				}
				else if (body.Shape.IsBox)
				{
					DrawCube(g, body.Position, body.Rotation, body.Shape.AsBox.Extends);
				}
			});
		}

		int ToScreenX(double v)
		{
			return (int)Math.Round(v);
		}
		int ToScreenY(double v)
		{
			return (int)camera.ScreenHeight - (int)Math.Round(v);
		}
		int Ceiling(double v)
		{
			return (int)Math.Ceiling(v);
		}

		public void DrawFunction(Graphics g, double scale, Func<double, double> func)
		{
			RectangleF rect = g.ClipBounds;
			
			int lastX = ToScreenX(0);
			int lastY = ToScreenY(func(0) * scale);

			for (double x = rect.X + 1; x < rect.Right; x++)
			{
				double y = func(x / scale) * scale;
				g.DrawLine(pen, lastX, lastY, ToScreenX(x), ToScreenY(y));
				lastX = ToScreenX(x);
				lastY = ToScreenY(y);
			}
		}
	}
}
