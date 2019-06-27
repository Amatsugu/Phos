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
}
