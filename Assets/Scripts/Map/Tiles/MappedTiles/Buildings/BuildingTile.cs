using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;

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
		protected readonly int rotationAngle;
		protected readonly quaternion rotation;

		private Entity _building;
		private Entity _offshorePlatform;
		private NativeArray<Entity> _healthBars;
		private NativeArray<Entity> _adjacencyConnectors;
		private int _connectorCount;

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

		public override TileEntity GetMeshEntity()
		{
			return buildingInfo.preserveGroundTile ? originalTile : buildingInfo;
		}

		protected virtual quaternion GetBuildingRotation()
		{
			return rotation;
		}

		public virtual void SetBuildingRotation(int rotation)
		{
		}

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
		}

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
			CreateMetaTiles();
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

		public void Build()
		{
			if (isBuilt)
				return;
			isBuilt = true;
			OnBuilt();
			RenderBuilding();
			map.InvokeOnBuilt(Coords);
		}

		protected virtual void OnBuilt()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.GetNameString()}", Coords);
		}

		public virtual void RenderBuilding()
		{
			if (buildingInfo.buildingMesh.mesh == null)
			{
				Debug.LogWarning($"No Building Assigned for {GetName()}");
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
			ApplyTileProperites();
			ApplyBonuses();
			ApplyBuffs();
			RenderDecorators();
		}

		public virtual void RenderSubMeshes(quaternion rot)
		{
			subMeshes = buildingInfo.buildingMesh.InstantiateSubMeshes(SurfacePoint, rot);
		}

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
					metaTiles[j++] = map.ReplaceTile(tgtTile, new MetaTile(tiles[i], tgtTile.Height, map, tgtTile.originalTile, this));
				}
			}
		}

		protected virtual Entity GetBuildingEntity()
		{
			return buildingInfo.buildingMesh.mesh != null ? _building : _tileEntity;
		}

		protected virtual void ApplyTileProperites()
		{
			var entity = GetBuildingEntity();
			Map.EM.SetComponentData(entity, new HexPosition { Value = Coords });
			var production = buildingInfo.production;
			var consumption = buildingInfo.consumption;
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

				Map.EM.AddSharedComponentData(entity, pData);
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

				Map.EM.AddSharedComponentData(entity, cData);
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
				ApplyBonuses();
		}

		protected virtual void ApplyBonuses()
		{
			if (!IsBuilt || !_isRendered)
				return;
			var entity = GetBuildingEntity();
			Map.EM.AddComponentData(entity, new ConsumptionMulti { Value = 1 });
			Map.EM.AddComponentData(entity, new ProductionMulti { Value = 1 });
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

		public virtual void RemoveBuff(HexCoords src)
		{
			if (buffSources.ContainsKey(src))
			{
				totalBuffs -= buffSources[src];
				buffSources.Remove(src);
				ApplyBuffs();
			}
		}

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

		public virtual void Deconstruct()
		{
			if (buildingInfo.useMetaTiles)
			{
				for (int i = 0; i < metaTiles.Length; i++)
					map.RevertTile(metaTiles[i]);
			}
			map.RevertTile(this);
		}

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
			Debug.Log(isBuilt);
			base.OnDeSerialized(tileData);
		}

		public void Die()
		{
			OnDeath();
			map.ReplaceTile(this, buildingInfo.customDeathTile ? buildingInfo.deathTile : originalTile);
		}

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