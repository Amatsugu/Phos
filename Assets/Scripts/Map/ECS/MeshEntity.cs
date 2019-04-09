using System;
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

	public virtual Entity GetEntity()
	{
		var em = Map.EM;
		if (_entity == null || !em.Exists(_entity))
		{
			//Debug.Log($"Create Entity {name}");
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

	protected virtual EntityArchetype GetArchetype() => Map.EM.CreateArchetype(GetComponents());

	public virtual ComponentType[] GetComponents()
	{
		return new ComponentType[]{
			typeof(Translation),
			typeof(LocalToWorld),
			typeof(NonUniformScale),
			typeof(RenderMesh),
			typeof(PerInstanceCullingTag)
		};
	}

	public Entity Instantiate(Vector3 position) => Instantiate(position, Vector3.one);

	public Entity Instantiate(Vector3 position, Vector3 scale)
	{
		var e = Map.EM.Instantiate(GetEntity());
		Map.EM.SetComponentData(e, new Translation { Value = position });
		Map.EM.SetComponentData(e, new NonUniformScale { Value = scale });
		return e;
	}

	public Entity Instantiate(Vector3 position, Vector3 scale, Entity parent)
	{
		var e = Map.EM.Instantiate(GetEntity());
		Map.EM.SetComponentData(e, new NonUniformScale { Value = scale });
		Map.EM.AddComponent(e, typeof(LocalTranslation));
		Map.EM.SetComponentData(e, new LocalTranslation { Value = position });
		Map.EM.AddComponent(e, typeof(ChildOf));
		Map.EM.SetComponentData(e, new ChildOf { parent = parent });
		return e;
	}
}
