using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Rotatable Mesh Enity")]
public class MeshEntityRotatable : MeshEntity
{
	public override ComponentType[] GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(Rotation)
		}).ToArray();
	}

	public Entity Instantiate(Vector3 position, Vector3 scale, Quaternion rotation)
	{
		var e = Map.EM.Instantiate(GetEntity());
		Map.EM.SetComponentData(e, new Translation { Value = position });
		Map.EM.SetComponentData(e, new NonUniformScale { Value = scale });
		Map.EM.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}

	public Entity Instantiate(Vector3 position, Vector3 scale, Quaternion rotation, Entity parent)
	{
		var e = Instantiate(position, scale, parent);
		Map.EM.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}
}
