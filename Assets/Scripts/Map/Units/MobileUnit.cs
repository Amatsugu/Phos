using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class MobileUnit
{
	public int id;
	public HexCoords occupiedTile;
	public MobileUnitInfo info;
	public Entity Entity { get; protected set; }
	public bool IsRendered { get; protected set; }

	private bool _isShown;


	public MobileUnit(int id, MobileUnitInfo info, Tile t)
	{
		this.id = id;
		this.info = info;
		OccupyTile(t);
	}

	public Entity Render()
	{
		if (IsRendered)
			return Entity;
		IsRendered = true;
		return Entity =  info.Instantiate(Map.ActiveMap[occupiedTile].SurfacePoint, Quaternion.identity, id);
	}

	public void Show(bool isShown)
	{
		if (isShown == _isShown)
			return;
		if(_isShown = isShown)
			Map.EM.RemoveComponent(Entity, typeof(Frozen));
		else
			Map.EM.AddComponent(Entity, typeof(Frozen));
	}

	public void MoveTo(Vector3 pos, int groupId)
	{
		if(!Map.EM.HasComponent<Destination>(Entity))
		{
			Map.EM.AddComponent(Entity, typeof(Destination));
			Map.EM.AddComponent(Entity, typeof(PathGroup));
		}
		if (Map.EM.HasComponent<Path>(Entity))
			Map.EM.RemoveComponent<Path>(Entity);
		Map.EM.SetComponentData(Entity, new Destination { Value = pos });
		Map.EM.SetComponentData(Entity, new PathGroup { Value = groupId, Progress = 0, Delay = 0 });
	}

	public virtual void Die()
	{
		//TODO: Death Effect
		Map.ActiveMap[occupiedTile].DeOccupyTile(id);
		Destroy();
	}

	public void Destroy()
	{
		Map.EM.DestroyEntity(Entity);
	}

	public bool OccupyTile(Tile tile)
	{
		if (occupiedTile == tile.Coords)
			return true;
		if (tile.OccupyTile(id))
		{
			Map.ActiveMap[occupiedTile].DeOccupyTile(id);
			occupiedTile = tile.Coords;
			Show(tile.IsShown);
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return id;
	}
}
