using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PhosCoreSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref PhosCore core, ref Translation t) =>
		{
			var baseAngle = (((float)Time.ElapsedTime % core.fireRate) / core.fireRate) * (math.PI * 2);

			for (int i = 0; i < 6; i++)
			{
				var curAngle = baseAngle + (math.PI/3) * i;
				Debug.DrawRay(t.Value + new float3(0, 10, 0), math.rotate(quaternion.RotateY(curAngle), Vector3.forward), Color.magenta);
			}
		});
	}
}


public struct PhosCore : IComponentData
{
	public float fireRate;
}