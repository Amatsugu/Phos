using Amatsugu.Phos;
using Amatsugu.Phos.UnitComponents;

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Profiling;

[BurstCompile]
public class UnitAttackSystem : ComponentSystem
{
	private Unity.Mathematics.Random _rand;
	private int _state = 0;
	private BuildPhysicsWorld _physicsWorld;
	private NativeList<int> _castHits;

	[ReadOnly]
	private ComponentDataFromEntity<Translation> _tranlationData;

	private ComponentDataFromEntity<Health> _healthData;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnMapLoaded += InitAttackSystem;
		_castHits = new NativeList<int>(Allocator.Persistent);
	}

	protected void InitAttackSystem()
	{
		_physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_rand = new Unity.Mathematics.Random();
		_rand.InitState();

		_tranlationData = GetComponentDataFromEntity<Translation>();
		_healthData = GetComponentDataFromEntity<Health>();

		GameEvents.OnMapLoaded -= InitAttackSystem;
		GameEvents.OnMapRegen += OnRegen;
		_state = 1;
	}

	private void OnRegen()
	{
		GameEvents.OnMapLoaded += InitAttackSystem;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameEvents.OnMapRegen -= OnRegen;
		_castHits.Dispose();
	}


	protected override void OnUpdate()
	{
		switch (_state)
		{
			case 0: //Idle
				break;

			case 1:
				AttackAI();
				break;
		}
	}

	private void AttackAI()
	{
		_castHits.Clear();
		//SelectTarget();
		RotateTurretAndShootAI();
	}

	private void RotateTurretAndShootAI()
	{
		Entities.WithNone<Disabled>().ForEach((Entity e, ref Translation t, ref Projectile projectile, ref AttackSpeed atkSpeed, ref AttackRange range, ref AttackTarget atkTarget, ref UnitHead head) =>
		{
			if (!EntityManager.Exists(atkTarget.Value))
			{
				PostUpdateCommands.RemoveComponent<AttackTarget>(e);
				return;
			}
			var pos = EntityManager.GetComponentData<CenterOfMass>(atkTarget.Value).Value;
			var dir = t.Value - pos;
			var curRot = EntityManager.GetComponentData<Rotation>(head.Value).Value;
			var aimDir = new float3(dir.x, 0, dir.z);
			var desRot = quaternion.LookRotation(aimDir, math.up());
			var aimRot = Quaternion.RotateTowards(curRot, desRot, 180 * Time.DeltaTime);
			PostUpdateCommands.SetComponent(head.Value, new Rotation
			{
				Value = aimRot
			});
			//if (aimRot != desRot)
			//	return;
			var dist = math.length(dir);
			if (atkSpeed.NextAttackTime > Time.ElapsedTime)
				return;
			atkSpeed.NextAttackTime = Time.ElapsedTime + atkSpeed.Value;
			dir = math.normalize(dir) * -20;

			var proj = ProjectileMeshEntity.ShootProjectile(PostUpdateCommands, projectile.Value, t.Value + new float3(0, 1, 0), dir, Time.ElapsedTime + 3);
		});
	}

	private void SelectTarget()
	{
		Entities.WithNone<Disabled, AttackTarget>().ForEach((Entity e, ref Translation t, ref UnitState state, ref FactionId faction, ref AttackSpeed atkSpeed, ref AttackRange range) =>
		{
			if (state.Value != UnitState.State.AttackOnSight)
				return;
			/*if (atkSpeed.NextAttackTime > Time.ElapsedTime)
				return;*/
			atkSpeed.NextAttackTime = Time.ElapsedTime + atkSpeed.Value;
			//Get Objects in Rect Range
			_physicsWorld.AABBCast(t.Value, range.MaxRange, new CollisionFilter
			{
				BelongsTo = (uint)faction.Value.Invert().AsCollisionLayer(),
				CollidesWith = ((uint)(CollisionLayer.Building | CollisionLayer.Unit)),
				GroupIndex = 0
			}, ref _castHits);
			//Get Circual Range
			for (int i = 0; i < _castHits.Length; i++)
			{
				if (_physicsWorld.PhysicsWorld.Bodies.Length <= _castHits[i])
					continue;

				var target = _physicsWorld.PhysicsWorld.Bodies[_castHits[i]].Entity;
				if (!EntityManager.HasComponent<Health>(target))
					continue;

				var pos = EntityManager.GetComponentData<CenterOfMass>(target).Value;
				var dir = t.Value - pos;
				var dist = math.length(dir);
				if (dist <= range.MaxRange && dist >= range.MinRange)
				{
					var turretDir = dir;
					turretDir.y = 0;
					PostUpdateCommands.AddComponent(e, new AttackTarget
					{
						Value = target
					});

					break;
				}
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

public struct MoveToTarget : IComponentData
{

}

public struct AttackTarget : IComponentData
{
	public Entity Value;
}

public struct AttackRange : IComponentData
{
	public AttackRange(float max) : this(0, max)
	{

	}

	public AttackRange(float min, float max)
	{
		MinRange = min;
		MaxRange = max;
	}

	public float MaxRange;
	public float MinRange;

	public bool IsInRange(float3 a, float3 b)
	{
		var dist = math.length(a - b);
		return IsInRange(dist);
	}

	public bool IsInRange(float dist) => dist >= MinRange && dist <= MaxRange;
}

public struct AttackSpeed : IComponentData, IEquatable<AttackSpeed>
{
	public float Value;
	public double NextAttackTime;

	public bool Equals(AttackSpeed other) => Value == other.Value;// && NextAttackTime == other.NextAttackTime;

	public override bool Equals(object obj) => obj is AttackSpeed atk ? Equals(atk) : false;

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