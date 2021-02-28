using Amatsugu.Phos.ECS;
using Amatsugu.Phos.UnitComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Units
{


	[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
	public class MobileUnitEntity : MeshEntityRotatable
	{
		public string description;
		[Header("Stats")]
		public float moveSpeed = 1;
		public float attackRange = 20;
		public float attackSpeed = 1;
		public float maxHealth;
		public int size;
		public float buildTime;
		[Header("Classification")]
		public int tier;
		public UnitDomain.Domain unitDomain;
		[EnumFlags]
		public UnitDomain.Domain unitTargetingDomain;
		public UnitClass.Class unitClass;
		public Sprite icon;
		[Header("Meshes")]
		public float3 centerOfMassOffset;
		public ConstructionMeshEntity constructionMesh;
		public SubMeshEntry[] subMeshes;
		public SubMeshIdentifier head;
		[CreateNewAsset("Assets/GameData/MapAssets/Projectiles", typeof(ProjectileMeshEntity))]
		public ProjectileMeshEntity projectile;
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes/UI/HealthBar", typeof(HealthBarDefination))]
		public HealthBarDefination healthBar;
		public float3 healthBarOffset;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Concat(new ComponentType[] {
			typeof(MoveSpeed),
			typeof(Heading),
			typeof(UnitId),
			typeof(Projectile),
			typeof(AttackSpeed),
			typeof(Health),
			typeof(FactionId),
			typeof(UnitState),
			typeof(PhysicsCollider),
			//typeof(PhysicsMass),
			typeof(AnimationSystem.AnimationData.Velocity),
			typeof(CenterOfMassOffset),
			typeof(CenterOfMass),
			typeof(TargetingDomain),
			typeof(AttackRange)
		});
		}

		public override void PrepareDefaultComponentData(Entity entity)
		{
			base.PrepareDefaultComponentData(entity);
			Map.EM.SetComponentData(entity, new MoveSpeed { Value = moveSpeed });
			Map.EM.SetComponentData(entity, new Heading { Value = Vector3.forward });
			Map.EM.SetComponentData(entity, new Projectile { Value = projectile.GetEntity() });
			Map.EM.SetComponentData(entity, new AttackSpeed { Value = 1f / attackSpeed });
			Map.EM.SetComponentData(entity, new Health { maxHealth = maxHealth, Value = maxHealth });
			//Map.EM.SetComponentData(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
			Map.EM.SetComponentData(entity, new CenterOfMassOffset { Value = centerOfMassOffset });
			Map.EM.SetComponentData(entity, new AttackRange(attackRange));

			switch (unitClass)
			{
				case UnitClass.Class.Turret:
					Map.EM.AddComponentData(entity, new UnitClass.Turret());
					break;
				case UnitClass.Class.Artillery:
					Map.EM.AddComponentData(entity, new UnitClass.Artillery());
					break;
				case UnitClass.Class.Support:
					Map.EM.AddComponentData(entity, new UnitClass.Support());
					break;
				case UnitClass.Class.FixedGun:
					Map.EM.AddComponentData(entity, new UnitClass.FixedGun());
					break;
			}
			switch (unitDomain)
			{
				case UnitDomain.Domain.Air:
					Map.EM.AddComponentData(entity, new UnitDomain.Air());
					break;
				case UnitDomain.Domain.Land:
					Map.EM.AddComponentData(entity, new UnitDomain.Land());
					break;
				case UnitDomain.Domain.Naval:
					Map.EM.AddComponentData(entity, new UnitDomain.Naval());
					break;
			}
			Map.EM.SetComponentData(entity, new TargetingDomain
			{
				Value = unitTargetingDomain
			});
		}

		public Entity Instantiate(float3 pos, Quaternion rotation, int id, Faction faction = Faction.None)
		{
			var e = Instantiate(pos, Vector3.one, rotation);
			Map.EM.SetComponentData(e, new UnitId { Value = id });
			Map.EM.SetComponentData(e, new FactionId { Value = faction });
			Map.EM.SetComponentData(e, new CenterOfMass { Value = pos + centerOfMassOffset });
			var collisionFilter = new CollisionFilter
			{
				CollidesWith = ~((uint)faction.Invert().AsCollisionLayer()),
				BelongsTo = (uint)(CollisionLayer.Unit | faction.AsCollisionLayer()),
				GroupIndex = 0
			};

			var physMat = Unity.Physics.Material.Default;
			physMat.CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents;

			Map.EM.SetComponentData(e, new PhysicsCollider
			{
				Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
				{
					Center = new float3(),
					Size = new float3(1, 1, 1),
					Orientation = quaternion.identity,
					BevelRadius = 0
				}, collisionFilter, physMat)
			});

			return e;
		}

		public NativeArray<Entity> InstantiateSubMeshes(quaternion rotation, Entity parent)
		{
			var e = new NativeArray<Entity>(subMeshes.Length, Allocator.Persistent);
			for (int i = 0; i < subMeshes.Length; i++)
			{
				var pos = subMeshes[i].offset; //math.rotate(rotation, subMeshes[i].offset);
				e[i] = subMeshes[i].mesh.Instantiate(pos, 1, rotation);
				Map.EM.AddComponent<LocalToParent>(e[i]);
			}
			for (int i = 0; i < subMeshes.Length; i++)
			{
#if UNITY_EDITOR
				if (subMeshes[i].parent.id == i)
					Debug.LogWarning($"{name} has a submesh [{subMeshes[i].mesh.name}] whose parent is assigned to itself");
#endif
				if (subMeshes[i].parent.id == -1 || subMeshes[i].parent.id == i)
					Map.EM.AddComponentData(e[i], new Parent { Value = parent });
				else
					Map.EM.AddComponentData(e[i], new Parent { Value = e[subMeshes[i].parent.id] });
			}
			return e;
		}

		internal StringBuilder GetNameString()
		{
			return GameRegistry.RarityColors.Colorize(name, tier);
		}

		internal string GetCostString()
		{
			return string.Empty;
		}
	}
}