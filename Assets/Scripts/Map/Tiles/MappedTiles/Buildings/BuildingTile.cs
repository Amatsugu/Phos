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


	private Entity _building;

	public BuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		buildingInfo = tInfo;
	}

	public override Entity Render()
	{
		if(buildingInfo.buildingMesh != null)
			_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint);
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
		if (buildingInfo.buildingMesh == null)
			return;
		if (isShown)
			Map.EM.RemoveComponent(_building, typeof(Frozen));
		else
			Map.EM.AddComponent(_building, typeof(Frozen));
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
		FindConduitConnections();
		base.OnPlaced();
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
	}

	public virtual void OnHQDisconnected()
	{
		if(_connectionInit)
		{
			Debug.Log("Disconnect, Finding new Connection...");
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
		Debug.Log("Disconnect, No Connections");
		Map.EM.AddComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		HasHQConnection = false;
	}

}