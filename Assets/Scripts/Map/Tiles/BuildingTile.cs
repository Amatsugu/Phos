using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public BuildingTileInfo buildingInfo;
	public int distanceToHQ;


	private Entity _building;

	public BuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
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
		if (Map.IsDisposing)
			return;
		if (buildingInfo.buildingMesh != null)
			Map.EM.DestroyEntity(_building);
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

	private bool _init = false;

	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void TileUpdated(Tile src)
	{
		if (CheckHQConnection(this))
			OnHQConnected();
		else
			OnHQDisconnected();
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		distanceToHQ = (int)Vector3.Distance(SurfacePoint, Map.ActiveMap.HQ.SurfacePoint);
		if (CheckHQConnection(this))
			OnHQConnected();
		else
			OnHQDisconnected();
		_init = true;
	}

	public virtual void OnHQConnected()
	{
		if (_init && HasHQConnection)
			return;
		HasHQConnection = true;
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile b)
				b.OnHQConnected();
		}
		if (Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
			Map.EM.RemoveComponent<ConsumptionDebuff>(_tileEntity);
	}

	public virtual void OnHQDisconnected()
	{
		if (_init && !HasHQConnection)
			return;
		if (!Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
		{
			Map.EM.AddComponent(_tileEntity, typeof(ConsumptionDebuff));
			Map.EM.SetComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		}
	}

	public bool CheckHQConnection(Tile prev)
	{
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] == prev)
				continue;
			if (neighbors[i] is SubHQTile)
			{
				OnHQConnected();
				return true;
			}
			if(neighbors[i] is PoweredBuildingTile p)
			{
				if (p.CheckHQConnection(this))
				{
					OnHQConnected();
					return true;
				}
			}
		}
		OnHQDisconnected();
		return false;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		OnHQDisconnected();
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
			neighbors[i].TileUpdated(this);
	}
}