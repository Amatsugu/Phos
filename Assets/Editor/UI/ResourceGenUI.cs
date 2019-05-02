using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceGenerator), true)]
public class ResourceGenUI : Editor
{
	private ResourceGenerator gen;
	private void OnEnable()
	{
		gen = target as ResourceGenerator;
		gen.previewTex = new Texture2D(512, 512);
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
			GUILayout.Box(gen.previewTex, GUILayout.Height(EditorGUIUtility.currentViewWidth - 20), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20));
		
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
