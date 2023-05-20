using UnityEngine;
using System.Collections.Generic;
using System;

public class SquareShapes
{

	/*************************************
	 * 
	 * Shapes
	 */

	public static IEnumerable<GridPoint> Rect(RectInt r)
	{
		return Rect(r.x, r.y, r.width, r.height);
	}

	public static IEnumerable<GridPoint> Rect(int x0, int y0, int width, int height)
	{
		int a = x0;
		int b = y0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				yield return new GridPoint(x + a, y + b);
			}
		}
	}

	public static IEnumerable<GridPoint> Disc(GridPoint p, int dist)
	{
		for (int dx = -dist; dx < dist + 1; dx++)
		{
			int absDX = (int)Mathf.Abs(dx);

			for (int dy = -dist + absDX; dy < dist - absDX + 1; dy++)
			{
				yield return new GridPoint(p.X + dx, p.Y + dy);
			}
		}
	}

	//Returns an iterable of GridPoints given by Brensenham's Line Algorithm between points A and B.
	public static IEnumerable<GridPoint> BLine(GridPoint a, GridPoint b)
	{
		return BLine(a.X, a.Y, b.X, b.Y);
	}

	public static IEnumerable<GridPoint> BLine(int x0, int y0, int x1, int y1)
	{

		if (Mathf.Abs(y1 - y0) < Mathf.Abs(x1 - x0))
		{
			if (x0 > x1)
			{
				return LineLow(x1, y1, x0, y0);
			}
			else
			{
				return LineLow(x0, y0, x1, y1);
			}
		}
		else
		{
			if (y0 > y1)
			{
				return LineHigh(x1, y1, x0, y0);
			}
			else
			{
				return LineHigh(x0, y0, x1, y1);
			}
		}

	}

	private static IEnumerable<GridPoint> LineLow(int x0, int y0, int x1, int y1)
	{


		int dx = x1 - x0;
		int dy = y1 - y0;
		int yi = 1;

		if (dy < 0)
		{
			yi = -1;
			dy = -dy;
		}

		int D = (2 * dy) - dx;
		int y = y0;

		for (int x = x0; x < x1; x++)
		{
			yield return new GridPoint(x, y);

			if (D > 0)
			{
				y = y + yi;
				D = D + (2 * (dy - dx));
			}
			else
			{
				D = D + 2 * dy;
			}

		}

	}
	private static IEnumerable<GridPoint> LineHigh(int x0, int y0, int x1, int y1)
	{

		int dx = x1 - x0;
		int dy = y1 - y0;
		int xi = 1;

		if (dx < 0)
		{
			xi = -1;
			dx = -dx;
		}

		int D = (2 * dx) - dy;
		int x = x0;

		for (int y = y0; y < y1; y++)
		{
			yield return new GridPoint(x, y);

			if (D > 0)
			{
				x = x + xi;
				D = D + (2 * (dx - dy));

			}
			else
			{
				D = D + 2 * dx;
			}

		}
	}

	//Returns points on the grid which intersect the circle x^2 + y^2 - r^ = 0 
	//Uses Brensenhams Variation of MidPoint Circle Algorithm for Integer Arithmetic.
	static public IEnumerable<GridPoint>Circle(int cx, int cy, int r)
	{
		List<GridPoint> onCircle= new List<GridPoint>();

		int x = 0;
		int y = r;
		int d = 3 - 2 * r;

		while( x <= y)
		{
			//The point in each octant
			onCircle.Add(new GridPoint(x, y));
			onCircle.Add(new GridPoint(x, -y));
			onCircle.Add(new GridPoint(-x, y));
			onCircle.Add(new GridPoint(-x, -y));
			onCircle.Add(new GridPoint(y, x));
			onCircle.Add(new GridPoint(y,-x));
			onCircle.Add(new GridPoint(-y, x));
			onCircle.Add(new GridPoint(-y, -x));

			if ( d < 0 )
			{
				d = d + 4 * x + 6;
				x = x + 1;
			}
			else
			{
				d = d + 4 * (x - y) + 10;
				x = x + 1;
				y = y - 1;
			}
		}

		foreach(GridPoint p in onCircle)
		{
			yield return new GridPoint(p.X + cx, p.Y + cy);
		}
	}
		


	//Rotates the given position n * 90 degrees counter clockwise around grid position(0,0)
	public static GridPoint Rotate90(GridPoint pos, int n)
	{
		n = Math.Abs(n);
		n = n % 4;
		if (n == 0)
		{
			return new GridPoint(pos.X, pos.Y);
		}
		else if (n == 1)
		{
			return new GridPoint(-pos.Y, pos.X);
		}
		else if (n == 2)
		{
			return new GridPoint(-pos.X, -pos.Y);
		}
		else //n==3
		{
			return new GridPoint(pos.Y, -pos.X);
		}
	}

	//Rotates the point n * 90 degress counter clockwise about the given position.
	public static GridPoint RotateAbout90(GridPoint a, GridPoint b, int n)
	{
		GridPoint c = Rotate90(new GridPoint(a.X - b.X, a.Y - b.Y), n);
		return new GridPoint(c.X + b.X, c.Y + b.Y);
	}

	//Reflects through x-axis
	public static GridPoint ReflectY(GridPoint a)
	{
		return new GridPoint(a.X, -a.Y);
	}

	//Reflects through y-axis
	public static GridPoint ReflectX(GridPoint a)
	{
		return new GridPoint(-a.X, a.Y);
	}

	//Reflects through x-axis, rotated counter clockwise by n * 45 degrees
	public static GridPoint ReflectBy(GridPoint p, int n)
	{
		GridPoint r = ReflectY(p);
		return Rotate90(r, n);

	}
}
