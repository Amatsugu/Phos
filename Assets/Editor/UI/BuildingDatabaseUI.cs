using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingDatabase))]
public class BuildingDatabaseUI : Editor
{
	private BuildingDatabase database;

	private void OnEnable()
	{
		database = target as BuildingDatabase;
		if (database.buildings == null && !Application.isPlaying)
		{
			Refresh();
			EditorUtility.SetDirty(database);
			Undo.RecordObject(database, "Building Database");
		}
	}

	public override void OnInspectorGUI()
	{
		GUI.enabled = !Application.isPlaying;
		if(GUILayout.Button("Refresh"))
		{
			Refresh();
			EditorUtility.SetDirty(database);
			Undo.RecordObject(database, "Building Database");
		}
		GUI.enabled = true;
		var guiStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				textColor = Color.red
			}
		};
		foreach (var categories in database.buildings)
		{
			GUILayout.Label($"{categories.Key.ToString()} [{categories.Value.Length}]", guiStyle);
			foreach (var building in categories.Value)
			{
				GUILayout.Label($"\tT{building.tier} : {building.name}");
			}
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
		database.buildings = new Dictionary<BuildingCategory, BuildingTileInfo[]>();
		foreach (var category in bd.Keys)
		{
			database.buildings.Add(category, bd[category].OrderBy(b => b.tier).ToArray());
		}
	}
}
