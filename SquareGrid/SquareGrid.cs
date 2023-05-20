using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

/*
 * Create a SquareGrid with dimensions: width and height.  Each square in the grid is defined by two integer co-ordinates x and y.  
 * positive X is right, positive Y is Up.  The square with co-ordiates (0,0) spans the cartesian x-axis, and y-axes from (0, 1).
 *
 */
public class SquareGrid
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	public int EdgeLength { get; private set; }
	public int GridLength { get; private set; }

	public int Length { get { return GridLength; } }

	public SquareGrid(int width, int height)
	{
		width = Math.Abs(width);
		height = Math.Abs(height);

		Width = width;
		Height = height;

		GridLength = width * height;
		EdgeLength = 1;

	}

	//Return the index of the point p in an array representation of the grid.
	public int Index(GridPoint p)
	{
		return Index(p.X, p.Y);

	}

	public int Index(int x, int y)
	{
		if (CheckBounds(x, y))
		{
			return Hash(x, y);
		}
		return -1;
	}

	public GridPoint DeIndex(int index)
	{
		if (CheckBounds(index))
		{
			int y = Mathf.FloorToInt(index / Width);
			int x = index % Width;

			return new GridPoint(x, y);
		}

		return null;
	}

	public bool CheckBounds(GridPoint p)
	{
		return CheckBounds(p.X, p.Y);
	}

	public bool CheckBounds(int index)
	{
		return (index >= 0 && index < Length);
	}
	public bool CheckBounds(int x, int y)
	{
		return (0 <= x && x < Width && 0 <= y && y < Height);
	}

	public bool CheckBounds(int x, int y, int width, int height)
	{
		return ( (x >= 0 ) && ( y >= 0 ) && ( x + width < Width ) && ( y + height < Height ));
	}

	//Returns Edge Adjacent Grid Positions which are in bounds of the grid.
	public IEnumerable<GridPoint> GetNeighbors(GridPoint pos)
	{
		GridPoint right = new GridPoint(pos.X + 1, pos.Y);
		GridPoint up = new GridPoint(pos.X, pos.Y + 1);
		GridPoint left = new GridPoint(pos.X - 1, pos.Y);
		GridPoint down = new GridPoint(pos.X, pos.Y - 1);

		if (CheckBounds(right))
			yield return right;

		if (CheckBounds(up))
			yield return up;

		if (CheckBounds(left))
			yield return left;

		if (CheckBounds(down))
			yield return down;
	}

	private int Hash(int x, int y)
	{
		return x + (y * Width);
	}

}
