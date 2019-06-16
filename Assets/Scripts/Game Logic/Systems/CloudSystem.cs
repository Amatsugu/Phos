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
	public float size = 2;
	public int maxClouds = 5000;

	private float _innerRadius;
	private float _xOff;

	struct CloudsJob : IJobForEachWithEntity<Translation, NonUniformScale>
	{
		public int fieldSize;
		public float size;
		public float innerRadius;
		public float xOff;

		public void Execute(Entity e, int index, ref Translation t, ref NonUniformScale s)
		{
			var p = Mathf.PerlinNoise(t.Value.x / 50f + xOff, t.Value.z / 50f);
			p = MathUtils.Map(p, 0, .5f, 0, 1);
			p *= p;
			World.Active.EntityManager.SetComponentData(e, new NonUniformScale { Value = new float3(2 * p, 1, 2 * p) });
		}
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_innerRadius = HexCoords.CalculateInnerRadius(size);
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		_xOff += Time.deltaTime;
		var job = new CloudsJob
		{
			fieldSize = maxClouds,
			innerRadius = _innerRadius,
			size = size,
			xOff = _xOff
		};
		return job.Schedule(this, inputDeps);
	}
}
