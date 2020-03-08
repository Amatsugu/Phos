using Unity.Burst;
using Unity.Entities;

using UnityEngine;

[BurstCompile]
public class DeathSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref Health health, ref UnitId id) =>
		{
			if (health.Value > 0)
				return;
			Debug.Log($"Killing {id.Value}");
			Map.ActiveMap.units[id.Value].Die();
		});
	}
}