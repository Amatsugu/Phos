﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

public class LineReneringSystem : JobComponentSystem
{
	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		base.OnCreate();
		var desc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<LineSegment>(),
				typeof(Translation),
				typeof(Rotation),
				typeof(NonUniformScale),
			},
			None = new ComponentType[]
			{
				typeof(Disabled),
				typeof(DisableRendering)
			}
		};
		_entityQuery = GetEntityQuery(desc);
		_entityQuery.SetChangedVersionFilter(typeof(LineSegment));
	}

	private struct LineSegmentJob : IJobChunk //IJobForEachWithEntity<LineSegment, Translation, Rotation, NonUniformScale>
	{
		[ReadOnly] public ComponentTypeHandle<LineSegment> lineType;
		public ComponentTypeHandle<Translation> translationType;
		public ComponentTypeHandle<Rotation> rotationType;
		public ComponentTypeHandle<NonUniformScale> scaleType;
		public uint LastVersion;


		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			if (!chunk.DidChange(lineType, LastVersion))
				return;

			var ls = chunk.GetNativeArray(lineType);
			var t = chunk.GetNativeArray(translationType);
			var r = chunk.GetNativeArray(rotationType);
			var s = chunk.GetNativeArray(scaleType);
			for (int i = 0; i < chunk.Count; i++)
			{
				t[i] = new Translation { Value = ls[i].Start };
				var dir = ls[i].End - ls[i].Start;
				r[i] = new Rotation { Value = quaternion.LookRotation(dir, math.up()) };
				var curScale = s[i];
				curScale.Value.z = math.length(dir);
				s[i] = curScale;
			}

		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var segmentJob = new LineSegmentJob
		{
			LastVersion = LastSystemVersion,
			lineType = GetComponentTypeHandle<LineSegment>(true),
			rotationType = GetComponentTypeHandle<Rotation>(false),
			translationType = GetComponentTypeHandle<Translation>(false),
			scaleType = GetComponentTypeHandle<NonUniformScale>(false),
		};
		var handle = segmentJob.Schedule(_entityQuery, inputDeps);

		return handle;
	}
}

public class LineWidthSystem : JobComponentSystem
{
	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		base.OnCreate();
		var desc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<LineWidth>(),
				typeof(NonUniformScale),
			},
			None = new ComponentType[]
			{
				typeof(Disabled),
				typeof(DisableRendering)
			}
		};
		_entityQuery = GetEntityQuery(desc);
		_entityQuery.SetChangedVersionFilter(typeof(LineWidth));
	}

	private struct LineWidthJob : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<LineWidth> widthType;
		public ComponentTypeHandle<NonUniformScale> scaleType;

		public uint LastVersion;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			if (!chunk.DidChange(widthType, LastVersion))
				return;
			var lw = chunk.GetNativeArray(widthType);
			var s = chunk.GetNativeArray(scaleType);

			for (int i = 0; i < chunk.Count; i++)
			{
				var curScale = s[i];
				curScale.Value.x = lw[i].Value;
				curScale.Value.y = lw[i].Value;
				s[i] = curScale;
			}

		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var widthJob = new LineWidthJob
		{
			LastVersion = LastSystemVersion,
			widthType = GetComponentTypeHandle<LineWidth>(true),
			scaleType = GetComponentTypeHandle<NonUniformScale>(false),
		};
		var handle = widthJob.Schedule(_entityQuery, inputDeps);

		return handle;
	}
}