using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CreateNewAssetAttribute))]
public class CreateNewAssetAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var attr = attribute as CreateNewAssetAttribute;
		position.width -= 55;
		EditorGUI.PropertyField(position, property, label);
		position.x += position.width + 5;
		position.width = 50;
		if(GUI.Button(position, "New"))
		{
			var type = attr.type;
			var instance = ScriptableObject.CreateInstance(type);
			var name = property.serializedObject.targetObject.name;
			var assetPath = $"{attr.path}/{name} {type.Name}.asset";
			AssetDatabase.CreateAsset(instance, assetPath);
			property.objectReferenceValue = instance;
			property.serializedObject.ApplyModifiedProperties();
			property.serializedObject.UpdateIfRequiredOrScript();
		}
	}
}
