using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
public class MobileUnitInfo : MeshEntityRotatable
{

	public enum UnitType
	{
		Land,
		Air,
		Naval
	}

	public float moveSpeed = 1;
	public float attackSpeed = 1;
	public float maxHealth;
	public int size;
	public UnitType type;
	public MeshEntityRotatable projectile;

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
		});
	}


	public Entity Instantiate(Vector3 pos, Quaternion rotation, int id, Faction faction = Faction.None)
	{
		var e = Instantiate(pos, Vector3.one, rotation);
		Map.EM.SetComponentData(e, new MoveSpeed { Value = moveSpeed });
		Map.EM.SetComponentData(e, new Heading { Value = Vector3.forward });
		Map.EM.SetComponentData(e, new UnitId { Value = id });
		Map.EM.SetComponentData(e, new Projectile { Value = projectile.GetEntity() });
		Map.EM.SetComponentData(e, new AttackSpeed { Value = attackSpeed });
		Map.EM.SetComponentData(e, new Health { maxHealth = maxHealth, Value = maxHealth });
		Map.EM.SetComponentData(e, new FactionId { Value = faction });

		Map.EM.AddComponentData(e, new PhysicsCollider
		{
			Value = Unity.Physics.BoxCollider.Create(new BoxGeometry 
			{ 
				Center = new float3(),
				Size = new float3(1, 1, 1),
				Orientation = quaternion.identity,
				BevelRadius = 0 
			}, CollisionFilter.Default, new Unity.Physics.Material
			{
				Flags = Unity.Physics.Material.MaterialFlags.EnableCollisionEvents
			})
		});
		Map.EM.AddComponentData(e, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1));
		

		return e;
	}
}
