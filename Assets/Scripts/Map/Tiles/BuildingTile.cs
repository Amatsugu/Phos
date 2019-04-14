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
		if (!Map.ActiveMap.IsRendered)
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
	private HexCoords _connectionSrc;
	private float _connectionTime;

	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void TileUpdated(Tile src, TileUpdateType updateType)
	{
		base.TileUpdated(src, updateType);
		if(updateType == TileUpdateType.Removed)
		{
			if(src.Coords == _connectionSrc)
			{
				if (src is PoweredBuildingTile pb && pb.Coords == _connectionSrc)
					OnHQDisconnected(pb, new HashSet<Tile>());
			}
		}else
		{
			if (!HasHQConnection)
				FindNewConnectionSrc();
		}
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		distanceToHQ = (int)Vector3.Distance(SurfacePoint, Map.ActiveMap.HQ.SurfacePoint);
		FindNewConnectionSrc();
		_init = true;
	}

	public void FindNewConnectionSrc()
	{
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		PoweredBuildingTile best = null;
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile pb && pb.HasHQConnection)
			{
				if (best == null)
					best = pb;
				else if (pb.distanceToHQ < best.distanceToHQ)
					best = pb;
			}
		}
		if (best != null)
			OnHQConnected(best);
	}

	public virtual void OnHQConnected(PoweredBuildingTile src)
	{
		if (_init && HasHQConnection)
			return;
		_connectionTime = Time.time;
		_connectionSrc = src.Coords;
		HasHQConnection = true;
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile p)
				p.OnHQConnected(this);
		}
		if (Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
			Map.EM.RemoveComponent<ConsumptionDebuff>(_tileEntity);
	}

	public virtual void OnHQDisconnected(PoweredBuildingTile src, HashSet<Tile> visited)
	{
		if (_init && !HasHQConnection)
			return;
		//if (src.Coords != _connectionSrc)
			//return;
		if (visited.Contains(this))
			return;
		visited.Add(this);
		HasHQConnection = false;
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile p)
			{
				p.OnHQDisconnected(this, visited);
			}
		}
		if (!Map.EM.HasComponent<ConsumptionDebuff>(_tileEntity))
		{
			Map.EM.AddComponent(_tileEntity, typeof(ConsumptionDebuff));
			Map.EM.SetComponentData(_tileEntity, new ConsumptionDebuff { distance = distanceToHQ });
		}
	}

	/*public void EstablishHQConnection()
	{
		if (this is SubHQTile)
		{
			HasHQConnection = true;
			return;
		}
		var visited = new HashSet<PoweredBuildingTile>();
		if (!CheckHQConnection(visited))
		{
			foreach (var tile in visited)
				tile.OnHQDisconnected();
		}
		else
			OnHQConnected();
	}

	public bool CheckHQConnection(HashSet<PoweredBuildingTile> visited)
	{
		visited.Add(this);
		var nT = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (nT[i] == null)
				continue;
			if (visited.Contains(nT[i]))
				continue;
			if (nT[i] is SubHQTile)
			{
				return true;
			}
			if (nT[i] is PoweredBuildingTile p)
			{
				if(p.CheckHQConnection(visited))
					return true;
			}
		}
		return false;
	}*/

}