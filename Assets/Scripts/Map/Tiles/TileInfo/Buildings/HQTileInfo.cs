using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Map Asset/Tile/HQ")]
public class HQTileInfo : BuildingTileInfo
{

	public ResourceIndentifier[] startingResources;

	public SubHQTileInfo subHQTile;
	public MobileUnitInfo unitInfo;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new HQTile(pos, height, this);
	}

}
