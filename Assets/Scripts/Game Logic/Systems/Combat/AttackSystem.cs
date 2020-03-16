using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitAttackSystem : ComponentSystem
{
	private DynamicMeshEntity _bullet;
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
		Debug.Log("Attack System: Init");
		_physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_castHits = new NativeList<int>(Allocator.Persistent);
		var op = Addressables.LoadAssetAsync<DynamicMeshEntity>("EnemyProjectile");
		op.Completed += e =>
		{
			_bullet = e.Result;
			_state = 1;
		};
		_rand = new Unity.Mathematics.Random();
		_rand.InitState();

		_tranlationData = GetComponentDataFromEntity<Translation>();
		_healthData = GetComponentDataFromEntity<Health>();

		GameEvents.OnMapLoaded -= InitAttackSystem;
		GameEvents.OnMapDestroyed += OnDestroy;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_castHits.Dispose();
		GameEvents.OnMapDestroyed -= OnDestroy;
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
		_tranlationData = GetComponentDataFromEntity<Translation>();
		_healthData = GetComponentDataFromEntity<Health>();
		Entities.WithNone<Disabled>().ForEach((ref AttackSpeed s, ref Translation t, ref Projectile p, ref UnitId id) =>
		{
			if (s.NextAttackTime <= Time.ElapsedTime)
			{
				s.NextAttackTime = Time.ElapsedTime + s.Value;

				//Get Objects in Rect Range
				var ab = new AABB
				{
					Center = t.Value,
					Extents = new float3(5, 50, 5)
				};
				_physicsWorld.PhysicsWorld.CollisionWorld.OverlapAabb(new OverlapAabbInput
				{
					Aabb = new Aabb
					{
						Max = ab.Max,
						Min = ab.Min
					},
					Filter = new CollisionFilter
					{
						BelongsTo = 1u << (int)Faction.Phos,
						CollidesWith = ~0u,
						GroupIndex = 0
					}
				}, ref _castHits);
				//Get Circual Range
				for (int i = 0; i < _castHits.Length; i++)
				{
					if (_physicsWorld.PhysicsWorld.Bodies.Length <= _castHits[i])
						continue;
					var body = _physicsWorld.PhysicsWorld.Bodies[_castHits[i]];
					var entity = body.Entity;
					if (!(EntityManager.HasComponent<Health>(entity) && EntityManager.HasComponent<FactionId>(entity)))
						continue;
					if (EntityManager.GetComponentData<FactionId>(entity).Value != Faction.Phos)
						continue;
					var pos = EntityManager.GetComponentData<Translation>(entity).Value;
					var dir = pos - t.Value;
					var dist = math.lengthsq(dir);
					if(dist <= 25)
					{
						var turretDir = dir;
						turretDir.y = 0;
						Debug.Log(EntityManager.GetName(entity));
						EntityManager.SetComponentData(Map.ActiveMap.units[id.Value].HeadEntity, new Rotation { Value = quaternion.LookRotation(turretDir, Vector3.up) });
						dir = math.normalize(dir) * 5;
						var proj = _bullet.BufferedInstantiate(PostUpdateCommands, t.Value, quaternion.LookRotation(dir, Vector3.up), dir);
						PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 1 });
						break;
					}
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