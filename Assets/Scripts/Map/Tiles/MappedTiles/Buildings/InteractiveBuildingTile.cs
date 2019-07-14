using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBuildingTile : PoweredBuildingTile
{
	public InteractiveBuildingTileInfo uiInfo;
	public InteractiveBuildingTile(HexCoords coords, float height, InteractiveBuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		uiInfo = tInfo;
	}
}
