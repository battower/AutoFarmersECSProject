using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;


//Basic Clss Representing a 2D Point on a Grid.
public class GridPoint : IEquatable<GridPoint>
{

	public static bool operator ==(GridPoint lhs, GridPoint rhs)
	{
		if (lhs is null)
		{
			if (rhs is null)
			{
				return true;
			}

			// Only the left side is null.
			return false;
		}
		// Equals handles case of null on right side.
		return lhs.Equals(rhs);
	}

	public static bool operator !=(GridPoint lhs, GridPoint rhs) => !(lhs == rhs);

	public static GridPoint operator +(GridPoint lhs, GridPoint rhs)
	{
		return lhs.Add(rhs);
	}

	public static GridPoint operator -(GridPoint lhs, GridPoint rhs)
	{
		return lhs.Sub(rhs);
	}

	public GridPoint(Vector3 v)
	{
		this.X = Mathf.FloorToInt(v.x);
		this.Y = Mathf.FloorToInt(v.z);

	}

	public GridPoint(Vector2Int v)
	{
		this.X = v.x;
		this.Y = v.y;
	}

	public GridPoint(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	public static int SquareDistance(GridPoint a, GridPoint b)
	{
		return (int)(Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y));
	}

	public int X { get; private set; }
	public int Y { get; private set; }

	public override string ToString() => $"({X}, {Y})";

	public override bool Equals(object obj) => this.Equals(obj as GridPoint);
	public override int GetHashCode() => (X, Y).GetHashCode();

	public bool Equals(GridPoint p)
	{
		if (p is null)
		{
			return false;
		}

		// Optimization for a common success case.
		if (Object.ReferenceEquals(this, p))
		{
			return true;
		}

		// If run-time types are not exactly the same, return false.
		if (this.GetType() != p.GetType())
		{
			return false;
		}

		// Return true if the fields match.
		// Note that the base class is not invoked because it is
		// System.Object, which defines Equals as reference equality.
		return (X == p.X) && (Y == p.Y);
	}

	public GridPoint Add(GridPoint b)
	{
		if (b is null) return this;

		return new GridPoint(X + b.X, Y + b.Y);
	}

	public GridPoint Sub(GridPoint b)
	{
		if(b is null) return this;	
		return new GridPoint(X - b.X, Y - b.Y);
	}

	public GridPoint Mult(int c)
	{
		return new GridPoint(X * c, Y * c);
	}

	public int SquareDistance(GridPoint b)
	{
		return (int)(Mathf.Abs(X - b.X) + Mathf.Abs(Y - b.Y));
	}

	public Vector3 GridVec3(float z=0.0f)
	{
		return new Vector3(X + 0.5f, z, Y + 0.5f);
	}

	public Vector2Int AsVec2Int()
	{
		return new Vector2Int(X, Y);
	}

	public void FromVec3(Vector3 pos)
	{
		X = Mathf.FloorToInt(pos.x);
		Y = Mathf.FloorToInt(pos.z);
	}

}
