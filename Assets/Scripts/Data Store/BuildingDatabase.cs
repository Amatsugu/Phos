using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Building Database")]
public class BuildingDatabase : ScriptableObject
{
	public Dictionary<BuildingCategory, BuildingTileInfo[]> buildings;

	public BuildingTileInfo[] this[BuildingCategory c]
	{
		get
		{
			if (buildings.ContainsKey(c))
				return buildings[c];
			else
				return new BuildingTileInfo[0];
		}
	}

	public void Refresh()
	{
		var bd = new Dictionary<BuildingCategory, List<BuildingTileInfo>>();
		var assets = AssetDatabase.FindAssets("t:BuildingTileInfo", new[] { "Assets/GameData/MapAssets/TileInfo/Buildings" });
		foreach (var asset in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(asset);
			var building = AssetDatabase.LoadAssetAtPath<BuildingTileInfo>(assetPath);
			if (!bd.ContainsKey(building.category))
				bd.Add(building.category, new List<BuildingTileInfo>());
			bd[building.category].Add(building);
		}
		buildings = new Dictionary<BuildingCategory, BuildingTileInfo[]>();
		foreach (var category in bd.Keys)
		{
			buildings.Add(category, bd[category].OrderBy(b => b.tier).ToArray());
		}
	}
}
