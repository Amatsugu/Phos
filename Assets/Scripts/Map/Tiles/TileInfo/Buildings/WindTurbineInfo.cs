using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTurbineInfo : BuildingTileInfo
{
	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new WindTurbileTile(pos, height, this);
	}
}
