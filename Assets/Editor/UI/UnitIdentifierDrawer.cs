using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnitIdentifier))]
public class UnitIdentifierDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var asset = AssetDatabase.FindAssets($"t:{nameof(UnitDatabase)}").First();
		var assetPath = AssetDatabase.GUIDToAssetPath(asset);
		var db = AssetDatabase.LoadAssetAtPath<UnitDatabase>(assetPath);
		var units = db.unitEntites.Values.Where(b => b.unit != null);
		var names = units.Select(b => $" [{b.unit.unitDomain}|{b.unit.unitClass}] T{b.unit.tier} {b.unit.name}\t {b.id}").Prepend("--Select Unit--").ToArray();
		var ids = units.Select(b => b.id + 1).Prepend(0).ToArray();
		EditorGUI.BeginProperty(position, label, property);
		var sProp = property.FindPropertyRelative("id");
		position = EditorGUI.PrefixLabel(position, label);
		var resPos = new Rect(position.x - 15, position.y, position.width, position.height);
		var selection =  EditorGUI.IntPopup(resPos, sProp.intValue + 1, names, ids) - 1;
		sProp.intValue = selection;
		EditorGUI.EndProperty();

	}
}
