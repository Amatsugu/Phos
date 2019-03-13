using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile
{
	public HexCoords Coords { get; protected set; }
	protected GameObject _tileObject;


	public Tile(HexCoords coords)
	{
		this.Coords = coords;
	}

	public abstract void RenderTile(Transform parent);

	public abstract void DestroyTile();


	public static bool operator ==(Tile a, Tile b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Tile a, Tile b)
	{
		return !a.Equals(b);
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

	public void Hide()
	{
		_tileObject.SetActive(false);
	}

	public void Show()
	{
		_tileObject.SetActive(true);
	}
}
