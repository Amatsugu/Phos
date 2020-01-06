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
				PostUpdateCommands.AddComponent(proj, new Acceleration { Value = new float3(0, -9.8f, 0) });
				PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 10 });
				var targetPoint = (float3)Map.ActiveMap.HQ.SurfacePoint;

				var dist = math.distance(t.Value, targetPoint);
				var angle = math.radians(45);
				var speed = math.sqrt((dist * 9.8f)/ math.sin(2*angle));

				var diff = targetPoint - t.Value;
				var aim = math.asin(diff.x/((diff.x * diff.x) + (diff.z * diff.z)));

				Debug.DrawRay(t.Value, Vector3.forward * diff.z, Color.blue, 1);
				Debug.DrawRay(t.Value, Vector3.right * diff.x, Color.red, 1);
				Debug.DrawRay(t.Value, Quaternion.Euler(0, aim * Mathf.Rad2Deg, 0) * Vector3.right * dist, Color.green, 1);
				//Debug.DrawRay(t.Value, math.rotate(quaternion.RotateY(aim), new float3(1,0,0)) * dist, Color.cyan, 1);

				var vel = new float3
				{
					x = speed * math.sin(angle),
					y = speed * math.cos(angle)
				};
				
				var vel2 = math.rotate(quaternion.RotateY(aim), vel);
				
				
				PostUpdateCommands.AddComponent(proj, new Velocity { Value = vel2 });
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
