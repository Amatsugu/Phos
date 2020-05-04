using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileDatabase))]
public class TileDatabaseUI : Editor
{
	private TileDatabase database;

	private void OnEnable()
	{
		database = target as TileDatabase;
		if (database.tileEntites == null)
			database.tileEntites = new Dictionary<int, TileDatabase.TileDefination>();
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
		GUI.enabled = true;
		foreach (var tileDef in database.tileEntites.Values)
		{
			var typeString = "Tile";
			if (tileDef.type.HasFlag(TileDatabase.TileType.Building))
				typeString += ", Building";
			if (tileDef.type.HasFlag(TileDatabase.TileType.Resource))
				typeString += ", Resource";
			EditorGUILayout.LabelField($"[{tileDef.id}]\t {tileDef.tile.name}:\t ({typeString})");
		}
	}

	public void Refresh()
	{
		var assets = AssetDatabase.FindAssets($"t:{nameof(TileEntity)}", new[] { "Assets/GameData/MapAssets/TileInfo" });
		//database.tileEntites.Clear();
		//database.tileEntites.Clear();
		foreach (var asset in assets)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(asset);
			var tileAsset = AssetDatabase.LoadAssetAtPath<TileEntity>(assetPath);
			database.RegisterTile(tileAsset, out _);
		}
	}
}
