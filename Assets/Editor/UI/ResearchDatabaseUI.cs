using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResearchDatabase))]
public class ResearchDatabaseUI : Editor
{
	public ResearchDatabase database;

	void OnEnable()
	{
		database = target as ResearchDatabase;
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Label($"Trees: {database.trees?.Count}");
		if (GUILayout.Button("Refresh"))
		{
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
			Undo.RecordObject(database, "Research Database");
		}
		foreach (var item in database.trees)
		{
			GUILayout.BeginHorizontal();
			GUI.skin.label.normal.textColor = Color.blue;
			GUILayout.Label($"{item.Key}:");
			GUI.skin.label.normal.textColor = Color.black;
			GUILayout.Label($"{item.Value.name}, {item.Value.tree.Count} Nodes");
			GUILayout.EndHorizontal();
		}
	}

	public void Refresh()
	{
		var db = database.trees ?? (database.trees = new Dictionary<BuildingCategory, ResearchTreeInfo>());
		var assets = AssetDatabase.FindAssets("t:ResearchTreeInfo", new[] { "Assets/GameData/Tech Trees" });
		foreach (var asset in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(asset);
			var treeInfo = AssetDatabase.LoadAssetAtPath<ResearchTreeInfo>(assetPath);
			if (db.ContainsValue(treeInfo))
			{
				var treePair = db.First(pair => pair.Value == treeInfo);
				if(treePair.Key != treeInfo.tree.category)
					db.Remove(treePair.Key);
			}
			if (db.ContainsKey(treeInfo.tree.category))
			{
				if(treeInfo != db[treeInfo.tree.category])
					Debug.LogWarning($"Replacing Exisiting tree '{db[treeInfo.tree.category].name}' with '{treeInfo.name}'");
				db[treeInfo.tree.category] = treeInfo;
			}
			else
				db.Add(treeInfo.tree.category, treeInfo);
		}
		CullDeleted();
	}

	private void CullDeleted()
	{
		var keys = database.trees.Keys.ToArray();
		var values = database.trees.Values.ToArray();
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] == null)
				database.trees.Remove(keys[i]);
		}
	}
}
