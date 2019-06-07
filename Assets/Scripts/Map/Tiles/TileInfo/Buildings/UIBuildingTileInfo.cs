﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBuildingTileInfo : BuildingTileInfo
{
	public RectTransform UIScreen;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new IteractiveBuildingTile(pos, height, this);
	}
}
