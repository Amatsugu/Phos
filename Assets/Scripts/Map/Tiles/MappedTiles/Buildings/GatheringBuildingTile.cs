using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

public class GatheringBuildingTile : PoweredBuildingTile
{
	public ResourceGatheringBuildingEntity gatherInfo;

	public GatheringBuildingTile(HexCoords coords, float height, Map map, ResourceGatheringBuildingEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
	{
		gatherInfo = tInfo;
	}

	public override void OnPlaced()
	{
		var fullRange = gatherInfo.gatherRange + gatherInfo.footprint.size;
		map.HexSelectForEach(Coords, fullRange, t =>
		{
			if (t is ResourceTile rt && (!rt.gatherer.isCreated))
			{
				for(int i = 0; i < gatherInfo.resourcesToGather.Length; i++)
				{
					var gather = gatherInfo.resourcesToGather[i];
					for (int j = 0; j < rt.resInfo.resourceYields.Length; j++)
					{
						var yeild = rt.resInfo.resourceYields[j];
						if (gather.id == yeild.id)
							rt.gatherer = Coords;
					} 
				}
			}
		}, true);
		base.OnPlaced();
	}

	[Obsolete]
	protected virtual void UpdateGather()
	{
		
	}

	public override void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
	{
		base.PrepareBuildingEntity(building, postUpdateCommands);
		
		postUpdateCommands.AddSharedComponent(building, PrepareProductionData());

	}

	public virtual ProductionData PrepareProductionData()
	{
		var fullRange = gatherInfo.gatherRange + gatherInfo.footprint.size;
		var resInRange = new Dictionary<int, int>();
		//var resTiles = new Dictionary<int, List<ResourceTile>>();
		map.HexSelectForEach(Coords, fullRange, t =>
		{
			if (t is ResourceTile rt && (!rt.gatherer.isCreated || rt.gatherer == Coords))
			{
				var yeild = rt.resInfo.resourceYields;
				for (int i = 0; i < yeild.Length; i++)
				{
					var yID = yeild[i].id;
					if (resInRange.ContainsKey(yID))
						resInRange[yID]++;
					else
						resInRange.Add(yID, 1);
				}
			}
		}, true);

		_productionData = new ProductionData
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
				_productionData.resourceIds[i] = res.id;
				_productionData.rates[i] = Mathf.CeilToInt(gatherInfo.resourcesToGather[i].ammount * resInRange[res.id]);
			}
		}
		return _productionData;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		//var fullRange = gatherInfo.gatherRange + gatherInfo.footprint.size;
		//map.HexSelectForEach(Coords, fullRange, t =>
		//{
		//	if (t is ResourceTile rt)
		//	{
		//		if (rt.gatherer == Coords)
		//			rt.gatherer = default;
		//	}
		//},true);
		//var tiles = map.HexSelect(Coords, fullRange * 2, true);
		//foreach (var tile in tiles)
		//{
		//	if (tile is GatheringBuildingTile gb)
		//	{
		//		gb.UpdateGather();
		//	}
		//}
	}
}