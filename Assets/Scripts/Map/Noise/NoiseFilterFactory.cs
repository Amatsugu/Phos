using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilterFactory
{
	public static INoiseFilter CreateNoiseFilter(NoiseSettings settings, int seed)
	{
		switch(settings.type)
		{
			case NoiseSettings.FilterType.Simple:
				return new SimpleNoiseFilter(settings.simpleNoiseSettings, seed);
			case NoiseSettings.FilterType.Rigid:
				return new RigidNoiseFilter(settings.rigidNoiseSettings, seed);
			default:
				return null;
		}
	}
}
