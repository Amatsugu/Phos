using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoiseFilter : INoiseFilter
{
	Noise noise = new Noise();
	NoiseSettings.SimpleNoiseSettings settings;

	public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings noiseSettings)
	{
		this.settings = noiseSettings;
	}

	public float Evaluate(Vector3 point)
	{

		float noiseValue = 0;
		float freq = settings.baseRoughness;
		float amp = 1;
		for (int i = 0; i < settings.layerCount; i++)
		{
			float v = noise.Evaluate(point * freq + settings.center);
			noiseValue += (v + 1) * .5f * amp;
			freq *= settings.roughness;
			amp *= settings.persistence;
		}
		noiseValue = noiseValue - settings.minValue;
		return noiseValue * settings.strength;
	}
}
