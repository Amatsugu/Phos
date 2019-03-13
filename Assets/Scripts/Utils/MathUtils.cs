using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MathUtils
{
	private MathUtils()
	{

	}

	public static float Map(float value, float min, float max, float a, float b)
	{
		return Mathf.Lerp(a, b, (value - min) / (max - min));
	}
}
