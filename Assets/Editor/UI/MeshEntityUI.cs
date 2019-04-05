using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshEntity), true)]

public class MeshEntityUI : Editor
{
	private MeshEntity meshEntity;

	private void OnEnable()
	{
		meshEntity = target as MeshEntity;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (meshEntity.material != null)
			CreateEditor(meshEntity.material).OnInspectorGUI();
	}
}
