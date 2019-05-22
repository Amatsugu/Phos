using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTile : Tile
{
	public readonly ResourceTileInfo resInfo;
	public HexCoords gatherer;
	public ResourceTile(HexCoords coords, float height, ResourceTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		resInfo = tInfo;
	}


	public override TileInfo GetMeshEntity()
	{
		return originalTile ?? info;
	}
}
