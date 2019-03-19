﻿using System.Collections;
using System.Collections.Generic;
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
			var architype = em.CreateArchetype(
				typeof(Translation),
				localToParent ? typeof(LocalToParent) : typeof(LocalToWorld),
				//typeof(ChunkWorldRenderBounds),
				typeof(NonUniformScale)
				);
			RenderMesh sharedMesh = new RenderMesh
			{
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				mesh = mesh,
				subMesh = 0,
				material = material,
				receiveShadows = true
			};

			_entity = em.CreateEntity(architype);
			em.AddSharedComponentData(_entity, sharedMesh);
			return _entity;
		}
		return _entity;
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