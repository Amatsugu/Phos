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

	public Entity Entity;
	public Entity HeadEntity;
	public bool IsRendered { get; protected set; }

	protected int _chunk;
	private bool _isShown;
	private Faction _faction;
	public Vector3 _position;

	public MobileUnit(int id, MobileUnitInfo info, Tile tile, int chunkId, Faction faction)
	{
		this.id = id;
		this.info = info;
		_position = tile.SurfacePoint;
		Coords = tile.Coords;
		_chunk = chunkId;
		_faction = faction;
	}

	public Entity Render()
	{
		if (IsRendered)
			return Entity;
		IsRendered = _isShown = true;
		Entity = info.Instantiate(_position, Quaternion.identity, id);
		if(info.head != null)
			HeadEntity = info.head.Instantiate(_position, 1, Quaternion.identity);
		Map.EM.SetComponentData(Entity, new FactionId { Value = _faction });
		return Entity;
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
		if (_isShown = isShown)
			Map.EM.RemoveComponent(Entity, typeof(Frozen));
		else
			Map.EM.AddComponent(Entity, typeof(Frozen));
	}

	public void MoveTo(Vector3 pos)
	{
		if (!Map.EM.HasComponent<Destination>(Entity))
		{
			Map.EM.AddComponent(Entity, typeof(Destination));
		}
		if(Map.EM.HasComponent<Path>(Entity))
		{ 
			Map.EM.RemoveComponent<PathProgress>(Entity);	
			Map.EM.RemoveComponent<Path>(Entity);
		}
		Map.EM.SetComponentData(Entity, new Destination { Value = pos });
	}

	public virtual void Die()
	{
		Map.ActiveMap.unitLocations[_chunk].Remove(id);
		Map.ActiveMap.units.Remove(id);
		Destroy();
		//TODO: Death Effect
	}

	public override int GetHashCode()
	{
		return id;
	}

	public void Destroy()
	{
		if (Map.EM.Exists(Entity))
			Map.EM.DestroyEntity(Entity);
		if (Map.EM.Exists(HeadEntity))
			Map.EM.DestroyEntity(HeadEntity);
	}
}