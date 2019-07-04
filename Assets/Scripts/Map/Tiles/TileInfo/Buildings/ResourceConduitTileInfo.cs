﻿using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Conduit")]
public class ResourceConduitTileInfo : BuildingTileInfo
{
	public int connectionRange;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResourceConduitTile(pos, height, this);
	}
}