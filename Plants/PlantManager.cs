using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantManager : Singleton<PlantManager>
{
	public Vector2Int mapSize;

	public Material plantMaterial;
	public Mesh plantMesh;


	public static Dictionary<int, List<Plant>> plants;
	public static Plant[] tilePlants;
	static List<int> plantSeeds;
	public static Dictionary<int, List<List<Matrix4x4>>> plantMatrices;
	MaterialPropertyBlock plantMatProps;
	float[] plantGrowthProperties;

	static List<Plant> pooledPlants;

	static List<Plant> soldPlants;
	static List<float> soldPlantTimers;

	public static int seedOffset;
	static int moneyForFarmers;
	static int moneyForDrones;

	public const int instancesPerBatch = 1023;

	public int plantCount=0;
	public static void RegisterSeed(int seed)
	{
		
		plantSeeds.Add(seed);
		plantMatrices.Add(seed, new List<List<Matrix4x4>>(Mathf.CeilToInt(Instance.mapSize.x * Instance.mapSize.y / (float)instancesPerBatch)));
		plantMatrices[seed].Add(new List<Matrix4x4>(instancesPerBatch));
		plants.Add(seed, new List<Plant>(Instance.mapSize.x * Instance.mapSize.y));

	}

	public static void SpawnPlant(int x, int y, int seed)
	{

		Debug.Log("SpawnPlant.");

		
		SquareGrid grid = new SquareGrid(Instance.mapSize.x, Instance.mapSize.y);
		Plant plant = pooledPlants[pooledPlants.Count - 1];
		pooledPlants.RemoveAt(pooledPlants.Count - 1);

		plant.Init(x, y, seed);
		plant.index = plants[seed].Count;
		plants[seed].Add(plant);

		int gridIndex = grid.Index(x, y);

		tilePlants[Instance.plantCount] = plant;
		GameManager.SetGroundState(new GridPoint(x,y), GroundState.Plant);

		List<List<Matrix4x4>> matrices = plantMatrices[seed];

		if (matrices[matrices.Count - 1].Count == instancesPerBatch)
		{
			matrices.Add(new List<Matrix4x4>(instancesPerBatch));
		}
		matrices[matrices.Count - 1].Add(plant.matrix);

		Instance.plantCount++;
	}



	public override void  Awake()
	{
		base.Awake();

		moneyForFarmers = 5;
		moneyForDrones = 0;

		seedOffset = Random.Range(int.MinValue, int.MaxValue);

		plants = new Dictionary<int, List<Plant>>();
		plantMatrices = new Dictionary<int, List<List<Matrix4x4>>>();

		plantSeeds = new List<int>();
		plantMatProps = new MaterialPropertyBlock();
		plantGrowthProperties = new float[instancesPerBatch];
		soldPlants = new List<Plant>(100);
		soldPlantTimers = new List<float>(100);

		int tileCount = mapSize.x * mapSize.y;
		pooledPlants = new List<Plant>(tileCount);
		for (int i = 0; i < tileCount; i++)
		{
			pooledPlants.Add(new Plant());
		}

		tilePlants = new Plant[mapSize.x * mapSize.y];

		SpawnPlant(0, 0, 1);

		
	}

	public void GrowPlants()
	{
		for (int i = 0; i < plantCount; i++)
		{

			//Debug.Log("GrowPlants: i: " + i);
			Plant plant = tilePlants[i];
			plant.Grow();
	
		}
	}

	public void ApplyMatrixToFarm(int seed, int index, Matrix4x4 matrix)
	{
		plantMatrices[seed][index / instancesPerBatch][index % instancesPerBatch] = matrix;
	}

	public void Update()
	{
		/*
		float smooth = 1f - Mathf.Pow(.1f, Time.deltaTime);
		*/

		GrowPlants();
		for (int i = 0; i < plantSeeds.Count; i++)
		{
			int seed = plantSeeds[i];
			Mesh plantMesh = Plant.meshLookup[seed];

			List<Plant> plantList = plants[seed];

			List<List<Matrix4x4>> matrices = plantMatrices[seed];
			for (int j = 0; j < matrices.Count; j++)
			{
				for (int k = 0; k < matrices[j].Count; k++)
				{
					Plant plant = plantList[j * instancesPerBatch + k];
					plant.growth = Mathf.Min(plant.growth + Time.deltaTime / 10f, 1f);
					plantGrowthProperties[k] = plant.growth;
				}
				//plantMatProps.SetFloatArray("_Growth", plantGrowthProperties);
				//Graphics.DrawMeshInstanced(plantMesh, 0, plantMaterial, matrices[j], plantMatProps);
				Graphics.DrawMeshInstanced(plantMesh, 0, plantMaterial, matrices[j]);
			}
		}
	
	}

	/*
	public static void DeletePlant(Plant plant)
	{
		pooledPlants.Add(plant);

		//List<Plant> plantList = plants[plant.seed];
		plantList[plant.index] = plantList[plantList.Count - 1];
		plantList[plant.index].index = plant.index;
		plantList.RemoveAt(plantList.Count - 1);

		List<List<Matrix4x4>> matrices = plantMatrices[plant.seed];
		List<Matrix4x4> lastBatch = matrices[matrices.Count - 1];
		if (lastBatch.Count == 0)
		{
			matrices.RemoveAt(matrices.Count - 1);
			lastBatch = matrices[matrices.Count - 1];
		}

		matrices[plant.index / instancesPerBatch][plant.index % instancesPerBatch] = lastBatch[lastBatch.Count - 1];
		lastBatch.RemoveAt(lastBatch.Count - 1);
	}

	public static bool IsHarvestable(int x, int y)
	{
		Plant plant = tilePlants[x, y];
		return plant != null && plant.growth >= 1f;
	}
	public static bool IsHarvestableAndUnreserved(int x, int y)
	{
		Plant plant = tilePlants[x, y];
		return plant != null && plant.growth >= 1f && plant.reserved == false;
	}
*/
}
