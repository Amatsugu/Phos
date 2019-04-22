using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Tile Info")]
public class ResourceTileInfo : TileInfo
{
	public override Tile CreateTile(HexCoords pos, float height)
	{
		return base.CreateTile(pos, height);
	}
}
