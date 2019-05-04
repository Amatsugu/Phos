using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

[CustomPropertyDrawer(typeof(BuildingTileInfo.Resource))]
public class ResourceListUI : PropertyDrawer
{
	private ResourceList _resList;
	private string[] _resources;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var idProp = property.FindPropertyRelative("id");
		var s = idProp.intValue;
		if(_resList == null)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ResourceList ResourceList")[0]);
			_resList = AssetDatabase.LoadAssetAtPath<ResourceList>(assetPath);
			_resources = _resList.resourceDefinations.Select(r => r.name).ToArray();
		}

		label.text = _resources[s];
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);
		var width = EditorGUIUtility.currentViewWidth - position.x;
		var resPos = new Rect(position.x - 15, position.y, width/2, position.height);
		var ammountPos = new Rect(resPos.x + (width/2), position.y, width/2, position.height);
		s = EditorGUI.Popup(resPos, s, _resources);
		idProp.intValue = s;
		EditorGUI.PropertyField(ammountPos, property.FindPropertyRelative("ammount"), GUIContent.none);
		EditorGUI.EndProperty();
	}

	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		var ve = new VisualElement();
		ve.Add(new PropertyField(property.FindPropertyRelative("name")));
		ve.Add(new PropertyField(property.FindPropertyRelative("ammount")));
		return ve;
	}
}
