using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
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
				var hBar = bar[i];
				if (!healthSrc.Exists(hBar.target))
					continue;
				var health = healthSrc[hBar.target];
				var fill = health.Value > 0 ? health.Value / health.maxHealth : 0f;
				scale[i] = new NonUniformScale
				{
					Value = new float3(hBar.size.x * fill, hBar.size.y, 1)
				};
			}
		}
	}

	[BurstCompile]
	private struct UpdateBarRotation : IJobChunk
	{
		[ReadOnly] public ArchetypeChunkComponentType<HealthBar> barType;
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
			var layerOffset = camFwd * -0.01f;
			for (int i = 0; i < chunk.Count; i++)
			{
				var healthBar = bar[i];
				if(!posSrc.Exists(healthBar.target))
				{
					continue;
				}
				rotations[i] = new Rotation
				{
					Value = camRot
				};
				var centerOffset = new float3((healthBar.size / -2f), 0);
				pos[i] = new Translation
				{
					Value = (posSrc[healthBar.target].Value + (layerOffset * (int)healthBar.type) + healthBar.offset) + centerOffset
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
			},
			None = new ComponentType[]
			{
				typeof(FrozenRenderSceneTag),
				typeof(Disabled)
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
			},
			None = new ComponentType[]
			{
				typeof(FrozenRenderSceneTag),
				typeof(Disabled)
			}
		};
		_barQuery = GetEntityQuery(barDesc);
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var hBarType = GetArchetypeChunkComponentType<HealthBar>(true);
		var rotJob = new UpdateBarRotation
		{
			camRot = _cam.rotation,
			camFwd = math.normalize(_cam.forward),
			posSrc = GetComponentDataFromEntity<CenterOfMass>(true),
			rotationType = GetArchetypeChunkComponentType<Rotation>(false),
			translationType = GetArchetypeChunkComponentType<Translation>(false),
			barType = hBarType,
		};
		inputDeps = rotJob.Schedule(_barQuery, inputDeps);

		var fillJob = new UpdateBarFillJob
		{
			healthSrc = GetComponentDataFromEntity<Health>(true),
			barType = hBarType,
			scaleType = GetArchetypeChunkComponentType<NonUniformScale>(false)
		};
		inputDeps = fillJob.Schedule(_fillQuery, inputDeps);
		return inputDeps;
	}
}

public struct HealthBar : IComponentData
{
	public enum BarType
	{
		BG,
		DecayFill,
		Fill,
	};
	public BarType type;
	public float3 offset;
	public float2 size;
	public Entity target;
}

public struct HealthBarFillTag : IComponentData
{

}
