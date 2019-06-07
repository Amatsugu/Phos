using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IteractiveBuildingTile : PoweredBuildingTile
{
	public InteractiveBuildingTileInfo uiInfo;
	public IteractiveBuildingTile(HexCoords coords, float height, InteractiveBuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		uiInfo = tInfo;
	}
}
