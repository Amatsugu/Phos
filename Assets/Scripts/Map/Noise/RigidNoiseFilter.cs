using UnityEngine;

public struct RigidNoiseFilter : INoiseFilter
{
	private Noise noise;
	private NoiseSettings settings;

	public RigidNoiseFilter(NoiseSettings noiseSettings, int seed = 0)
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
		float weight = 1;
		for (int i = 0; i < settings.layerCount; i++)
		{
			float v = 1 - Mathf.Abs(noise.Evaluate(point * freq + settings.center));
			v *= v;
			v *= weight;
			weight = Mathf.Clamp(v * settings.weightMultiplier, 0, 1);

			noiseValue += v * amp;
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