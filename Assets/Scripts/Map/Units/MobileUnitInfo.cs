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
		});
	}


	public Entity Instantiate(Vector3 pos, Quaternion rotation, int id)
	{
		var e = Instantiate(pos + Vector3.up * 5, Vector3.one, rotation);
		Map.EM.SetComponentData(e, new MoveSpeed { Value = moveSpeed });
		Map.EM.SetComponentData(e, new Heading { Value = Vector3.forward });
		Map.EM.SetComponentData(e, new UnitId { Value = id });
		Map.EM.SetComponentData(e, new Projectile { Value = projectile.GetEntity() });
		Map.EM.SetComponentData(e, new AttackSpeed { Value = attackSpeed });

		Map.EM.AddComponentData(e, new PhysicsCollider
		{
			Value = Unity.Physics.BoxCollider.Create(new BoxGeometry { Center = default, Size = new float3(1, 1, 1), Orientation = quaternion.identity }, CollisionFilter.Default, Unity.Physics.Material.Default)
		});

		Map.EM.AddComponentData(e, new PhysicsVelocity
		{
			Linear = new float3()
		});

		Map.EM.AddComponentData(e, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1));
		

		return e;
	}
}
