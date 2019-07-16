﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public bool isCompleted;
	}


	public Dictionary<BuildingCategory, int> activeResearch;

	public Dictionary<int, ResearchProgress> researchProgress;

	public ResearchDatabase rDatabase;

	private static  ResearchSystem _INST;

	protected override void OnCreate()
	{
		base.OnCreate();

		LoadResearchProgress();
		EventManager.AddEventListener("OnGameSaving", SaveResearchProgress);

		_INST = this;
	}

	void LoadResearchProgress()
	{
		activeResearch = new Dictionary<BuildingCategory, int>();
		researchProgress = new Dictionary<int, ResearchProgress>();
		var prog = researchProgress.Values.ToArray();
		for (int i = 0; i < prog.Length; i++)
		{
			if(prog[i].isCompleted)
				rDatabase[prog[i].identifier].reward?.ActivateReward();
		}
	}

	void SaveResearchProgress()
	{
		//TODO: Save info
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		rDatabase = GameRegistry.ResearchDatabase;
		EventManager.AddEventListener("OnTick", OnTick);
	}

	protected void OnTick()
	{
#if DEBUG
		Entities.WithAll<ProductionData>().ForEach(e =>
		{
			ProcessResearch(BuildingCategory.Tech, 10);
		});
#endif

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
		var isComplete = true;
		for (int i = 0; i < r.resources.Length; i++)
		{
			if (r.rProgress[i] == r.resources[i].ammount)
				continue;
			isComplete = false;
			var resource = new ResourceIndentifier
			{
				id = r.resources[i].id,
				ammount = Mathf.Floor(Mathf.Min((1 * multi), r.resources[i].ammount))
			};
			ResourceSystem.LogDemand(resource);
			if (ResourceSystem.HasResource(resource))
			{
				r.lastTickProgress[i] = (int)resource.ammount;
				r.rProgress[i] += (int)resource.ammount;
				ResourceSystem.ConsumeResource(resource);
			}
			else
				return false;
		}
		if(isComplete)
		{
			r.isCompleted = true;
			Debug.Log($"{r.identifier.category} {r.identifier.researchId} Completed");
			rDatabase[r.identifier].reward?.ActivateReward();
			activeResearch[category] = -1;
		}
		return true;
	}

	public static void SetActiveResearch(ResearchIdentifier identifier)
	{
		Debug.Log($"{identifier.category} {identifier.researchId} {identifier.GetHashCode()} ");
		if(!_INST.researchProgress.ContainsKey(identifier.GetHashCode()))
		{
			var cost = _INST.rDatabase[identifier].resourceCost;
			_INST.researchProgress.Add(identifier.GetHashCode(), new ResearchProgress
			{
				identifier = identifier,
				resources = cost,
				rProgress = new int[cost.Length],
				lastTickProgress = new int[cost.Length]
			});
		}
		if (_INST.activeResearch.ContainsKey(identifier.category))
			_INST.activeResearch[identifier.category] = identifier.GetHashCode();
		else
			_INST.activeResearch.Add(identifier.category, identifier.GetHashCode());
		Debug.Log($"{_INST.researchProgress.Count} {_INST.activeResearch[identifier.category]}");

	}

	public static ResearchProgress GetActiveResearchProgress(BuildingCategory category)
	{
		if (!_INST.activeResearch.ContainsKey(category))
			return null;
		if (_INST.activeResearch[category] == -1)
			return null;
		return _INST.researchProgress[_INST.activeResearch[category]];
	}

	protected override void OnUpdate()
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