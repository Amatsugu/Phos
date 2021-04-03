using Amatsugu.Phos;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Rotatable Mesh Enity")]
[Serializable]
public class MeshEntityRotatable : MeshEntity
{
	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(Rotation)
		});
	}

	public Entity Instantiate(float3 position, float3 scale, Quaternion rotation)
	{
		var e = Instantiate(position, scale);
		GameRegistry.EntityManager.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float3 scale, quaternion rotation)
	{
		var e = BufferedInstantiate(commandBuffer, position, scale);
		commandBuffer.SetComponent(e, new Translation { Value = position });
		commandBuffer.SetComponent(e, new Rotation { Value = rotation });
		return e;
	}

	public Entity Instantiate(float3 position, float scale, quaternion rotation)
	{
		var e = Instantiate(position, scale);
		GameRegistry.EntityManager.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, float3 position, float scale, quaternion rotation)
	{
		var e = BufferedInstantiate(commandBuffer, position, scale);
		commandBuffer.SetComponent(e, new Translation { Value = position });
		commandBuffer.SetComponent(e, new Rotation { Value = rotation });
		return e;
	}

	/*public Entity Instantiate(Vector3 position, Vector3 scale, Quaternion rotation, Entity parent)
	{
		var e = Instantiate(position, scale, parent);
		Map.EM.SetComponentData(e, new Rotation { Value = rotation });
		return e;
	}*/
}