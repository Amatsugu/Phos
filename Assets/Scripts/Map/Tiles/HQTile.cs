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

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
		var foundation = Map.ActiveMap.HexSelect(Coords, buildingInfo.size);
		for (int i = 0; i < foundation.Count; i++)
		{
			if(foundation[i] != this)
				foundation[i].UpdateHeight(Height);
		}
	}

	
}

public class SubHQTile : PoweredBuildingTile
{
	public SubHQTile(HexCoords coords, float height, SubHQTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void OnHQDisconnected()
	{
		//base.OnHQDisconnected();
		OnHQConnected();
	}
}
