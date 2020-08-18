using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Profiling;

namespace Amatsugu.Phos.Tiles
{
	public class BuildingTile : Tile, IDeconstructable
	{
		public readonly BuildingTileEntity buildingInfo;
		public int upgradeLevel = 0;
		public bool IsBuilt => isBuilt;

		protected StatsBuffs buffs;
		protected bool isBuilt;
		protected bool buidlingRendered;

		private Entity _building;
		private Entity _offshorePlatform;
		private NativeArray<Entity> _healthBars;
		private NativeArray<Entity> _adjacencyConnectors;
		private int _connectorCount;
		private NativeArray<Entity> _subMeshes;

		public BuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			buildingInfo = tInfo;
			buffs = StatsBuffs.Default;
			

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

		public override TileEntity GetMeshEntity()
		{
			return buildingInfo.preserveGroundTile ? originalTile : buildingInfo;
		}

		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
			if (buildingInfo.buildingMesh != null)
			{
				Map.EM.SetComponentData(_building, new Translation { Value = SurfacePoint });
				for (int i = 0; i < _subMeshes.Length; i++)
				{
					var h = Map.EM.GetComponentData<Translation>(_subMeshes[i]);
					h.Value = new float3(h.Value.x, SurfacePoint.y, h.Value.z);
					Map.EM.SetComponentData(_subMeshes[i], h);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (!_isRendered)
				return;
			try
			{
				if (buildingInfo.buildingMesh != null)
					Map.EM.DestroyEntity(_building);
				if (buildingInfo.isOffshore && buildingInfo.offshorePlatformMesh != null)
					Map.EM.DestroyEntity(_offshorePlatform);
				if (_healthBars.IsCreated)
				{
					Map.EM.DestroyEntity(_healthBars);
					_healthBars.Dispose();
				}
				if (_connectorCount > 0)
					Map.EM.DestroyEntity(_adjacencyConnectors);
				Map.EM.DestroyEntity(_subMeshes);
			}
			catch
			{
			}
		}

		public override void OnHide()
		{
			base.OnHide();
			if (buildingInfo.buildingMesh.mesh != null)
				Map.EM.AddComponent(_building, typeof(FrozenRenderSceneTag));
			if (_healthBars.IsCreated)
				Map.EM.AddComponent<FrozenRenderSceneTag>(_healthBars);
			if (_connectorCount > 0)
			{
				for (int i = 0; i < _adjacencyConnectors.Length; i++)
				{
					if (Map.EM.Exists(_adjacencyConnectors[i]))
						Map.EM.AddComponent<FrozenRenderSceneTag>(_adjacencyConnectors);
				}
			}
			Map.EM.AddComponent(_subMeshes, typeof(FrozenRenderSceneTag));
		}

		public override void OnShow()
		{
			base.OnShow();
			if (buildingInfo.buildingMesh.mesh != null)
				Map.EM.RemoveComponent(_building, typeof(FrozenRenderSceneTag));
			if (_healthBars.IsCreated)
				Map.EM.RemoveComponent<FrozenRenderSceneTag>(_healthBars);
			if (_connectorCount > 0)
			{
				for (int i = 0; i < _adjacencyConnectors.Length; i++)
				{
					if (Map.EM.Exists(_adjacencyConnectors[i]))
						Map.EM.RemoveComponent<FrozenRenderSceneTag>(_adjacencyConnectors);
				}
			}
			Map.EM.RemoveComponent(_subMeshes, typeof(FrozenRenderSceneTag));
		}

		protected virtual quaternion GetBuildingRotation()
		{
			var rand = new Unity.Mathematics.Random((uint)Coords.GetHashCode());
			var r = Coords.GetHashCode() % 360;//rand.NextFloat(360);
			return quaternion.RotateY(math.radians(r));
		}

		public void Build()
		{
			if (isBuilt)
				return;
			isBuilt = true;
			if (buildingInfo.constructionMesh != null)
				Map.EM.DestroyEntity(_building);
			OnBuilt();
			RenderBuilding();
			map.InvokeOnBuilt(Coords);
		}

		public virtual void RenderBuilding()
		{
			if (buildingInfo.buildingMesh.mesh == null)
				UnityEngine.Debug.LogWarning($"No Building Assigned for {base.GetName()}");
			else
			{
				var rot = GetBuildingRotation();
				_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint, rot, GameRegistry.TileDatabase.entityIds[buildingInfo], buildingInfo.maxHealth, buildingInfo.faction);
				_subMeshes = buildingInfo.buildingMesh.InstantiateSubMeshes(SurfacePoint, rot);
			}

			if (buildingInfo.isOffshore && IsUnderwater && buildingInfo.offshorePlatformMesh != null)
				_offshorePlatform = buildingInfo.offshorePlatformMesh.Instantiate(SurfacePoint);
			ApplyTileProperites();
			ApplyBonuses();
			ApplyBuffs();
			RenderDecorators();
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

		public void Die()
		{
			OnDeath();
			map.ReplaceTile(this, buildingInfo.customDeathTile ? buildingInfo.deathTile : originalTile);
		}

		public virtual void OnDeath()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Warning, $"Building Destroyed: {buildingInfo.GetNameString()}", Coords);
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
			StartConstruction();
		}

