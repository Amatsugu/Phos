using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
	public HexCoords coord;

	private MapRenderer _renderer;

	private void Awake()
	{
		_renderer = transform.parent.parent.GetComponent<MapRenderer>();
	}

	private void OnMouseEnter()
	{
		
	}

	private void OnMouseOver()
	{
	}

	private void OnMouseExit()
	{
		
	}

	private void OnMouseUp()
	{
		_renderer.TileSelected(coord);
	}
}
