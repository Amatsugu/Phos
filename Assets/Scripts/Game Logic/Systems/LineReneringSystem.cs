using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LineReneringSystem : JobComponentSystem
{
	struct LineSegmentJob : IJobForEachWithEntity<LineSegment, Translation, Rotation, NonUniformScale>
	{
		public void Execute(Entity e, int index, ref LineSegment ls, ref Translation t, ref Rotation r, ref NonUniformScale s)
		{
			t.Value = ls.Start;
			var dir = ls.End - ls.Start;
			r.Value = Quaternion.LookRotation(dir, Vector3.up);
			s.Value.z = Vector3.Magnitude(dir);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var segmentJob = new LineSegmentJob();
		var handle = segmentJob.Schedule(this, inputDeps);

		return handle;
	}
}


public class LineWidthSystem : JobComponentSystem
{
	struct LineWidthJob : IJobForEachWithEntity<LineWidth, NonUniformScale>
	{
		public void Execute(Entity e, int index, ref LineWidth lw, ref NonUniformScale s)
		{
			s.Value.x = lw.Value;
			s.Value.y = lw.Value;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var widthJob = new LineWidthJob();
		var handle = widthJob.Schedule(this, inputDeps);

		return handle;
	}
}