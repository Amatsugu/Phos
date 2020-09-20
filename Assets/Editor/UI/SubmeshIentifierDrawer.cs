using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using Amatsugu.Phos.TileEntities;

[CustomPropertyDrawer(typeof(BuildingMeshEntity.SubMeshIdentifier))]
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
			var selection = b.buildingMesh.subMeshes.Select(sb => sb.mesh?.name ?? "[Empty]").ToArray(); 
			s = EditorGUI.Popup(resPos, s, selection);
			idProp.intValue = s;
		}
		EditorGUI.EndProperty();
	}

}
