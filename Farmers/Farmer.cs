using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum FState
{
	None,
	FollowingPath,
	Idle,
	Wander,
	Tilling,
	Plant
}
[System.Serializable]

public class Farmer
{
	public Matrix4x4 Matrix { get; private set; }
	public int Index { get; private set; }
	public GridPoint GridPosition { get; private set; }
	public Vector3 WorldPosition { get; private set; }

	public int X { get { return GridPosition.X; } }	
	public int Y { get { return GridPosition.Y; } }

	public Matrix4x4 testMatrix;

	private Vector3 destination;
	private List<Vector3> path;

	private float speed = 2.0f;

	private FState curState;
	private FState nextState;
	private FState lastState;

	//DoWander state vars
	private bool hasDestination;
	private bool atDestination;

	//DoIdle state vars
	private bool hasStartedIdle;
	private float idleTime;
	private float idleTimeMax;

	//DoTill vars
	private bool foundTillingZone;
	private RectInt tillingZone;

	//DoHarvest vars
	private bool hasBoughtSeeds;

	public TestFarmerSO testingSO;
	private Vector3 lastWaypoint;

	public Farmer(int index, GridPoint pos)
	{
		Index = index;
		GridPosition = pos;
		WorldPosition = GridPosition.GridVec3();

		Matrix =  Matrix4x4.TRS(WorldPosition, Quaternion.identity,Vector3.one);

		path = new List<Vector3>();
		curState = FState.None;
	}



	//Returns a Rectangle  within the grid of at most, width and hight and distance away from the given position. 
	private RectInt GetRandomZone(int maxWidth, int maxHeight, int maxDist)
	{
		int width = Random.Range(1, maxWidth);
		int height = Random.Range(1, maxHeight);
		int minX = X + Random.Range(-maxDist, maxDist - width);
		int minY = Y + Random.Range(-maxDist, maxDist - height);

		if (minX < 0) minX = 0;
		if (minY < 0) minY = 0;

		if (minX + width >= GameManager.WorldWidth)
			minX = GameManager.WorldWidth - 1 - width;

		if (minY + height >= GameManager.WorldHeight)
			minY = GameManager.WorldHeight - 1 - height;

		return new RectInt(minX, minY, width, height);

	}

