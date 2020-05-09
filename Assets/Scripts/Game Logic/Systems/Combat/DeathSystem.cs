using Unity.Burst;
using Unity.Entities;

using UnityEngine;

[BurstCompile]
public class DeathSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.WithNone<Disabled>().ForEach((Entity e, ref Health health, ref UnitId id) =>
		{
			if (health.Value > 0 || health.maxHealth == 0)
				return;
			Debug.Log($"Unit [{id.Value}] dying");
			GameRegistry.GameMap.units[id.Value].Die();
		});
		Entities.WithNone<Disabled>().WithAll<BuildingId>().ForEach((Entity e, ref Health health, ref HexPosition pos) =>
		{
			if (!pos.Value.isCreated)
				return;
			if (health.Value > 0 || health.maxHealth == 0)
				return;
			Debug.Log($"Building {pos.Value} dying");
			((BuildingTile)GameRegistry.GameMap[pos.Value]).Die();
		});
	}
}