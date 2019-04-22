using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTile : Tile
{
	public readonly ResourceTileInfo resInfo;

	public ResourceTile(HexCoords coords, float height, ResourceTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		resInfo = tInfo;
	}
}