	public bool IsTillableInZone(int x, int y)
	{
		if(GameManager.IsTillable(x,y))
		{
			if (x >= tillingZone.xMin && x <= tillingZone.xMax)
			{
				if (y >= tillingZone.yMin && y <= tillingZone.yMax)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void DoIdle()
	{
		if (!hasStartedIdle)
		{
			hasStartedIdle = true;

			idleTime = 0.0f;
			idleTimeMax = Random.Range(1, 4);

			//Debug.Log("Farmer: " + Index + " " + "Idle for: " + idleTimeMax);
		}

		idleTime += Time.deltaTime;

		if (idleTime > idleTimeMax)
		{
			hasStartedIdle = false;

			idleTime = 0.0f;
			curState = FState.None;
		}
	}

	private void DoWander()
	{
		Debug.Log("Do Wander...");
		//If entering DoWander state, Find a path to a random traversable position a random distance away between say, (10 and 50)
		//If no path exists, exit state and enter None state.
		//Otherwise if already in DoWander state and have a wandering destination, enter follow path state, and return once the path has been followed.
		//Once the Follow Path state is complete, we have reached destination. DoWander is Entered Again,  enters Idle State.

		if (!hasDestination)
		{
			GridPoint dest = GetDestination();

			//Debug.Log("Destination: " + dest);

			if( dest != null)
			{ 
				if( Pathing.Instance.FindPath(GridPosition, dest))
				{

					Debug.Log("Found Path");

					destination = dest.GridVec3();
					AddPath();

					hasDestination = true;
					atDestination = false;

					lastState = FState.Wander;
					curState = FState.FollowingPath;
				}
				else
				{
					//Debug.Log("NoPath");
					curState = FState.None;
				}
			}
			else
			{
				//Debug.Log("Destination but no Path.");
				//No path
				curState = FState.None;
			}

		}
		else if (atDestination)
		{
		//	Debug.Log("Reched Destination.");
			hasDestination= false;
			curState = FState.None;
		}

	}

	private void DoTill()
	{
		//Debug.Log("DoTill: Entered");
		if (foundTillingZone == false)
		{

			RectInt rZone = GetRandomZone(4, 4, 5);

			if (GameManager.IsZoneTillable(rZone))//Checks if zone is blocked by rocks or plants or is already tilled.  if not, good to go.
			{
				//Debug.Log("Found Zone: " + rZone);
				//Debug.Log("");

				foundTillingZone = true;
				tillingZone = rZone;
			}
			else
			{
				//Debug.Log("RandomZOne not tillable.");
				//Debug.Log("DoTill: Exit to FState.None");
				//Debug.Log("");
				curState = FState.None;
			}
		}
		else
		{
			if (IsTillableInZone(X, Y))
			{

				path.Clear();
				GameManager.TillGround(X, Y);
				//Debug.Log("IsTillableInZone.  TillGround");
				//Debug.Log("");
			}
			else
			{
				if (path.Count == 0)
				{
					GridPoint dest = GameManager.Instance.GridSearch(GridPosition, 25, GameManager.IsTillable, tillingZone, 1);
					if (Pathing.Instance.FindPath(GridPosition, dest))
					{
						//Debug.Log("Not TillableInZone, Found Path.");
						//Debug.Log("DoTill Exit to FollowPath.");
						//Debug.Log("");

						AddPath();
						curState = FState.FollowingPath;
						lastState = FState.Tilling;

					}
					else
					{
						//Debug.Log("No Path.");
						//Debug.Log("DoTill Exit to FState.None.:");
						foundTillingZone = false;
						curState = FState.None;
					}
				}
			}
		}
	}

	void PlantSeedsAI()
	{

		Debug.Log("Enter PlantSeeds: hasBoughtSeeds = " + hasBoughtSeeds);

		if (hasBoughtSeeds == false)
		{
			if (X == 12 && Y == 12)
			{
				Debug.Log("Bought Seeds");
				hasBoughtSeeds = true;
			}
			else if (path.Count == 0)
			{
				Debug.Log("Going to buy seeds");
				Pathing.Instance.FindPath(GridPosition, new GridPoint(12, 12));

				AddPath();

				Debug.Log("Exiting PlantSeeds to FollowPath.");
				curState = FState.FollowingPath;
				lastState = FState.Plant;
			}
		}
		else
		{
			if ((GameManager.IsReadyForPlant(X, Y)))
			{
				
				path.Clear();
				int seed = Mathf.FloorToInt(Mathf.PerlinNoise(X / 10f, Y / 10f) * 10) + PlantManager.seedOffset;

				Debug.Log("IsReadyForPlant.  Spawn Plant.  Pos: " + GridPosition);
				PlantManager.SpawnPlant(X, Y, seed);
				hasBoughtSeeds= false;
				Debug.Log("Finished PlantSeeds");
				curState = FState.None;


			}
			else
			{
				if (path.Count == 0)
				{
					
					GridPoint dest = GameManager.Instance.GridSearch(GridPosition, 25, GameManager.IsPlantable, tillingZone, 1);

					Debug.Log("Get Path to Plantable Tile. Dest: " + dest);

					if (Pathing.Instance.FindPath(GridPosition, dest))
					{
						Debug.Log("Exiting PlantSeeds to FollowPath");
						AddPath();
						curState = FState.FollowingPath;
						lastState = FState.Plant;

					}
					else
					{
						Debug.Log("Exiting PlantSeeds to None State.  No Path.");
						//hasBoughtSeeds = false;
						curState = FState.None;
					}
				}
			}

		}
		Debug.Log("");

	}

	private void DoFollow()
	{

		//Path is in reverse order from end->start.
		if (path.Count > 0)
		{

			Vector3 waypoint = path[path.Count - 1];

			if (waypoint != lastWaypoint)
			{
				string debugString = "";

				foreach (Vector3 v in path)
				{
					GridPoint wp = new GridPoint(v);
					debugString = " " + debugString + wp + " ";
				}
				//Debug.Log("Following Path: " + debugString);

				lastWaypoint = waypoint;
			}

			if (WorldPosition == waypoint)//reached waypoint.
			{
				path.RemoveAt(path.Count - 1);

			}
			else//move towards waypoint.
			{
				WorldPosition = Vector3.MoveTowards(WorldPosition, waypoint, speed * Time.deltaTime);
				GridPosition.FromVec3(WorldPosition);
				Matrix = Matrix4x4.TRS(WorldPosition, Quaternion.identity, Vector3.one);
			}
		}
		else
		{

			atDestination = true;
			curState = lastState;
		}
	}

	private GridPoint GetDestination()
	{
		if(testingSO != null)
		{
			if (testingSO.isTesting)
			{
				return new GridPoint(testingSO.destination);
			}
			
		}

		int rDistance = Random.Range(5, 10);
		return Pathing.RandomTraversable(SquareShapes.Circle(X, Y, rDistance));
	}
	private void AddPath()
	{
		path.Clear();
		foreach (GridPoint p in Pathing.Instance.IterateLatestPath())
		{
			path.Add(p.GridVec3());
		}
	}


	private void DecideWhatToDo()
	{

		if(testingSO != null)
		{
			if (testingSO.isTesting)
			{
				if (testingSO.setFollowPath)
				{
					testingSO.setFollowPath = false;
					curState = FState.Wander;
					return;
				}
			}
		}

		int r = Random.Range(2, 4);
		if (r == 0)
		{
			curState = FState.Idle;
		}
		else if (r == 1)
		{
			curState = FState.Wander;
		}
		else if(r== 2)
		{
			curState = FState.Tilling;
		}
		else if(r == 3)
		{
			curState = FState.Plant;
		}


		Debug.Log("Decided what to do.  It was : " + curState);
		Debug.Log("");
	}

	private void ResetPosition(GridPoint pos)
	{
		GridPosition = pos;
		WorldPosition = GridPosition.GridVec3();

		Matrix = Matrix4x4.TRS(WorldPosition, Quaternion.identity, Vector3.one);
	}

	public void Update()
	{
		if(testingSO != null)
		{
			testingSO.farmerGridPos = GridPosition.AsVec2Int();

			if (testingSO.isTesting)
			{
				if (testingSO.resetPosition)
				{
					testingSO.resetPosition = false;
					ResetPosition(new GridPoint(testingSO.position));
				}
			}
		}

		if(curState == FState.None)
		{
			DecideWhatToDo();
		}
		else if(curState == FState.Wander)
		{
			DoWander();
		}
		else if( curState == FState.Idle)
		{
			DoIdle();
		}
		else if( curState == FState.Tilling)
		{
			DoTill();
		}
		else if(curState == FState.Plant)
		{
			PlantSeedsAI();
		}
		else if( curState == FState.FollowingPath)
		{
			DoFollow();
		}

		if (Index == 0)
		{
			/*
			Debug.Log("Farmer: " + Index + "\r\t State: " + curState + "\r\tWorldPos: " + WorldPosition + "\r\tGridPos: " + GridPosition + "\r\tDest: " + destination);
			
			string debugString = "";

			foreach (Vector3 v in path)
			{
			debugString = " " + debugString + v.ToString() + " ";
			}

			Debug.Log("\r\t Path: " + debugString);
			Debug.Log("");
			*/
		}
	}

}
