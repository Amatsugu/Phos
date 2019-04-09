using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MathUtils
{
	public static float Map(float value, float min, float max, float a, float b)
	{
		return Mathf.Lerp(a, b, (value - min) / (max - min));
	}

	public static float Range(this System.Random r, float min, float max)
	{
		return Map((float)r.NextDouble(), 0, 1, min, max);
	}

	public static float NextFloat(this System.Random r)
	{
		return (float)r.NextDouble();
	}
}
