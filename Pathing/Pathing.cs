using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pathing : Singleton<Pathing>
{
	public delegate bool IsNavigableDelegate(int x, int y);
	public delegate bool CheckMatchDelegate(int x, int y);

	public static bool IsNavigableDefault(int x, int y)
	{
		return false;// (Farm.tileRocks[x, y] == null);
	}
	public static bool IsNavigableAll(int x, int y)
	{
		return true;
	}

	//Returns a random point from the enumerable which is traversable.
	static public GridPoint RandomTraversable(IEnumerable<GridPoint> points)
	{
		List<GridPoint> traversable = new List<GridPoint>();

		foreach(GridPoint p in points)
		{
			if(Instance.IsTraversable(p))
			{
				traversable.Add(p);
			}
		}

		if(traversable.Count > 0)
		{
			int i = Random.Range(0, traversable.Count);
			return traversable[i];
		}

		return null;
	}

	//Returns a random point on the grid which is traversable.
	static public GridPoint RandomTraversable()
	{
		int rIndex = Random.Range(0, Instance.grid.Length);
		while (Instance.IsBlocked(rIndex))
		{
			rIndex = Random.Range(0, Instance.grid.Length);
		}

		return Instance.grid.DeIndex(rIndex);

	}

	static public bool IsZoneTraversable(int x, int y, int width, int height)
	{
		if (Instance.grid.CheckBounds(x, y, width, height))
		{
			for (int j = x; j < x + width; j++)
			{
				for (int k = y; k < y + height; k++)
				{
		
					int i = Instance.grid.Index(j, k);
					if (!Instance.IsTraversable(i) )
						return false;
				}
			}

			return true;

		}

		return false;
	}

	private MovementTile[] tiles;
	private SquareGrid grid;

	[SerializeField]
	private SettingsSO settings;

	private MovementTile startTile;
	private MovementTile endTile;

	public override void Awake()
	{
		base.Awake();
		grid = new SquareGrid(settings.WorldWidth, settings.WorldHeight);
		tiles = new MovementTile[grid.Length];

		for (int i = 0; i < tiles.Length; i++)
		{
			tiles[i] = new MovementTile(i, grid.DeIndex(i), true, 0);
		}

	}

	public bool FindPath(GridPoint a, GridPoint b)
	{
		if(grid.CheckBounds(a) && grid.CheckBounds(b))
		{

			int ai = grid.Index(a);
			int bi = grid.Index(b);

			startTile = tiles[ai];
			endTile = tiles[bi];

			return FindPath(startTile, endTile);
		}
		return false;
	}

	public IEnumerable<GridPoint> IterateLatestPath()
	{

		MovementTile cur = endTile;
		while (cur != startTile)
		{
			yield return cur.Position;

			cur = cur.Parent;
		}

	}

	//Finds path using A* algorithm.  Returns True if path found, False otherwise.
	private bool FindPath(MovementTile start, MovementTile end)
	{
		if (start.IsTraversable && end.IsTraversable)
		{

			Heap<MovementTile> open = new Heap<MovementTile>(tiles.Length);
			HashSet<MovementTile> closed = new HashSet<MovementTile>();

			open.Add(start);
			while (open.Count > 0)
			{

				MovementTile cur = open.RemoveFirst();
				closed.Add(cur);

				if (cur == end)
				{

					return true;
				}
;
				foreach (MovementTile neighbor in GetNeighbors(cur))
				{

					if (!neighbor.IsTraversable || closed.Contains(neighbor))
					{
						continue;
					}

					int moveCost = cur.GCost + neighbor.MoveCost + Distance(cur,neighbor);
					if (moveCost < neighbor.GCost || !open.Contains(neighbor))
					{
						neighbor.GCost = moveCost;
						neighbor.HCost = Distance(neighbor, end);
						neighbor.Parent = cur;

						if (!open.Contains(neighbor))
						{
							open.Add(neighbor);
						}
						else
						{
							open.UpdateItem(neighbor);
						}
					}
				}
			}
		}

		return false;
	}

	//quick dirty estimate of distance.
	private int Distance(MovementTile a, MovementTile b)
	{
		return a.Position.SquareDistance(b.Position);
	}

	public IEnumerable<MovementTile> GetNeighbors(MovementTile tile)
	{
		//Debug.Log("GetNeighbors for: " + tile.Position);
		//Debug.Log("");
		foreach (GridPoint neighbor in grid.GetNeighbors(tile.Position))
		{
			
			int index = grid.Index(neighbor);
			//Debug.Log("     Neighbor:Index:     " + neighbor + " : " + index);

			yield return tiles[index];
		}
	}

	public IEnumerable<GridPoint> BlockedTiles()
	{
		for(int i = 0; i < tiles.Length; i++ )
		{
			if (!tiles[i].IsTraversable)
			{
				yield return tiles[i].Position;
			}
		}
	}

	public bool IsTraversable(GridPoint p)
	{
		if (grid.CheckBounds(p))
		{
			return IsTraversable(grid.Index(p));
		}

		return false;
	}
	private bool IsTraversable(int index)
	{
		return tiles[index].IsTraversable;
	}

	private bool IsBlocked(int index)
	{
		return !tiles[index].IsTraversable;
	}

	public void BlockTile(GridPoint pos)
	{
		if (grid.CheckBounds(pos))
		{
			int index = grid.Index(pos);
			BlockTile(index);
		}
	}

	private void BlockTile(int index)
	{
		if (grid.CheckBounds(index))
		{
			tiles[index].IsTraversable = false;
		}
	}

	private void UnBlockTile(int index)
	{
		if (grid.CheckBounds(index))
		{
			tiles[index].IsTraversable = true;
		}
	}

	static public void BlockTiles(IEnumerable<GridPoint> blocked)
	{
		foreach (GridPoint pos in blocked)
		{
			Instance.BlockTile(pos);
		}
	}

	static public void UnBlockTiles(IEnumerable<GridPoint> unblock)
	{
		foreach(GridPoint pos in unblock)
		{
			Instance.UnBlockTile(Instance.grid.Index(pos));
		}
	}

	// Update is called once per frame
	void Update()
    {
        
    }
	public static bool IsRock(int x, int y)
	{
		return false;// (Farm.tileRocks[x, y] != null);
	}
	public static bool IsStore(int x, int y)
	{
		return false;// Farm.storeTiles[x, y];
	}
	public static bool IsTillable(int x, int y)
	{
		return false;// (Farm.groundStates[x, y] == GroundState.Default);
	}
	public static bool IsReadyForPlant(int x, int y)
	{
		return false;  // (Farm.groundStates[x, y] == GroundState.Tilled);
	}


}
