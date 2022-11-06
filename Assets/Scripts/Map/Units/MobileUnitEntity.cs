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

using static UnityEngine.EventSystems.EventTrigger;

namespace Amatsugu.Phos.Units
{


	[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
	public class MobileUnitEntity : ScriptableObject
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
		public GameObject unitPrefab;
		public UnitDomain.Domain unitTargetingDomain;
		public UnitClass.Class unitClass;
		public Sprite icon;
		public GameObject projectilePrefab;
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes/UI/HealthBar", typeof(HealthBarDefination))]
		public HealthBarDefination healthBar;
		public float3 healthBarOffset;

		public virtual Entity InstantiateUnit(float3 position, DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands, Faction faction)
		{
			var prefabId = GameRegistry.PrefabDatabase[unitPrefab];
			var prefab = prefabs[prefabId];
			var unitInst = postUpdateCommands.Instantiate(prefab.value);
			postUpdateCommands.SetComponent(unitInst, new Translation { Value = position });
			postUpdateCommands.SetComponent(unitInst, new Rotation { Value = quaternion.identity });
			if(projectilePrefab != null)
				postUpdateCommands.AddComponent(unitInst, new Projectile { Value = prefabs[GameRegistry.PrefabDatabase[projectilePrefab]] });

			return unitInst;
		}

		protected virtual void PrepareUnitDomain(Entity entity, EntityCommandBuffer postUpdateCommands)
		{
			switch (unitClass)
			{
				case UnitClass.Class.Turret:
					postUpdateCommands.AddComponent(entity, new UnitClass.Turret());
					break;
				case UnitClass.Class.Artillery:
					postUpdateCommands.AddComponent(entity, new UnitClass.Artillery());
					break;
				case UnitClass.Class.Support:
					postUpdateCommands.AddComponent(entity, new UnitClass.Support());
					break;
				case UnitClass.Class.FixedGun:
					postUpdateCommands.AddComponent(entity, new UnitClass.FixedGun());
					break;
			}
			switch (unitDomain)
			{
				case UnitDomain.Domain.Air:
					postUpdateCommands.AddComponent(entity, new UnitDomain.Air());
					break;
				case UnitDomain.Domain.Land:
					postUpdateCommands.AddComponent(entity, new UnitDomain.Land());
					break;
				case UnitDomain.Domain.Naval:
					postUpdateCommands.AddComponent(entity, new UnitDomain.Naval());
					break;
			}
			postUpdateCommands.AddComponent(entity, new TargetingDomain
			{
				Value = unitTargetingDomain
			});
		}

		public virtual void PrepareComponentData(Entity entity, EntityCommandBuffer postUpdateCommands)
		{
			PrepareUnitDomain(entity, postUpdateCommands);

			postUpdateCommands.AddComponent(entity, new UnitId { Value = GameRegistry.UnitDatabase.entityIds[this] });
			postUpdateCommands.AddComponent(entity, new MoveSpeed { Value = moveSpeed });
			postUpdateCommands.AddComponent(entity, new Heading { Value = Vector3.forward });
			postUpdateCommands.AddComponent(entity, new AttackSpeed { Value = 1f / attackSpeed });
			postUpdateCommands.AddComponent(entity, new Health { maxHealth = maxHealth, Value = maxHealth });
			postUpdateCommands.AddComponent(entity, new AttackRange(attackRange));

			//GameRegistry.EntityManager.SetComponentData(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
		}

		public virtual List<GameObject> GetPrefabs()
		{
			var prefabs = new List<GameObject>();
			if (unitPrefab != null)
				prefabs.Add(unitPrefab);
			if (projectilePrefab != null)
				prefabs.Add(projectilePrefab);
			return prefabs;
		}

		public virtual void Init(Entity entity, EntityCommandBuffer postUpdateCommands)
		{

		}

		internal StringBuilder GetNameString() => GameRegistry.RarityColors.Colorize(name, tier);

		internal string GetCostString() => string.Empty;
	}
}