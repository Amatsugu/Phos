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

	public float strength;
	[Range(1, 8)]
	public int layerCount;
	public float baseRoughness;
	public float roughness;
	public float persistence;
	public float minValue;
	public Vector3 center;
	[ConditionalHide("type", 1)]
	public float weightMultiplier;
}
