
using Amatsugu.Phos.Assets.Editor.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Amatsugu.Phos.Editor.Drawer
{
	[CustomPropertyDrawer(typeof(StructureFootprint))]
	public class FootprintDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//base.OnGUI(position, property, label);
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);
			var pos = EditorGUIUtility.GUIToScreenRect(position);
			var size = property.FindPropertyRelative("size");
			var fp = property.FindPropertyRelative("footprint");
			if(fp.arraySize == 0)
				fp.arraySize = 1;
			property.serializedObject.ApplyModifiedProperties();
			if(GUI.Button(position, "Edit Footprint"))
			{
				var w = EditorWindow.CreateInstance(typeof(FootprintEditorWindow)) as FootprintEditorWindow;
				w.footprint = property;
				position.x = Screen.width - 10;
				w.titleContent = new GUIContent("Footprint");
				w.ShowAsDropDown(pos, new Vector2(position.width, position.width));
			}
			EditorGUI.EndProperty();
		}
	}
}

