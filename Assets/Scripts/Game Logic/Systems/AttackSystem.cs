using AnimationSystem.AnimationData;
using AnimationSystem.Animations;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitAttackSystem : ComponentSystem
{
	private MeshEntityRotatable _bullet;
	private Unity.Mathematics.Random _rand;
	protected override void OnStartRunning()
	{
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
		};
		_rand = new Unity.Mathematics.Random();
		_rand.InitState();
	}

	protected override void OnUpdate()
	{
		Entities.ForEach((ref AttackSpeed s, ref Translation t, ref Projectile p) => {
			if (Time.ElapsedTime >= s.Value)
			{
				var proj = _bullet.BufferedInstantiate(PostUpdateCommands, t.Value, Vector3.one);
				/*PostUpdateCommands.AddComponent(proj, new SeekTarget 
				{ 
					Value = (Map.ActiveMap.HQ.SurfacePoint + Vector3.up * 5),
					MaxAccel = 5
				});*/
				PostUpdateCommands.AddComponent(proj, new Acceleration { Value = new float3(0, -9.8f, 0) });
				PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 10 });
				var dist = math.distance(t.Value, Map.ActiveMap.HQ.SurfacePoint + Vector3.up * 5);
				var angle = math.radians(45);
				var speed = math.sqrt((dist * 9.8f)/ math.sin(angle));
				var vel = new float3
				{
					x = speed * math.cos(angle),
					y = speed * math.sin(angle)
				};
				PostUpdateCommands.AddComponent(proj, new Velocity { Value = vel });
				s.Value = (float)Time.ElapsedTime + 1;
			}

		});
		
	}
}

public struct AttackSpeed : IComponentData
{
	public float Value;
	public override bool Equals(object obj) => Value.Equals(obj);
	public override int GetHashCode() => Value.GetHashCode();
	public static bool operator ==(AttackSpeed left, AttackSpeed right) => left.Equals(right);
	public static bool operator !=(AttackSpeed left, AttackSpeed right) => !(left == right);
}

public struct Projectile : IComponentData
{
	public Entity Value;
	public override bool Equals(object obj) => Value.Equals(obj);
	public override int GetHashCode() => Value.GetHashCode();
	public static bool operator ==(Projectile left, Projectile right) => left.Equals(right);
	public static bool operator !=(Projectile left, Projectile right) => !(left == right);
}
