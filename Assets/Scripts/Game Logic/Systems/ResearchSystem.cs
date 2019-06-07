using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResearchSystem : ComponentSystem
{
	public Dictionary<int, ResearchTree> trees = new Dictionary<int, ResearchTree>(6);

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
	}

	protected override void OnUpdate()
	{
		Entities.WithAll<ResearchBuildingTag>().WithNone<InactiveBuildingTag, BuildingOffTag, FirstTickTag>().ForEach(e =>
		{
			ResourceIndentifier resource = default; //TODO: Get resource
			if (ResourceSystem.HasResource(resource))
				ResourceSystem.ConsumeResource(resource);
			else
				PostUpdateCommands.AddComponent(e, new InactiveBuildingTag());
		});

		Entities.WithAll<ResearchBuildingTag, InactiveBuildingTag>().WithNone<BuildingOffTag, FirstTickTag>().ForEach(e =>
		{
			ResourceIndentifier resource = default; //TODO: Get resource
			if (ResourceSystem.HasResource(resource))
			{
				ResourceSystem.ConsumeResource(resource);
				PostUpdateCommands.RemoveComponent<InactiveBuildingTag>(e);
			}
		});
	}

	public void SetResearchTree(int id, ResearchTree tree)
	{
		if (trees.ContainsKey(id))
			trees.Add(id, tree);
		else
			trees[id] = tree;
	}

	public void ProgressResearch(int treeId, int techId, ResourceIndentifier resources)
	{

	}
}
