using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CloudSystem : JobComponentSystem
{
	public MeshEntity cloudMesh;
	public float cloudSize = 4;
	public int gridSize = 2;
	public NoiseSettings noiseSettings;
	public float noiseScale = 50;

	public static INoiseFilter cloudFilter;
	public static CloudSystem INST;

	private float _shortDiag;
	private Vector3 _offset;
	private float _cloudFieldNormalizedWidth;
	private Vector3 _windDir;
	private Transform _cam;
	private float _windSpeed;
	private float _maxNoiseValue;
	private float _innerRadius;
	private InitializeClouds _init;
	private NativeArray<float2> _cloudField;
	private NativeArray<float3> _cloudPos;

	private WeatherState _prevWeatherState;
	private WeatherState _curWeatherState;
	private WeatherState _nextState;
	private float _transitionTime = 0;
	private int _dir = 1;

	[ExcludeComponent(typeof(ShadowOnlyTag))]
	struct CloudsJob : IJobForEach<CloudData, Translation, NonUniformScale>
	{
		public float size;
		[ReadOnly]
		public NativeArray<float2> field;
		public float3 camPos;
		public float3 rawCamPos;
		public float disolveDist;
		public float disolveUpper;
		public float disolveLower;

		public void Execute(ref CloudData c, ref Translation t, ref NonUniformScale s)
		{
			t.Value = c.pos + camPos;

			var cloudSize = field[c.index].x;
			var cloudHeight = field[c.index].y;

			var vDist = t.Value.y - rawCamPos.y;
			var disolve = Vector3.SqrMagnitude(new float3(t.Value.x, 0, t.Value.z) - new float3(rawCamPos.x, 0, rawCamPos.z)) / disolveDist;
			if (disolve <= 1)
			{
				if (vDist > -disolveLower && vDist < 0)
				{
					vDist = -vDist;
					var dT = vDist / disolveLower;
					dT = math.pow(dT, 8);
					disolve = math.lerp(disolve, 1, dT);
				}
				else if (vDist < disolveUpper && vDist >= 0)
				{
					var dT = (vDist / disolveUpper);
					dT = dT.Pow(8);
					disolve = math.lerp(disolve, 1, dT);
				}
				else
					disolve = 1;
				disolve -= .5f;
				disolve = math.clamp(disolve, 0, 1);
				disolve *= 2;
				disolve = disolve * disolve * disolve;
				cloudSize = math.lerp(0, cloudSize, disolve);
				cloudHeight = math.lerp(0, cloudHeight, disolve);
			}
			s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
		}

	}

	[RequireComponentTag(typeof(ShadowOnlyTag))]
	struct CloudShadowsJob : IJobForEach<CloudData, Translation, NonUniformScale>
	{
		public float size;
		[ReadOnly]
		public NativeArray<float2> field;
		public float3 camPos;

		public void Execute(ref CloudData c, ref Translation t, ref NonUniformScale s)
		{
			t.Value = c.pos + camPos;

			var cloudSize = field[c.index].x;
			var cloudHeight = field[c.index].y;
			s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
		}
	}

	struct GenerateFieldJob : IJobParallelFor
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
		INST = this;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_shortDiag = HexCoords.CalculateShortDiagonal(gridSize);
		_innerRadius = HexCoords.CalculateInnerRadius(gridSize);
		_cam = Camera.main.transform;
		_init = Object.FindObjectOfType<InitializeClouds>();
		noiseSettings = _init.noiseSettings;
		cloudFilter = NoiseFilterFactory.CreateNoiseFilter(noiseSettings, 1);
		_windDir = UnityEngine.Random.insideUnitSphere;
		_cloudFieldNormalizedWidth = (_init.fieldWidth * _shortDiag) / 2f;
		_windSpeed = _init.windSpeed;
		_maxNoiseValue = 1 - noiseSettings.minValue;
		if (_cloudField.IsCreated)
			_cloudField.Dispose();
		_curWeatherState = _init.weatherDefinations[0].state;
		_prevWeatherState = _init.weatherDefinations[0].state;
		_nextState = _init.weatherDefinations[1].state;
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
		var windSampleDir = Mathf.PerlinNoise(_offset.x / 50, _offset.z / 50) * Mathf.PI * 4; ;
		var windSampleStr = Mathf.PerlinNoise(_offset.x / 100 + 100, _offset.z / 100 + 100) * Mathf.PI * 4; ;
		_windDir.x = Mathf.Cos(windSampleDir);
		_windDir.z = Mathf.Sin(windSampleDir);
		_windDir.y = Mathf.Cos(windSampleStr);

		_offset += _windDir * Time.deltaTime * _windSpeed;
		GenerateCloudField();
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

		_transitionTime += Time.deltaTime * _dir * .5f;
		_curWeatherState = WeatherState.Lerp(_prevWeatherState, _nextState, _transitionTime);


		if (_transitionTime > 1f || _transitionTime < 0)
		{
			_transitionTime = math.clamp(_transitionTime, 0, 1);
			_dir *= -1;
		}

		_init.cloudMesh.material.SetColor("_BaseColor", _curWeatherState.cloudColor);

		return cloudShadowJob.Schedule(this, dep);
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

	protected override void OnDestroyManager()
	{
		_cloudField.Dispose();
	}
}
