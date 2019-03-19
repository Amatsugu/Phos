using System;
using System.Collections;
using System.Collections.Generic;
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

	protected Entity _tileEntity;

	public Tile(HexCoords coords)
	{
		this.Coords = coords;
	}
	public Tile(HexCoords coords, float height, TileInfo tInfo = null) : this(coords)
	{
		Height = height;
		info = tInfo;
		SurfacePoint = new Vector3(coords.WorldX, height, coords.WorldZ);
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
		Map.EM.SetComponentData(_tileEntity, new NonUniformScale { Value = new Vector3(1, height, 1) });
		SurfacePoint = new Vector3(Coords.WorldX, height, Coords.WorldZ);
		foreach (var renderer in info.renderers)
			renderer.Render(this, _tileEntity);
	}


	public virtual void Destroy()
	{
		Map.EM.DestroyEntity(_tileEntity);
	}

	public virtual void TileClicked()
	{

	}

	public virtual Entity Render()
	{
		_tileEntity = info.Instantiate(Coords.WorldXZ, new Vector3(1, Height, 1));
		foreach (var renderer in info.renderers)
		{
			renderer.Render(this, _tileEntity);
		}
		return _tileEntity;
	}

	internal void SetParent(Entity parent)
	{
		Map.EM.AddComponent(_tileEntity, typeof(Parent));
		Map.EM.SetComponentData(_tileEntity, new Parent { Value = parent });
	}
}
