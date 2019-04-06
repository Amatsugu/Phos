using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Tile
{
	public HexCoords Coords { get; protected set; }
	public Vector3 SurfacePoint { get; protected set; }
	public float Height { get; protected set; }

	public TileInfo info;

	public float moisture, temperature;
	public int biomeId;
	public bool IsShown { get; private set; }

	protected Entity _tileEntity;
	private NativeArray<Entity> _decor;

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
		OnHeightChanged();
		UpdateDecorations();
	}

	public virtual void OnHeightChanged()
	{
		Map.EM.SetComponentData(_tileEntity, new NonUniformScale { Value = new Vector3(1, Height, 1) });
		SurfacePoint = new Vector3(Coords.worldX, Height, Coords.worldZ);
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

	}

	public virtual void OnPlaced()
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
			_decor.Dispose();
		}
	}

	public virtual void Show(bool isShown)
	{
		if (isShown == IsShown)
			return;
		if(isShown)
		{
			Map.EM.RemoveComponent(_decor, typeof(FrozenRenderSceneTag));
		}else
		{
			Map.EM.AddComponent(_decor, typeof(FrozenRenderSceneTag));
		}
		IsShown = isShown;
	}

	public virtual Entity Render()
	{
		IsShown = true;
		_tileEntity = info.Instantiate(Coords, new Vector3(1, Height, 1));
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
