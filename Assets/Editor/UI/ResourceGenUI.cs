using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceGenerator), true)]
public class ResourceGenUI : Editor
{
	private ResourceGenerator gen;
	private bool fold;
	private void OnEnable()
	{
		gen = target as ResourceGenerator;
		gen.previewTex = new Texture2D(256, 256);
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
			GUILayout.Box(gen.previewTex, GUILayout.Height(Mathf.Min(EditorGUIUtility.currentViewWidth - 20, gen.previewTex.height)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20));
		if (gen.resource != null)
		{
			fold = EditorGUILayout.InspectorTitlebar(fold, gen.resource);
			Editor.CreateEditor(gen.resource).OnInspectorGUI();
		}
		
	}

	private void GenTex()
	{
		var noise = NoiseFilterFactory.CreateNoiseFilter(gen.settings, 0);
		for (int z = 0; z < gen.previewTex.height; z++)
		{
			for (int x = 0; x < gen.previewTex.width; x++)
			{
				var c = gen.GetSample(x, z, noise);
				gen.previewTex.SetPixel(x, z, new Color(c, c, c));
			}
		}
		gen.previewTex.Apply();
	}
}
