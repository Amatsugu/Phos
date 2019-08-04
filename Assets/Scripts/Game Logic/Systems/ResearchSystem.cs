using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ResourceSystem))]
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

	private bool isTick;

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
		EventManager.AddEventListener("OnTick", () => isTick = true);
	}

	protected override void OnUpdate()
	{
		if (!isTick)
			return;
		isTick = false;
		//Reset last tick count
		for (int i = 0; i < 7; i++)
		{
			var c = (BuildingCategory)i;
			if (!activeResearch.ContainsKey(c))
				continue;
			var rId = activeResearch[c];
			if (rId == -1)
				continue;
			researchProgress[rId].lastTickProgress = new int[researchProgress[rId].resources.Length];
		}

		Entities.WithNone<ConsumptionDebuff, InactiveBuildingTag, BuildingOffTag, FirstTickTag>()
			.ForEach((Entity e, ref ResearchBuildingCategory c, ref ResearchConsumptionMulti m, ref BuildingId id) =>
		{
			ProcessResearch(c.Value, m.Value, id.Value);
		});
	}

	void ProcessResearch(BuildingCategory category, float multi, int buildingSrc)
	{
		if (!activeResearch.ContainsKey(category))
			return;
		if (activeResearch[category] == -1)
			return;
		var r = researchProgress[activeResearch[category]];
		var isComplete = true;
		for (int i = 0; i < r.resources.Length; i++)
		{
			if (r.isCompleted || GameRegistry.Cheats.INSTANT_RESEARCH)
				break;
			if (r.rProgress[i] == r.resources[i].ammount)
				continue;
			isComplete = false;
			var resource = new ResourceIndentifier
			{
				id = r.resources[i].id,
				ammount = Mathf.Floor(Mathf.Min((.01f * multi * r.resources[i].ammount), r.resources[i].ammount))
			};
			GameRegistry.ResourceSystem.LogDemand(resource.id, (int)resource.ammount, buildingSrc);
			if (ResourceSystem.HasResource(resource))
			{
				r.lastTickProgress[i] += (int)resource.ammount;
				r.rProgress[i] += (int)resource.ammount;
				ResourceSystem.ConsumeResource(resource, demandSrc: buildingSrc);
			}
		}

		if(isComplete)
		{
			r.isCompleted = true;
			Debug.Log($"Research: {rDatabase[r.identifier].name} Completed"); 
			NotificationsUI.Notify(NotifType.Info, $"Research Complete: {rDatabase[r.identifier].name}");
			rDatabase[r.identifier].reward?.ActivateReward();
			activeResearch[category] = -1;
			EventManager.InvokeEvent("OnResearchComplete");
		}
	}

	public static void SetActiveResearch(ResearchIdentifier identifier)
	{
		Debug.Log($"Research: {_INST.rDatabase[identifier].name} Started");
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
		NotificationsUI.Notify(NotifType.Info, $"Research Started: {_INST.rDatabase[identifier].name}");

	}

	public static ResearchProgress GetActiveResearchProgress(BuildingCategory category)
	{
		if (!_INST.activeResearch.ContainsKey(category))
			return null;
		if (_INST.activeResearch[category] == -1)
			return null;
		return _INST.researchProgress[_INST.activeResearch[category]];
	}

	public static bool IsResearchUnlocked(ResearchIdentifier research)
	{
		if (_INST.researchProgress.ContainsKey(research.GetHashCode()))
			return _INST.researchProgress[research.GetHashCode()].isCompleted;
		return false;
	}

	public static bool IsCategoryUnlocked(BuildingCategory category)
	{
		var id = new ResearchIdentifier
		{
			researchId = _INST.rDatabase[category].BaseNode.id,
			category = category
		}.GetHashCode();
		return _INST.researchProgress.ContainsKey(id);
	}

	public static void UnlockCategory(BuildingCategory category)
	{
		var id = new ResearchIdentifier
		{
			researchId = _INST.rDatabase[category].BaseNode.id,
			category = category
		};

		if (_INST.researchProgress.ContainsKey(id.GetHashCode()))
			return;
		_INST.researchProgress.Add(id.GetHashCode(), new ResearchProgress
		{
			identifier = id,
			isCompleted = true
		});
		NotificationsUI.Notify(NotifType.Info, $"Research Tree Unlocked: {_INST.rDatabase[id].name}");
	}

	public static void UnlockResearch(ResearchIdentifier identifier)
	{
		var id = identifier.GetHashCode();
		if (_INST.researchProgress.ContainsKey(id))
		{
			_INST.researchProgress[id].isCompleted = true;
		}else
		{
			_INST.researchProgress.Add(id, new ResearchProgress
			{
				identifier = identifier,
				isCompleted = true
			});
		}
	}

	public static ResearchProgress GetResearchProgress(ResearchIdentifier identifier)
	{
		if (_INST.researchProgress.ContainsKey(identifier.GetHashCode()))
			return _INST.researchProgress[identifier.GetHashCode()];
		else
			return null;
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