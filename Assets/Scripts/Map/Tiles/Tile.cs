using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public abstract class Tile
{
	public HexCoords Coords { get; protected set; }
	public Vector3 SurfacePoint { get; protected set; }
	public float Height { get; protected set; }

	public bool isShown = false;
	public TileInfo info;

	protected GameObject _tileObject;
	protected Entity _curEntity;
	protected EntityManager _entityManager;


	public Tile(HexCoords coords)
	{
		this.Coords = coords;
	}

	public abstract void Render(Transform parent);

	public abstract void TileClicked();

	public abstract void Destroy();

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
		if (_curEntity == null)
			_tileObject.transform.localScale = new Vector3(1, Height = height, 1);
		else
			_entityManager.SetComponentData(_curEntity, new NonUniformScale { Value = new Vector3(1, Height, 1) });
		SurfacePoint = new Vector3(Coords.WorldX, height, Coords.WorldZ);
	}

	public void Show(bool shown)
	{
		if (shown == isShown)
			return;
		isShown = shown;
		_tileObject.SetActive(isShown);
	}

	public abstract void Render(EntityManager entityManager);
}
