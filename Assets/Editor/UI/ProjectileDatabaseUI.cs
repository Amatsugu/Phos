using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileDatabase))]
public class ProjectileDatabaseUI : Editor
{
	private ProjectileDatabase database;

	private void OnEnable()
	{
		database = target as ProjectileDatabase;
		if (database.entityDefs == null)
			database.entityDefs = new Dictionary<int, ProjectileDatabase.ProjectileDefination>();
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
		}
		if (GUILayout.Button("Reset"))
		{
			database.Reset();
			Refresh();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(database);
		}
		GUI.enabled = true;
		foreach (var unitDef in database.entityDefs.Values)
		{
			EditorGUILayout.LabelField($"[{unitDef.id}]\t {(unitDef.projectile == null ? "Null" : unitDef.projectile.name)}:\t ({unitDef.projectile.faction} | {unitDef.projectile.damage} DMG)");
		}
	}

	public void Refresh()
	{
		var assets = AssetDatabase.FindAssets($"t:{nameof(ProjectileMeshEntity)}", new[] { "Assets/GameData/MapAssets/Projectiles" });
		foreach (var assetGUID in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
			var asset = AssetDatabase.LoadAssetAtPath<ProjectileMeshEntity>(assetPath);
			database.RegisterUnit(asset, out _);
		}
	}
}
