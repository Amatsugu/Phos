using AnimationSystem.AnimationData;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PhosCoreSystem : ComponentSystem
{

	private MeshEntityRotatable _bullet;
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
		};
	}

	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref PhosCore core, ref Translation t) =>
		{
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{
				var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2);

				for (int i = 0; i < 6; i++)
				{
					var curAngle = baseAngle + (math.PI / 3) * i;
					var dir = math.rotate(quaternion.RotateY(curAngle), Vector3.forward);
					Debug.DrawRay(t.Value + new float3(0, 10, 0), dir, Color.magenta);
					var proj = _bullet.BufferedInstantiate(PostUpdateCommands, t.Value + dir, Vector3.one);
					PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 5 });
					PostUpdateCommands.AddComponent(proj, new Velocity { Value = dir });

				}
				core.nextVolleyTime += core.fireRate;
			}
		});
	}
}


public struct PhosCore : IComponentData
{
	public float spinRate;
	public float fireRate;
	public double nextVolleyTime;
}