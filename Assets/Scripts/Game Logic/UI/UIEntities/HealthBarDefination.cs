using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "ECS/UI/Health Bar")]
[Serializable]
public class HealthBarDefination : ScriptableObject
{
	public HealthBarEntity healthBarBg;
	public HealthBarEntity healthBarFill;
	public bool animated;
	[ConditionalHide("animated")]
	public HealthBarEntity healthBarDecayFill;

	public NativeArray<Entity> Instantiate(Entity target, float3 offset)
	{
		var bars = new NativeArray<Entity>(animated ? 3 : 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

		bars[0] = healthBarBg.Instantiate(target, HealthBar.BarType.BG, offset);
		bars[1] = healthBarFill.Instantiate(target, HealthBar.BarType.Fill, offset);
		if(animated)
			bars[2] = healthBarDecayFill.Instantiate(target, HealthBar.BarType.DecayFill, offset);
		return bars;
	}

}
