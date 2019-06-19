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
	public int gridSize = 2;
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
	private float _camDisolveDist;
	private float _camDisolveOffset;
	public float _disolveUpper;
	public float _disolveLower;
	private float _innerRadius;

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
		public float3 rawCamPos;
		[ReadOnly]
		public float maxValue;
		[ReadOnly]
		public float disolveDist;
		[ReadOnly]
		public float disolveUpper;
		[ReadOnly]
		public float disolveLower;

		public void Execute(Entity e, int index, ref CloudData c, ref Translation t, ref NonUniformScale s)
		{
			t.Value = c.pos + camPos;
			var cloudSize = cloudFilter.Evaluate(new Vector3(t.Value.x / 50f, t.Value.z / 50f, 0) + offset);
			cloudSize = math.max(0, cloudSize);
			cloudSize = MathUtils.Map(cloudSize, 0, maxValue, 0, 1);
			cloudSize = 1 - cloudSize;
			cloudSize = cloudSize * cloudSize * cloudSize;
			cloudSize = 1 - cloudSize;


			var cloudHeight = cloudFilter.Evaluate(new Vector3(t.Value.x / 15f + 200, t.Value.z / 15f + 200) + offset);
			cloudHeight = MathUtils.Map(cloudHeight, 0, 1, 1, 4);


			var vDist = t.Value.y - rawCamPos.y;
			//disolve = Vector3.SqrMagnitude(t.Value - (rawCamPos)) / disolveDist;
			var disolve = Vector3.SqrMagnitude(new float3(t.Value.x, 0, t.Value.z) - new float3(rawCamPos.x, 0, rawCamPos.z)) / disolveDist;
			if (disolve <= 1)
			{
				if (vDist > -disolveLower && vDist < 0)
				{
					vDist = -vDist;
					disolve = math.lerp(disolve, 1, vDist / disolveLower);
				}
				else if (vDist < disolveUpper && vDist >= 0)
				{
					disolve = math.lerp(disolve, 1, (vDist / disolveUpper));
				}
				else
					disolve = 1;
				disolve -= .5f;
				disolve = math.clamp(disolve, 0, 1);
				disolve *= 2;
				//disolve = 1 - disolve;
				disolve = disolve * disolve * disolve;
				//disolve = 1 - disolve;
				cloudSize = math.lerp(0, cloudSize, disolve);
				cloudHeight = math.lerp(0, cloudHeight, disolve);
			}
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
		_shortDiag = HexCoords.CalculateShortDiagonal(gridSize);
		_innerRadius = HexCoords.CalculateInnerRadius(gridSize);
		_cam = Camera.main.transform;
		var init = Object.FindObjectOfType<InitializeClouds>();
		noiseSettings = init.noiseSettings;
		cloudFilter = NoiseFilterFactory.CreateNoiseFilter(noiseSettings, 1);
		_windDir = UnityEngine.Random.insideUnitSphere;
		_cloudFieldWidth = init.fieldWidth / 2f;
		_windSpeed = init.windSpeed;
		_maxValue = 1 - noiseSettings.minValue;
		_camDisolveDist = init.camDisolveDist * init.camDisolveDist;
		_camDisolveOffset = init.disolveOffsetDist;
		_disolveLower = init.disolveLowerBound;
		_disolveUpper = init.disolveUpperBound;
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var windSampleDir = Mathf.PerlinNoise(_offset.x / 50, _offset.z / 50) * Mathf.PI * 2; ;
		var windSampleStr = Mathf.PerlinNoise(_offset.x / 100 + 100, _offset.z / 100 + 100) * Mathf.PI * 2; ;
		_windDir.x = Mathf.Cos(windSampleDir);
		_windDir.z = Mathf.Sin(windSampleDir);
		_windDir.y = Mathf.Cos(windSampleStr);

		_offset += _windDir * Time.deltaTime * _windSpeed;

		var pos = HexCoords.SnapToGrid(_cam.position, _innerRadius, gridSize);
		var job = new CloudsJob
		{
			fieldSize = maxClouds,
			size = size,
			offset = _offset,
			camPos = new float3(pos.x - _cloudFieldWidth * _shortDiag, 0, pos.z),
			maxValue = _maxValue,
			disolveDist = _camDisolveDist,
			rawCamPos = _cam.position + (_cam.forward * _camDisolveOffset),
			disolveLower = _disolveLower,
			disolveUpper = _disolveUpper
		};
		Debug.DrawRay(job.rawCamPos, Vector3.right, Color.blue);
		Debug.DrawRay(job.rawCamPos, Vector3.up * _disolveUpper, Color.green);
		Debug.DrawRay(job.rawCamPos, Vector3.up * -_disolveLower, Color.yellow);
		return job.Schedule(this, inputDeps);
	}
}
