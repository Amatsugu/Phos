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
		nextTic = nextTic.AddSeconds(1 / ticRate);
		if(resCount.Length != ResourceDatabase.ResourceCount)
			resCount = new int[ResourceDatabase.ResourceCount];
		totalDemand = new int[ResourceDatabase.ResourceCount];
		totalProduction = new int[ResourceDatabase.ResourceCount];

        Entities.WithAll<FirstTickTag>().ForEach(e =>
        {
            PostUpdateCommands.RemoveComponent(e, typeof(FirstTickTag));
        });

        Entities.WithNone<BuildingOffTag, ConsumptionDebuff>().ForEach<ConsumptionData>((e, c) =>
		{
			for (int i = 0; i < c.resourceIds.Length; i++)
			{
				if (!ConsumeResourse(e, c.resourceIds[i], c.rates[i]))
					break;
			}
		});

		Entities.WithNone<BuildingOffTag>().ForEach((Entity e, ConsumptionData c, ref ConsumptionDebuff d) =>
		{
			for (int i = 0; i < c.resourceIds.Length; i++)
			{
				if (!ConsumeResourse(e, c.resourceIds[i], c.rates[i], d.distance * ConsumptionDebuff.multi))
					break;
			}
		});


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
	}

	void ProduceResource()
	{

	}

	bool ConsumeResourse(Entity e, int rID, int rate, float multi = 1)
	{
		var totalRate = multi == 1 ? rate : (int)(rate * multi);
		totalDemand[rID] -= totalRate;
		if (resCount[rID] < totalRate)
		{
			if (!EntityManager.HasComponent<InactiveBuildingTag>(e))
			{
				PostUpdateCommands.AddComponent(e, new InactiveBuildingTag());
				return false;
			}
		}
		else
		{
			if (EntityManager.HasComponent<InactiveBuildingTag>(e))
				PostUpdateCommands.RemoveComponent(e, typeof(InactiveBuildingTag));
			resCount[rID] -= totalRate;
		}
		return true;
	}

	protected override void OnStopRunning()
	{
	}

	public static void ConsumeResourses(ResourceIndentifier[] resources, float multi = 1)
	{
		for (int i = 0; i < resources.Length; i++)
			ConsumeResource(resources[i], multi);
	}

	public static void ConsumeResource(ResourceIndentifier resource, float multi = 1) => resCount[resource.id] -= Mathf.FloorToInt(resource.ammount * multi);

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
}
