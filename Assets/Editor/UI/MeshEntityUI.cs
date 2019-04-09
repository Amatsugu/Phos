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
		{
			EditorGUILayout.InspectorTitlebar(true, meshEntity.material);
			CreateEditor(meshEntity.material).OnInspectorGUI();
		}
		if(meshEntity is BuildingTileInfo b)
		{
			if(b.buildingMesh != null && b.buildingMesh != b)
			{
				EditorGUILayout.InspectorTitlebar(true, b.buildingMesh);
				CreateEditor(b.buildingMesh).OnInspectorGUI();
			}
		}
		if (meshEntity is HQTileInfo hq)
		{
			if (hq.foundationTile != null && hq.foundationTile != hq)
			{
				EditorGUILayout.InspectorTitlebar(true, hq.foundationTile);
				CreateEditor(hq.foundationTile).OnInspectorGUI();
			}
		}
	}
}
