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
	public bool IsUnderwater { get; protected set; }

	public readonly TileInfo info;

	public float moisture, temperature;
	public int biomeId;
	public bool IsShown { get; private set; }
	public TileInfo originalTile;


	protected Entity _tileEntity;
	private NativeArray<Entity> _decor;
	protected bool _isRendered;

	public Tile(HexCoords coords, float height, TileInfo tInfo = null)
	{
		Coords = coords;
		Height = 5;//height;
		info = tInfo;
		if (height < Map.ActiveMap.seaLevel)
		{
			SurfacePoint = new Vector3(Coords.worldX, Map.ActiveMap.seaLevel, Coords.worldZ);
			IsUnderwater = true;
		}
		else
		{
			IsUnderwater = false;
			SurfacePoint = new Vector3(Coords.worldX, Height, Coords.worldZ);
		}
	}

	public Tile SetBiome(int biome, float moisture, float temperature)
	{
		this.moisture = moisture;
		this.temperature = temperature;
		biomeId = biome;
		return this;
	}

	public virtual string GetName()
	{
		return $"{info.name} {Coords}";
	}

	public virtual string GetDescription()
	{
		return info.description;
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
		if (height < Map.ActiveMap.seaLevel)
		{
			SurfacePoint = new Vector3(Coords.worldX, Map.ActiveMap.seaLevel, Coords.worldZ);
			IsUnderwater = true;
		}
		else
		{
			IsUnderwater = false;
			SurfacePoint = new Vector3(Coords.worldX, Height, Coords.worldZ);
		}
		OnHeightChanged();
		UpdateDecorations();
		SendTileUpdate(TileUpdateType.Height);
	}

	public virtual void OnHeightChanged()
	{
		if (!_isRendered)
			return;
		if(info.mesh != null)
			Map.EM.SetComponentData(_tileEntity, new NonUniformScale { Value = new Vector3(1, Height, 1) });

	}

	private void UpdateDecorations()
	{
		if (!_isRendered)
			return;
		int lastIndex = 0;
		for (int i = 0; i < info.decorators.Length; i++)
		{
			var count = info.decorators[i].GetDecorEntityCount(this);
			var slice = _decor.Slice(lastIndex, count);
			info.decorators[i].UpdateHeight(slice, this);
			lastIndex += count;
		}
	}

	public virtual void OnRemoved()
	{
		SendTileUpdate(TileUpdateType.Removed);
	}

	public virtual void OnPlaced()
	{
		SendTileUpdate(TileUpdateType.Placed);
	}

	public virtual void SendTileUpdate(TileUpdateType type)
	{
		var neighbors = Map.ActiveMap.GetNeighbors(this);
		for (int i = 0; i < 6; i++)
			neighbors[i]?.TileUpdated(this, type);
	}

	public enum TileUpdateType
	{
		Placed,
		Removed,
		Height
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
		_isRendered = true;
		_tileEntity = GetMeshEntity().Instantiate(Coords, new Vector3(1, Height, 1));
		return _tileEntity;
	}

	public virtual void RenderDecorators()
	{
		if (info.decorators.Length == 0)
			return;
		_decor = new NativeArray<Entity>(info.decorators.Sum(t => t.GetDecorEntityCount(this)), Allocator.Persistent);
		int lastIndex = 0;
		for (int i = 0; i < info.decorators.Length; i++)
		{
			var e = info.decorators[i].Render(this);
			var count = info.decorators[i].GetDecorEntityCount(this);
			for (int j = lastIndex; j < count; j++)
			{
				_decor[j] = e[j - lastIndex];
			}
			lastIndex += count;
		}
	}
}
