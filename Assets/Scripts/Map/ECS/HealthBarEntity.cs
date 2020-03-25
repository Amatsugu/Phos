using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Rotatable Mesh Enity")]
public class HealthBarEntity : MeshEntityRotatable
{
	[Header("HealthBar")]
	public bool isFill;

	public override IEnumerable<ComponentType> GetComponents()
	{
		var c = base.GetComponents().Concat(new ComponentType[]{
			typeof(HealthBar),
		});
		if (isFill)
			c.Append(typeof(HealthBarFillTag));
		return c;
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
	}

	public Entity Instantiate(Entity target)
	{
		var e = Instantiate(0, 0, quaternion.identity);
		Map.EM.SetComponentData(e, new HealthBar { target = target });
		return e;
	}

}