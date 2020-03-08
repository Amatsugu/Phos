﻿using UnityEngine;

public class InteractiveBuildingTileInfo : BuildingTileInfo
{
	public RectTransform UIScreen;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new InteractiveBuildingTile(pos, height, this);
	}
}