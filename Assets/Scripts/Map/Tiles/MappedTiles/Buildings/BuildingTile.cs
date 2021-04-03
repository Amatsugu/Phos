using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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

		private Entity _building;
		private Entity _offshorePlatform;
		private NativeArray<Entity> _healthBars;
		private NativeArray<Entity> _adjacencyConnectors;
		private int _connectorCount;
		protected ProductionData _productionData;
		protected ConsumptionData _consumptionData;

		public BuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, int rotation) : base(coords, height, map, tInfo)
		{
			buildingInfo = tInfo;
			totalBuffs = StatsBuffs.Default;
			buffSources = new Dictionary<HexCoords, StatsBuffs>();
			rotationAngle = rotation;
			this.rotation = quaternion.RotateY(math.radians(60 * rotation));
			if (tInfo.useMetaTiles)
				metaTiles = new MetaTile[tInfo.footprint.footprint.Length - 1];
		}

		/// <summary>
		/// Gets the Rich Text string representing information about the production of this building
		/// </summary>
		/// <returns>String Builder</returns>
		public StringBuilder GetProductionString()
		{
			if (_productionData.resourceIds == null)
				return new StringBuilder();
			//return buildingInfo.GetProductionString(totalBuffs);
			var sb = new StringBuilder().AppendLine("Production"); ;
			for (int i = 0; i < _productionData.resourceIds.Length; i++)
			{
				var res = _productionData.resourceIds[i];
				var rate = _productionData.rates[i] * totalBuffs.productionMulti;
				if (rate == 0)
					continue;
				sb.Append(ResourceDatabase.GetResourceString(res)).Append(" +").Append(math.floor(rate).ToString()).Append(" (").Append(Math.Round(totalBuffs.productionMulti * 100, 2)).AppendLine("%)");
			}
			if (_consumptionData.resourceIds == null)
				return sb;
			sb.AppendLine("Consumption");
			for (int i = 0; i < _consumptionData.resourceIds.Length; i++)
			{
				var res = _consumptionData.resourceIds[i];
				var rate = _consumptionData.rates[i] * totalBuffs.productionMulti;
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


		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
			StartConstruction();
			CreateMetaTiles();
		}

		/// <summary>
		/// Start the construction animations for this building
		/// </summary>
		[Obsolete]
		protected virtual void StartConstruction()
		{
			return;
			if (buildingInfo.constructionMesh != null)
			{
				var pos = SurfacePoint;
				var height = buildingInfo.buildingMesh.height;
				if (buildingInfo.isOffshore && IsUnderwater && buildingInfo.offshorePlatformMesh != null)
				{
					buildingInfo.constructionMesh.Instantiate(pos, GetBuildingRotation(), height, buildingInfo.constructionTime, buildingInfo.offshorePlatformMesh);
				}
				buildingInfo.constructionMesh.Instantiate(SurfacePoint, GetBuildingRotation(), buildingInfo.buildingMesh.height, buildingInfo.buildingMesh, buildingInfo.constructionTime);
			}
		}


		/// <summary>
		/// Completes the build phase of this building
		/// </summary>
		[Obsolete]
		public void Build()
		{
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
			var building = GameRegistry.EntityManager.GetComponentData<Building>(tileInst);
			BuildingStart(building, postUpdateCommands);
		}

		protected virtual void BuildingStart(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
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
			var production = buildingInfo.production;
			var consumption = buildingInfo.consumption;
			if (production.Length > 0)
			{
				_productionData = new ProductionData
				{
					resourceIds = new int[production.Length],
					rates = new int[production.Length]
				};
				for (int i = 0; i < production.Length; i++)
				{
					var rId = production[i].id;
					_productionData.resourceIds[i] = rId;
					_productionData.rates[i] = (int)production[i].ammount;
				}

				postUpdateCommands.AddSharedComponent(building, _productionData);
			}
			if (consumption.Length > 0)
			{
				_consumptionData = new ConsumptionData
				{
					resourceIds = new int[consumption.Length],
					rates = new int[consumption.Length]
				};
				for (int i = 0; i < consumption.Length; i++)
				{
					var rId = consumption[i].id;
					_consumptionData.resourceIds[i] = rId;
					_consumptionData.rates[i] = (int)consumption[i].ammount;
				}

				postUpdateCommands.AddSharedComponent(building, _consumptionData);
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

		public override void Start()
		{
			base.Start();
			if (!isBuilt)
				return;
			ApplyAdjacencyBonuses();
			ApplyBuffs();
		}

		/// <summary>
		/// Replace tiles in this building's footprint with meta tiles
		/// </summary>
		[Obsolete]
		public virtual void CreateMetaTiles()
		{
			return;
			if (buildingInfo.useMetaTiles)
			{
				var tiles = buildingInfo.footprint.GetOccupiedTiles(Coords, rotationAngle);
				for (int i = 0, j = 0; i < tiles.Length; i++)
				{
					if (tiles[i] == Coords)
						continue;
					var tgtTile = map[tiles[i]];
					var mt = map.ReplaceTile(tgtTile, new MetaTile(tiles[i], tgtTile.Height, map, tgtTile.originalTile, this));
					metaTiles[j++] = mt;
				}
			}
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
			if (!IsBuilt)
				return;
			if (updateType == TileUpdateType.Placed || updateType == TileUpdateType.Removed)
				ApplyAdjacencyBonuses();
		}

		/// <summary>
		/// Applies adjanceny bonuses to neighboring tiles
		/// </summary>
		[Obsolete]
		protected virtual void ApplyAdjacencyBonuses()
		{
			return;
			if (!IsBuilt || !_isRendered)
				return;
			var neighbors = map.GetNeighbors(Coords);
			if (!_adjacencyConnectors.IsCreated)
				_adjacencyConnectors = new NativeArray<Entity>(6 * 3, Allocator.Persistent);
			if (_connectorCount > 0)
			{
				Map.EM.DestroyEntity(_adjacencyConnectors);
			}
			for (int i = 0; i < buildingInfo.adjacencyEffects.Length; i++)
			{
				var effect = buildingInfo.adjacencyEffects[i];
				for (int n = 0; n < neighbors.Length; n++)
				{
					if (effect.ApplyBonus(this, neighbors[n]))
					{
						var slice = _adjacencyConnectors.Slice(n * 3, 3);
						effect.RenderConnectionLine(SurfacePoint, neighbors[n].SurfacePoint, ref slice);
						_connectorCount += 3;
					}
				}
			}
		}

		/// <summary>
		/// Add and apply a buff to this tile
		/// </summary>
		/// <param name="src">The source tile for the buff</param>
		/// <param name="buff">The buff to apply</param>
		public virtual void AddBuff(HexCoords src, StatsBuffs buff)
		{
			if (buffSources.ContainsKey(src))
			{
				totalBuffs -= buffSources[src];
				buffSources[src] = buff;
			}
			else
				buffSources.Add(src, buff);
			totalBuffs += buff;
			ApplyBuffs();
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
				ApplyBuffs();
			}
		}

		/// <summary>
		/// Applies the current set of buffs to this tile
		/// </summary>
		[Obsolete]
		protected virtual void ApplyBuffs()
		{
			if (!isBuilt || !_isRendered)
				return;
			//var e = GetBuildingEntity();
			////Production
			//if (!GameRegistry.EntityManager.HasComponent<ProductionMulti>(e))
			//	GameRegistry.EntityManager.AddComponentData(e, new ProductionMulti { Value = totalBuffs.productionMulti + 1 });
			//else
			//	GameRegistry.EntityManager.SetComponentData(e, new ProductionMulti { Value = totalBuffs.productionMulti + 1});
			////Consumption
			//if (!GameRegistry.EntityManager.HasComponent<ConsumptionMulti>(e))
			//	GameRegistry.EntityManager.AddComponentData(e, new ConsumptionMulti { Value = totalBuffs.consumptionMulti + 1 });
			//else
			//	GameRegistry.EntityManager.SetComponentData(e, new ConsumptionMulti { Value = totalBuffs.consumptionMulti + 1 });
			////Health
			//var curHealth = GameRegistry.EntityManager.GetComponentData<Health>(e);
			//curHealth.maxHealth = buildingInfo.maxHealth + totalBuffs.structureHealth;
			//curHealth.Value += totalBuffs.structureHealth;
			//GameRegistry.EntityManager.SetComponentData(e, curHealth);
		}

		/// <summary>
		/// Deconstruct this building, reverting it to it's original tile
		/// </summary>
		public virtual void Deconstruct()
		{
			//if (buildingInfo.useMetaTiles)
			//{
			//	for (int i = 0; i < metaTiles.Length; i++)
			//		map.RevertTile(metaTiles[i]);
			//}
			//map.RevertTile(this);
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