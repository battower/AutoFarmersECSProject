using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SettingsSO : ScriptableObject
{
	public bool isTesting;
	public bool drawGizmos;
	public bool blockedTilesGizmos;
	public bool farmerGizmos;

	[Space(10)]
	public int worldWidth;
	public int worldHeight;

	[Space(10)]
	public Material farmerMaterial;
	public Mesh farmerMesh;
	[Space(10)]
	public Mesh groundMesh;
	public Material groundMaterial;
	[Space(10)]
	public Mesh rockMesh;
	public Material rockMaterial;
	[Space(10)]
	public Mesh storeMesh;
	public Material storeMaterial;


	public int initialFarmerCount;

	public int rockRectsMinWidth;
	public int rockRectsMinHeight;
	public int rockRectsMaxHeight;
	public int rockRectsMaxWidth;

	public bool DrawGizmos { get { return drawGizmos; } }
	public bool BlockedTileGizmos { get { return blockedTilesGizmos; } }

	public bool FarmerGizmos { get { return farmerGizmos; } }


	public int  WorldWidth {get{return worldWidth;}}
	public int WorldHeight { get { return worldHeight; } }

	public int InitialFarmerCount { get { return initialFarmerCount;}}	

	public Mesh GroundMesh { get { return groundMesh;}}
	public Mesh FarmerMesh {  get { return farmerMesh;}}

	public Material GroundMaterial { get { return groundMaterial;}}
	public Material FarmerMaterial { get { return farmerMaterial;}}



}
