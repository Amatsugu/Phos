using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public enum PlacementMode
{
	Single,
	Path
}

[CreateAssetMenu(menuName = "Map Asset/Tile/Building")]
public class BuildingTileInfo : TileInfo
{
	[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
	public MeshEntityRotatable buildingMesh;
	[Range(1, 6)]
	public int tier;
	public BuildingCategory category;
	public int size = 0;
	public int influenceRange = 0;
	[SerializeField]
	public ResourceIndentifier[] cost;
	public Sprite icon;
	public PlacementMode placementMode = PlacementMode.Single;
	public BuildingIdentifier upgrade;

	public ResourceIndentifier[] production;
	public ResourceIndentifier[] consumption;


	/*public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(FirstTickTag)
		});
	}*/

	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = base.Instantiate(pos, scale);
		if (production.Length > 0)
		{
			var pData = new ProductionData
			{
				resourceIds = new int[production.Length],
				rates = new int[production.Length]
			};
			for (int i = 0; i < production.Length; i++)
			{
				var rId = production[i].id;
				pData.resourceIds[i] = rId;
				pData.rates[i] = (int)production[i].ammount;
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
				var rId = consumption[i].id;
				cData.resourceIds[i] = rId;
				cData.rates[i] = (int)consumption[i].ammount;
			}

			Map.EM.AddSharedComponentData(e, cData);
		}
		Map.EM.AddComponent(e, typeof(FirstTickTag));
		return e;
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		if(consumption.Length != 0 || production.Any(p => p.id == 0))
			return new PoweredBuildingTile(pos, height, this);
		else
			return new BuildingTile(pos, height, this);
	}

	public string GetProductionString()
	{
		var costString = "";
		for (int i = 0; i < production.Length; i++)
		{
			costString += $"{ResourceDatabase.GetResourceString(production[i].id)} +{production[i].ammount}/t";
			if (i < production.Length - 1)
				costString += "\n";
		}
		return costString;
	}

	public string GetCostString()
	{
		var costString = "";
		for (int i = 0; i < cost.Length; i++)
		{
			var id = cost[i].id;
			var curCost = $"{ResourceDatabase.GetResourceString(id)} -{cost[i].ammount}";
			if (ResourceSystem.resCount[id] < cost[i].ammount)
				curCost = $"<color=#ff0000>{curCost}</color>";
			costString += curCost;
			if (i != cost.Length - 1)
				costString += "\n";
		}
		if (consumption.Length > 0)
			costString += "\n";
		for (int i = 0; i < consumption.Length; i++)
		{
			costString += $"{ResourceDatabase.GetResourceString(consumption[i].id)} -{consumption[i].ammount}/t";
			if (i < consumption.Length -1)
				costString += "\n";
		}
		return costString;
	}
}
