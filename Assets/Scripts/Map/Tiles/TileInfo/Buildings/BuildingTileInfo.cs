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
	public MeshEntityRotatable constructionMesh;
	public float constructionTime = 2;
	[Range(1, 6)]
	public int tier = 1;
	public BuildingCategory category;
	public int size = 0;
	public int flattenOuterRange = 0;
	public int resourceTransferRange = 0;
	[SerializeField]
	public ResourceIndentifier[] cost;
	public Sprite icon;
	public PlacementMode placementMode = PlacementMode.Single;
	public BuildingIdentifier upgradeTarget;

	public ResourceIndentifier[] production;
	public ResourceIndentifier[] consumption;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] { typeof(BuildingOffTag), typeof(BuildingId) });
	}

	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = base.Instantiate(pos, scale);
		Map.EM.SetComponentData(e, new BuildingId
		{
			Value = GameRegistry.BuildingDatabase.GetId(this)
		});
		return e;
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		if(consumption.Length != 0 || production.Any(p => p.id == 0))
			return new PoweredBuildingTile(pos, height, this);
		else
			return new BuildingTile(pos, height, this);
	}

	public virtual string GetProductionString()
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

	public virtual string GetCostString()
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
