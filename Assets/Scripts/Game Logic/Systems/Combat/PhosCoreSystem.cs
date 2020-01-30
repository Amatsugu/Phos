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
	private bool _isReady = false;
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
			_isReady = true;
		};
	}

	protected override void OnUpdate()
	{
		if (!_isReady)
			return;
		Entities.WithNone<Disabled>().ForEach((Entity e, ref PhosCore core, ref HexPosition p) =>
		{
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{
				var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2);

				var t = Map.ActiveMap[p.coords];
				for (int i = 0; i < 6; i++)
				{
					var curAngle = baseAngle + (math.PI / 3) * i;
					var dir = math.rotate(quaternion.RotateY(curAngle), Vector3.forward);
					Debug.DrawRay(t.SurfacePoint + new Vector3(0, 10, 0), dir, Color.magenta);
					var proj = _bullet.BufferedInstantiate(PostUpdateCommands, (float3)t.SurfacePoint + dir + new float3(0,3,0), Vector3.one);
					PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 30 });
					PostUpdateCommands.AddComponent(proj, new Velocity { Value = dir * 10 });

				}
				core.nextVolleyTime = Time.ElapsedTime + core.fireRate;
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