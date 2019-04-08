using System;

using Unity.Entities;


public class ResourceSystem : ComponentSystem
{
	public static int[] resCount;
	public static int[] lastTickNet;
    public static int maxStorage;

	public DateTime nextTic;
	public float ticRate = 1;


	protected override void OnStartRunning()
	{
		resCount = new int[ResourceDatabase.ResourceCount];
		lastTickNet = new int[ResourceDatabase.ResourceCount];
        maxStorage = 1000;
		nextTic = DateTime.Now.AddSeconds(1 / ticRate);
	}

	protected override void OnUpdate()
	{
		if (DateTime.Now < nextTic)
			return;
		nextTic = nextTic.AddSeconds(1 / ticRate);
        lastTickNet = new int[ResourceDatabase.ResourceCount];
		Entities.WithNone<InactiveBuilding, BuildingOffTag, FirstTickTag>().ForEach<ProductionData>((e, p) =>
		{
			for (int i = 0; i < p.resourceIds.Length; i++)
			{
				int res = p.resourceIds[i];
                lastTickNet[res] += p.rates[i];
				if (resCount[res] == maxStorage)
					continue;
				resCount[res] += p.rates[i];
				if (resCount[res] > maxStorage)
					resCount[res] = maxStorage;
			}
		});

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
	}

	void ProduceResource()
	{

	}

	bool ConsumeResourse(Entity e, int rID, int rate, float multi = 1)
	{
		var totalRate = multi == 1 ? rate : (int)(rate * multi);
		if (resCount[rID] < totalRate)
		{
			if (!EntityManager.HasComponent<InactiveBuilding>(e))
			{
				PostUpdateCommands.AddComponent(e, new InactiveBuilding());
				return false;
			}
		}
		else
		{
			if (EntityManager.HasComponent<InactiveBuilding>(e))
				PostUpdateCommands.RemoveComponent(e, typeof(InactiveBuilding));
			resCount[rID] -= totalRate;
			lastTickNet[rID] -= totalRate;
		}
		return true;
	}

	protected override void OnStopRunning()
	{
	}
}
