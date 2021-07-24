using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class BuildingTile : Tile, IDeconstructable
	{
		public readonly BuildingTileEntity buildingInfo;
		public int upgradeLevel = 0;
		public bool IsBuilt => isBuilt;

		public int Rotation => rotationAngle;

		protected StatsBuffs totalBuffs;
		protected bool isBuilt;
		protected bool buidlingRendered;
		protected Dictionary<HexCoords, StatsBuffs> buffSources;
		protected MetaTile[] metaTiles;
		protected bool hasBuilding;
		protected NativeArray<Entity> subMeshes;
		protected int rotationAngle;
		protected quaternion rotation;

		private Entity _offshorePlatform;
		private NativeArray<Entity> _healthBars;
		private NativeArray<Entity> _adjacencyConnectors;
		private int _connectorCount;
		protected ResourceProduction[] _productionData;
		protected ResourceConsumption[] _consumptionData;

		public BuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, int rotation) : this(coords, height, map, tInfo)
		{
			buildingInfo = tInfo;
			rotationAngle = rotation;
			this.rotation = quaternion.RotateY(math.radians(60 * rotation));
			if (tInfo.useMetaTiles)
				metaTiles = new MetaTile[tInfo.footprint.footprint.Length - 1];
		}

		public BuildingTile(HexCoords coords, float height, Map map, TileEntity tInfo) : base(coords, height, map, tInfo)
		{
			totalBuffs = StatsBuffs.Default;
			buffSources = new Dictionary<HexCoords, StatsBuffs>();
		}

		/// <summary>
		/// Gets the Rich Text string representing information about the production of this building
		/// </summary>
		/// <returns>String Builder</returns>
		public StringBuilder GetProductionString()
		{
			if (_productionData == null || _productionData.Length == 0)
				return new StringBuilder();
			//return buildingInfo.GetProductionString(totalBuffs);
			var sb = new StringBuilder().AppendLine("Production"); ;
			for (int i = 0; i < _productionData.Length; i++)
			{
				var res = _productionData[i].resourceId;
				var rate = _productionData[i].rate * totalBuffs.productionMulti;
				if (rate == 0)
					continue;
				sb.Append(ResourceDatabase.GetResourceString(res)).Append(" +").Append(math.floor(rate).ToString()).Append(" (").Append(Math.Round(totalBuffs.productionMulti * 100, 2)).AppendLine("%)");
			}
			if (_consumptionData == null || _consumptionData.Length == 0)
				return sb;
			sb.AppendLine("Consumption");
			for (int i = 0; i < _consumptionData.Length; i++)
			{
				var res = _consumptionData[i].resourceId;
				var rate = _consumptionData[i].rate * totalBuffs.productionMulti;
				if (rate == 0)
					continue;
				sb.Append(ResourceDatabase.GetResourceString(res)).Append(" -").Append(math.floor(rate).ToString()).Append(" (").Append(Math.Round(totalBuffs.consumptionMulti * 100, 2)).AppendLine("%)");
			}
			return sb;
		}

		public virtual Entity InstantiateBuilding(Entity tileInst, DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			var prefabId = GameRegistry.PrefabDatabase[buildingInfo.buildingPrefab];
			var prefab = prefabs[prefabId];
			var buildingInst = postUpdateCommands.Instantiate(prefab.value);
			postUpdateCommands.AddComponent(buildingInst, new Parent { Value = tileInst });
			postUpdateCommands.AddComponent<LocalToParent>(buildingInst);
			postUpdateCommands.SetComponent(buildingInst, new Rotation { Value = rotation });
			postUpdateCommands.AddComponent<Building>(tileInst, buildingInst);
			return buildingInst;
		}

		public virtual void CreateMetaTiles(DynamicBuffer<TileInstance> tileEntities, Entity buildingInstance, DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			if (!buildingInfo.useMetaTiles)
				return;
			var footprint = buildingInfo.footprint.GetOccupiedTiles(Coords, rotationAngle);
			postUpdateCommands.AddComponent<MultiTileTag>(buildingInstance);
			var buffer = postUpdateCommands.AddBuffer<SubTile>(buildingInstance);
			for (int i = 0; i < footprint.Length; i++)
			{
				var c = footprint[i];
				var tile = map[c];
				var metaTile = new MetaTile(c, Height, map, GetGroundTileInfo(), this);
				map.ReplaceTile(tile, metaTile, prefabs, tileEntities, postUpdateCommands);
				buffer.Add(c);
			}
			buffer.Add(Coords);
		}

		/// <summary>
		/// The TileEntity that contains information about the underlying tile
		/// </summary>
		public override TileEntity MeshEntity => buildingInfo.preserveGroundTile ? originalTile : buildingInfo;

		/// <summary>
		/// Gets the rotation of the building on this tile
		/// </summary>
		/// <returns>Quaternion of the building's rotation</returns>
		protected virtual quaternion GetBuildingRotation()
		{
			return rotation;
		}

		/// <summary>
		/// Gets the resources that this building will return when deconstructed
		/// </summary>
		/// <returns>Array of resource Identifiers</returns>
		public ResourceIndentifier[] GetResourceRefund()
		{
			var res = new ResourceIndentifier[buildingInfo.cost.Length];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = buildingInfo.cost[i] * 0.5f;
			}
			return res;
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
			StartConstruction();
		}

		/// <summary>
		/// Start the construction animations for this building
		/// </summary>
		[Obsolete]
		protected virtual void StartConstruction()
		{
		}

		/// <summary>
		/// Completes the build phase of this building
		/// </summary>
		[Obsolete]
		public void Build()
		{
			isBuilt = true;
			OnBuilt();
			//if (isBuilt)
			//	return;
			//isBuilt = true;
			//OnBuilt();
			//RenderBuilding();
			//Start();
			//if(buildingInfo.useMetaTiles)
			//{
			//	for (int i = 0; i < metaTiles.Length; i++)
			//	{
			//		metaTiles[i].Start();
			//	}
			//}
			//map.InvokeOnBuilt(Coords);
		}

		public override void Start(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{
			base.Start(tileInst, postUpdateCommands);
		}

		public virtual void BuildingStart(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
		{
		}

		public Entity Build(Entity tileInst, DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			var buildingInst = InstantiateBuilding(tileInst, prefabs, postUpdateCommands);
			PrepareBuildingEntity(buildingInst, postUpdateCommands);
			return buildingInst;
		}

		/// <summary>
		/// Callback for when this build phase completes
		/// </summary>
		protected virtual void OnBuilt()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.GetNameString()}", Coords);
		}

		public virtual void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			postUpdateCommands.AddComponent(building, new HexPosition { Value = Coords });
			var pData = _productionData = PrepareResourceProduction().Values.ToArray();
			if (pData.Length > 0)
			{
				var productionBuffer = postUpdateCommands.AddBuffer<ResourceProduction>(building);
				for (int i = 0; i < pData.Length; i++)
					productionBuffer.Add(pData[i]);
			}
			var cData = _consumptionData= PrepareResourceConsumtion().Values.ToArray();
			if (cData.Length > 0)
			{
				var productionBuffer = postUpdateCommands.AddBuffer<ResourceConsumption>(building);
				for (int i = 0; i < cData.Length; i++)
					productionBuffer.Add(cData[i]);
			}
			postUpdateCommands.AddComponent(building, new Health
			{
				maxHealth = buildingInfo.maxHealth,
				Value = buildingInfo.maxHealth
			});
			postUpdateCommands.AddComponent(building, new ConsumptionMulti { Value = 1 });
			postUpdateCommands.AddComponent(building, new ProductionMulti { Value = 1 });
			postUpdateCommands.AddComponent(building, typeof(FirstTickTag));
			postUpdateCommands.AddComponent(building, new BuildingId { Value = GameRegistry.BuildingDatabase[buildingInfo] });

			//if (buildingInfo.healthBar != null)
			//	_healthBars = buildingInfo.healthBar.Instantiate(building, buildingInfo.centerOfMassOffset + buildingInfo.healthBarOffset);
		}

		public virtual Dictionary<int, ResourceConsumption> PrepareResourceConsumtion()
		{
			var consumptions = new Dictionary<int, ResourceConsumption>();
			var consumption = buildingInfo.consumption;
			for (int i = 0; i < consumption.Length; i++)
				consumptions.AppendResource(consumption[i]);
			return consumptions;
		}

		public virtual Dictionary<int, ResourceProduction> PrepareResourceProduction()
		{
			var consumptions = new Dictionary<int, ResourceProduction>();
			var production = buildingInfo.production;
			for (int i = 0; i < production.Length; i++)
				consumptions.AppendResource(production[i]);
			return consumptions;
		}

		/// <summary>
		/// Add and apply a buff to this tile
		/// </summary>
		/// <param name="src">The source tile for the buff</param>
		/// <param name="buff">The buff to apply</param>
		public virtual void AddBuff(HexCoords src, StatsBuffs buff)
		{
			Debug.Log("Add Buff Called");
			if (buffSources.ContainsKey(src))
			{
				totalBuffs -= buffSources[src];
				buffSources[src] = buff;
			}
			else
				buffSources.Add(src, buff);
			totalBuffs += buff;
			AddBuffEvent();
		}

		/// <summary>
		/// Remove a previously applied buff from this tile
		/// </summary>
		/// <param name="src">The tile that applied the buff</param>
		public virtual void RemoveBuff(HexCoords src)
		{
			if (buffSources.ContainsKey(src))
			{
				totalBuffs -= buffSources[src];
				buffSources.Remove(src);
				AddBuffEvent();
			}
		}

		private void AddBuffEvent()
		{
			var buffEvents = GameRegistry.EntityManager.GetBuffer<BuffEvent>(GameRegistry.MapEntity);
			buffEvents.Add(new BuffEvent { tile = Coords });
		}

		public virtual void ApplyBuffs(Entity tileEntity, Entity buildingEntity, EntityCommandBuffer postUpdateCommands)
		{
			Debug.Log($"Apply Buffs Called for {GetNameString()}");
			if (!isBuilt || !_isRendered)
				return;

			postUpdateCommands.SetComponent(buildingEntity, new ProductionMulti { Value = totalBuffs.productionMulti + 1 });
			postUpdateCommands.SetComponent(buildingEntity, new ConsumptionMulti { Value = totalBuffs.consumptionMulti + 1 });

			//TODO: Figure out health buffs
			//var curHealth = postUpdateCommands.GetComponentData<Health>(tileEntity);
			//curHealth.maxHealth = buildingInfo.maxHealth + totalBuffs.structureHealth;
			//curHealth.Value += totalBuffs.structureHealth;
			//postUpdateCommands.SetComponentData(tileEntity, curHealth);
		}

		/// <summary>
		/// Deconstruct this building, reverting it to it's original tile
		/// </summary>
		public virtual void Deconstruct(DynamicBuffer<GenericPrefab> prefabs, DynamicBuffer<TileInstance> tileInstances, EntityCommandBuffer postUpdateCommands)
		{
			map.RevertTile(this, prefabs, tileInstances, postUpdateCommands);
			if(buildingInfo.useMetaTiles)
			{
				var tiles = buildingInfo.footprint.GetOccupiedTiles(Coords, rotationAngle);
				for (int i = 0; i < tiles.Length; i++)
					map.RevertTile(map[tiles[i]], prefabs, tileInstances, postUpdateCommands);
			}
		}

		/// <summary>
		/// Can this tile be deconstructed by the provided faction
		/// </summary>
		/// <param name="faction">The faction attempting to deconstruct this tile</param>
		/// <returns></returns>
		public virtual bool CanDeconstruct(Faction faction) => buildingInfo.faction == faction;

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			if (IsBuilt)
				tileData.Add("isBuilt", null);
			base.OnDeSerialized(tileData);
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			if (tileData.ContainsKey("isBuilt"))
				isBuilt = true;
			base.OnDeSerialized(tileData);
		}

		/// <summary>
		/// Kills this bulding, causing it to be destroyed
		/// </summary>
		public void Die()
		{
			OnDeath();
		}

		/// <summary>
		/// Callback for when this building dies
		/// </summary>
		public virtual void OnDeath()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Warning, $"Building Destroyed: {buildingInfo.GetNameString()}", Coords);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (!_isRendered)
				return;
			if (_adjacencyConnectors.IsCreated)
				_adjacencyConnectors.Dispose();
		}
	}
}