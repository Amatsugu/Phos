using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NoiseSettings
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
		public float strength;
		[Range(1, 8)]
		public int layerCount;
		public float baseRoughness;
		public float roughness;
		public float persistence;
		public float minValue;
		public Vector3 center;
	}

	[System.Serializable]
	public class RigidNoiseSettings : SimpleNoiseSettings
	{
		public float weightMultiplier;
	}
}
