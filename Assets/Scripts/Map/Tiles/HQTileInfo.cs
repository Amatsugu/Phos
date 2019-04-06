using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Map Asset/Tile/HQ")]
public class HQTileInfo : BuildingTileInfo
{

	public BuildingTileInfo foundationTile;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new HQTile(pos, height, this);
	}
}
