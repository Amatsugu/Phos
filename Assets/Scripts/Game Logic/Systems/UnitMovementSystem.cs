using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref Translation t, ref Heading h, ref MoveSpeed m) =>
		{
			t.Value +=  h.Value * m.Value * Time.deltaTime;
			t.Value.y = Map.ActiveMap.GetHeight(t.Value, 1);
		});
	}
}
