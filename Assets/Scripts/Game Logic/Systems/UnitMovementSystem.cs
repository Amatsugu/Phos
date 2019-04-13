using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : ComponentSystem
{
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
	}

	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref Translation t, ref Heading h, ref MoveSpeed m, ref Destination d) =>
		{
			h.Value = math.normalizesafe(d.Value - t.Value);
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
			t.Value +=  h.Value * m.Value * Time.deltaTime;
			t.Value.y = Map.ActiveMap.GetHeight(t.Value, 1);
		});


	}
}
