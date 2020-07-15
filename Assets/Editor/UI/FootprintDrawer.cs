using Amatsugu.Phos.Editor.Window;

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
			if(GUI.Button(position, "Edit"))
			{
				var w = EditorWindow.GetWindow<EditFootprintEditorWindow>("Structure Footprint");
				w.ShowAsDropDown(position, new Vector2(position.width, position.width));
				w.Init(property);
			}
			EditorGUI.EndProperty();
		}
	}
}

