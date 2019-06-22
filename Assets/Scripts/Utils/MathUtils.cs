using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class MathUtils
{
	public static float Remap(this float value, float min, float max, float a, float b)
	{
		return (value - min) / (max - min) * (b - a) + a;
	}

	public static float Pow(this float value, float e) => math.pow(value, e);

	public static float Lerp(this float a, float b, float t) => math.lerp(a, b, t);

	public static float Range(this System.Random r, float min, float max)
	{
		return Remap(r.NextFloat(), 0, 1, min, max);
	}

	public static float NextFloat(this System.Random r)
	{
		return r.Next(1000) / 1000f;
	}
}
