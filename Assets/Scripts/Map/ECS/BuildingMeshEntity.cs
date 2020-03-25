using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingMeshEntity : MeshEntityRotatable
{

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]
		{
			typeof(Health),
			typeof(BuildingId),
			typeof(FactionId),
			typeof(BuildingOffTag),
		});
	}

	/*
	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
	}
	*/

	public Entity Instantiate(float3 position, quaternion rotation, int id, float maxHealth, Faction faction)
	{
		var e = Instantiate(position, 1, rotation);
		Map.EM.SetComponentData(e, new Health
		{
			Value = maxHealth,
			maxHealth = maxHealth
		});
		Map.EM.SetComponentData(e, new BuildingId {	Value = id });
		Map.EM.SetComponentData(e, new FactionId { Value = faction });
		Map.EM.RemoveComponent<BuildingOffTag>(e);
		return e;
	}
}
