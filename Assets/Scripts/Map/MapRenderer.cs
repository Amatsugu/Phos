using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

[Obsolete]
public class MapRenderer : MonoBehaviour
{
	public GameObject oceanPlane;
	public UnityEngine.UI.Image img;
	public int mapRes = 4;
	public int renderDistance = 1;

	[HideInInspector]
	public Map map;


	public SerializedMap serializedMap;

	private Transform _ocean;
	private Camera _cam;
	private Vector3 _lastCamPos;
	private Quaternion _lastCamRot;

	private Plane[] _camPlanes;
	private EntityManager _entityManager;
#if UNITY_EDITOR
	private NativeArray<HexCoords> _navDataKeys;
	private NativeArray<float> _navDataValues;
	private bool _showNavData;
#endif

	private MapAuthoring _mapAuthoring;

	private void Awake()
	{
		_mapAuthoring = GetComponent<MapAuthoring>();
	}

	internal void SetMap(Map map, GameState gameState)
	{
		this.map.Dispose();
		this.map = map;
		gameState.map = map;
		//GameRegistry.InitGame(gameState);
		_lastCamPos = default;
		_lastCamRot = default;

		GameEvents.InvokeOnMapLoaded();
	}

	private void OnValidate()
	{
		renderDistance = math.max(1, renderDistance);
	}

	private void Start()
	{
		_cam = GameRegistry.Camera;
		_lastCamPos = _cam.transform.position;
		_camPlanes = GeometryUtility.CalculateFrustumPlanes(_cam);
		Init();
		GameEvents.OnMapRegen += Regenerate;
		GameEvents.InvokeOnGameLoaded();
	}

	private void OnDestroy()
	{
		map?.Dispose();
#if UNITY_EDITOR
		if (_navDataKeys.IsCreated)
			_navDataKeys.Dispose();
		if (_navDataValues.IsCreated)
			_navDataValues.Dispose();
#endif
	}

	public void Init()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		map = _mapAuthoring.generator.GenerateMap(transform);
		GameRegistry.InitGame(map);
		_mapAuthoring.generator.GenerateFeatures(map);
		
		var pos = oceanPlane.transform.localScale;
		pos *= 2;
		pos.y = map.seaLevel;
		_ocean = Instantiate(oceanPlane, pos, Quaternion.identity).GetComponent<Transform>();

#if UNITY_EDITOR
		void Load()
		{
			Debug.Log("Nav Data");
			if (_navDataKeys.IsCreated)
				_navDataKeys.Dispose();
			if (_navDataValues.IsCreated)
				_navDataValues.Dispose();
			var d = map.GenerateNavData();
			_navDataKeys = d.GetKeyArray(Allocator.Persistent);
			_navDataValues = d.GetValueArray(Allocator.Persistent);

			d.Dispose();
		};

		GameEvents.OnMapLoaded += Load;
		GameEvents.OnMapChanged += Load;
#endif

		UnityEngine.Debug.Log("Map Load Invoke");
		GameEvents.InvokeOnMapLoaded();
		GameEvents.InvokeOnGameReady();
	}

	private void LateUpdate()
	{
#if UNITY_EDITOR
		if (Input.GetKeyUp(KeyCode.F9))
			_showNavData = !_showNavData;
		if (_showNavData)
		{
			for (int i = 0; i < _navDataKeys.Length; i++)
			{
				var coord = _navDataKeys[i];
				var val = _navDataValues[i];
				var tile = map[coord];
				if (!tile.IsShown)
					continue;
				var color = Color.Lerp(Color.red, Color.cyan, val / 5f);
				Debug.DrawRay(tile.SurfacePoint, math.up() * math.abs(val) * .5f, color);
			}
		}
#endif

		if (_mapAuthoring.generator.Regen)
		{
			GameEvents.InvokeOnMapRegen();
			_mapAuthoring.generator.Regen = false;
		}
	}

	public void Regenerate()
	{
		map.Destroy();
		Destroy(_ocean.gameObject);
		Init();

		_lastCamPos = Vector3.zero;
	}
}