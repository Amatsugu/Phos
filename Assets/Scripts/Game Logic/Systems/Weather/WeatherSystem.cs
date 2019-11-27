using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WeatherSystem : JobComponentSystem
{
	public MeshEntity cloudMesh;
	public float cloudSize = 4;
	public int gridSize = 2;
	public NoiseSettings noiseSettings;
	public float noiseScale = 50;

	public static INoiseFilter cloudFilter;

	private float _shortDiag;
	private Vector3 _offset;
	private float _cloudFieldNormalizedWidth;
	private Vector3 _windDir;
	private Transform _cam;
	private float _maxNoiseValue;
	private float _innerRadius;
	private InitializeWeather _init;
	private NativeArray<float2> _cloudField;
	private NativeArray<float3> _cloudPos;

	private WeatherDefination _curWeather;
	private WeatherDefination _nextWeather;
	private WeatherState _curWeatherState;
	private System.Random _rand;
	private float _nextWeatherTime;
	private float _transitionTime = 0;
	private float _totalWeatherChance;
	private Fog _fogComponent;
	private PhysicallyBasedSky _skyComponent;
	private int _dir = 1;
	private static WeatherSystem _INST;
	private Transform _rainTransform;

	public struct GenerateFieldJob : IJobParallelFor
	{
		public float3 camPos;
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
			var pos = cloudPos[i] + camPos;
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

	protected override void OnCreate()
	{
		base.OnCreate();
		_INST = this;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_shortDiag = HexCoords.CalculateShortDiagonal(gridSize);
		_innerRadius = HexCoords.CalculateInnerRadius(gridSize);
		_cam = Camera.main.transform;
		_init = Object.FindObjectOfType<InitializeWeather>();
		noiseSettings = _init.noiseSettings;
		cloudFilter = NoiseFilterFactory.CreateNoiseFilter(noiseSettings, 1);
		_windDir = UnityEngine.Random.insideUnitSphere;
		_cloudFieldNormalizedWidth = (_init.fieldWidth * _shortDiag) / 2f;
		_maxNoiseValue = 1 - noiseSettings.minValue;
		if (_cloudField.IsCreated)
			_cloudField.Dispose();
		_rand = new System.Random(Map.ActiveMap.Seed);
		_totalWeatherChance = _init.weatherDefinations.Sum(d => d.chance);
		_rainTransform = _init.rainVfx.transform;

		SelectNextWeather();
		_curWeather = _nextWeather;
		_nextWeather = null;
		_init.volumeProfile.TryGet(out _fogComponent);
		_init.volumeProfile.TryGet(out _skyComponent);
		ApplyWeather(_curWeather.state);
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

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		SimulateWeather();
		var pos = HexCoords.SnapToGrid(_cam.position, _innerRadius, gridSize);
		var job = new CloudsJob
		{
			size = cloudSize,
			field = _cloudField,
			camPos = new float3(pos.x - _cloudFieldNormalizedWidth, 0, pos.z),
			disolveDist = _init.camDisolveDist * _init.camDisolveDist,
			rawCamPos = _cam.position + (_cam.forward * _init.disolveOffsetDist),
			disolveLower = _init.disolveLowerBound,
			disolveUpper = _init.disolveUpperBound
		};
		var dep = job.Schedule(this, inputDeps);

		var cloudShadowJob = new CloudShadowsJob
		{
			size = cloudSize,
			field = _cloudField,
			camPos = job.camPos
		};

		_rainTransform.position = new Vector3(_cam.position.x, _init.clouldHeight, _cam.position.z);


		return cloudShadowJob.Schedule(this, dep);
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

		if (Time.time >= _nextWeatherTime && _nextWeather == null)
		{
			SelectNextWeather();
			_transitionTime = 0;
		}

		if (_nextWeather != null)
		{
			_transitionTime += Time.DeltaTime;
			var t = _transitionTime / _nextWeather.transitionTime;
			t = math.clamp(t, 0, 1);
			ApplyWeather(WeatherState.Lerp(_curWeather.state, _nextWeather.state, t)); ;

			if (_transitionTime >= _nextWeather.transitionTime)
			{
				_curWeather = _nextWeather;
				ApplyWeather(_curWeather.state);
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
				break;
			case WeatherState.ParticleType.Snow:
				_init.rainVfx.SetFloat("Percipitation", 1-state.percipitation);
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
			if(selection <= 0)
			{
				_nextWeather = _init.weatherDefinations[i];
				_nextWeatherTime = _rand.Range(_nextWeather.duration.x, _nextWeather.duration.y) + _nextWeather.transitionTime;
				Debug.Log($"Transitioning To {_nextWeather}, {_nextWeatherTime}s Duration");
				_nextWeatherTime += (float)Time.ElapsedTime;
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
		var pos = HexCoords.SnapToGrid(_cam.position, _innerRadius, gridSize);
		var generate = new GenerateFieldJob
		{
			camPos = new float3(pos.x - _cloudFieldNormalizedWidth, 0, pos.z),
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

	}


	protected override void OnDestroy()
	{
		ApplyWeather(_init.weatherDefinations[0].state);
		base.OnDestroy();
	}

	protected override void OnDestroyManager()
	{
		_cloudField.Dispose();
	}
}
