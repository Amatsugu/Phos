using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/TileInfo")]
public class TileInfo : ScriptableObject
{
	public GameObject tilePrefab;
	public Mesh mesh;
	public Material material;

	private Entity _entity;

	public Entity GetEntity(EntityManager em)
	{
		if(_entity == null || !em.Exists(_entity))
		{
			Debug.Log($"Create Entity {name}");
			var architype = em.CreateArchetype(
				typeof(Translation),
				typeof(Rotation),
				typeof(PerInstanceCullingTag),
				typeof(LocalToWorld),
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
}
