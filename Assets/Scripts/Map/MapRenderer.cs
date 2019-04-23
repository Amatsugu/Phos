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
using Unity.Jobs;
using UnityEngine.SceneManagement;

public class MapRenderer : MonoBehaviour
{
	public TileInfo tile;
	public MapGenerator generator;
	public GameObject oceanPlane;

	public ResourceList resourceList;

	[HideInInspector]
	public Map map;
	[HideInInspector]
	public Vector3 min, max;
	private Transform _ocean;
	private Camera _cam;
	private Vector3 _lastCamPos;
	private Plane[] _camPlanes;
	private EntityManager _entityManager;

	private void Awake()
	{
		_cam = FindObjectOfType<Camera>();
		ResourceDatabase.Init(resourceList.resourceDefinations);
		Init();
		_lastCamPos = _cam.transform.position;
		_camPlanes = GeometryUtility.CalculateFrustumPlanes(_cam);
		min = Vector3.zero;
		max = new Vector3(map.totalWidth * map.shortDiagonal, 0, map.totalHeight * 1.5f);
		_cam.transform.position = new Vector3(max.x / 2, 50, max.z / 2);
	}

	private void OnDestroy()
	{
		map.Dispose();
	}

	public void Init()
	{
		_entityManager = World.Active.EntityManager;
		map = generator.GenerateMap(transform);
		generator.GenerateFeatures(map);
		map.Render(_entityManager);
		var pos = oceanPlane.transform.localScale;
		pos *= 2;
		pos.y = map.seaLevel;
		_ocean = Instantiate(oceanPlane, pos, Quaternion.identity).GetComponent<Transform>();
	}

	

	private void Update()
	{
		//TODO: Remove this when testing complete
		if (Input.GetKey(KeyCode.R))
		{
			map.Destroy();
			SceneManager.LoadScene(0);
		}

		var camPos = _cam.transform.position;
		if (_lastCamPos != camPos)
		{
			GeometryUtility.CalculateFrustumPlanes(_cam, _camPlanes);
			map.UpdateView(_camPlanes);
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


