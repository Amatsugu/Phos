using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceGenerator), true)]
public class ResourceGenUI : Editor
{
	private ResourceGenerator gen;
	private bool fold;

	private Color[] _previewTexColors;
	private Texture2D _previewTex;
	private void OnEnable()
	{
		gen = target as ResourceGenerator;
		_previewTex = new Texture2D(256, 256);
		_previewTex.filterMode = FilterMode.Point;
		_previewTexColors = _previewTex.GetPixels();
		GenTex();
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if (EditorGUI.EndChangeCheck())
			GenTex();
		if (GUILayout.Button("Preview"))
		{
			gen.preview = !gen.preview;
			GenTex();
		}
		if(gen.preview)
			GUILayout.Box(_previewTex, GUILayout.Height(Mathf.Min(EditorGUIUtility.currentViewWidth - 20, _previewTex.height)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20));
		if (gen.resource != null)
		{
			fold = EditorGUILayout.InspectorTitlebar(fold, gen.resource);
			Editor.CreateEditor(gen.resource).OnInspectorGUI();
		}
		
	}

	private void GenTex()
	{
		var map = gen.PrepareMap(_previewTex.width, _previewTex.height);
		for (int z = 0; z < _previewTex.height; z++)
		{
			for (int x = 0; x < _previewTex.width; x++)
			{
				var c = 1- map[x + z * _previewTex.width];
				_previewTexColors[x + z * _previewTex.height] = new Color(c, c, c);
				//_previewTex.SetPixel(x, z, new Color(c, c, c));
			}
		}
		_previewTex.SetPixels(_previewTexColors);
		_previewTex.Apply();
	}
}
