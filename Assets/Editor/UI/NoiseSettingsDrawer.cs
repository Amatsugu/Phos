using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//[CustomPropertyDrawer(typeof(NoiseSettings))]
public class NoiseSettingsDrawer : PropertyDrawer
{
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		return base.CreatePropertyGUI(property);
		//var container = new VisualElement();

		//var noiseType = new PropertyField(property.FindPropertyRelative("simpleNoiseSettings.strength"), "Type");
		//container.Add(noiseType);

		//return container;

	}

	//public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//{
	//	base.OnGUI(position, property, label);
	//	GUILayout.Button("Preview");
	//}
}
