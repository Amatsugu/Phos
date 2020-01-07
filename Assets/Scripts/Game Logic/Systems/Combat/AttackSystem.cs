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
				PostUpdateCommands.AddComponent(proj, new Gravity { Value = 9.8f });
				PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 15 });
				var targetPoint = (float3)Map.ActiveMap.HQ.SurfacePoint;

				PostUpdateCommands.AddComponent(proj, new Velocity { Value = GetAttackVector(t.Value.x, targetPoint) });
				s.Value += 1;
			}

		});
	}

	public static float3 GetAttackVector(float3 pos, float3 target, float fireAngle = 45)
	{
		var dist = math.distance(pos, target);

		var angle = math.radians(fireAngle);

		float g = 9.8f;
		var diff = target - pos;
		

		float tanG = math.tan(angle);
		float upper = math.sqrt(g) * math.sqrt(dist) * math.sqrt(tanG * tanG + 1.0f);
		float lower = math.sqrt(2 * tanG - ((2 * diff.y) / dist));

		float v = upper / lower;

		var vel = new float3
		{
			z = v * math.cos(angle),
			y = v * math.sin(angle)
		};
		diff.y = 0;
		return vel;
		//return math.rotate(quaternion.LookRotation(diff, Vector3.up), vel);
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
