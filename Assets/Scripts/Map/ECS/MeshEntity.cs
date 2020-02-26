using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(menuName = "ECS/Mesh Entity")]
public class MeshEntity : ScriptableObject
{
	public Mesh mesh;
	public Material material;
	public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
	public bool receiveShadows = true;
	public bool isStatic = true;
	public bool nonUniformScale = true;

	protected Entity _entity;

	public virtual Entity GetEntity()
	{
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		if (_entity == null || !em.Exists(_entity))
		{
			Debug.Log($"<b>Create Entity:</b> {name}");
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
#if DEBUG
			em.SetName(_entity, $"{this.GetType().Name}: {name}");
#endif
			return _entity;
		}
		return _entity;
	}

	protected virtual EntityArchetype GetArchetype() => 
		World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(isStatic ? GetComponents().Append(typeof(Static)).ToArray() : GetComponents().ToArray());

	public virtual IEnumerable<ComponentType> GetComponents()
	{
		return new ComponentType[]{
			typeof(Translation),
			typeof(LocalToWorld),
			nonUniformScale ? typeof(NonUniformScale) : typeof(Scale),
			typeof(RenderMesh),
			typeof(PerInstanceCullingTag),
			typeof(WorldRenderBounds),
			typeof(ChunkWorldRenderBounds),
			typeof(Disabled)
		};
	}

	public Entity Instantiate(float3 position) => Instantiate(position, Vector3.one);

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float3 scale)
	{
		var e = commandBuffer.Instantiate(GetEntity());
		commandBuffer.RemoveComponent(e, typeof(Disabled));
		commandBuffer.SetComponent(e, new Translation { Value = position });
		commandBuffer.SetComponent(e, new NonUniformScale { Value = scale });
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float scale)
	{
		var e = commandBuffer.Instantiate(GetEntity());
		commandBuffer.RemoveComponent(e, typeof(Disabled));
		commandBuffer.SetComponent(e, new Translation { Value = position });
		if(nonUniformScale)
			commandBuffer.SetComponent(e, new NonUniformScale { Value = new float3(scale, scale, scale) });
		else
			commandBuffer.SetComponent(e, new Scale { Value = scale });
		return e;
	}

	public Entity Instantiate(Vector3 position, Vector3 scale)
	{
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		var e = em.Instantiate(GetEntity());
		em.SetComponentData(e, new Translation { Value = position });
		em.SetComponentData(e, new NonUniformScale { Value = scale });
		em.RemoveComponent<Disabled>(e);
		return e;
	}

	public Entity Instantiate(Vector3 position, float scale)
	{
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		var e = em.Instantiate(GetEntity());
		em.SetComponentData(e, new Translation { Value = position });
		if (nonUniformScale)
			em.SetComponentData(e, new NonUniformScale { Value = new float3(scale, scale, scale) });
		else
			em.SetComponentData(e, new Scale { Value = scale });
		em.RemoveComponent<Disabled>(e);
		return e;
	}

	public void Instantiate(NativeArray<Entity> output)
	{
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		em.Instantiate(GetEntity(), output);
		em.RemoveComponent<Disabled>(output);
	}

}
