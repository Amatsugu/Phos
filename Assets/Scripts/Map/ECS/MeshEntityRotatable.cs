using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Rotatable Mesh Enity")]
public class MeshEntityRotatable : MeshEntity
{
	protected override EntityArchetype GetArchetype(bool localToParent = false) => Map.EM.CreateArchetype(
				typeof(Translation),
				typeof(Rotation),
				localToParent ? typeof(LocalToParent) : typeof(LocalToWorld),
				typeof(NonUniformScale),
				typeof(RenderMesh),
				typeof(PerInstanceCullingTag)
				);

	public Entity Instantiate(Vector3 position, Vector3 scale, Quaternion rotation)
	{
		var e = Map.EM.Instantiate(GetEntity());
		Map.EM.SetComponentData(e, new Translation { Value = position });
		Map.EM.SetComponentData(e, new NonUniformScale { Value = scale });
		Map.EM.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}
}
