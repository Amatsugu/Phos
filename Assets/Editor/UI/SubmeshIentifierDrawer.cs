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

[CustomPropertyDrawer(typeof(SubMeshIdentifier))]
public class SubmeshIdentifierDrawer : PropertyDrawer
{

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var idProp = property.FindPropertyRelative("id");
		var s = idProp.intValue;
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);
		var width = position.width;
		var resPos = new Rect(position.x, position.y, width, position.height);
		if(property.serializedObject.targetObject is BuildingTileEntity b)
		{
			var selection = b.buildingMesh.subMeshes.Select(sb => sb.mesh != null ? sb.mesh.name : "[Empty]").Prepend("[None]").ToArray(); 
			s = EditorGUI.Popup(resPos, s+1, selection);
			idProp.intValue = s-1;
		}else if(property.serializedObject.targetObject is MobileUnitEntity u)
		{
			var selection = u.subMeshes.Select(sb => sb.mesh != null ? sb.mesh.name : "[Empty]").Prepend("[None]").ToArray();
			s = EditorGUI.Popup(resPos, s + 1, selection);
			idProp.intValue = s - 1;
		}
		EditorGUI.EndProperty();
	}

}
