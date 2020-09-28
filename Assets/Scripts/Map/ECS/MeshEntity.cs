using Amatsugu.Phos;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Mesh Entity")]
[System.Serializable]
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
		var em = Map.EM;
		if (_entity == null || !em.Exists(_entity))
		{
			UnityEngine.Debug.Log($"<b>Create Entity:</b> {name} [{GetType().Name}]");
			var architype = GetArchetype();

			_entity = em.CreateEntity(architype);
			PrepareDefaultComponentData(_entity);
			return _entity;
		}
#if DEBUG
		//PrepareDefaultComponentData(_entity);
#endif
		return _entity;
	}

	public virtual void PrepareDefaultComponentData(Entity entity)
	{
		var em = Map.EM;
		RenderMesh sharedMesh = new RenderMesh
		{
			castShadows = castShadows,
			mesh = mesh,
			subMesh = 0,
			material = material,
			receiveShadows = receiveShadows
		};
		em.SetSharedComponentData(entity, sharedMesh);
		em.SetComponentData(entity, new RenderBounds
		{
			Value = new AABB
			{
				Center = mesh.bounds.center,
				Extents = mesh.bounds.extents
			}
		});
#if DEBUG
		em.SetName(entity, $"{this.GetType().Name}: {name}");
#endif
	}

	protected virtual EntityArchetype GetArchetype() =>
		Map.EM.CreateArchetype(isStatic ? GetComponents().Append(typeof(Static)).ToArray() : GetComponents().ToArray());

	public virtual IEnumerable<ComponentType> GetComponents()
	{
		return new ComponentType[]{
			typeof(Translation),
			nonUniformScale ? typeof(NonUniformScale) : typeof(Scale),
			typeof(RenderMesh),
			typeof(LocalToWorld),
			typeof(PerInstanceCullingTag),
			typeof(WorldRenderBounds),
			typeof(ChunkWorldRenderBounds),
			typeof(RenderBounds),
			typeof(Disabled),
		};
	}

	public Entity Instantiate(float3 position) => Instantiate(position, Vector3.one);

	public virtual Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float3 scale)
	{
		var e = commandBuffer.Instantiate(GetEntity());
		commandBuffer.RemoveComponent(e, typeof(Disabled));
		commandBuffer.SetComponent(e, new Translation { Value = position });
		
		if (nonUniformScale)
			commandBuffer.SetComponent(e, new NonUniformScale { Value = scale });
		else
			commandBuffer.SetComponent(e, new Scale { Value = scale.x });
		return e;
	}

	public virtual Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float scale)
	{
		var e = commandBuffer.Instantiate(GetEntity());
		commandBuffer.RemoveComponent(e, typeof(Disabled));
		commandBuffer.SetComponent(e, new Translation { Value = position });
		if (nonUniformScale)
			commandBuffer.SetComponent(e, new NonUniformScale { Value = new float3(scale, scale, scale) });
		else
			commandBuffer.SetComponent(e, new Scale { Value = scale });
		return e;
	}

	public virtual Entity Instantiate(float3 position, float3 scale)
	{
		var em = Map.EM;
		var e = em.Instantiate(GetEntity());
		em.SetComponentData(e, new Translation { Value = position });
		if (nonUniformScale)
			em.SetComponentData(e, new NonUniformScale { Value = scale });
		else
			em.SetComponentData(e, new Scale { Value = scale.x });
		em.RemoveComponent<Disabled>(e);
		return e;
	}

	public virtual void Instantiate(NativeArray<Entity> output)
	{
		var em = Map.EM;
		em.Instantiate(GetEntity(), output);
		em.RemoveComponent<Disabled>(output);
	}
}