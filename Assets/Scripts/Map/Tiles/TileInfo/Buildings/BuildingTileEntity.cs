using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum PlacementMode
{
	Single,
	Path
}

[CreateAssetMenu(menuName = "Map Asset/Tile/Building")]
public class BuildingTileEntity : TileEntity
{
	[Header("Mesh")]
	[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
	public MeshEntityRotatable buildingMesh;
	public float3 centerOfMassOffset;
	public MeshEntityRotatable constructionMesh;

	[Header("Stats")]
	[Range(1, 6)]
	public int tier = 1;

	public float constructionTime = 2;
	public BuildingCategory category;
	public int size = 0;
	public int flattenOuterRange = 0;

	[Header("Offshore")]
	public bool isOffshore;
	[ConditionalHide("isOffshore")]
	public bool offshoreOnly;
	[ConditionalHide("isOffshore")]
	[CreateNewAsset("Assets/GameData/MapAssets/Meshes", typeof(MeshEntityRotatable))]
	public MeshEntityRotatable offshorePlatformMesh;

	[Header("Building Info")]
	public PlacementMode placementMode = PlacementMode.Single;
	public float maxHealth;

	public BuildingIdentifier upgradeTarget;
	public Sprite icon;
	public bool preserveGroundTile;

	[Header("Resources")]
	[SerializeField]
	public ResourceIndentifier[] cost;

	public ResourceIndentifier[] production;
	public ResourceIndentifier[] consumption;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] 
		{
			typeof(BuildingOffTag), 
			typeof(BuildingId),
			typeof(Health),
			typeof(CenterOfMassOffset),
			typeof(CenterOfMass),
		});
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		Map.EM.SetComponentData(entity, new Health
		{
			maxHealth = maxHealth,
			Value = maxHealth
		});
		Map.EM.SetComponentData(entity, new CenterOfMassOffset
		{
			Value = centerOfMassOffset
		});
	}

	public override Entity Instantiate(HexCoords pos, float height)
	{
		var e = base.Instantiate(pos, height);
		var p = new float3(pos.worldX, height, pos.worldZ);
		Map.EM.SetComponentData(e, new CenterOfMass { Value = p + centerOfMassOffset });
		return e;
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		if (consumption.Length != 0 || production.Any(p => p.id == 0))
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
			if (i < consumption.Length - 1)
				costString += "\n";
		}
		return costString;
	}
}