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
		var buildings = db.buildings.Values.Where(b => b != null);
		var names = buildings.Select(b => $" [{b.info.category}] T{b.info.tier} {b.info.name}").Prepend("--Select Building--").ToArray();
		var ids = buildings.Select(b => b.id + 1).Prepend(0).ToArray();
		EditorGUI.BeginProperty(position, label, property);
		var sProp = property.FindPropertyRelative("id");
		position = EditorGUI.PrefixLabel(position, label);
		var resPos = new Rect(position.x - 15, position.y, position.width, position.height);
		var selection =  EditorGUI.IntPopup(resPos, sProp.intValue + 1, names, ids) - 1;
		sProp.intValue = selection;
		EditorGUI.EndProperty();

	}
}
