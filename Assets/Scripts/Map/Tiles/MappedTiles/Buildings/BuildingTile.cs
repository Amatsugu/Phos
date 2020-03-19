using Unity.Entities;
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

	public override TileInfo GetMeshEntity()
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
		}
		catch
		{
		}
	}

	public override void Show(bool isShown)
	{
		if (IsShown != isShown)
		{
			if (!Map.EM.Exists(_building))
			{
				base.Show(isShown);
				return;
			}
			if (isShown)
				Map.EM.RemoveComponent(_building, typeof(Frozen));
			else
				Map.EM.AddComponent(_building, typeof(Frozen));
		}
		base.Show(isShown);
	}

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
			_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint);

		if (buildingInfo.isOffshore && buildingInfo.offshorePlatformMesh != null)
			_offshorePlatform = buildingInfo.offshorePlatformMesh.Instantiate(SurfacePoint);
		PrepareEntity();
		OnBuilt();
		RenderDecorators();
	}

	protected virtual void PrepareEntity()
	{
		if (Map.EM.HasComponent<BuildingId>(_tileEntity))
			Map.EM.SetComponentData(_tileEntity, new BuildingId
			{
				Value = GameRegistry.BuildingDatabase.GetId(buildingInfo)
			});
		else
			Map.EM.AddComponentData(_tileEntity, new BuildingId
			{
				Value = GameRegistry.BuildingDatabase.GetId(buildingInfo)
			});
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

			Map.EM.AddSharedComponentData(_tileEntity, pData);
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

			Map.EM.AddSharedComponentData(_tileEntity, cData);
		}
		Map.EM.RemoveComponent<BuildingOffTag>(_tileEntity);
		Map.EM.AddComponent(_tileEntity, typeof(FirstTickTag));
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
				Map.EM.RemoveComponent<ConsumptionDebuff>(_tileEntity);
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
		if (!Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
			Map.EM.AddComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		HasHQConnection = false;
		InfoPopupUI.ShowPopup(Coords, null, "No Power Connection", "This tile is not being powered by a Resource Conduit and results in a consumtion penalty.");
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		InfoPopupUI.HidePopup(Coords);
	}
}