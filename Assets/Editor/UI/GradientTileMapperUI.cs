using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GradientTileMapper), true)]
public class GradientTileMapperUI : Editor
{

	private GradientTileMapper _mapper;
	private int lastSize = 0;
	private bool warn;
	private void OnEnable()
	{
		_mapper = target as GradientTileMapper;
		_mapper.tileGradient.mode = GradientMode.Fixed;
		lastSize = _mapper.tileGradient.colorKeys.Length;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		/*EditorGUILayout.BeginVertical();
		for(int i = 0; i < _mapper.tileGradient.colorKeys.Length; i++)
		{
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.ColorField(_mapper.tileGradient.colorKeys[i].color);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();*/
		if(EditorGUI.EndChangeCheck())
		{
			_mapper.tileGradient.mode = GradientMode.Fixed;
			/*if (_mapper.tileGradient.colorKeys.Length == lastSize)
				return;*/
			lastSize = _mapper.tileGradient.colorKeys.Length;
			var t = new TileInfo[_mapper.Tiles.Length];
			_mapper.Tiles.CopyTo(t, 0);
			_mapper.tiles.Clear();
			for (int i = 0; i < lastSize; i++)
			{
				//_mapper.Tiles[i] = t[i];
				try
				{
					if (i > t.Length - 1)
						_mapper.tiles.Add(_mapper.tileGradient.colorKeys[i].color, null);
					else
						_mapper.tiles.Add(_mapper.tileGradient.colorKeys[i].color, t[i]);
					warn = false;
				}catch
				{
					warn = true;
					//Debug.LogError("All colors in the gradient must be unique");
				}
			}
			_mapper.Tiles = new TileInfo[lastSize];
			_mapper.tiles.Values.CopyTo(_mapper.Tiles, 0);
		}
		EditorGUILayout.BeginFadeGroup(warn ? 1 : 0);
		EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(new GUIContent("All colors in the gradient must be unique"));
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndFadeGroup();
	}
}
