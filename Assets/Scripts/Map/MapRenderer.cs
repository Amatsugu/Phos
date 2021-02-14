using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class MapRenderer : MonoBehaviour
{
	public TileEntity tile;
	public MapGenerator generator;
	public GameObject oceanPlane;
	public MeshEntityRotatable line;
	public TileDatabase tileDatabase;
	public UnityEngine.UI.Image img;
	public int mapRes = 4;
	public int renderDistance = 1;

	[HideInInspector]
	public Map map;

	[HideInInspector]
	public Vector3 min, max;

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

	internal void SetMap(Map map, GameState gameState)
	{
		this.map.Dispose();
		this.map = map;
		gameState.map = map;
		GameRegistry.InitGame(gameState);
		map.Render(_entityManager);
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
		min = Vector3.zero;
		Init();
		max = new Vector3(map.totalWidth * map.shortDiagonal, 0, map.totalHeight * 1.5f);
		_cam.transform.position = new Vector3(max.x / 2, 50, max.z / 2);
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
		map = generator.GenerateMap(transform);
		GameRegistry.InitGame(map);
		generator.GenerateFeatures(map);
		map.Render(_entityManager);
		
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

		if (generator.Regen)
		{
			GameEvents.InvokeOnMapRegen();
			generator.Regen = false;
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