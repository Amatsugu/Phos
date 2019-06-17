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
	public float size = 4;
	public int maxClouds = 5000;
	public NoiseSettings noiseSettings;
	public float noiseScale = 50;

	public static INoiseFilter cloudFilter;
	public static CloudSystem INST;

	private float _shortDiag;
	private Vector3 _offset;
	private float _cloudFieldWidth;
	private Vector3 _windDir;
	private Transform _cam;
	private float _windSpeed;
	private float _maxValue;

	struct CloudsJob : IJobForEachWithEntity<CloudData, Translation, NonUniformScale>
	{
		[ReadOnly]
		public int fieldSize;
		[ReadOnly]
		public float size;
		[ReadOnly]
		public Vector3 offset;
		[ReadOnly]
		public float3 camPos;
		[ReadOnly]
		public float maxValue;

		public void Execute(Entity e, int index, ref CloudData c, ref Translation t, ref NonUniformScale s)
		{
			t.Value = c.pos + camPos;

			//var cloudSize = Mathf.PerlinNoise(t.Value.x / 50f + xOff, t.Value.z / 50f) * test;
			var cloudSize = cloudFilter.Evaluate(new Vector3(t.Value.x / 50f, t.Value.z / 50f, 0) + offset);
			cloudSize = math.max(0, cloudSize);
			cloudSize = MathUtils.Map(cloudSize, 0, maxValue, 0, 1);
			cloudSize = 1 - cloudSize;
			cloudSize = cloudSize * cloudSize * cloudSize;
			cloudSize = 1 - cloudSize;


			var cloudHeight = cloudFilter.Evaluate(new Vector3(t.Value.x / 15f + 200, t.Value.z / 15f + 200) + offset);
			cloudHeight = MathUtils.Map(cloudHeight, 0, 1, 1, 4);
			//cloudHeight = Mathf.RoundToInt(cloudHeight);


			s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		INST = this;
	}

	public void UpdateSettings()
	{
		OnStartRunning();
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_shortDiag = HexCoords.CalculateShortDiagonal(2);
		_cam = Camera.main.transform;
		var init = Object.FindObjectOfType<InitializeClouds>();
		noiseSettings = init.noiseSettings;
		cloudFilter = NoiseFilterFactory.CreateNoiseFilter(noiseSettings, 1);
		_windDir = UnityEngine.Random.insideUnitSphere;
		_cloudFieldWidth = init.fieldWidth / 2f;
		_windSpeed = init.windSpeed;
		_maxValue = 1 - noiseSettings.minValue;
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var windSampleDir = Mathf.PerlinNoise(_offset.x, _offset.z) * Mathf.PI * 2; ;
		//windSampleDir = MathUtils.Map(windSampleDir, 0, _maxValue, 0, 1);
		var windSampleStr = Mathf.PerlinNoise(_offset.x + 100, _offset.z + 100) * Mathf.PI * 2; ;
		_windDir.x = Mathf.Cos(windSampleDir);
		_windDir.z = Mathf.Sin(windSampleDir);
		_windDir.y = Mathf.Cos(windSampleStr);

		_offset += _windDir * Time.deltaTime * _windSpeed;

		var pos = HexCoords.FromPosition(_cam.position, 2).worldXZ;
		var job = new CloudsJob
		{
			fieldSize = maxClouds,
			size = size,
			offset = _offset,
			camPos = new float3(pos.x - _cloudFieldWidth * _shortDiag, 0, pos.z),
			maxValue = _maxValue
		};
		return job.Schedule(this, inputDeps);
	}
}
