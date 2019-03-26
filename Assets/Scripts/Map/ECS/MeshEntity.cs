using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Mesh Entity")]
public class MeshEntity : ScriptableObject
{
	public Mesh mesh;
	public Material material;

	protected Entity _entity;

	public virtual Entity GetEntity(bool localToParent = false)
	{
		var em = Map.EM;
		if (_entity == null || !em.Exists(_entity))
		{
			Debug.Log($"Create Entity {name}");
			var architype = GetArchetype();
			RenderMesh sharedMesh = new RenderMesh
			{
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				mesh = mesh,
				subMesh = 0,
				material = material,
				receiveShadows = true
			};

			_entity = em.CreateEntity(architype);
			em.SetSharedComponentData(_entity, sharedMesh);
			return _entity;
		}
		return _entity;
	}

	protected virtual EntityArchetype GetArchetype(bool localToParent = false)
	{
		return Map.EM.CreateArchetype(
				typeof(Translation),
				localToParent ? typeof(LocalToParent) : typeof(LocalToWorld),
				//typeof(ChunkWorldRenderBounds),
				typeof(NonUniformScale),
				typeof(RenderMesh),
				null
				);
	}

	public Entity Instantiate(Vector3 position) => Instantiate(position, Vector3.one);

	public Entity Instantiate(Vector3 position, Vector3 scale)
	{
		var e = Map.EM.Instantiate(GetEntity());
		Map.EM.SetComponentData(e, new Translation { Value = position });
		Map.EM.SetComponentData(e, new NonUniformScale { Value = scale });
		return e;
	}
}
