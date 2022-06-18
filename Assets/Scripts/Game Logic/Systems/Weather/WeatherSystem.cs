using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public partial class WeatherSystem : SystemBase
{
	public MeshEntity cloudMesh;
	public float cloudSize = 4;
	public int gridSize = 2;
	public NoiseSettings noiseSettings;
	public float noiseScale = 50;
	public float WindSpeed => _curWeatherState.windSpeed;

	public static INoiseFilter cloudFilter;

	private float _shortDiag;
	private float3 _offset;
	private float _cloudFieldNormalizedHalfWidth;
	private Transform _cam;
	private float _maxNoiseValue;
	private float _innerRadius;
	private InitializeWeather _init;
	private NativeArray<float2> _cloudField;
	private NativeArray<float3> _cloudPos;

	private float3 _windDir;
	private WeatherDefination _curWeather;
	private WeatherDefination _nextWeather;
	private WeatherState _prevWeatherState;
	private WeatherState _curWeatherState;
	private System.Random _rand;
	private float _nextWeatherTime;
	private float _transitionTime = 0;
	private float _totalWeatherChance;
	private Fog _fogComponent;

	//private PhysicallyBasedSky _skyComponent;
	private static WeatherSystem _INST;

	private Transform _rainTransform;

	public struct GenerateFieldJob : IJobParallelFor
	{
		public float3 camPos;
		public float3 camCenteringOffset;
		public quaternion camRot;
		public int gridSize;
		public float innerRadius;
		public int fieldWidth;
		public float3 offset;
		public float cloudDensity;
		public float maxNoiseValue;

		[ReadOnly]
		public NativeArray<float3> cloudPos;

		public NativeArray<float2> cloudField;

		public void Execute(int i)
		{
			var pos = math.rotate(camRot, cloudPos[i]) + HexCoords.SnapToGrid(camPos - camCenteringOffset, innerRadius, gridSize);
			var cloudSize = cloudFilter.Evaluate(pos / 50f + offset, cloudDensity);

			cloudSize = math.max(0, cloudSize);
			cloudSize = MathUtils.Remap(cloudSize, 0, maxNoiseValue, 0, 1);
			cloudSize = 1 - cloudSize;
			cloudSize = cloudSize * cloudSize * cloudSize;
			cloudSize = 1 - cloudSize;

			var cloudHeight = cloudFilter.Evaluate((pos / 15f) + new float3(200, 200, 200) + offset);
			cloudHeight = math.max(0, cloudHeight);
			cloudHeight = MathUtils.Remap(cloudHeight, 0, 1, 1, 4);

			cloudField[i] = new float2(math.clamp(cloudSize, 0, 1), cloudHeight);
		}
	}

	private EntityQuery _cloudQuery;
	private EntityQuery _cloudShadowQuery;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnWeatherInit += Init;
		var cloudDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<CloudData>(),
				typeof(Translation),
				typeof(NonUniformScale),
			},
			None = new ComponentType[]
			{
				typeof(ShadowOnlyTag)
			}
		};
		_cloudQuery = GetEntityQuery(cloudDesc);
		var shadowDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<CloudData>(),
				typeof(Translation),
				typeof(NonUniformScale),
				typeof(ShadowOnlyTag)
			},
		};
		_cloudShadowQuery = GetEntityQuery(shadowDesc);
		_INST = this;
	}

	protected void Init()
	{
		GameEvents.OnWeatherInit -= Init;
		_shortDiag = HexCoords.CalculateShortDiagonal(gridSize);
		_innerRadius = HexCoords.CalculateInnerRadius(gridSize);
		_cam = Camera.main.transform;
		_init = Object.FindObjectOfType<InitializeWeather>();
		if (_init == null)
			return;
		noiseSettings = _init.noiseSettings;
		cloudFilter = NoiseFilterFactory.CreateNoiseFilter(noiseSettings, 1);
		_windDir = UnityEngine.Random.insideUnitSphere;
		_cloudFieldNormalizedHalfWidth = (_init.fieldWidth * _shortDiag) / 2f;
		_maxNoiseValue = 1 - noiseSettings.minValue;
		if (_cloudField.IsCreated)
			_cloudField.Dispose();
		_rand = new System.Random(GameRegistry.GameMap?.Seed ?? 0);
		_totalWeatherChance = _init.weatherDefinations.Sum(d => d.chance);
		_rainTransform = _init.rainVfx.transform;

		SelectNextWeather();
		_curWeather = _nextWeather;
		_nextWeather = null;
		_init.volumeProfile.TryGet(out _fogComponent);
		//_init.volumeProfile.TryGet(out _skyComponent);
		ApplyWeather(_curWeather.state);
		_prevWeatherState = _curWeatherState;
		_cloudField = new NativeArray<float2>(_init.fieldWidth * _init.fieldHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_cloudPos = new NativeArray<float3>(_init.fieldWidth * _init.fieldHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int z = 0; z < _init.fieldHeight; z++)
		{
			for (int x = 0; x < _init.fieldWidth; x++)
			{
				_cloudPos[x + z * _init.fieldWidth] = HexCoords.OffsetToWorldPosXZ(x, z, _innerRadius, gridSize);
			}
		}
	}


	protected override void OnUpdate()
	{
		if (_init == null)
			return;
		SimulateWeather();
		var pos = _cam.position; //HexCoords.SnapToGrid(_cam.position, _innerRadius, gridSize);
		var cloudType = GetComponentTypeHandle<CloudData>(true);
		var translationType = GetComponentTypeHandle<Translation>(false);
		var scaleType = GetComponentTypeHandle<NonUniformScale>(false);
		var camRot = quaternion.RotateY(math.radians(_cam.eulerAngles.y));
		var camCenteringOffset = math.rotate(camRot, new float3(_cloudFieldNormalizedHalfWidth, 0, 0));
		var cloudJob = new CloudsJob
		{
			size = cloudSize,
			field = _cloudField,
			camPos = new float3(pos.x, 0, pos.z),
			camCenteringOffset = camCenteringOffset,
			camRot = camRot,
			disolveDist = _init.camDisolveDist * _init.camDisolveDist,
			rawCamPos = _cam.position + (_cam.forward * _init.disolveOffsetDist),
			disolveLower = _init.disolveLowerBound,
			disolveUpper = _init.disolveUpperBound,
			cloudType = cloudType,
			translationType = translationType,
			scaleType = scaleType,
			innerRadius = _innerRadius,
			gridSize = gridSize
		};
		Dependency = cloudJob.Schedule(_cloudQuery, Dependency);

		var cloudShadowJob = new CloudShadowsJob
		{
			size = cloudSize,
			field = _cloudField,
			camPos = cloudJob.camPos,
			camCenteringOffset = camCenteringOffset,
			camRot = camRot,
			cloudType = cloudType,
			translationType = translationType,
			scaleType = scaleType,
			innerRadius = _innerRadius,
			gridSize = gridSize
		};

		_rainTransform.position = new Vector3(_cam.position.x, _init.clouldHeight, _cam.position.z);

		Dependency = cloudShadowJob.Schedule(_cloudShadowQuery, Dependency);
	}

	public void SimulateWeather()
	{
		var windSampleDir = Mathf.PerlinNoise(_offset.x / 50, _offset.z / 50) * Mathf.PI * 4; ;
		var windSampleStr = Mathf.PerlinNoise(_offset.x / 100 + 100, _offset.z / 100 + 100) * Mathf.PI * 4; ;
		_windDir.x = Mathf.Cos(windSampleDir);
		_windDir.z = Mathf.Sin(windSampleDir);
		_windDir.y = Mathf.Cos(windSampleStr);
		_init.rainVfx.SetVector3("Wind", _windDir * -1);
		_offset += _windDir * Time.DeltaTime * _curWeatherState.windSpeed;
		GenerateCloudField();

		if (Time.ElapsedTime >= _nextWeatherTime && _nextWeather == null)
		{
			SelectNextWeather();
			_transitionTime = 0;
		}

		if (_nextWeather != null)
		{
			_transitionTime += Time.DeltaTime;
			var t = _transitionTime / _nextWeather.transitionTime;
			t = math.clamp(t, 0, 1);
			ApplyWeather(WeatherState.Lerp(_prevWeatherState, _nextWeather.state, t)); ;

			if (_transitionTime >= _nextWeather.transitionTime)
			{
				_curWeather = _nextWeather;
				ApplyWeather(_curWeatherState);
				_nextWeather = null;
			}
		}
	}

	public void ApplyWeather(WeatherState state)
	{
		_curWeatherState = state;

		//Sky
		//_skyComponent.skyTint.value = _curWeatherState.skyColor;

		//Fog
		if (_curWeatherState.fogDensity == 9999)
			_fogComponent.active = false;
		else
			_fogComponent.active = true;
		_fogComponent.maximumHeight.value = _curWeatherState.fogHeight;
		_fogComponent.baseHeight.value = _curWeatherState.fogBaseHeight;
		_fogComponent.depthExtent.value = _curWeatherState.fogDensity;
		_fogComponent.albedo.value = _curWeatherState.fogColor;

		//Clouds
		_init.cloudMesh.material.SetColor("_BaseColor", state.cloudColor);

		//Particles
		switch (state.weatherType)
		{
			case WeatherState.ParticleType.Rain:
				_init.rainVfx.SetFloat("Percipitation", state.percipitation);
				if (state.percipitation == 0)
					_init.rainVfx.gameObject.SetActive(false);
				else
					_init.rainVfx.gameObject.SetActive(true);
				break;

			case WeatherState.ParticleType.Snow:
				_init.rainVfx.SetFloat("Percipitation", 1 - state.percipitation);
				break;

			case WeatherState.ParticleType.None:
				_init.rainVfx.SetFloat("Percipitation", state.percipitation);
				break;
		}

		//Sun
#if DEBUG
		if (_init.sun != null)
		{
#endif
			_init.sun.colorTemperature = _curWeatherState.sunTemp;
			_init.sun.color = _curWeatherState.sunColor;
#if DEBUG
		}
#endif
	}

	public void SelectNextWeather()
	{
		var chance = _totalWeatherChance - _curWeather?.chance ?? 0;
		var selection = _rand.Range(0, chance);

		for (int i = 0; i < _init.weatherDefinations.Length; i++)
		{
			if (_init.weatherDefinations[i] == _curWeather)
				continue;
			selection -= _init.weatherDefinations[i].chance;
			if (selection <= 0)
			{
				_nextWeather = _init.weatherDefinations[i];
				_nextWeatherTime = _rand.Range(_nextWeather.duration.x, _nextWeather.duration.y) + _nextWeather.transitionTime;
				UnityEngine.Debug.Log($"Transitioning To {_nextWeather}, {_nextWeatherTime}s Duration");
				_nextWeatherTime += (float)Time.ElapsedTime;
				_prevWeatherState = _curWeatherState;
				break;
			}
		}
	}

	public static void SkipWeather()
	{
		_INST._nextWeather = null;
		_INST._transitionTime = 0;
		_INST._nextWeatherTime = (float)_INST.Time.ElapsedTime;
	}

	public void GenerateCloudField()
	{
		var pos = _cam.position; //HexCoords.SnapToGrid(_cam.position, _innerRadius, gridSize);
		var camRot = quaternion.RotateY(math.radians(_cam.eulerAngles.y));
		var camCenteringOffset = math.rotate(camRot, new float3(_cloudFieldNormalizedHalfWidth, 0, 0));
		var generate = new GenerateFieldJob
		{
			camPos = new float3(pos.x, 0, pos.z),
			camCenteringOffset = camCenteringOffset,
			camRot = quaternion.RotateY(math.radians(_cam.eulerAngles.y)),
			cloudDensity = 1 - _curWeatherState.cloudDensity,
			cloudField = _cloudField,
			cloudPos = _cloudPos,
			fieldWidth = _init.fieldWidth,
			gridSize = gridSize,
			innerRadius = _innerRadius,
			maxNoiseValue = _maxNoiseValue,
			offset = _offset
		};
		generate.Schedule(_cloudField.Length, _cloudField.Length / 8).Complete();
#if UNITY_EDITOR
		DebugUtilz.DrawBounds(
			generate.camPos - new float3(_cloudFieldNormalizedHalfWidth, 0, 0), 
			generate.camPos 
				+ new float3(_cloudFieldNormalizedHalfWidth, 20, _init.clouldHeight * _shortDiag),
			Color.red);
#endif
	}

	protected override void OnStopRunning()
	{
		if (_init?.rainVfx != null)
			ApplyWeather(_init.weatherDefinations[0].state);
		base.OnStopRunning();
	}

	protected override void OnDestroy()
	{
		if (_cloudField.IsCreated)
			_cloudField.Dispose();
		if (_cloudPos.IsCreated)
			_cloudPos.Dispose();
	}
}