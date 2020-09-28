using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Units;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitDatabase))]
public class UnitDatabaseUI : Editor
{
	private UnitDatabase database;

	private void OnEnable()
	{
		database = target as UnitDatabase;
		if (database.unitEntites == null)
			database.unitEntites = new Dictionary<int, UnitDatabase.UnitDefination>();
		Refresh();
	}

	public override void OnInspectorGUI()
	{
		GUI.enabled = !Application.isPlaying;
		if(GUILayout.Button("Refresh"))
		{
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
			Undo.RecordObject(database, "Tile Database");
		}
		if (GUILayout.Button("Reset"))
		{
			database.Reset();
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
			Undo.RecordObject(database, "Tile Database");
		}
		GUI.enabled = true;
		foreach (var unitDef in database.unitEntites.Values)
		{
			EditorGUILayout.LabelField($"[{unitDef.id}]\t {(unitDef.info == null ? "Null" : unitDef.info.name)}:\t ({unitDef.info.unitDomain}|{unitDef.info.unitClass})");
		}
	}

	public void Refresh()
	{
		var assets = AssetDatabase.FindAssets($"t:{nameof(MobileUnitEntity)}", new[] { "Assets/GameData/MapAssets/Mobile Units" });
		foreach (var asset in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(asset);
			var unitAsset = AssetDatabase.LoadAssetAtPath<MobileUnitEntity>(assetPath);
			database.RegisterUnit(unitAsset, out _);
		}
	}
}
