using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


//TODO: Create System
public class HQSystem : ComponentSystem
{
	public NativeArray<int> resCount;
	public int maxStorage;

	public float nextTic;
	public float ticRate = 1;

	protected override void OnStartRunning()
	{
		resCount = new NativeArray<int>(ResourceDatabase.ResourceCount, Allocator.Persistent);
		maxStorage = 100;
		nextTic = Time.time + (1 / ticRate);
	}

	protected override void OnUpdate()
	{
		if (Time.time < nextTic)
			return;
		nextTic += Time.time + (1 / ticRate);
		Entities.ForEach<ProductionData>((e, p) =>
		{
			for (int i = 0; i < p.resourceIds.Length; i++)
			{
				int res = p.resourceIds[i];
				if(resCount[i] + p.productionRates[i] <= maxStorage)
					resCount[i] += p.productionRates[i];
			}
		});
		Debug.Log($"{ResourceDatabase.GetResourceName(0)}: {resCount[0]}");
	}

	protected override void OnStopRunning()
	{
		resCount.Dispose();
	}
}
