using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileDecorator), true)]
public class DecoratorUI : Editor
{
	private TileDecorator decorator;
	private void OnEnable()
	{
		decorator = target as TileDecorator;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (decorator.meshEntity != null)
		{
			EditorGUILayout.InspectorTitlebar(true, decorator.meshEntity);
			CreateEditor(decorator.meshEntity).OnInspectorGUI();
		}
	}
}
