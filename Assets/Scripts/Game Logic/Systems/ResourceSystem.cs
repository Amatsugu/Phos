using System;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ResourceSystem : ComponentSystem
{
	public static int[] resCount;
	public static int[] totalDemand;
	public static int[] totalProduction;
	public static int maxStorage;

	public DateTime nextTic;
	public float ticRate = 1;

	protected override void OnCreate()
	{
		base.OnCreate();
		resCount = new int[ResourceDatabase.ResourceCount];
		totalDemand = new int[ResourceDatabase.ResourceCount];
		totalProduction = new int[ResourceDatabase.ResourceCount];
		maxStorage = 1000;
	}

	protected override void OnStartRunning()
	{
		nextTic = DateTime.Now.AddSeconds(1 / ticRate);
	}

	protected override void OnUpdate()
	{
		if (DateTime.Now < nextTic)
			return;
		//Init Tick
		nextTic = nextTic.AddSeconds(1 / ticRate);
		totalDemand = new int[ResourceDatabase.ResourceCount];
		totalProduction = new int[ResourceDatabase.ResourceCount];
		EventManager.InvokeEvent("OnTick");


		//Consumption
		Entities.WithNone<BuildingOffTag, ConsumptionDebuff>().ForEach<ConsumptionData>((e, c) =>
		{
			if (HasAllResources(c.resourceIds, c.rates))
			{
				ConsumeResourses(c.resourceIds, c.rates);
				if (EntityManager.HasComponent<InactiveBuildingTag>(e))
					PostUpdateCommands.RemoveComponent<InactiveBuildingTag>(e);
			}else
			{
				if (!EntityManager.HasComponent<InactiveBuildingTag>(e))
					PostUpdateCommands.AddComponent(e, new InactiveBuildingTag());

			}

		});

		//Debuffed Consumption
		Entities.WithNone<BuildingOffTag>().ForEach((Entity e, ConsumptionData c, ref ConsumptionDebuff d) =>
		{
			if (HasAllResources(c.resourceIds, c.rates, d.distance * ConsumptionDebuff.multi))
			{
				ConsumeResourses(c.resourceIds, c.rates, d.distance * ConsumptionDebuff.multi);
				if (EntityManager.HasComponent<InactiveBuildingTag>(e))
					PostUpdateCommands.RemoveComponent<InactiveBuildingTag>(e);
			}
			else
			{
				if (!EntityManager.HasComponent<InactiveBuildingTag>(e))
					PostUpdateCommands.AddComponent(e, new InactiveBuildingTag());
			}
		});

		//Production
		Entities.WithNone<InactiveBuildingTag, BuildingOffTag, FirstTickTag>().ForEach<ProductionData>((e, p) =>
		{
			for (int i = 0; i < p.resourceIds.Length; i++)
			{
				int res = p.resourceIds[i];
				totalProduction[res] += p.rates[i];
				if (resCount[res] == maxStorage)
					continue;
				resCount[res] += p.rates[i];
				if (resCount[res] > maxStorage)
					resCount[res] = maxStorage;
			}
		});

		//Prevent production of resources on first tick
		Entities.WithAll<FirstTickTag>().ForEach(e =>
		{
			if (EntityManager.Exists(e))
				PostUpdateCommands.RemoveComponent(e, typeof(FirstTickTag));
		});
	}

	void ProduceResource()
	{

	}

	bool HasAllResources(int[] ids, int[] rates, float multi = 1, bool recordDemand = true)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			var totalRate = multi == 1 ? rates[i] : (int)(rates[i] * multi);
			if(recordDemand)
				totalDemand[ids[i]] -= totalRate;
			if (resCount[ids[i]] < totalRate)
			{
				return false;
			}
		}

		return true;
	}

	bool HasAllResources(ResourceIndentifier[] resources, float multi = 1, bool recordDemand = false)
	{
		for (int i = 0; i < resources.Length; i++)
		{
			var totalRate = multi == 1 ? (int)resources[i].ammount : (int)(resources[i].ammount * multi);
			if (recordDemand)
				totalDemand[resources[i].id] -= totalRate;
			if (resCount[resources[i].id] < totalRate)
				return false;
		}

		return true;
	}

	void ConsumeResourses(int[] ids, int[] rates, float multi = 1)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			var totalRate = multi == 1 ? rates[i] : (int)(rates[i] * multi);
			resCount[ids[i]] -= totalRate;
		}
	}

	protected override void OnStopRunning()
	{
	}

	public static void ConsumeResourses(ResourceIndentifier[] resources, float multi = 1)
	{
		for (int i = 0; i < resources.Length; i++)
			ConsumeResource(resources[i], multi);
	}

	public static void ConsumeResource(ResourceIndentifier resource, float multi = 1)
	{
		var ammount = Mathf.FloorToInt(resource.ammount * multi);
		resCount[resource.id] -= ammount;
	}

	public static void AddResources(ResourceIndentifier[] resources, float multi = 1)
	{
		for (int i = 0; i < resources.Length; i++)
			AddResource(resources[i]);
	}

	public static void AddResource(ResourceIndentifier resource, float multi = 1) => resCount[resource.id] += Mathf.FloorToInt(resource.ammount * multi);

	public static bool HasResourses(ResourceIndentifier[] resources, float multi = 1)
	{
		for (int i = 0; i < resources.Length; i++)
		{
			var id = resources[i].id;
			if (resCount[id] < Mathf.FloorToInt(resources[i].ammount * multi))
				return false;
		}
		return true;
	}

	public static bool HasResource(ResourceIndentifier resource, float multi = 1)
	{
		return resCount[resource.id] >= Mathf.FloorToInt(resource.ammount * multi);
	}

	public static void LogDemand(ResourceIndentifier resource, float multi = 1)
	{
		var totalRate = multi == 1 ? (int)resource.ammount : (int)(resource.ammount * multi);
		totalDemand[resource.id] -= totalRate;
	}
}
