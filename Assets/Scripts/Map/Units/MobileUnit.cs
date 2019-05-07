using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class MobileUnit
{
	public int id;
	private HexCoords _occupiedTile;
	public MobileUnitInfo info;
	public bool IsRendered { get; private set; }

	private Entity _unitEntity;
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
			return _unitEntity;
		IsRendered = true;
		return _unitEntity =  info.Instantiate(_occupiedTile.worldXZ, Quaternion.identity);
	}

	public void Shown(bool isShown)
	{
		if (isShown == _isShown)
			return;
		if(_isShown = isShown)
			Map.EM.RemoveComponent(_unitEntity, typeof(Frozen));
		else
			Map.EM.AddComponent(_unitEntity, typeof(Frozen));
	}

	public void MoveTo(Vector3 pos)
	{
		if(!Map.EM.HasComponent<Destination>(_unitEntity))
			Map.EM.AddComponent(_unitEntity, typeof(Destination));
		Map.EM.SetComponentData(_unitEntity, new Destination { Value = pos });
	}

	public virtual void Die()
	{
		//TODO: Death Effect
		Destroy();
	}

	public void Destroy()
	{
		Map.EM.DestroyEntity(_unitEntity);
	}

	public void OccupyTile(Tile tile)
	{
		if (_occupiedTile.isCreated)
			Map.ActiveMap[_occupiedTile].DeOccupyTile();
		if (tile.OccupyTile(this))
			_occupiedTile = tile.Coords;
	}

	public override int GetHashCode()
	{
		return id;
	}
}
