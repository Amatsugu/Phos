using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitInfo), true)]
public class UnitInfoUI : Editor
{
	private TileInfo info;
	private void OnEnable()
	{
		info = (target as UnitInfo).tile;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (info != null)
		{
			EditorGUILayout.InspectorTitlebar(true, info);
			CreateEditor(info).OnInspectorGUI();
		}
	}
}
