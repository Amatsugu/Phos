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

	protected override void PrepareEntity()
	{
		base.PrepareEntity();
		var fullRange = gatherInfo.gatherRange + gatherInfo.size;
		var resInRange = new Dictionary<int, int>();
		var resTiles = new Dictionary<int, List<ResourceTile>>();
		Map.ActiveMap.HexSelectForEach(Coords, fullRange, t =>
		{
			if (t is ResourceTile rt && !rt.gatherer.isCreated)
			{
				var yeild = rt.resInfo.resourceYields;
				for (int i = 0; i < yeild.Length; i++)
				{
					var yID = yeild[i].id;
					if (resInRange.ContainsKey(yID))
					{
						resInRange[yID]++;
						resTiles[yID].Add(rt);
					}
					else
					{
						resInRange.Add(yID, 1);
						resTiles.Add(yID, new List<ResourceTile> { rt });
					}
				}
			}
		}, true);

		var prodData = new ProductionData
		{
			resourceIds = new int[gatherInfo.resourcesToGather.Length],
			rates = new int[gatherInfo.resourcesToGather.Length]
		};

		var approxRates = new int[gatherInfo.resourcesToGather.Length];
		for (int i = 0; i < gatherInfo.resourcesToGather.Length; i++)
		{
			var res = gatherInfo.resourcesToGather[i];
			if (resInRange.ContainsKey(res.id))
			{
				prodData.resourceIds[i] = res.id;
				prodData.rates[i] = Mathf.FloorToInt(gatherInfo.resourcesToGather[i].ammount * resInRange[res.id]);
				var gatheredTiles = resTiles[res.id];
				for (int j = 0; j < gatheredTiles.Count; j++)
				{
					gatheredTiles[j].gatherer = Coords;
				}
			}
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
