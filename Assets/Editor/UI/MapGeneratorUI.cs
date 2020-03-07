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
	private Color[] _previewTexColors;
	private Texture2D _previewTex;

	private void OnEnable()
	{
		UnityEditor.EditorPrefs.SetBool("DeveloperMode", false);

		creator = target as RandomGenerator;
		_previewTex = new Texture2D(256, 256);
		_previewTex.filterMode = FilterMode.Point;
		_previewTexColors = _previewTex.GetPixels();
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
			if(GUILayout.Button(_previewTex, new GUIStyle { border = new RectOffset(0,0,0,0), imagePosition = ImagePosition.ImageOnly, alignment = TextAnchor.MiddleCenter}, GUILayout.Height(Mathf.Min(EditorGUIUtility.currentViewWidth - 20, _previewTex.height)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20)))
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
		float max = 0f, min = float.MaxValue;
		int w = _previewTex.width, h = _previewTex.height;
		var hMap = new float[h * w];
		var landToSeaRatio = 0f;
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var sX = MathUtils.Remap(x, 0, w, 0, creator.Size.x * Map.Chunk.SIZE);
				var sZ = MathUtils.Remap(z, 0, h, 0, creator.Size.y * Map.Chunk.SIZE);
				var sample = creator.GenerateHeight(sX, sZ);
				if (sample > max)
					max = sample;
				if (sample < min)
					min = sample;
				if (sample > creator.seaLevel)
					landToSeaRatio++;
				hMap[x + z * w] = sample;
			}
		}
		Random.InitState(creator.seed);
		var temp = creator.biomePainter.GetTempMap(w, h, hMap, min, max, creator.seaLevel);
		var filter = NoiseFilterFactory.CreateNoiseFilter(creator.noiseLayers[0].noiseSettings, creator.seed);
		var moist = creator.biomePainter.GetMoistureMap(w, h, filter, creator.noiseScale);

		var waterGrad = new Gradient();
		waterGrad.SetKeys(new GradientColorKey[]
		{
						new GradientColorKey(new Color(66/255f, 209/255f, 245/255f), 0),
						new GradientColorKey(new Color(0, 11/255f, 69/255f), 1)
		}, new GradientAlphaKey[]
		{
						new GradientAlphaKey(1, 1)
		});
		_lsr = landToSeaRatio / hMap.Length;
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var a = hMap[x + z * w];
				var i = x + z * w;
				var (tile, _) = creator.biomePainter.GetTile(moist[i], temp[i], a, creator.seaLevel);
				var color = tile.material.color;
				if (a > creator.seaLevel)
				{
					if (z - 1 >= 0)
					{
						var b = hMap[x + (z - 1) * w];
						if (a - b < -.2f)
							color = color * 1.2f;
						else if (a - b > .2f)
							color = color * .8f;
					}
					color.a = 1;
					_previewTexColors[x + z * _previewTex.height] = color;
				}else
				{
					var d = MathUtils.Remap(a, 0, creator.seaLevel, 1, 0);
					
					_previewTexColors[x + z * _previewTex.height] = Color.Lerp(color, waterGrad.Evaluate(d), d);
					//_previewTex.SetPixel(x, z, Color.Lerp(new Color(0, 0, .01f), Color.cyan, MathUtils.Map(a, 0, creator.seaLevel, .5f, 1)));
				}
			}
		}
		_previewTex.SetPixels(_previewTexColors);
		_previewTex.Apply();
	}
}
