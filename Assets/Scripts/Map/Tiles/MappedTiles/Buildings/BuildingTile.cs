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
			if (buildingInfo.buildingMesh != null)
			{
				Map.EM.SetComponentData(_building, new Translation { Value = SurfacePoint });
				for (int i = 0; i < subMeshes.Length; i++)
				{
					var h = Map.EM.GetComponentData<Translation>(subMeshes[i]);
					h.Value = new float3(h.Value.x, SurfacePoint.y, h.Value.z);
					Map.EM.SetComponentData(subMeshes[i], h);
				}
			}
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
		protected virtual void StartConstruction()
		{
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

		public override Entity Render()
		{
			var e = base.Render();
			if (isBuilt)
			{
				//isBuilt = false;
				RenderBuilding();
			}
			return e;
		}

		public override void OnHide()
		{
			base.OnHide();
			if (hasBuilding)
				Map.EM.AddComponent(_building, typeof(DisableRendering));
			if (_healthBars.IsCreated)
				Map.EM.AddComponent<DisableRendering>(_healthBars);
			if (_connectorCount > 0)
			{
				for (int i = 0; i < _adjacencyConnectors.Length; i++)
				{
					if (Map.EM.Exists(_adjacencyConnectors[i]))
						Map.EM.AddComponent<DisableRendering>(_adjacencyConnectors[i]);
				}
			}
			Map.EM.AddComponent(subMeshes, typeof(DisableRendering));
		}

		public override void OnShow()
		{
			base.OnShow();
			if (hasBuilding)
				Map.EM.RemoveComponent(_building, typeof(DisableRendering));
			if (_healthBars.IsCreated)
				Map.EM.RemoveComponent<DisableRendering>(_healthBars);
			if (_connectorCount > 0)
			{
				for (int i = 0; i < _adjacencyConnectors.Length; i++)
				{
					if (Map.EM.Exists(_adjacencyConnectors[i]))
						Map.EM.RemoveComponent<DisableRendering>(_adjacencyConnectors);
				}
			}
			Map.EM.RemoveComponent(subMeshes, typeof(DisableRendering));
		}

		/// <summary>
		/// Completes the build phase of this building
		/// </summary>
		public void Build()
		{
			if (isBuilt)
				return;
			isBuilt = true;
			OnBuilt();
			RenderBuilding();
			Start();
			if(buildingInfo.useMetaTiles)
			{
				for (int i = 0; i < metaTiles.Length; i++)
				{
					metaTiles[i].Start();
				}
			}
			map.InvokeOnBuilt(Coords);
		}

		/// <summary>
		/// Callback for when this build phase completes
		/// </summary>
		protected virtual void OnBuilt()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.GetNameString()}", Coords);
		}

		/// <summary>
		/// Instantiates the building entities for this tile
		/// </summary>
		public virtual void RenderBuilding()
		{
			if (buildingInfo.buildingMesh.mesh == null)
			{
				Debug.LogWarning($"No Building Assigned for {GetNameString()}");
			}
			else
			{
				hasBuilding = true;
				var rot = GetBuildingRotation();
				_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint, rot, GameRegistry.TileDatabase.entityIds[buildingInfo], buildingInfo.maxHealth, buildingInfo.faction);
				RenderSubMeshes(rot);
			}

			if (buildingInfo.isOffshore && IsUnderwater && buildingInfo.offshorePlatformMesh != null)
				_offshorePlatform = buildingInfo.offshorePlatformMesh.Instantiate(SurfacePoint);
			
			RenderDecorators();
		}

		public override void Start()
		{
			base.Start();
			if (!isBuilt)
				return;
			ApplyTileProperites();
			ApplyAdjacencyBonuses();
			ApplyBuffs();
		}

		/// <summary>
		/// Instantiate the submesses for this building tile
		/// </summary>
		/// <param name="rot">The rotation of the building</param>
		public virtual void RenderSubMeshes(quaternion rot)
		{
			subMeshes = buildingInfo.buildingMesh.InstantiateSubMeshes(rot, _building);
		}

		/// <summary>
		/// Replace tiles in this building's footprint with meta tiles
		/// </summary>
		public virtual void CreateMetaTiles()
		{
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

		/// <summary>
		/// Gets the DOTS Entity for the building on this tile
		/// </summary>
		/// <returns>Entity</returns>
		protected virtual Entity GetBuildingEntity()
		{
			return buildingInfo.buildingMesh.mesh != null ? _building : _tileEntity;
		}

		/// <summary>
		/// Applies component data to entities on this building
		/// </summary>
		protected virtual void ApplyTileProperites()
		{
			var entity = GetBuildingEntity();
			Map.EM.SetComponentData(entity, new HexPosition { Value = Coords });
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

				Map.EM.AddSharedComponentData(entity, _productionData);
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

				Map.EM.AddSharedComponentData(entity, _consumptionData);
			}
			Map.EM.SetComponentData(entity, new Health
			{
				maxHealth = buildingInfo.maxHealth,
				Value = buildingInfo.maxHealth
			});
			Map.EM.AddComponentData(entity, new ConsumptionMulti { Value = 1 });
			Map.EM.AddComponentData(entity, new ProductionMulti { Value = 1 });
			Map.EM.AddComponent(entity, typeof(FirstTickTag));
			if (buildingInfo.healthBar != null)
				_healthBars = buildingInfo.healthBar.Instantiate(entity, buildingInfo.centerOfMassOffset + buildingInfo.healthBarOffset);
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
		protected virtual void ApplyAdjacencyBonuses()
		{
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
		protected virtual void ApplyBuffs()
		{
			if (!isBuilt || !_isRendered)
				return;
			var e = GetBuildingEntity();
			//Production
			if (!Map.EM.HasComponent<ProductionMulti>(e))
				Map.EM.AddComponentData(e, new ProductionMulti { Value = totalBuffs.productionMulti + 1 });
			else
				Map.EM.SetComponentData(e, new ProductionMulti { Value = totalBuffs.productionMulti + 1});
			//Consumption
			if (!Map.EM.HasComponent<ConsumptionMulti>(e))
				Map.EM.AddComponentData(e, new ConsumptionMulti { Value = totalBuffs.consumptionMulti + 1 });
			else
				Map.EM.SetComponentData(e, new ConsumptionMulti { Value = totalBuffs.consumptionMulti + 1 });
			//Health
			var curHealth = Map.EM.GetComponentData<Health>(e);
			curHealth.maxHealth = buildingInfo.maxHealth + totalBuffs.structureHealth;
			curHealth.Value += totalBuffs.structureHealth;
			Map.EM.SetComponentData(e, curHealth);
		}

		/// <summary>
		/// Deconstruct this building, reverting it to it's original tile
		/// </summary>
		public virtual void Deconstruct()
		{
			if (buildingInfo.useMetaTiles)
			{
				for (int i = 0; i < metaTiles.Length; i++)
					map.RevertTile(metaTiles[i]);
			}
			map.RevertTile(this);
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
			map.ReplaceTile(this, buildingInfo.customDeathTile ? buildingInfo.deathTile : originalTile);
		}

		/// <summary>
		/// Callback for when this building dies
		/// </summary>
		public virtual void OnDeath()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Warning, $"Building Destroyed: {buildingInfo.GetNameString()}", Coords);
		}

		public override void Destroy()
		{
			base.Destroy();
			if (!_isRendered)
				return;
			DestroyBuilding();
		}

		/// <summary>
		/// Destorys the entities associated with this building and frees memory
		/// </summary>
		protected virtual void DestroyBuilding()
		{
			if (World.DefaultGameObjectInjectionWorld != null)
			{
				if (buildingInfo.buildingMesh != null)
					Map.EM.DestroyEntity(_building);
				if (buildingInfo.isOffshore && buildingInfo.offshorePlatformMesh != null)
					Map.EM.DestroyEntity(_offshorePlatform);
				if (_healthBars.IsCreated)
					Map.EM.DestroyEntity(_healthBars);
				if (_connectorCount > 0)
					Map.EM.DestroyEntity(_adjacencyConnectors);
				Map.EM.DestroyEntity(subMeshes);
			}

			if (_healthBars.IsCreated)
				_healthBars.Dispose();
			if (_adjacencyConnectors.IsCreated)
				_adjacencyConnectors.Dispose();
			if (subMeshes.IsCreated)
				subMeshes.Dispose();
		}
	}
}