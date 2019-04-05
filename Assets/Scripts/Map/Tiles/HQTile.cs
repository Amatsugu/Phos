using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HQTile : BuildingTile
{
	public HQTile(HexCoords coords, float height, HQTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void OnPlaced()
	{
		var info = this.info as HQTileInfo;
		var tilesToReplace = Map.ActiveMap.HexSelect(Coords, info.size);
		for (int i = 0; i < tilesToReplace.Count; i++)
		{
			if(tilesToReplace[i] != this)
				Map.ActiveMap.ReplaceTile(tilesToReplace[i], info.foundationTile);
		}
	}
}
