using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Building Database")]
public class BuildingDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	private void OnEnable()
	{
		if (buildings == null)
			Reset();
	}

	public Dictionary<BuildingCategory, int[]> buildingCategories;

	public Dictionary<int, BuildingDefination> buildings;

	[SerializeField]
	private BuildingDefination[] _buildingValues;

	[Serializable]
	public class BuildingDefination
	{
		public int id;
		public BuildingTileEntity info;
		public BuildingCategory category;

		public override int GetHashCode()
		{
			return id;
		}
	}

	public void Reset()
	{
		buildings = new Dictionary<int, BuildingDefination>();
		buildingCategories = new Dictionary<BuildingCategory, int[]>();
	}

	public void OnBeforeSerialize()
	{
		_buildingValues = buildings.Values.ToArray();
		return;
	}

	public void OnAfterDeserialize()
	{
		buildings = new Dictionary<int, BuildingDefination>();
		for (int i = 0; i < _buildingValues.Length; i++)
		{
			buildings.Add(_buildingValues[i].id, _buildingValues[i]);
		}

		var tmp = new Dictionary<BuildingCategory, List<int>>();
		buildingCategories = new Dictionary<BuildingCategory, int[]>();
		foreach (var building in _buildingValues)
		{
			if (!tmp.ContainsKey(building.category))
				tmp.Add(building.category, new List<int>());
			tmp[building.category].Add(building.id);
		}
		foreach (var key in tmp.Keys)
			buildingCategories.Add(key, tmp[key].ToArray());
	}

	public BuildingDefination[] this[BuildingCategory c]
	{
		get
		{
			if (buildingCategories.ContainsKey(c))
			{
				var ids = buildingCategories[c];
				var output = new BuildingDefination[ids.Length];
				for (int i = 0; i < ids.Length; i++)
					output[i] = buildings[ids[i]];
				return output;
			}
			else
				return Array.Empty<BuildingDefination>();
		}
	}

	public BuildingDefination this[BuildingIdentifier identifier]
	{
		get
		{
			return buildings[identifier.id];
		}
	}
}