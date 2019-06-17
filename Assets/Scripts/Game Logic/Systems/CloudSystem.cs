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

	private float _shortDiag;
	private float _xOff;
	private float _cloudFieldWidth;
	private Transform _cam;

	struct CloudsJob : IJobForEachWithEntity<CloudData, Translation, NonUniformScale>
	{
		[ReadOnly]
		public int fieldSize;
		[ReadOnly]
		public float size;
		[ReadOnly]
		public float xOff;
		[ReadOnly]
		public float3 camPos;

		public void Execute(Entity e, int index, ref CloudData c, ref Translation t, ref NonUniformScale s)
		{
			t.Value = c.pos + camPos;
			var cloudSize = Mathf.PerlinNoise(t.Value.x / 50f + xOff, t.Value.z / 50f);
			cloudSize -= .6f;
			cloudSize = math.max(0, cloudSize);
			cloudSize = MathUtils.Map(cloudSize, 0, .6f, 0, 1);
			cloudSize = 1 - cloudSize;
			cloudSize = cloudSize * cloudSize * cloudSize * cloudSize * cloudSize;
			cloudSize = 1 - cloudSize;


			var cloudHeight = Mathf.PerlinNoise(t.Value.x / 15f + 200, t.Value.z / 15f + 200 + xOff);
			cloudHeight = MathUtils.Map(cloudHeight, 0, 1, 1, 4);
			//cloudHeight = Mathf.RoundToInt(cloudHeight);


			s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
		}
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_shortDiag = HexCoords.CalculateShortDiagonal(2);
		_cam = Camera.main.transform;
		var init = Object.FindObjectOfType<InitializeClouds>();
		noiseSettings = init.noiseSettings;
		_cloudFieldWidth = init.fieldWidth / 2f;
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		_xOff += Time.deltaTime * .1f;
		var pos = HexCoords.FromPosition(_cam.position, 2).worldXZ;
		var job = new CloudsJob
		{
			fieldSize = maxClouds,
			size = size,
			xOff = _xOff,
			camPos = new float3(pos.x - _cloudFieldWidth * _shortDiag, 0, pos.z)
		};
		return job.Schedule(this, inputDeps);
	}
}
