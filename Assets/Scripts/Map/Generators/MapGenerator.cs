﻿using UnityEngine;

public abstract class MapGenerator : ScriptableObject
{
	public Vector2 Size = new Vector2(20, 20);
	public TileMapper tileMapper;
	public float seaLevel = 4;
	public float edgeLength = 1;
	public FeatureGenerator[] featureGenerators;
	public bool useJobs = true;

	[HideInInspector]
	public bool Regen;

	public float InnerRadius => Mathf.Sqrt(3f) / 2f * edgeLength;

	public void GenerateFeatures(Map map)
	{
		if (featureGenerators == null)
			return;
		foreach (var fg in featureGenerators)
		{
			UnityEngine.Debug.Log("<b>Running Feature Generator:</b> " + fg.GeneratorName);
			fg.Generate(map);
		}
	}

	public abstract Map GenerateMap(Transform parent = null);
}