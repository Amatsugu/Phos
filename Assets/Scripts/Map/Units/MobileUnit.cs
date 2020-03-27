using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

public class MobileUnit
{
	public int id;
	public MobileUnitEntity info;

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
	public Vector3 _position;
	public bool IsRendered { get; protected set; }

	private bool _isShown;
	private Faction _faction;
	private NativeArray<Entity> _healhBar;

	public MobileUnit(int id, MobileUnitEntity info, Tile tile, Faction faction)
	{
		this.id = id;
		this.info = info;
		_position = tile.SurfacePoint;
		Coords = tile.Coords;
		_faction = faction;
	}

	public Entity Render()
	{
		if (IsRendered)
			return Entity;
		IsRendered = _isShown = true;
		Entity = info.Instantiate(_position, Quaternion.identity, id, _faction);
		if (info.head != null)
			HeadEntity = info.head.Instantiate(_position, new float3(1, 1, 1), Quaternion.identity);
		Map.EM.SetComponentData(Entity, new FactionId { Value = _faction });
		if(info.healthBar != null)
			_healhBar = info.healthBar.Instantiate(Entity, info.centerOfMassOffset + info.healthBarOffset);
		return Entity;
	}

	public void UpdatePos(Vector3 pos)
	{
		_position = pos;
		Coords = HexCoords.FromPosition(pos, Map.ActiveMap.tileEdgeLength);
	}

	public void Show(bool isShown)
	{
		if (isShown == _isShown)
			return;
		if (_isShown = isShown)
		{
			Map.EM.RemoveComponent(Entity, typeof(FrozenRenderSceneTag));
			Map.EM.RemoveComponent(HeadEntity, typeof(FrozenRenderSceneTag));
			if(_healhBar.IsCreated)
				Map.EM.RemoveComponent(_healhBar, typeof(FrozenRenderSceneTag));
		}
		else
		{
			Map.EM.AddComponent(Entity, typeof(FrozenRenderSceneTag));
			Map.EM.AddComponent(HeadEntity, typeof(FrozenRenderSceneTag));
			if(_healhBar.IsCreated)
				Map.EM.AddComponent(_healhBar, typeof(FrozenRenderSceneTag));
		}
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
		if (_healhBar.IsCreated)
			Map.EM.DestroyEntity(_healhBar);
	}
}