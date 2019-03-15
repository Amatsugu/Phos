using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
	public HexCoords coord;

	private Map<Tile3D> _map;
	private MapRenderer _renderer;

	private void Awake()
	{
		_renderer = transform.parent.parent.GetComponent<MapRenderer>();
		_map = _renderer.map;
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
		_map[coord].TileClicked();
		_renderer.TileSelected(coord);
	}
}
