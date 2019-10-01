using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct WeatherState
{
	[Header("Clouds")]
	public float cloudDensity;
	public Color cloudColor;
	[Header("Atmosphere")]
	public float windSpeed;
	public float percipitation;
	public ParticleType weatherType;
	public bool isStorm;
	[Header("Fog")]
	public float fogDensity;
	public float fogBaseHeight;
	public float fogHeight;
	public Color fogColor;
	[Header("Lighting")]
	public Color skyColor;
	public float sunBrightness;
	public Color sunColor;
	[Range(1000, 20000)]
	public float sunTemp;

	public enum ParticleType
	{
		None,
		Rain,
		Snow
	}

	public static WeatherState Lerp(WeatherState a, WeatherState b, float t)
	{
		return new WeatherState
		{
			cloudDensity = a.cloudDensity.Lerp(b.cloudDensity, t),
			windSpeed = math.lerp(a.windSpeed, b.windSpeed, t),
			cloudColor = Color.Lerp(a.cloudColor, b.cloudColor, t),
			percipitation = a.percipitation.Lerp(b.percipitation, t),
			weatherType = t >= .5f ? a.weatherType : b.weatherType,
			fogDensity = a.fogDensity.Lerp(b.fogDensity, t),
			fogHeight = a.fogHeight.Lerp(b.fogHeight, t),
			fogBaseHeight = a.fogBaseHeight.Lerp(b.fogBaseHeight, t),
			fogColor = Color.Lerp(a.fogColor, b.fogColor, t),
			isStorm = t >= .5f ? a.isStorm : b.isStorm,
			sunBrightness = a.sunBrightness.Lerp(b.sunBrightness, t),
			sunColor = Color.Lerp(a.sunColor, b.sunColor, t),
			skyColor = Color.Lerp(a.skyColor, b.skyColor, t),
			sunTemp = a.sunTemp.Lerp(b.sunTemp, t),
		};
	}
}
