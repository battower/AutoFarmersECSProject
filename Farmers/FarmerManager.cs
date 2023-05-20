using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FarmerManager : Singleton<FarmerManager>
{

	static public void SpawnFarmer(GridPoint pos)
	{
		int index = Instance.nextIndex++;
		Farmer farmer = new Farmer(index, pos);

		Instance.farmers[index] = farmer;
		Instance.farmerMatrices.Add(farmer.Matrix);

		if (index == 0)
		{
			farmer.testingSO = Instance.testFarmerSO;
			Instance.firstFarmer = farmer;
		}

	}

	static public Farmer GetFirstFarmer()
	{
		return Instance.firstFarmer;
	}


	public Farmer firstFarmer;
	private int initCount;

	[SerializeField]
	SettingsSO sett;

	[SerializeField]
	TestFarmerSO testFarmerSO;

	private SquareGrid grid;
	private Dictionary<int, Farmer> farmers;
	private List<Matrix4x4> farmerMatrices;

	private int nextIndex = 0;

	public override void Awake()
	{
		base.Awake();

		grid = new SquareGrid(sett.WorldWidth, sett.WorldHeight);
		farmers = new Dictionary<int, Farmer>();
		farmerMatrices = new List<Matrix4x4>();
	}

	public void Update()
	{
		int count = farmers.Count;
		for(int i = 0; i < count; i++)
		{
			farmers[i].Update();
		}
	}

}
