using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum PlacementMode
{
	Single,
	Path
}

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Building")]
	[Serializable]
	public class BuildingTileEntity : TileEntity
	{
		[Header("Rendering")]
		public GameObject buildingPrefab;
		public float3 centerOfMassOffset;
		public ConstructionMeshEntity constructionMesh;
		public bool preserveGroundTile;
		public bool customDeathTile;
		[ConditionalHide("customDeathTile")]
		public TileEntity deathTile;

		[Header("Stats")]
		[Range(1, 6)]
		public int tier = 1;
		public float constructionTime = 2;
		public BuildingCategory category;
		public StructureFootprint footprint;
		public bool useMetaTiles;
		public int flattenOuterRange = 0;

		[Header("Offshore")]
		public bool isOffshore;
		[ConditionalHide("isOffshore")]
		public bool offshoreOnly;
		[ConditionalHide("isOffshore")]
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes", typeof(MeshEntityRotatable))]
		public MeshEntityRotatable offshorePlatformMesh;

		[Header("Building Info")]
		public Sprite icon;
		public Faction faction;
		[Header("Health")]
		public float maxHealth = 100;
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes/UI/HealthBar", typeof(HealthBarDefination))]
		public HealthBarDefination healthBar;
		public float3 healthBarOffset;

		[Header("Validator")]
		public PlacementValidator validator;

		[Header("Resources")]
		[SerializeField]
		public ResourceIndentifier[] cost;
		public ResourceIndentifier[] production;
		public ResourceIndentifier[] consumption;

		[Header("Adjacency Bonuses")]
		public AdjacencyEffect[] adjacencyEffects;


		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public virtual BuildingTile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			if (consumption.Length != 0 || production.Any(p => p.id == 0))
				return new PoweredBuildingTile(pos, height, map, this, rotation);
			else
				return new BuildingTile(pos, height, map, this, rotation);
		}

		public virtual StringBuilder GetProductionString()
		{
			var prodString = new StringBuilder();
			for (int i = 0; i < production.Length; i++)
			{
				prodString.Append($"+{production[i].ammount}{ResourceDatabase.GetResourceString(production[i].id)}/t");
				if (i < production.Length - 1)
					prodString.Append(" ");
			}
			return prodString;
		}

		public virtual StringBuilder GetProductionString(StatsBuffs buffs)
		{
			if (buffs.productionMulti == 0)
				return GetProductionString();
			var prodString = new StringBuilder();
			for (int i = 0; i < production.Length; i++)
			{
				prodString.Append($"+{Mathf.Round(production[i].ammount * buffs.productionMulti)}{ResourceDatabase.GetResourceString(production[i].id)}/t")
					.Append($"({MathUtils.Round(buffs.productionMulti * 100, 2).ToNumberString()}) ");
				if (i < production.Length - 1)
					prodString.Append(" ");
			}
			return prodString;
		}

		public virtual StringBuilder GetCostString()
		{
			var costString = new StringBuilder();
			for (int i = 0; i < cost.Length; i++)
			{
				var id = cost[i].id;
				var curCost = $"\u2011{cost[i].ammount}{ResourceDatabase.GetResourceString(id)}";
				if (ResourceSystem.resCount[id] < cost[i].ammount)
					curCost = $"<color=#ff0000>{curCost}</color>";
				costString.Append(curCost);
				if (i != cost.Length - 1)
					costString.Append(" ");
			}
			return costString;
		}

		public virtual StringBuilder GetUpkeepString()
		{
			var upkeepString = new StringBuilder();
			for (int i = 0; i < consumption.Length; i++)
			{
				upkeepString.Append($"\u2011{consumption[i].ammount}{ResourceDatabase.GetResourceString(consumption[i].id)}/t");
				if (i < consumption.Length - 1)
					upkeepString.Append(" ");
			}
			return upkeepString;
		}

		public virtual StringBuilder GetUpkeepString(StatsBuffs buffs)
		{
			if (buffs.consumptionMulti == 0)
				return GetUpkeepString();
			var upkeepString = new StringBuilder();
			for (int i = 0; i < consumption.Length; i++)
			{
				upkeepString.Append($"\u2011{Mathf.Round(consumption[i].ammount * buffs.consumptionMulti)}{ResourceDatabase.GetResourceString(consumption[i].id)}/t")
					.Append($"({MathUtils.Round(buffs.consumptionMulti * 100, 2).ToNumberString()}) ");
				if (i < consumption.Length - 1)
					upkeepString.Append(" ");
			}
			return upkeepString;
		}

		public override StringBuilder GetNameString()
		{
			return GameRegistry.RarityColors.Colorize(name, tier);
		}
	}
}