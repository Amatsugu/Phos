using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ChildOfSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.WithNone<Disabled, Frozen>().ForEach((Entity e, ref ChildOf c, ref LocalTranslation lT, ref ParentHeigharcy p, ref Translation t) =>
		{
			if (!lT.Applied)
			{
				t.Value = p.totalOfset + lT.Value;
				lT.Applied = true;
			}
		});
		/*
		Entities.WithNone<Disabled, Frozen>().ForEach((Entity e, ref ChildOf c) =>
		{
			if (EntityManager.HasComponent<Frozen>(c.parent))
				PostUpdateCommands.AddComponent(e, new Frozen());
		});

		Entities.WithNone<Disabled>().ForEach((Entity e, ref ChildOf c, ref Frozen _) =>
		{
			if (!EntityManager.HasComponent<Frozen>(c.parent))
				PostUpdateCommands.RemoveComponent<Frozen>(e);
		});
		*/
		Entities.WithNone<ParentHeigharcy>().ForEach((Entity e, ref ChildOf c) =>
		{
			if (EntityManager.Exists(c.parent))
				PostUpdateCommands.AddComponent(e, DetermineHeigharcy(c.parent));
			else
				PostUpdateCommands.RemoveComponent<ChildOf>(e);
		});
	}

	private ParentHeigharcy DetermineHeigharcy(Entity parent)
	{
		float3 totalOffset = new float3(0, 0, 0);
		while (EntityManager.HasComponent<ChildOf>(parent))
		{
			var c = EntityManager.GetComponentData<ChildOf>(parent);
			var p = EntityManager.GetComponentData<LocalTranslation>(parent);
			totalOffset += p.Value;
			parent = c.parent;
		}
		totalOffset += EntityManager.GetComponentData<Translation>(parent).Value;
		return new ParentHeigharcy
		{
			topParent = parent,
			totalOfset = totalOffset
		};
	}
}

public struct LocalTranslation : IComponentData
{
	public float3 Value;
	public bool Applied;
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