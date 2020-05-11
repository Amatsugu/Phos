using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameState
{
	public Map map;
	public string baseName;
	public HashSet<int> unlockedBuildings;

	public GameState(Map map, HashSet<int> unlockedBuildings = null)
	{
		this.map = map;
		this.unlockedBuildings = unlockedBuildings ?? new HashSet<int>();
		baseName = default;
	}
}
