using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GatheringBuildingTile : PoweredBuildingTile
{
	public ResourceGatheringBuildingInfo gatherInfo;

	public GatheringBuildingTile(HexCoords coords, float height, ResourceGatheringBuildingInfo tInfo) : base(coords, height, tInfo)
	{
		gatherInfo = tInfo;
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		var fullRange = gatherInfo.gatherRange + gatherInfo.size;
		var tilesInRange = Map.ActiveMap.HexSelect(Coords, fullRange);
		var gatherTileInfo = gatherInfo.resourcesToGather.Select(r => ResourceDatabase.GetResourceTile(r.id)).ToArray();
		var prodData = new ProductionData
		{
			rates = new int[gatherInfo.resourcesToGather.Length],
			resourceIds = new int[gatherInfo.resourcesToGather.Length]
		};
		foreach (var tile in tilesInRange)
		{
			if (tile is ResourceTile rt)
			{
				for (int i = 0; i < gatherInfo.resourcesToGather.Length; i++)
				{
					if (gatherTileInfo[i] == rt.info && !rt.gatherer.isCreated)
					{
						rt.gatherer = Coords;
						prodData.rates[i] += gatherInfo.resourcesToGather[i].ammount;
						break;
					}
				}
			}
		}
		for (int i = 0; i < gatherInfo.resourcesToGather.Length; i++)
		{
			prodData.resourceIds[i] = gatherInfo.resourcesToGather[i].id;
		}
		if (Map.EM.HasComponent<ProductionData>(_tileEntity))
		{
			var exisitingProdData = Map.EM.GetSharedComponentData<ProductionData>(_tileEntity);
			if (exisitingProdData.rates?.Length > 0)
			{
				prodData.rates = exisitingProdData.rates.Concat(prodData.rates).ToArray();
				prodData.resourceIds = exisitingProdData.resourceIds.Concat(prodData.resourceIds).ToArray();
				Map.EM.SetSharedComponentData(_tileEntity, prodData);
			}
		}
		else
			Map.EM.AddSharedComponentData(_tileEntity, prodData);

		var p = Map.EM.GetSharedComponentData<ProductionData>(_tileEntity);
		for (int i = 0; i < p.resourceIds.Length; i++)
		{
			Debug.Log($"{p.resourceIds[i]} : {p.rates[i]}");
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		var fullRange = gatherInfo.gatherRange + gatherInfo.size;
		var tilesInRange = Map.ActiveMap.HexSelect(Coords, fullRange, true);
		foreach (var tile in tilesInRange)
		{
			if (tile is ResourceTile rt)
			{
				if (rt.gatherer == Coords)
					rt.gatherer = default;
			}
		}
		var tiles = Map.ActiveMap.HexSelect(Coords, fullRange * 2, true);
		foreach (var tile in tiles)
		{
			if(tile is GatheringBuildingTile gb)
			{
				gb.OnPlaced();
			}
		}
	}
}
