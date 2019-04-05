using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public BuildingTileInfo buildingInfo;

	public BuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		buildingInfo = tInfo;
	}

	public override void UpdateHeight(float height)
	{
		Height = height;
		Map.EM.SetComponentData(_tileEntity, new Translation { Value = new Vector3(Coords.worldX, height, Coords.worldZ) });
		SurfacePoint = new Vector3(Coords.worldX, height, Coords.worldZ);
	}
}
