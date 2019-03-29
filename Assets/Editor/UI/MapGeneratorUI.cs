using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomGenerator), true)]
public class MapGeneratorUI : Editor
{
	private RandomGenerator creator;
	public static Editor editor;
	private bool _autoRegen;
	private void OnEnable()
	{
		creator = target as RandomGenerator;
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

		creator.biomeFold = EditorGUILayout.InspectorTitlebar(creator.biomeFold, creator.biomePainter);
		using (var check = new EditorGUI.ChangeCheckScope())
		{
			if (creator.biomeFold)
			{
				CreateCachedEditor(creator.biomePainter, null, ref editor);
				editor.OnInspectorGUI();
				if (_autoRegen)
				{
					creator.Regen = creator.Regen ? true : check.changed;
				}
			}
		}
	}
}
