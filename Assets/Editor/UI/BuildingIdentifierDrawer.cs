using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuildingIdentifier))]
public class BuildingIdentifierDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var asset = AssetDatabase.FindAssets("t:BuildingDatabase").First();
		var assetPath = AssetDatabase.GUIDToAssetPath(asset);
		var db = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(assetPath);
		var buildings = db.buildings.SelectMany(c => c.Value.Select(b => $"[{c.Key}][T{b.tier}] {b.name}")).Prepend("--Select Building--").ToArray();
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.Popup(position, 0, buildings);
		EditorGUI.EndProperty();
	}
}
