using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


public class HQSystem : ComponentSystem
{
	public static int[] resCount;
	public static int maxStorage;

	public DateTime nextTic;
	public float ticRate = 1;


	protected override void OnStartRunning()
	{
		resCount = new int[ResourceDatabase.ResourceCount];
		maxStorage = 1000;
		nextTic = DateTime.Now.AddSeconds(1 / ticRate);
	}

	protected override void OnUpdate()
	{
		if (DateTime.Now < nextTic)
			return;
		nextTic = nextTic.AddSeconds(1 / ticRate);
		Entities.ForEach<ProductionData>((e, p) =>
		{
			for (int i = 0; i < p.resourceIds.Length; i++)
			{
				int res = p.resourceIds[i];
				if (resCount[res] == maxStorage)
					continue;
				resCount[res] += p.rates[i];
				if (resCount[res] > maxStorage)
					resCount[res] = maxStorage;
			}
		});
	}

	protected override void OnStopRunning()
	{
		//resCount.Dispose();
	}
}
