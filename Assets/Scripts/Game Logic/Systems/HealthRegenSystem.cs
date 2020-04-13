using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class HealthRegenSystem : JobComponentSystem
{
	private struct RegenJob : IJobChunk
	{
		public ArchetypeChunkComponentType<Health> healthType;
		[ReadOnly] public ArchetypeChunkComponentType<HealthRegen> regenType;
		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var healths = chunk.GetNativeArray(healthType);
			var regens = chunk.GetNativeArray(regenType);
			for (int i = 0; i < chunk.Count; i++)
			{
				var h = healths[i];
				if (h.Value < h.maxHealth)
					h.Value = math.min(h.maxHealth, h.Value + regens[i].Value);
			}
		}
	}

	private EntityQuery _entityQuery;

	protected override void OnCreate()
	{
		base.OnCreate();
		_entityQuery = GetEntityQuery(typeof(Health), ComponentType.ReadOnly<HealthRegen>());
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new RegenJob
		{
			healthType = GetArchetypeChunkComponentType<Health>(false),
			regenType = GetArchetypeChunkComponentType<HealthRegen>(true)
		};
		return job.Schedule(_entityQuery, inputDeps);
	}
}