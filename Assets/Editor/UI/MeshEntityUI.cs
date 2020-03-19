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
		if(meshEntity is BuildingTileEntity b)
		{
			if(b.buildingMesh != null && b.buildingMesh != b)
			{
				EditorGUILayout.InspectorTitlebar(true, b.buildingMesh);
				CreateEditor(b.buildingMesh).OnInspectorGUI();
			}
		}
		/*if (meshEntity is HQTileInfo hq)
		{
			if (hq.subHQTile != null && hq.subHQTile != hq)
			{
				EditorGUILayout.InspectorTitlebar(true, hq.subHQTile);
				CreateEditor(hq.subHQTile).OnInspectorGUI();
			}
		}*/
	}
}
