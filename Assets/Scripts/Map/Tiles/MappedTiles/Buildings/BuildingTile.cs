using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public readonly BuildingTileInfo buildingInfo;
	public int distanceToHQ;
	public int upgradeLevel = 0;
	public bool IsBuilt => _isBuilt;

	private Entity _building;
	private bool _isBuilt;

	public BuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		buildingInfo = tInfo;
		if (GameRegistry.Cheats.INSTANT_BUILD)
			_isBuilt = true;
	}

	public override Entity Render()
	{
		if(_isBuilt)
		{
			if(buildingInfo.buildingMesh != null)
				_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint);
		}else
		{
			if (buildingInfo.constructionMesh != null)
				_building = buildingInfo.constructionMesh.Instantiate(SurfacePoint);
		}
		return base.Render();
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
		}catch
		{

		}
	}

	public override void Show(bool isShown)
	{
		base.Show(isShown);
		if (!Map.EM.Exists(_building))
			return;
		if (isShown)
			Map.EM.RemoveComponent(_building, typeof(Frozen));
		else
			Map.EM.AddComponent(_building, typeof(Frozen));
	}

	public void Build()
	{
		if (_isBuilt)
			return;
		_isBuilt = true;
		if (buildingInfo.constructionMesh != null)
			Map.EM.DestroyEntity(_building);
		_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint);
		PrepareEntity();
		OnBuilt();
	}

	protected virtual void PrepareEntity()
	{
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
		 

	protected virtual void OnBuilt()
	{
		NotificationsUI.NotifyWithTarget(NotifType.Info, $"Construction Complete: {buildingInfo.name}", this);
		
	}
}

public class PoweredBuildingTile : BuildingTile
{
	public bool HasHQConnection { get; protected set; }

	protected bool _connectionInit;

	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
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
		var closestConduit = Map.ActiveMap.conduitGraph.GetClosestNode(Coords);
		if (closestConduit == null)
			OnHQDisconnected();
		else
		{
			var conduit = (Map.ActiveMap[closestConduit.conduitPos] as ResourceConduitTile);
			if (conduit == null || !conduit.HasHQConnection)
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
		if(_connectionInit)
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
		if(!Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
			Map.EM.AddComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		HasHQConnection = false;
		InfoPopupUI.ShowPopup(Coords, null, "No Power Connection", "This tile is not being directly powered and results in a consumtion penalty.");
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		InfoPopupUI.HidePopup(Coords);
	}

}