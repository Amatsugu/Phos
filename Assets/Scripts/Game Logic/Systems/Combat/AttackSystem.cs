using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

[BurstCompile]
public class UnitAttackSystem : ComponentSystem
{
	private ProjectileMeshEntity _bullet;
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
	}

	protected void InitAttackSystem()
	{
		_physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_castHits = new NativeList<int>(Allocator.Persistent);
		var op = Addressables.LoadAssetAsync<ProjectileMeshEntity>("PlayerProjectile");
		op.Completed += e =>
		{
			if (e.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				_bullet = e.Result;
				_state = 1;
			}
		};
		_rand = new Unity.Mathematics.Random();
		_rand.InitState();

		_tranlationData = GetComponentDataFromEntity<Translation>();
		_healthData = GetComponentDataFromEntity<Health>();

		GameEvents.OnMapLoaded -= InitAttackSystem;
		GameEvents.OnMapRegen += OnRegen;
		GameEvents.OnMapDestroyed += Destroy;
	}

	private void OnRegen()
	{
		GameEvents.OnMapLoaded += InitAttackSystem;
	}

	protected void Destroy()
	{
		_castHits.Dispose();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameEvents.OnMapRegen -= OnRegen;
		GameEvents.OnMapDestroyed -= Destroy;
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
		SelectTarget();
		RotateTurretAndShootAI();
	}

	private void RotateTurretAndShootAI()
	{
		Entities.WithNone<Disabled>().ForEach((Entity e, ref Translation t, ref AttackSpeed atkSpeed, ref AttackTarget atkTarget) =>
		{
			if (!EntityManager.Exists(atkTarget.Value))
				return;
			var pos = EntityManager.GetComponentData<CenterOfMass>(atkTarget.Value).Value;
			var dir = t.Value - pos;
			var dist = math.lengthsq(dir);
			if (dist > 20 * 20)
			{
				PostUpdateCommands.RemoveComponent<AttackTarget>(e);
				return;
			}
			if (atkSpeed.NextAttackTime > Time.ElapsedTime)
				return;
			atkSpeed.NextAttackTime = Time.ElapsedTime + atkSpeed.Value;
			//PostUpdateCommands.SetComponent(Map.ActiveMap.units[id.Value].HeadEntity, new Rotation { Value = quaternion.LookRotation(turretDir, Vector3.up) });
			dir = math.normalize(dir) * -20;
			var proj = _bullet.BufferedInstantiate(PostUpdateCommands, t.Value + new float3(0, 1, 0), scale: 0.2f, velocity: dir);
			PostUpdateCommands.AddComponent(proj, new DeathTime { Value = Time.ElapsedTime + 3 });
		});
	}

	private void SelectTarget()
	{
		int range = 20;
		Entities.WithNone<Disabled, AttackTarget>().ForEach((Entity e, ref Translation t, ref FactionId faction, ref AttackSpeed atkSpeed) =>
		{
			/*if (atkSpeed.NextAttackTime > Time.ElapsedTime)
				return;*/
			atkSpeed.NextAttackTime = Time.ElapsedTime + atkSpeed.Value;
			//Get Objects in Rect Range
			_physicsWorld.AABBCast(t.Value, new float3(range, range, range), new CollisionFilter
			{
				BelongsTo = 1u << (int)faction.Value,
				CollidesWith = ~((1u << (int)faction.Value) | (1u << (int)Faction.None) | (1u << (int)Faction.PlayerProjectile) | (1u << (int)Faction.PhosProjectile) | (1u << (int)Faction.Tile) | (1u << (int)Faction.Unit)),
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
				var dist = math.lengthsq(dir);
				if (dist <= range * range)
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

public struct AttackTarget : IComponentData
{
	public Entity Value;
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