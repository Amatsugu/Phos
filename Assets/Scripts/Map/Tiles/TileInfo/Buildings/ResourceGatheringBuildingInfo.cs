using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Resoruce Gathering")]
public class ResourceGatheringBuildingInfo : BuildingTileInfo
{
	public Resource[] resourcesToGather;
	public int gatherRange = 3;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new PoweredBuildingTile(pos, height, this);
	}

	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = base.Instantiate(pos, scale);
		var fullRange = gatherRange + size;
		var tilesInRange = Map.ActiveMap.HexSelect(pos, fullRange).Where(t => t is ResourceTile);
		var gatherTileInfo = resourcesToGather.Select(r => ResourceDatabase.GetResourceTile(r.id)).ToArray();
		var prodData = new ProductionData
		{
			rates = new int[resourcesToGather.Length],
			resourceIds = new int[resourcesToGather.Length]
		};
		foreach (var resourceTile in tilesInRange)
		{
			for (int i = 0; i < resourcesToGather.Length; i++)
			{
				Debug.Log(resourceTile.info);
				if(gatherTileInfo[i] == resourceTile.info)
				{
					prodData.rates[i] += resourcesToGather[i].ammount;
					break;
				}
			}
		}
		for (int i = 0; i < resourcesToGather.Length; i++)
		{
			prodData.resourceIds[1] = resourcesToGather[i].id;
		}
		if (Map.EM.HasComponent<ProductionData>(e))
		{
			var exisitingProdData = Map.EM.GetSharedComponentData<ProductionData>(e);
			if (exisitingProdData.rates?.Length > 0)
			{
				prodData.rates = exisitingProdData.rates.Concat(prodData.rates).ToArray();
				prodData.resourceIds = exisitingProdData.resourceIds.Concat(prodData.resourceIds).ToArray();
			}
		}
		Map.EM.AddSharedComponentData(e, prodData);
		return e;
	}
}
