using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator), true)]
public class MapGeneratorUI : Editor
{
	private MapGenerator creator;
	private bool _autoRegen;
	private void OnEnable()
	{
		creator = target as MapGenerator;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		GUILayout.BeginVertical();
		if (!Application.isPlaying || _autoRegen)
			GUI.enabled = false;
		if(GUILayout.Button(new GUIContent("Regenerate","Regenerate the Map")))
		{
			creator.Regen = true;
		}
		if (!Application.isPlaying || _autoRegen)
			GUI.enabled = true;
		_autoRegen = GUILayout.Toggle(_autoRegen, new GUIContent("Auto Regen", "Regenerate automatically"));
		GUILayout.EndVertical();
		if(_autoRegen)
			creator.Regen = EditorGUI.EndChangeCheck();
	}
}
