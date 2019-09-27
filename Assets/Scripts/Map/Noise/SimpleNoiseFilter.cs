using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SimpleNoiseFilter : INoiseFilter
{
	Noise noise;
	NoiseSettings settings;

	public SimpleNoiseFilter(NoiseSettings noiseSettings, int seed = 0)
	{
		noise = new Noise(seed);
		this.settings = noiseSettings;
	}

	public float Evaluate(Vector3 point) => Evaluate(point, settings.minValue);

	public float Evaluate(Vector3 point, float minValue)
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
		noiseValue -= minValue;
		return noiseValue * settings.strength;
	}

	public void SetSeed(int seed)
	{
		noise = new Noise(seed);
	}
}
