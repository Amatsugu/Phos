﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Amatsugu.Phos.TileEntities;

[CustomEditor(typeof(BuildingDatabase))]
public class BuildingDatabaseUI : Editor
{
	private BuildingDatabase database;

	private void OnEnable()
	{
		database = target as BuildingDatabase;
		if (database.buildingCategories == null && !Application.isPlaying)
		{
			UnityEngine.Debug.LogWarning("Building DB resetting");
			database.Reset();
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GUI.enabled = !Application.isPlaying;
		GUI.enabled = database.tileDatabase != null;
		if(GUILayout.Button("Refresh"))
		{
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
		}
		GUI.enabled = true;
		var guiStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				textColor = Color.red
			}
		};
		foreach (var categories in database.buildingCategories)
		{
			GUILayout.Label($"{categories.Key} ({categories.Value.Length})", guiStyle);
			foreach (var buildingID in categories.Value)
			{
				var building = database.buildings[buildingID].info;
				if(building != null)
					GUILayout.Label(new GUIContent($"[ID:{buildingID}] \tT{(building.tier)} : {building?.name}"));
				else
					GUILayout.Label($"[ID:{buildingID}] \tT<missing/deleted asset>");
			}
		}
	}

	public void Refresh()
	{
		database.Reset();
		var bd = new Dictionary<BuildingCategory, List<BuildingTileEntity>>();
		var assets = AssetDatabase.FindAssets($"t:{nameof(BuildingTileEntity)}", new[] { "Assets/GameData/MapAssets/TileInfo/Buildings" });
		foreach (var asset in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(asset);
			var building = AssetDatabase.LoadAssetAtPath<BuildingTileEntity>(assetPath);
			if (!bd.ContainsKey(building.category))
				bd.Add(building.category, new List<BuildingTileEntity>());
			bd[building.category].Add(building);
		}
		foreach (var category in bd.Keys)
		{
			var ordered = bd[category].OrderBy(b => b.tier).ToArray();
			AddBuildings(category, ordered);
		}
		CullDeleted();
	}

	public void AddBuildings(BuildingCategory category, BuildingTileEntity[] orderedBuildings)
	{
		if (!database.buildingCategories.ContainsKey(category))
			database.buildingCategories.Add(category, new int[orderedBuildings.Length]);
		else
			database.buildingCategories[category] = new int[orderedBuildings.Length];
		for (int i = 0; i < orderedBuildings.Length; i++)
		{
			var b = orderedBuildings[i];
			var existingB = database.buildings.Values.FirstOrDefault(bd => bd.info == b);
			if (existingB == null)
			{
				existingB = new BuildingDatabase.BuildingDefination
				{
					info = b,
					id = database.tileDatabase.entityIds[b],
					category = b.category
				};
				database.buildings.Add(existingB.id, existingB);
			}else
			{
				existingB.category = b.category;
			}
			database.buildingCategories[category][i] = existingB.id;
		}
	}

	public void CullDeleted()
	{
		var deleteIds = new List<int>();
		foreach (var bd in database.buildings.Values)
		{
			if (bd.info == null)
				deleteIds.Add(bd.id);
		}
		for (int i = 0; i < deleteIds.Count; i++)
		{
			database.buildings.Remove(deleteIds[i]);
		}
	}
}
