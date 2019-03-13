using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
	public enum FilterType
	{
		Simple,
		Rigid
	}

	public FilterType type;
	[ConditionalHide("type", 0)]
	public SimpleNoiseSettings simpleNoiseSettings;
	[ConditionalHide("type", 1)]
	public RigidNoiseSettings rigidNoiseSettings;

	[System.Serializable]
	public class SimpleNoiseSettings
	{
		public float strength = 1;
		[Range(1, 8)]
		public int layerCount = 1;
		public float baseRoughness = 1;
		public float roughness = 2;
		public float persistence = .5f;
		public float minValue = 1;
		public Vector3 center;
	}

	[System.Serializable]
	public class RigidNoiseSettings : SimpleNoiseSettings
	{
		public float weightMultiplier = .8f;
	}
}
