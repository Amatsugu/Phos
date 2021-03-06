﻿using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Effects/Weather")]
public class WeatherDefination : ScriptableObject
{
	public WeatherState state;
	public float chance;
	public float transitionTime;
	public float2 duration;
}