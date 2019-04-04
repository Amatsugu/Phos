using System;

using Unity.Entities;


public class HQSystem : ComponentSystem
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

        Entities.ForEach<ConsumptionData>((e, p) =>
		{
			for (int i = 0; i < p.resourceIds.Length; i++)
			{
				int res = p.resourceIds[i];
				if(resCount[res] < p.rates[i])
				{
					if (!EntityManager.HasComponent<InactiveBuilding>(e))
                    {
						PostUpdateCommands.AddComponent(e, new InactiveBuilding());
                        break;
                    }
				}
				else
				{
					if (EntityManager.HasComponent<InactiveBuilding>(e))
						PostUpdateCommands.RemoveComponent(e, typeof(InactiveBuilding));
					resCount[res] -= p.rates[i];
				}
                lastTickNet[res] -= p.rates[i];
			}
		});
	}

	protected override void OnStopRunning()
	{
	}
}
