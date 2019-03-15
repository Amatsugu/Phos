using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class MapRenderer : MonoBehaviour
{
	public TileInfo tile;
	public MapGenerator generator;
	public GameObject oceanPlane;
	public GameObject selector;
	public bool useECS = true;


	[HideInInspector]
	public Map<Tile3D> map;
	[HideInInspector]
	public Vector3 min, max;
	public GameObject headquartersObj;

	private Transform _ocean;
	//private GameObject _hq;
	private Camera _cam;
	private Vector3 _lastCamPos;
	private Plane[] _camPlanes;

	private void Start()
	{
		if(!useECS)
			Init();
		_cam = FindObjectOfType<Camera>();
		_lastCamPos = _cam.transform.position;
		_camPlanes = GeometryUtility.CalculateFrustumPlanes(_cam);
		var chunkSize = Map<Tile3D>.Chunk.SIZE;
		min = Vector3.zero;
		max = new Vector3(map.Width * chunkSize * map.ShortDiagonal, 0, map.Height * chunkSize * 1.5f);
		_cam.transform.position = new Vector3(max.x / 2, 50, max.z / 2);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void AfterScene()
	{
		var mapRenderer = GameObject.FindObjectOfType<MapRenderer>();
		if (!mapRenderer.useECS)
			return;
		mapRenderer.Init();
		var em = World.Active.GetOrCreateManager<EntityManager>();

		mapRenderer.map.Render(em);
	}

	public void Init()
	{
		map = generator.GenerateMap(transform);
		if(!useECS)
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
		map.HexFlatten(pos, 1, 6, Map<Tile3D>.FlattenMode.Average);
		selector.transform.position = map[pos].SurfacePoint;
		Instantiate(headquartersObj, map[pos].SurfacePoint, Quaternion.identity);
	}

	public float GetHeight(HexCoords coord, int radius = 0)
	{
		if (radius == 0)
		{
			var t = map[coord];
			if (t == null)
				return map.SeaLevel;
			return t.Height;
		}
		var selection = map.HexSelect(coord, radius);
		if (selection.Count == 0)
			return map.SeaLevel;
		var max = selection.Max(t => t.Height);
		return (max < map.SeaLevel) ? map.SeaLevel : max;
	}

	private void Update()
	{
		var camPos = _cam.transform.position;
		if (_lastCamPos != camPos)
		{
			if(!useECS)
			{
				GeometryUtility.CalculateFrustumPlanes(_cam, _camPlanes);
				map.UpdateView(_camPlanes);
			}
			_lastCamPos = _cam.transform.position;
			_ocean.position = new Vector3(_lastCamPos.x, _ocean.position.y, _lastCamPos.z + 2 * _ocean.localScale.z);
		}
		if (generator.Regen)
		{
			generator.Regen = false;
			map.Destroy();
			Destroy(_ocean.gameObject);
			Init();
		}
		
	}
}
