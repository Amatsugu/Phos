using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class MobileUnit
{
	public int id;
	public MobileUnitInfo info;


	public HexCoords Coords { get; protected set; }
	public Vector3 Position
	{
		get => Map.EM.GetComponentData<Translation>(Entity).Value;
		set
		{
			UpdatePos(value);
		}
	}

	public Entity Entity { get; protected set; }
	public bool IsRendered { get; protected set; }

	protected int _chunk;
	private bool _isShown;
	public Vector3 _position;


	public MobileUnit(int id, MobileUnitInfo info, Tile tile, int chunkId)
	{
		this.id = id;
		this.info = info;
		_position = tile.SurfacePoint;
		Coords = tile.Coords;
		_chunk = chunkId;
	}

	public Entity Render()
	{
		if (IsRendered)
			return Entity;
		IsRendered  = _isShown = true;
		return Entity =  info.Instantiate(_position, Quaternion.identity, id);
	}

	public void UpdatePos(Vector3 pos)
	{
		_position = pos;
		Coords = HexCoords.FromPosition(pos, Map.ActiveMap.tileEdgeLength);
	}

	public void UpdateChunk()
	{
		var newChunk = Coords.GetChunkIndex(Map.ActiveMap.width);
		Map.ActiveMap.MoveUnit(id, _chunk, newChunk);
		_chunk = newChunk;
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

	public void MoveTo(Vector3 pos)
	{
		if(!Map.EM.HasComponent<Destination>(Entity))
		{
			Map.EM.AddComponent(Entity, typeof(Destination));
		}
		Map.EM.RemoveComponent<PathProgress>(Entity);
		//Map.EM.RemoveComponent<Path>(Entity);
		Map.EM.SetComponentData(Entity, new Destination { Value = pos });
	}

	public virtual void Die()
	{
		//TODO: Death Effect
		//Map.ActiveMap[occupiedTile].DeOccupyTile(id);
	}

	public override int GetHashCode()
	{
		return id;
	}
}
