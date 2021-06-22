using Amatsugu.Phos;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Healthbar Mesh Enity")]
public class HealthBarEntity : MeshEntityRotatable
{
	[Header("HealthBar")]
	public float2 size;

	public override IEnumerable<ComponentType> GetComponents()
	{
		var c = base.GetComponents().Concat(new ComponentType[]{
			typeof(HealthBar),
		});
		return c;
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
	}

	public Entity Instantiate(Entity target, HealthBar.BarType type, float3 offset)
	{
		var e = Instantiate(float3.zero, new float3(size,1), quaternion.identity);
		GameRegistry.EntityManager.SetComponentData(e, new HealthBar
		{
			target = target,
			type = type,
			offset = offset,
			size = size
		});
		if (type != HealthBar.BarType.BG)
			GameRegistry.EntityManager.AddComponent<HealthBarFillTag>(e);
		return e;
	}
}