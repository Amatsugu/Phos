using Newtonsoft.Json;

using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

public struct GameState
{
	[JsonIgnore]
	public Map map;
	public string baseName;
	public int[] resCount;
	public HashSet<int> unlockedBuildings;

	public GameState(Map map, HashSet<int> unlockedBuildings = null)
	{
		this.map = map;
		this.unlockedBuildings = unlockedBuildings ?? new HashSet<int>();
		resCount = new int[ResourceDatabase.ResourceCount];
		baseName = "Base Name";
	}
}
