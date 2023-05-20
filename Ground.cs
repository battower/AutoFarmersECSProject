using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum GroundState
{
	Default,
	Tilled,
	Plant
}

/* Handles the rendering of the ground mesh and textures 
 */

public class Ground
{

	public Mesh mesh;
	Matrix4x4[][] matrices;
	public Material material;

	static float[][] propsArr;
	MaterialPropertyBlock[] matPropBlockArr;

	private int instancesPerBatch = 1023;
	private SquareGrid grid;

	private GroundState[] groundStates;

	public Ground(Mesh mesh, Material material, SquareGrid grid)
	{
		this.mesh = mesh;
		this.material = material;
		this.grid = grid;

		groundStates = new GroundState[grid.Length];
		for(int i = 0; i < groundStates.Length; i++)
		{
			groundStates[i] = GroundState.Default;
		}

		InitBatches();
	}

	public void SetGroundState(GridPoint p, GroundState gs)
	{
		if (grid.CheckBounds(p))
		{
			groundStates[grid.Index(p)] = gs;
		}
	}

	public bool IsTilled(int x, int y)
	{
		if (grid.CheckBounds(x, y))
		{
			int i = grid.Index(x, y);
			return groundStates[i] == GroundState.Tilled;
		}
		return false;
	}

	public bool IsPlantable(int x, int y)
	{
		if (grid.CheckBounds(x, y))
		{
			int i = grid.Index(x, y);
			return groundStates[i] == GroundState.Plant;
		}
		return false;
	}

	public bool IsZoneTillable(int x, int y, int width, int height)
	{
		if(grid.CheckBounds(x,y,width, height))
		{
			for(int j = x; j < x + width; j++)
			{
				for(int k = y; k < y + height; k++)
				{
					int i = grid.Index(j, k);
					if (groundStates[i] != GroundState.Default)
						return false;
				}
			}

			return true;

		}

		return false;
	}

	public GroundState? GetGroundState(int x, int y)
	{
		if (grid.CheckBounds(x, y))
		{
			int index = grid.Index(x, y);
			return groundStates[index];
		}
		return null;
	}

	public bool IsTillable(int x, int y)
	{
		if (grid.CheckBounds(x, y))
		{
			if (groundStates[grid.Index(x, y)] == GroundState.Default)
			{
				return true;
			}
		}
		return false;
	}

	public void TillGround(int x, int y)
	{
		if(grid.CheckBounds(x,y))
		{
			int index = grid.Index(x, y);
			groundStates[index] = GroundState.Tilled;

			SetProperty(index, 1.0f);
		}
	}

	public void SetProperty(int index, float val)
	{
		if (index >= 0 && index < grid.Length)
		{
			int batchID = index / instancesPerBatch;
			int batchIndex = index % instancesPerBatch;

			propsArr[batchID][batchIndex] = val;
		}

	}

	public void SetProperty(GridPoint p, float val)
	{
		int index = grid.Index(p);
		SetProperty(index, val);
	}

	public void ResetProperties()
	{
		for (int i = 0; i < grid.Length; i++)
		{
			int batchID = i / instancesPerBatch;
			int batchIndex = i % instancesPerBatch;
			propsArr[batchID][batchIndex] = 0.0f;
		}
	}

	Matrix4x4 GroundMatrix(int index)
	{
		GridPoint gPos = grid.DeIndex(index);
		Vector3 wPos = gPos.GridVec3();// grid.ToWorld(gPos);
		float zRot = Random.Range(0, 2) * 180f;

		return Matrix4x4.TRS(wPos, Quaternion.Euler(90f, 0f, zRot), Vector3.one);
	}

	private void InitBatches()
	{
		int numTiles = grid.Length;

		matrices = new Matrix4x4[Mathf.CeilToInt((float)numTiles / instancesPerBatch)][];//Ground Matrices broken up into batches
		matPropBlockArr = new MaterialPropertyBlock[matrices.Length];//Material Block for each batch of instances.
		propsArr = new float[matrices.Length][];//Float value property of each ground matrix,  sensitive to either >=.5 or not, broken up into batches.

		for (int i = 0; i < matrices.Length; i++)
		{
			matPropBlockArr[i] = new MaterialPropertyBlock();
			if (i < matrices.Length - 1)
			{
				matrices[i] = new Matrix4x4[instancesPerBatch];
			}
			else
			{
				matrices[i] = new Matrix4x4[numTiles - i * instancesPerBatch];
			}

			propsArr[i] = new float[matrices[i].Length];
		}

		for (int i = 0; i < numTiles; i++)
		{
			int batchID = i / instancesPerBatch;
			int batchIndex = i % instancesPerBatch;

			matrices[batchID][batchIndex] = GroundMatrix(i);
		}
	}
	public void Draw()
	{
		for (int i = 0; i < matrices.Length; i++)
		{

			matPropBlockArr[i].SetFloatArray("_Flag", propsArr[i]);
			Graphics.DrawMeshInstanced(mesh, 0, material, matrices[i], matrices[i].Length, matPropBlockArr[i]);
		}
	}
}
