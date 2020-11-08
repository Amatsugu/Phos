using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BiomePainter), true)]
public class BiomePainterUI : Editor
{
	private BiomePainter painter;
	private void OnEnable()
	{
		painter = target as BiomePainter;
		if (painter == null)
			return;
		if (painter.biomes == null)
			painter.biomes = new TileMapper[16];
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		var biomeProp = serializedObject.FindProperty("biomes.Array.data[0]");
		//Debug.Log(biomeProp?.objectReferenceValue?.name);
		GUILayout.BeginHorizontal();
		for (int i = -1; i < 4; i++)
		{
			if (i == -1)
				GUILayout.Label("M\\T");
			else
				GUILayout.Label($"{3-i}");
		}
		GUILayout.EndHorizontal();
		for (int z = 3; z >= 0; z--)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{z}  ");
			for (int x = 3; x >= 0; x--)
			{
				painter.biomes[x + z * 4] = EditorGUILayout.ObjectField(painter.biomes[x + z * 4], typeof(TileMapper), false) as TileMapper;
				//var prop = serializedObject.FindProperty($"biomes.Array.data[{x + z * 4}]");
				//EditorGUILayout.ObjectField(prop);
			}
			GUILayout.EndHorizontal();
		}
		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(painter);
		}
		
	}
}
