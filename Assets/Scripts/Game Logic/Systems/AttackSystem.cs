using AnimationSystem.AnimationData;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitAttackSystem : ComponentSystem
{
	private MeshEntityRotatable _bullet;
	protected override void OnStartRunning()
	{
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
		};
	}

	protected override void OnUpdate()
	{
		Entities.ForEach((ref AttackSpeed s, ref Translation t, ref Projectile p) => {
			if (Time.ElapsedTime >= s.Value)
			{
				var proj = _bullet.BufferedInstantiate(PostUpdateCommands, t.Value, Vector3.one);
				PostUpdateCommands.AddComponent(proj, new Velocity { Value = ((Map.ActiveMap.HQ.SurfacePoint + Vector3.up * 5) - (Vector3)t.Value).normalized * 5});
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
