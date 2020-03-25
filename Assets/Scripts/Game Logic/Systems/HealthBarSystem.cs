using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(UnitMovementSystem))]
public class HealthBarSystem : JobComponentSystem
{
	[BurstCompile]
	private struct UpdateBarFillJob : IJobChunk
	{
		[ReadOnly] public ArchetypeChunkComponentType<HealthBar> barType;
		[ReadOnly] public ComponentDataFromEntity<Health> healthSrc;
		public ArchetypeChunkComponentType<NonUniformScale> scaleType;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var scale = chunk.GetNativeArray(scaleType);
			var bar = chunk.GetNativeArray(barType);
			for (int i = 0; i < chunk.Count; i++)
			{
				if(!healthSrc.Exists(bar[i].target))
					continue;
				var health = healthSrc[bar[i].target];
				var fill = health.Value > 0 ? health.Value / health.maxHealth : 0f;
				scale[i] = new NonUniformScale
				{
					Value = new float3(fill, 1, 1)
				};
			}
		}
	}

	[BurstCompile]
	private struct UpdateBarRotation : IJobChunk
	{
		[ReadOnly] public ArchetypeChunkComponentType<HealthBar> barType;
		[ReadOnly] public ArchetypeChunkComponentType<HealthBarFillTag> fillType;
		[ReadOnly] public ComponentDataFromEntity<CenterOfMass> posSrc;
		public ArchetypeChunkComponentType<Translation> translationType;
		public ArchetypeChunkComponentType<Rotation> rotationType;
		public EntityCommandBuffer.Concurrent cmd;
		public quaternion camRot;
		public float3 camFwd;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var rotations = chunk.GetNativeArray(rotationType);
			var bar = chunk.GetNativeArray(barType);
			var pos = chunk.GetNativeArray(translationType);
			var offset = new float3(-0.5f, 1, 0);
			offset += chunk.HasChunkComponent(fillType) ? camFwd * -0.1f : float3.zero;
			for (int i = 0; i < chunk.Count; i++)
			{
				if(!posSrc.Exists(bar[i].target))
				{
					continue;
				}
				rotations[i] = new Rotation
				{
					Value = camRot
				};
				pos[i] = new Translation
				{
					Value = posSrc[bar[i].target].Value + offset
				};
			}
		}
	}

	private EntityQuery _fillQuery;
	private EntityQuery _barQuery;
	private Transform _cam;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		_cam = GameRegistry.Camera.transform;
		var fillDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				typeof(NonUniformScale),
				ComponentType.ReadOnly<HealthBar>(),
				ComponentType.ReadOnly<HealthBarFillTag>(),
			}
		};
		_fillQuery = GetEntityQuery(fillDesc);
		var barDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				typeof(Translation),
				typeof(Rotation),
				ComponentType.ReadOnly<HealthBar>(),
			}
		};
		_barQuery = GetEntityQuery(barDesc);
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var hBarType = GetArchetypeChunkComponentType<HealthBar>(true);
		var rotJob = new UpdateBarRotation
		{
			barType = hBarType,
			camRot = _cam.rotation,
			camFwd = _cam.forward,
			posSrc = GetComponentDataFromEntity<CenterOfMass>(true),
			rotationType = GetArchetypeChunkComponentType<Rotation>(false),
			translationType = GetArchetypeChunkComponentType<Translation>(false),
			fillType = GetArchetypeChunkComponentType<HealthBarFillTag>(true)
		};
		inputDeps = rotJob.Schedule(_barQuery, inputDeps);

		var fillJob = new UpdateBarFillJob
		{
			barType = hBarType,
			healthSrc = GetComponentDataFromEntity<Health>(true),
			scaleType = GetArchetypeChunkComponentType<NonUniformScale>(false)
		};
		inputDeps = fillJob.Schedule(_fillQuery, inputDeps);
		return inputDeps;
	}
}

public struct HealthBar : IComponentData
{
	public Entity target;
}

public struct HealthBarFillTag : IComponentData
{

}
