using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile
{
	public HexCoords Coords { get; protected set; }
	public bool isShown = false;
	protected GameObject _tileObject;


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

	public void Show(bool shown)
	{
		if (shown == isShown)
			return;
		isShown = shown;
		_tileObject.SetActive(isShown);
	}
}
