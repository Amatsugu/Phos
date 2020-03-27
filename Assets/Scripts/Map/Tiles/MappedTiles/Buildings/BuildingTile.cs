using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

public class BuildingTile : Tile
{
	public readonly BuildingTileEntity buildingInfo;
	public int distanceToHQ;
	public int upgradeLevel = 0;
	public bool IsBuilt => _isBuilt;

	private Entity _building;
	private Entity _offshorePlatform;
	protected bool _isBuilt;
	private NativeArray<Entity> _healthBars;

	public BuildingTile(HexCoords coords, float height, BuildingTileEntity tInfo) : base(coords, height, tInfo)
	{
		buildingInfo = tInfo;
	}

	public override Entity Render()
	{
		var e = base.Render();
		if (_isBuilt)
		{
			_isBuilt = false;
			Build();
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
		if (!Map.ActiveMap.IsRendered)
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
		}
		catch
		{
		}
	}

	public override void OnHide()
	{
		base.OnHide();
		if (buildingInfo.buildingMesh != null)
			Map.EM.AddComponent(_building, typeof(FrozenRenderSceneTag));
		if (_healthBars.IsCreated)
			Map.EM.AddComponent<FrozenRenderSceneTag>(_healthBars);
	}

	public override void OnShow()
	{
		base.OnShow();
		if (buildingInfo.buildingMesh != null)
			Map.EM.RemoveComponent(_building, typeof(FrozenRenderSceneTag));
		if (_healthBars.IsCreated)
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_healthBars);
	}

	protected virtual quaternion GetBuildingRotation() => quaternion.identity;

	public void Build()
	{
		if (_isBuilt)
			return;
		_isBuilt = true;
		if (buildingInfo.constructionMesh != null)
			Map.EM.DestroyEntity(_building);
		if (buildingInfo.buildingMesh.mesh == null)
			UnityEngine.Debug.LogWarning($"No Building Assigned for {base.GetName()}");
		else
			_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint, GetBuildingRotation(), GameRegistry.BuildingDatabase.GetId(buildingInfo), buildingInfo.maxHealth, buildingInfo.faction);

		if (buildingInfo.isOffshore && buildingInfo.offshorePlatformMesh != null)
			_offshorePlatform = buildingInfo.offshorePlatformMesh.Instantiate(SurfacePoint);
		PrepareEntity();
		OnBuilt();
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
		if (Map.EM.HasComponent<Health>(entity))
			Map.EM.SetComponentData(entity, new Health
			{
				maxHealth = buildingInfo.maxHealth,
				Value = buildingInfo.maxHealth
			});
		else
			Map.EM.AddComponentData(entity, new Health
			{
				maxHealth = buildingInfo.maxHealth,
				Value = buildingInfo.maxHealth
			});
		Map.EM.AddComponent(entity, typeof(FirstTickTag));
		if (buildingInfo.healthBar != null)
			_healthBars = buildingInfo.healthBar.Instantiate(entity, buildingInfo.centerOfMassOffset + buildingInfo.healthBarOffset);
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
		NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.name}", this);
	}
}

public class PoweredBuildingTile : BuildingTile
{
	public bool HasHQConnection { get; protected set; }

	protected bool _connectionInit;

	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileEntity tInfo) : base(coords, height, tInfo)
	{
	}

	public override string GetDescription()
	{
		return base.GetDescription() + "\n" +
			$"Has HQ Connection: {HasHQConnection} {Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity)}";
	}

	public override void OnPlaced()
	{
		distanceToHQ = (int)Vector3.Distance(SurfacePoint, Map.ActiveMap.HQ.SurfacePoint);
		base.OnPlaced();
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		FindConduitConnections();
	}

	public virtual void FindConduitConnections()
	{
		var closestConduit = Map.ActiveMap.conduitGraph.GetClosestConduitNode(Coords);
		if (closestConduit == null)
			OnHQDisconnected();
		else
		{
			var conduit = (Map.ActiveMap[closestConduit.conduitPos] as ResourceConduitTile);
			if (!conduit.HasHQConnection)
				OnHQDisconnected();
			else if (conduit.IsInPoweredRange(Coords))
				OnHQConnected();
			else
				OnHQDisconnected();
		}
		_connectionInit = true;
	}

	public virtual void OnHQConnected()
	{
		if (_connectionInit)
		{
			if (HasHQConnection)
				return;
			if (!HasHQConnection)
				Map.EM.RemoveComponent<ConsumptionDebuff>(GetBuildingEntity());
		}
		HasHQConnection = true;
		InfoPopupUI.HidePopup(Coords);
	}

	public virtual void OnHQDisconnected()
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
		if (!Map.EM.HasComponent<ConsumptionDebuff>(e))
			Map.EM.AddComponentData(e, new ConsumptionDebuff { distance = distanceToHQ });
		HasHQConnection = false;
		InfoPopupUI.ShowPopup(Coords, null, "No Power Connection", "This tile is not being powered by a Resource Conduit and results in a consumtion penalty.");
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		InfoPopupUI.HidePopup(Coords);
	}
}