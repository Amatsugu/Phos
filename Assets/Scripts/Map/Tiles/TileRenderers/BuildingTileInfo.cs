using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Building")]
public class BuildingTileInfo : TileInfo
{
	public int size = 3;
	public int influenceRange = 6;


	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var p = pos.worldXZ;
		p.y = scale.y;
		var e = Instantiate(p, Vector3.one);
		var influenceTiles = Map.ActiveMap.HexSelect(pos, influenceRange);
		var production = new ProductionData
		{
			resourceIds = new int[]
			{
				ResourceDatabase.GetResourceId("Power"),
				ResourceDatabase.GetResourceId("Stone"),
				ResourceDatabase.GetResourceId("Food"),
				ResourceDatabase.GetResourceId("Water")
			}
		};
		production.productionRates = new int[production.resourceIds.Length];
		for (int i = 0; i < production.resourceIds.Length; i++)
		{
			production.productionRates[i] = 10 + influenceTiles.Count(t => t.info == ResourceDatabase.GetResourceTile(production.resourceIds[i]));
		}

		Map.EM.AddSharedComponentData(e, production);
		return e;
	}
}
