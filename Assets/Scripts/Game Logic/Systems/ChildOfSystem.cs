using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChildOfSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		/*Entities.ForEach((Entity e, ref ChildOf c, ref LocalTranslation lT, ref ParentHeigharcy p, ref Translation t) =>{
			t.Value = p.totalOfset + lT.position;
			if (EntityManager.HasComponent<FrozenRenderSceneTag>(c.parent) && !EntityManager.HasComponent<FrozenRenderSceneTag>(e))
				PostUpdateCommands.AddSharedComponent(e, new FrozenRenderSceneTag());
			else
				PostUpdateCommands.RemoveComponent(e, typeof(FrozenRenderSceneTag));
		});

		Entities.WithNone<ParentHeigharcy>().ForEach((Entity e, ref ChildOf c) =>
		{
			PostUpdateCommands.AddComponent(e, DetermineHeigharcy(c.parent));
		});
		*/

	}

	ParentHeigharcy DetermineHeigharcy(Entity parent)
	{
		float3 totalOffset = new float3(0,0,0);
		while(EntityManager.HasComponent<ChildOf>(parent))
		{
			var c = EntityManager.GetComponentData<ChildOf>(parent);
			var p = EntityManager.GetComponentData<LocalTranslation>(parent);
			totalOffset -= p.position;
			parent = c.parent;
		}
		return new ParentHeigharcy
		{
			topParent = parent,
			totalOfset = totalOffset
		};
	}
}

public struct LocalTranslation : IComponentData
{
	public float3 position;
}

public struct ChildOf : IComponentData
{
	public Entity parent;
}

public struct ParentHeigharcy : IComponentData
{
	public Entity topParent;
	public float3 totalOfset;
}
