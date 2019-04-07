using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building")]
public class BuildingTileInfo : TileInfo
{
	public MeshEntity buildingMesh;
	public int size = 0;
	public int powerTransferRadius = 0;
	public int influenceRange = 0;

	public Resource[] production;
	public Resource[] consumption;

	[System.Serializable]
	public struct Resource
	{
		public string name;
		public int ammount;
		public int perTileBonus;
	}

	public override ComponentType[] GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(FirstTickTag)
		}).ToArray();
	}

	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = base.Instantiate(pos, scale);
		var influenceTiles = Map.ActiveMap.HexSelect(pos, influenceRange).GroupBy(t => t.info.name).ToDictionary(g => g.Key, g => g.Count());
		if (production.Length > 0)
		{
			var pData = new ProductionData
			{
				resourceIds = new int[production.Length],
				rates = new int[production.Length]
			};
			for (int i = 0; i < production.Length; i++)
			{
				var rId = ResourceDatabase.GetResourceId(production[i].name);
				var tileName = ResourceDatabase.GetResourceTile(rId)?.name ?? production[i].name;
				pData.resourceIds[i] = rId;
				pData.rates[i] = production[i].ammount + (influenceTiles.ContainsKey(tileName) ? influenceTiles[tileName] * production[i].perTileBonus : 0);
			}

			Map.EM.AddSharedComponentData(e, pData);
		}
		if (consumption.Length > 0)
		{

			var cData = new ConsumptionData
			{
				resourceIds = new int[consumption.Length],
				rates = new int[consumption.Length]
			};
			for (int i = 0; i < consumption.Length; i++)
			{
				var rId = ResourceDatabase.GetResourceId(consumption[i].name);
				var tileName = ResourceDatabase.GetResourceTile(rId)?.name ?? consumption[i].name;
				cData.resourceIds[i] = rId;
				cData.rates[i] = consumption[i].ammount + (influenceTiles.ContainsKey(tileName) ? influenceTiles[tileName] * consumption[i].perTileBonus : 0);
			}

			Map.EM.AddSharedComponentData(e, cData);
		}
		return e;
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		if(consumption.Length == 0)
			return new BuildingTile(pos, height, this);
		else
			return new PoweredBuildingTile(pos, height, this);
	}
}
