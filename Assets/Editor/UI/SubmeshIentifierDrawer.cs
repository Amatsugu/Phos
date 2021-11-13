using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.ECS;
using Amatsugu.Phos.Units;
using System;

[CustomPropertyDrawer(typeof(SubMeshIdentifier))]
public class SubmeshIdentifierDrawer : PropertyDrawer
{

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		base.OnGUI(position, property, label);
		return;
		var idProp = property.FindPropertyRelative("id");
		var s = idProp.intValue;
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);
		var width = position.width;
		var resPos = new Rect(position.x, position.y, width, position.height);

		//Debug.Log(property.serializedObject.targetObject.name);
		string[] items = null;
		int i = 0;
		switch (property.serializedObject.targetObject)
		{
			case BuildingTileEntity b:
				//items = b.buildingMesh.subMeshes.Select(sb => sb.mesh != null ? $"[{i++}] {sb.mesh.name}" : "[Empty]").Prepend("[None]").ToArray(); 
				items = Array.Empty<string>();
				break;
			case MobileUnitEntity u:
				//items = u.subMeshes.Select(sb => sb.mesh != null ? $"[{i++}] {sb.mesh.name}" : "[Empty]").Prepend("[None]").ToArray();
				break;
			case BuildingMeshEntity bm:
				//items = bm.subMeshes.Select(sb => sb.mesh != null ? $"[{i++}] {sb.mesh.name}" : "[Empty]").Prepend("[Default]").ToArray();
				break;

		}
		s = EditorGUI.Popup(resPos, s+1, items ?? Array.Empty<string>());
		idProp.intValue = s-1;
		EditorGUI.EndProperty();
	}

}
