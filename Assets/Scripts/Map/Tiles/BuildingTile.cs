using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public BuildingTile(HexCoords coords, float height, TileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void UpdateHeight(float height)
	{
		Height = height;
		Map.EM.SetComponentData(_tileEntity, new Translation { Value = new Vector3(Coords.worldX, height, Coords.worldZ) });
		SurfacePoint = new Vector3(Coords.worldX, height, Coords.worldZ);
	}
}
