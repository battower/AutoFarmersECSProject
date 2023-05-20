using System.Collections;

public class MovementTile : IHeapItem<MovementTile>
{

	public int Index { get; private set; }
	public GridPoint Position { get; private set; }
	public bool IsTraversable { get; set; }

	public int HeapIndex { get; set; }
	public MovementTile Parent { get; set; }
	public int MoveCost { get; set; }

	public int FCost { get { return GCost + HCost; } }
	public int GCost { get; set; }
	public int HCost { get; set; }

	public MovementTile(int index, GridPoint pos, bool isTraversable, int moveCost)
	{
		Index = index;
		Position = pos;
		IsTraversable = isTraversable;
		MoveCost = moveCost;
		GCost = 0;
		HCost = 0;
	}

	public int CompareTo(MovementTile tile)
	{
		int compare = FCost.CompareTo(tile.FCost);
		if (compare == 0)
		{
			compare = HCost.CompareTo(tile.HCost);
		}
		return -compare;
	}
}
