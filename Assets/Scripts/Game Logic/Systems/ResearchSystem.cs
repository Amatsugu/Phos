using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResearchSystem : ComponentSystem
{
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
}
