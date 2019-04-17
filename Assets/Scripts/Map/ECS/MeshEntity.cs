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
	public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
	public bool receiveShadows = true;

	protected Entity _entity;

	public virtual Entity GetEntity()
	{
		var em = World.Active.EntityManager;
		if (_entity == null || !em.Exists(_entity))
		{
			//Debug.Log($"Create Entity {name}");
			var architype = GetArchetype();
			RenderMesh sharedMesh = new RenderMesh
			{
				castShadows = castShadows,
				mesh = mesh,
				subMesh = 0,
				material = material,
				receiveShadows = receiveShadows
			};

			_entity = em.CreateEntity(architype);
			em.SetSharedComponentData(_entity, sharedMesh);
			return _entity;
		}
		return _entity;
	}

	protected virtual EntityArchetype GetArchetype() => World.Active.EntityManager.CreateArchetype(GetComponents());

	public virtual ComponentType[] GetComponents()
	{
		return new ComponentType[]{
			typeof(Translation),
			typeof(LocalToWorld),
			typeof(NonUniformScale),
			typeof(RenderMesh),
			typeof(PerInstanceCullingTag),
		};
	}

	public Entity Instantiate(Vector3 position) => Instantiate(position, Vector3.one);

	public Entity Instantiate(Vector3 position, Vector3 scale)
	{
		var em = World.Active.EntityManager;
		var e = em.Instantiate(GetEntity());
		em.SetComponentData(e, new Translation { Value = position });
		em.SetComponentData(e, new NonUniformScale { Value = scale });
		return e;
	}

	public Entity Instantiate(Vector3 position, Vector3 scale, Entity parent)
	{
		var em = World.Active.EntityManager;
		var e = Map.EM.Instantiate(GetEntity());
		em.SetComponentData(e, new NonUniformScale { Value = scale });
		em.AddComponent(e, typeof(LocalTranslation));
		em.SetComponentData(e, new LocalTranslation { Value = position });
		em.AddComponent(e, typeof(ChildOf));
		em.SetComponentData(e, new ChildOf { parent = parent });
		return e;
	}
}
