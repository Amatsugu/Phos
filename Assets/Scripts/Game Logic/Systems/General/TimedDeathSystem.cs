using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;

[BurstCompile]
public class TimedDeathSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref DeathTime t) =>
		{
			
			if (UnityEngine.Time.time >= t.Value)
				PostUpdateCommands.DestroyEntity(e);
		});
	}

}

public struct DeathTime : IComponentData
{
	public double Value;
}