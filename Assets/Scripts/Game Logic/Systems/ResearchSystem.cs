using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResearchSystem : ComponentSystem
{
	public class ResearchProgress
	{
		public ResearchIdentifier identifier;
		public int[] lastTickProgress;
		public ResourceIndentifier[] resources;
		public int[] rProgress;
	}


	public Dictionary<BuildingCategory, int> activeResearch;

	public Dictionary<int, ResearchProgress> researchProgress;

	public ResearchDatabase rDatabase;

	private static  ResearchSystem _INST;

	protected override void OnCreate()
	{
		base.OnCreate();
		_INST = this;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		rDatabase = GameRegistry.ResearchDatabase;
	}

	protected override void OnUpdate()
	{
		Entities.WithAll<ResearchBuildingTag>().WithNone<InactiveBuildingTag, BuildingOffTag, FirstTickTag>()
			.ForEach((Entity e, ref ResearchBuildingCategory c, ref ResearchConsumptionMulti m) =>
		{
			if (!ProcessResearch(c.Value, m.Value))
				PostUpdateCommands.AddComponent(e, new InactiveBuildingTag { });
		});

		Entities.WithAll<ResearchBuildingTag, InactiveBuildingTag>().WithNone<BuildingOffTag, FirstTickTag>()
			.ForEach((Entity e, ref ResearchBuildingCategory c, ref ResearchConsumptionMulti m) =>
		{
			if (ProcessResearch(c.Value, m.Value))
				PostUpdateCommands.RemoveComponent<InactiveBuildingTag>(e);
		});
	}

	bool ProcessResearch(BuildingCategory category, float multi)
	{
		if (!activeResearch.ContainsKey(category))
			return true;
		if (activeResearch[category] == -1)
			return true;
		var r = researchProgress[activeResearch[category]];
		for (int i = 0; i < r.resources.Length; i++)
		{
			if (r.rProgress[i] == r.resources[i].ammount)
				continue;
			var resource = new ResourceIndentifier
			{
				id = r.resources[i].id,
				ammount = Mathf.Floor(Mathf.Min((1 * multi), r.resources[i].ammount))
			};
			if (ResourceSystem.HasResource(resource))
			{
				r.lastTickProgress[i] = (int)resource.ammount;
				r.rProgress[i] += (int)resource.ammount;
				ResourceSystem.ConsumeResource(resource);
				return true;
			}
			else
				return false;
		}
		return true;
	}

	public void SetActiveResearch(ResearchIdentifier identifier)
	{
		var cost = rDatabase[identifier].resourceCost;
		researchProgress.Add(identifier.GetHashCode(), new ResearchProgress
		{
			identifier = identifier,
			resources = cost,
			rProgress = new int[cost.Length],
			lastTickProgress = new int[cost.Length]
		});
	}

	public static ResearchProgress GetActiveResearchProgress(BuildingCategory category)
	{
		return _INST.researchProgress[_INST.activeResearch[category]];
	}

	public void ProgressResearch(ResearchIdentifier research, ResourceIndentifier resources)
	{

	}
}

public struct ResearchBuildingCategory : IComponentData
{
	public BuildingCategory Value;
}

public struct ResearchConsumptionMulti : IComponentData
{
	public float Value;
}