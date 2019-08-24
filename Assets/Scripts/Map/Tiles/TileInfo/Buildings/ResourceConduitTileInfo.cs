﻿using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Conduit")]
public class ResourceConduitTileInfo : BuildingTileInfo
{
	public int poweredRange;
	public int connectionRange;
	public MeshEntityRotatable lineEntity;
	public MeshEntityRotatable lineEntityInactive;
	public MeshEntityRotatable energyPacket;
	public Vector3 powerLineOffset;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResourceConduitTile(pos, height, this);
	}
}