using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilterFactory
{
	public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
	{
		switch(settings.type)
		{
			case NoiseSettings.FilterType.Simple:
				return new SimpleNoiseFilter(settings.simpleNoiseSettings);
			case NoiseSettings.FilterType.Rigid:
				return new RigidNoiseFilter(settings.rigidNoiseSettings);
			default:
				return null;
		}
	}
}