		protected virtual void StartConstruction()
		{
			if (buildingInfo.constructionMesh != null)
				_building = buildingInfo.constructionMesh.Instantiate(SurfacePoint);
		}

		protected virtual void OnBuilt()
		{
			NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.GetNameString()}", Coords);
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
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
					if (effect.AddBonus(this, neighbors[n]))
					{
						var slice = _adjacencyConnectors.Slice(n * 3, 3);
						effect.RenderConnectionLine(SurfacePoint, neighbors[n].SurfacePoint, ref slice);
						_connectorCount += 3;
					}
				}
			}
		}

		public virtual void AddProductionMulti(float ammount)
		{
			buffs.productionMulti += ammount;
			ApplyBuffs();
		}

		public virtual void AddConsumptionMulti(float ammount)
		{
			buffs.consumptionMulti += ammount;
			ApplyBuffs();
		}

		public virtual void AddBuff(StatsBuffs stats)
		{
			buffs += stats;
			ApplyBuffs();
		}

		public virtual void RemoveBuff(StatsBuffs stats)
		{
			buffs -= stats;
			ApplyBuffs();
		}

		protected virtual void ApplyBuffs()
		{
			if (!isBuilt || !_isRendered)
				return;
			var e = GetBuildingEntity();
			//Production
			if (!Map.EM.HasComponent<ProductionMulti>(e))
				Map.EM.AddComponentData(e, new ProductionMulti { Value = buffs.productionMulti });
			else
				Map.EM.SetComponentData(e, new ProductionMulti { Value = buffs.productionMulti });
			//Consumption
			if(!Map.EM.HasComponent<ConsumptionMulti>(e))
				Map.EM.AddComponentData(e, new ConsumptionMulti { Value = buffs.consumptionMulti });
			else
				Map.EM.SetComponentData(e, new ConsumptionMulti { Value = buffs.consumptionMulti });
			//Health
			var curHealth = Map.EM.GetComponentData<Health>(e);
			curHealth.maxHealth = buildingInfo.maxHealth + buffs.structureHealth;
			curHealth.Value += buffs.structureHealth;
			Map.EM.SetComponentData(e, curHealth);
		}

		public void Deconstruct()
		{
			map.RevertTile(this);
			Debug.Log("Desconstruct");
		}

		public virtual bool CanDeconstruct(Faction faction) => buildingInfo.faction == faction;

		public ResourceIndentifier[] GetResourceRefund()
		{
			var res = new ResourceIndentifier[buildingInfo.cost.Length];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = buildingInfo.cost[i] * 0.5f;
			}
			return res;
		}

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

	}

	public class PoweredBuildingTile : BuildingTile
	{
		public bool HasHQConnection { get; protected set; }

		protected bool connectionInit;
		protected PoweredMetaTile[] metaTiles;

		private int _connectionNotif = -1;

		public PoweredBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			if (tInfo.useMetaTiles)
				metaTiles = new PoweredMetaTile[tInfo.footprint.footprint.Length - 1];
		}

		public override string GetDescription()
		{
			return base.GetDescription() + "\n" +
				$"Has HQ Connection: {HasHQConnection} {Map.EM.HasComponent<ConsumptionMulti>(_tileEntity)}";
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
		}

		protected virtual void OnBuiltAndPowered()
		{

		}

		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			FindConduitConnections();
		}

		public virtual void FindConduitConnections()
		{
			Profiler.BeginSample("Find Conduit Connections");
			var closestConduit = map.conduitGraph.GetClosestConduitNode(Coords);
			if (closestConduit == null)
			{
				if (MetaTilesHasConnection())
					HQConnected();
				else
					HQDisconnected();
			}
			else
			{
				
				var conduit = (map[closestConduit.conduitPos] as ResourceConduitTile);
				if (!conduit.HasHQConnection)
					HQDisconnected();
				else if (conduit.IsInPoweredRange(Coords))
					HQConnected();
				else
				{
					if (MetaTilesHasConnection())
						HQConnected();
					else
						HQDisconnected();
				}
			}
			connectionInit = true;
			Profiler.EndSample();
		}

		public virtual bool MetaTilesHasConnection()
		{
			if (!buildingInfo.useMetaTiles)
				return false;
			for (int i = 0; i < metaTiles.Length; i++)
			{
				if (metaTiles[i].HasHQConnection)
					return true;
			}
			return false;
		}

		public virtual void HQConnected()
		{
			if (connectionInit)
			{
				if (HasHQConnection)
					return;
				if (!HasHQConnection)
					Map.EM.RemoveComponent<BuildingOffTag>(GetBuildingEntity());
			}
			HasHQConnection = true;
			OnConnected();
		}

		public virtual void HQDisconnected()
		{
			if (connectionInit)
			{
				if (HasHQConnection)
				{
					HasHQConnection = false;
					connectionInit = false;
					FindConduitConnections();
					return;
				}
				else
					return;
			}
			var e = GetBuildingEntity();
			if (!Map.EM.HasComponent<BuildingOffTag>(e))
				Map.EM.AddComponent<BuildingOffTag>(e);
			HasHQConnection = false;
			OnDisconnected();
		}

		public virtual void OnConnected()
		{
			Debug.Log($"{info.name}: {Coords} connected");
			if (IsBuilt)
				OnBuiltAndPowered();
			if (_connectionNotif != -1)
			{
				InfoPopupUI.RemovePopupNotif(Coords, _connectionNotif);
				_connectionNotif = -1;
			}
		}

		public virtual void OnDisconnected()
		{
			Debug.Log($"{info.name}: {Coords} disconnected");
			if(_connectionNotif == -1)
				_connectionNotif = InfoPopupUI.ShowPopupNotif(this, null, "No Power Connection", "This tile is not being powered by a Resource Conduit and cannot opperate");
		}

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			base.OnSerialize(tileData);
			tileData.Add("connectionInit", null);
			tileData.Add("hasHQConnection", null);
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			connectionInit = tileData.ContainsKey("connectionInit");
			HasHQConnection = tileData.ContainsKey("hasHQConnection");
			if (!HasHQConnection)
				OnDisconnected();
			else
				OnConnected();
			base.OnDeSerialized(tileData);
		}

		public override void OnRemoved()
		{
			base.OnRemoved();
			InfoPopupUI.HidePopup(Coords);
		}
	}
}