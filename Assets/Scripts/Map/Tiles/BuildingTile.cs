using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public TileInfo originalTile;
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

	private bool _init;

	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void TileUpdated(Tile src)
	{
		EstablishHQConnection();
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		distanceToHQ = (int)Vector3.Distance(SurfacePoint, Map.ActiveMap.HQ.SurfacePoint);
		EstablishHQConnection();
		_init = true;
	}

	public virtual void OnHQConnected()
	{
		if (_init && HasHQConnection)
			return;
		HasHQConnection = true;
		if (Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
			Map.EM.RemoveComponent<ConsumptionDebuff>(_tileEntity);
	}

	public virtual void OnHQDisconnected()
	{
		if (_init && !HasHQConnection)
			return;
		HasHQConnection = false;
		if (!Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
		{
			Map.EM.AddComponent(_tileEntity, typeof(ConsumptionDebuff));
			Map.EM.SetComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		}
	}

	public void EstablishHQConnection()
	{
		if (this is SubHQTile)
		{
			HasHQConnection = true;
			return;
		}
		var	visited = new HashSet<PoweredBuildingTile>();
		if (CheckHQConnection(visited))
		{
			foreach (var tile in visited)
				tile.OnHQConnected();
		}
		else
		{
			foreach (var tile in visited)
				tile.OnHQDisconnected();
		}
	}

	public bool CheckHQConnection(HashSet<PoweredBuildingTile> visited = null)
	{
		visited.Add(this);
		bool foundHQ = false;
		var nT = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (nT[i] == null)
				continue;
			if (visited.Contains(nT[i]))
				continue;
			if (nT[i] is SubHQTile)
			{
				foundHQ = true;
				continue;
			}
			if (nT[i] is PoweredBuildingTile p)
			{
				if(p.CheckHQConnection(visited))
				{
					foundHQ = true;
				}

			}
		}
		return foundHQ;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
			neighbors[i].TileUpdated(this);
	}
}