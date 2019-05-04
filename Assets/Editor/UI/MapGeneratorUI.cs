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
	private float _lsr;

	private void OnEnable()
	{
		creator = target as RandomGenerator;
		creator.previewTex = new Texture2D(256, 256);
		GenTex();
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		GUILayout.BeginVertical();
		if (!Application.isPlaying || _autoRegen)
			GUI.enabled = false;
		if(GUILayout.Button(new GUIContent("Regenerate","Regenerate the Map")))
			creator.Regen = true;
		if (!Application.isPlaying || _autoRegen)
			GUI.enabled = true;
		_autoRegen = GUILayout.Toggle(_autoRegen, new GUIContent("Auto Regen", "Regenerate automatically"));
		GUILayout.EndVertical();
		var hasChange = EditorGUI.EndChangeCheck();
		if (_autoRegen)
		{
			creator.Regen = hasChange;
			GenTex();
		}

		if (GUILayout.Button("Preview"))
		{
			creator.preview = !creator.preview;
			if(creator.preview)
				GenTex();
		}
		if(creator.preview)
		{
			if (hasChange)
				GenTex();
			if (_lsr < creator.landSeaRatio)
				GUILayout.Label($"<color=red><b>Land to Sea Ratio {_lsr} < {creator.landSeaRatio}</b></color>", new GUIStyle { richText = true });
			if(GUILayout.Button(creator.previewTex, new GUIStyle { border = new RectOffset(0,0,0,0), imagePosition = ImagePosition.ImageOnly, alignment = TextAnchor.MiddleCenter}, GUILayout.Height(Mathf.Min(EditorGUIUtility.currentViewWidth - 20, creator.previewTex.height)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20)))
			{
				creator.seed++;
				GenTex();
			}
		}
		if (creator.biomePainter != null)
		{
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

	public void GenTex()
	{
		creator.InitFilters();
		var max = 0f;
		int w = creator.previewTex.width, h = creator.previewTex.height;
		var hMap = new float[h * w];
		var landToSeaRatio = 0f;
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var sX = MathUtils.Map(x, 0, w, 0, creator.Size.x * Map.Chunk.SIZE);
				var sZ = MathUtils.Map(z, 0, h, 0, creator.Size.y * Map.Chunk.SIZE);
				var sample = creator.GenerateHeight(sX, sZ);
				if (sample > max)
					max = sample;
				if (sample > creator.seaLevel)
					landToSeaRatio++;
				hMap[x + z * w] = sample;
			}
		}
		_lsr = landToSeaRatio / hMap.Length;
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var a = hMap[x + z * w];
				if (a > creator.seaLevel)
				{
					var color = new Color(0, .8f, 0);
					if (z - 1 >= 0)
					{
						var b = hMap[x + (z - 1) * w];
						if (a - b < -.3f)
							color = new Color(.4f, .9f, .4f);
						else if (a - b > .3f)
							color = new Color(0, .6f, 0);
					}
					creator.previewTex.SetPixel(x, z, color);
				}else
				{
					creator.previewTex.SetPixel(x, z, Color.Lerp(new Color(0, 0, .01f), Color.cyan, MathUtils.Map(a, 0, creator.seaLevel, .5f, 1)));
				}
			}
		}
		creator.previewTex.Apply();
	}
}
