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
public class CenterOfMassSystem : JobComponentSystem
{
	public struct CenterOfMassJob : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;
		[ReadOnly] public ComponentTypeHandle<CenterOfMassOffset> centerOfMassOffsetType;
		public ComponentTypeHandle<CenterOfMass> centerOfMassType;
		public uint lastVersion;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			if (!(chunk.DidChange(translationType, lastVersion) && chunk.DidChange(centerOfMassOffsetType, lastVersion)))
				return;
			var positions = chunk.GetNativeArray(translationType);
			var offsets = chunk.GetNativeArray(centerOfMassOffsetType);
			var centerOfMasses = chunk.GetNativeArray(centerOfMassType);

			for (int i = 0; i < chunk.Count; i++)
			{
				centerOfMasses[i] = new CenterOfMass
				{
					Value = positions[i].Value + offsets[i].Value
				};
			}
		}
	}

	private EntityQuery _query;

	protected override void OnCreate()
	{
		base.OnCreate();
		var desc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadOnly<CenterOfMassOffset>(),
				typeof(CenterOfMass)
			}
		};
		_query = GetEntityQuery(desc);
		_query.SetChangedVersionFilter(new ComponentType[] { typeof(Translation), typeof(CenterOfMassOffset) });
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new CenterOfMassJob
		{
			centerOfMassOffsetType = GetComponentTypeHandle<CenterOfMassOffset>(true),
			translationType = GetComponentTypeHandle<Translation>(true),
			centerOfMassType = GetComponentTypeHandle<CenterOfMass>(false),
			lastVersion = LastSystemVersion
		};
		return job.Schedule(_query, inputDeps);
	}
}



public struct CenterOfMassOffset : IComponentData
{
	public float3 Value;
}

public struct CenterOfMass : IComponentData
{
	public float3 Value;
}