using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
	public TileInfo tile;
	public MapGenerator generator;
	public GameObject oceanPlane;
	[HideInInspector]
	public Map<Tile3D> map;
	public GameObject selector;
	public Vector2 viewPadding;

	private Transform _ocean;
	private Camera _cam;
	private Vector3 _lastCamPos;

	private void Start()
	{
		Init();
		_cam = FindObjectOfType<Camera>();
	}

	public void Init()
	{
		map = generator.GenerateMap(transform);
		map.Render(transform);
		var pos = oceanPlane.transform.localScale;
		pos *= 2;
		pos.y = map.SeaLevel;
		_ocean = Instantiate(oceanPlane, pos, Quaternion.identity).GetComponent<Transform>();

	}

	public void TileSelected(HexCoords pos)
	{
		if (!selector.activeInHierarchy)
			selector.SetActive(true);
		selector.transform.position = map[pos].SurfacePoint;
	}

	public float GetHeight(HexCoords coord, int radius = 0)
	{
		if (radius == 0)
			return map[coord].Height;
		var selection = map.HexSelect(coord, radius);
		if (selection.Count == 0)
			return map.SeaLevel;
		var max = selection.Max(t => t.Height);
		return (max < map.SeaLevel) ? map.SeaLevel : max;
	}

	private void Update()
	{
		if(_lastCamPos != _cam.transform.position)
		{
			map.UpdateView(_cam, viewPadding);
			_lastCamPos = _cam.transform.position;
			_ocean.position = new Vector3(_lastCamPos.x, _ocean.position.y, _lastCamPos.z + 2 * _ocean.localScale.z);
		}
		if (generator.Regen)
		{
			generator.Regen = false;
			map.Destroy();
			Destroy(_ocean);
			Init();
		}
		
	}

}
