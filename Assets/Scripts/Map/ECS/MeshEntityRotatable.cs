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
}
