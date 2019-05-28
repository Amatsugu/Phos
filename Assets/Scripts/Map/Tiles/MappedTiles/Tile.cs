using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Tile
{
	public HexCoords Coords { get; protected set; }
	public Vector3 SurfacePoint { get; protected set; }
	public float Height { get; protected set; }
	public bool IsOccupied { get => _occupancyCount > 0; }
	public bool IsFullyOccupied { get => _occupancyCount == MAX_OCCUPANCY; }

	public readonly TileInfo info;

	public float moisture, temperature;
	public int biomeId;
	public bool IsShown { get; private set; }
	public TileInfo originalTile;


	protected Entity _tileEntity;
	private NativeArray<Entity> _decor;

	public const int MAX_OCCUPANCY = 4;
	private int[] _occupyingUnits = new int[MAX_OCCUPANCY];
	private int _occupancyCount = 0;

	public Tile(HexCoords coords, float height, TileInfo tInfo = null)
	{
		Coords = coords;
		Height = height;
		info = tInfo;
		SurfacePoint = new Vector3(coords.worldX, height, coords.worldZ);
	}

	public Tile SetBiome(int biome, float moisture, float temperature)
	{
		this.moisture = moisture;
		this.temperature = temperature;
		biomeId = biome;
		return this;
	}

	public bool OccupyTile(int unitId)
	{
		if (_occupancyCount == MAX_OCCUPANCY)
			return false;
		for (int i = 0; i < MAX_OCCUPANCY; i++)
		{
			if (_occupyingUnits[i] == 0)
			{
				_occupyingUnits[i] = unitId;
				_occupancyCount++;
				break;
			}
		}
		return true;
	}

	public void DeOccupyTile(int unitId)
	{
		for (int i = 0; i < MAX_OCCUPANCY; i++)
		{
			if(_occupyingUnits[i] == unitId)
			{
				_occupyingUnits[i] = 0;
				_occupancyCount--;
				return;
			}
		}
	}

	public virtual string GetName()
	{
		return $"{info.name} {Coords}";
	}

	public virtual string GetDescription()
	{
		var unitStr = "";
		for (int i = 0; i < MAX_OCCUPANCY; i++)
		{
			if (_occupyingUnits[i] == 0)
				continue;
			var e = Map.ActiveMap.units[_occupyingUnits[i]].Entity;
			unitStr += $"Unit[{_occupyingUnits[i]}]";
			if (Map.EM.HasComponent<Path>(e))
			{
				var pg = Map.EM.GetComponentData<PathGroup>(e);
				unitStr += $"Progress:{pg.Progress} Group:{pg.Value}";
			}
			unitStr += "\n";
		}
		return $"{info.description}\n" +
			$"Occupancy: {_occupancyCount}/{MAX_OCCUPANCY}\n" +
			unitStr;
	}

	public int GetOccupancyId(int unitId)
	{
		if (_occupancyCount == 0)
			throw new Exception("There are no units occupying this tile");
		for (int i = 0; i < MAX_OCCUPANCY; i++)
		{
			if (_occupyingUnits[i] == unitId)
				return i;
		}
		throw new Exception($"Unit [{unitId}] is not occupying this tile. Occupants [{string.Join(", ",_occupyingUnits)}]");
	}

	public float3 GetOccipancyPos(int unitId)
	{
		var oId = GetOccupancyId(unitId);
		var a = (360f / (MAX_OCCUPANCY-1)) * oId * Mathf.Deg2Rad;
		return new float3(Mathf.Cos(a), 0, Mathf.Sin(a)) * .5f;
	}

	public int[] GetUnits()
	{
		return _occupyingUnits;
	}

	// override object.Equals
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		var t = (Tile)obj;
		return (t.Coords == Coords);
	}

	// override object.GetHashCode
	public override int GetHashCode()
	{
		return Coords.GetHashCode();
	}

	public void UpdateHeight(float height)
	{
		Height = height;
		SurfacePoint = new Vector3(Coords.worldX, Height, Coords.worldZ);
		OnHeightChanged();
		UpdateDecorations();
	}

	public virtual void OnHeightChanged()
	{
		Map.EM.SetComponentData(_tileEntity, new NonUniformScale { Value = new Vector3(1, Height, 1) });
	}

	private void UpdateDecorations()
	{
		int lastIndex = 0;
		for (int i = 0; i < info.decorators.Length; i++)
		{
			var count = info.decorators[i].GetDecorEntityCount(this);
			var slice = _decor.Slice(lastIndex, count);
			info.decorators[i].UpdateHeight(slice, this, _tileEntity);
			lastIndex += count;
		}
	}

	public virtual void OnRemoved()
	{
		var neighbors = Map.ActiveMap.GetNeighbors(this);
		for (int i = 0; i < 6; i++)
			neighbors[i]?.TileUpdated(this, TileUpdateType.Removed);
	}

	public virtual void OnPlaced()
	{
		var neighbors = Map.ActiveMap.GetNeighbors(this);
		for (int i = 0; i < 6; i++)
			neighbors[i]?.TileUpdated(this, TileUpdateType.Placed);
	}

	public enum TileUpdateType
	{
		Placed,
		Removed
	}

	public virtual void TileUpdated(Tile src, TileUpdateType updateType)
	{

	}

	public virtual void Destroy()
	{
		try
		{
			Map.EM.DestroyEntity(_tileEntity);
			Map.EM.DestroyEntity(_decor);
		}catch
		{

		}finally
		{
			if(_decor.IsCreated)
				_decor.Dispose();
		}
	}

	public virtual void Show(bool isShown)
	{
		if (IsShown == isShown)
			return;
		IsShown = isShown;
		if(isShown)
		{
			//Map.EM.RemoveComponent(_decor, typeof(Frozen));
			Map.EM.RemoveComponent(_decor, typeof(FrozenRenderSceneTag));
		}
		else
		{
			//Map.EM.AddComponent(_decor, typeof(Frozen));
			Map.EM.AddComponent(_decor, typeof(FrozenRenderSceneTag));
		}
	}

	public virtual TileInfo GetMeshEntity()
	{
		return info;
	}

	public virtual Entity Render()
	{
		IsShown = true;
		_tileEntity = GetMeshEntity().Instantiate(Coords, new Vector3(1, Height, 1));
		if (info.decorators.Length == 0)
			return _tileEntity;
		_decor = new NativeArray<Entity>(info.decorators.Sum(t => t.GetDecorEntityCount(this)), Allocator.Persistent);
		int lastIndex = 0;
		for (int i = 0; i < info.decorators.Length; i++)
		{
			var e = info.decorators[i].Render(this, _tileEntity);
			var count = info.decorators[i].GetDecorEntityCount(this);
			for (int j = lastIndex; j < count; j++)
			{
				_decor[j] = e[j - lastIndex];
			}
			lastIndex += count;
		}
		return _tileEntity;
	}
}
