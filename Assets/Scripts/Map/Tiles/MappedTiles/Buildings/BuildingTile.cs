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

		protected StatsBuffs buffs;
		protected bool isBuilt;
		protected bool buidlingRendered;

		private Entity _building;
		private Entity _offshorePlatform;
		private NativeArray<Entity> _healthBars;
		private NativeArray<Entity> _adjacencyConnectors;
		private int _connectorCount;

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
				isBuilt = false;
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
				Map.EM.SetComponentData(_building, new Translation { Value = SurfacePoint });
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
		}

		protected virtual quaternion GetBuildingRotation() => quaternion.identity;

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
				_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint, GetBuildingRotation(), GameRegistry.TileDatabase.entityIds[buildingInfo], buildingInfo.maxHealth, buildingInfo.faction);

			if (buildingInfo.isOffshore && buildingInfo.offshorePlatformMesh != null)
				_offshorePlatform = buildingInfo.offshorePlatformMesh.Instantiate(SurfacePoint);
			PrepareEntity();
			ApplyBonuses();
			RenderDecorators();
		}

		protected virtual Entity GetBuildingEntity()
		{
			return buildingInfo.buildingMesh.mesh != null ? _building : _tileEntity;
		}

		protected virtual void PrepareEntity()
		{
			var entity = GetBuildingEntity();
			/*
			Map.EM.AddComponentData(entity, new BuildingId
			{
				Value = GameRegistry.BuildingDatabase.GetId(buildingInfo)
			});
			*/
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
			if (!IsBuilt)
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

		public virtual void AddProductionMulti(float ammount)
		{
			buffs.productionMulti += ammount;
			OnBuffRecieved();
		}

		public virtual void AddConsumptionMulti(float ammount)
		{
			buffs.consumptionMulti += ammount;
			OnBuffRecieved();
		}

		public void AddBuff(StatsBuffs stats)
		{
			buffs += stats;
			OnBuffRecieved();
		}

		public void RemoveBuff(StatsBuffs stats)
		{
			buffs -= stats;
			OnBuffRecieved();
		}

		protected virtual void OnBuffRecieved()
		{
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
			base.OnDeSerialized(tileData);
		}

	}

	public class PoweredBuildingTile : BuildingTile
	{
		public bool HasHQConnection { get; protected set; }

		protected bool _connectionInit;
		private int _connectionNotif = -1;

		public PoweredBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
		{
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

		protected override void PrepareEntity()
		{
			base.PrepareEntity();
			FindConduitConnections();
		}

		public virtual void FindConduitConnections()
		{
			var closestConduit = map.conduitGraph.GetClosestConduitNode(Coords);
			if (closestConduit == null)
				HQDisconnected();
			else
			{
				var conduit = (map[closestConduit.conduitPos] as ResourceConduitTile);
				if (!conduit.HasHQConnection)
					HQDisconnected();
				else if (conduit.IsInPoweredRange(Coords))
					HQConnected();
				else
					HQDisconnected();
			}
			_connectionInit = true;
		}

		public virtual void HQConnected()
		{
			if (_connectionInit)
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
			if (_connectionInit)
			{
				if (HasHQConnection)
				{
					HasHQConnection = false;
					_connectionInit = false;
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
				InfoPopupUI.RemovePopupNotif(Coords, _connectionNotif);
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
			_connectionInit = tileData.ContainsKey("connectionInit");
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