using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FeatureGenerator : ScriptableObject
{
	public string GeneratorName
	{
		get
		{
			return name;
		}
	}
	public abstract void Generate(Map map);
}
