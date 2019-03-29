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
		if (painter.biomes == null || painter.biomes.Length != 16)
			painter.biomes = new TileInfo[16];
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Label("Horizontal Temperature");
		GUILayout.Label("Vertical Moisture");
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
				painter.biomes[x + z * 4] = EditorGUILayout.ObjectField(painter.biomes[x + z * 4], typeof(TileInfo), false) as TileInfo;
			}
			GUILayout.EndHorizontal();
		}
	}
}
