using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Solar Panel")]
public class SolarPanelTileInfo : BuildingTileInfo
{
	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new SolarPanelTile(pos, height, this);
	}
}
