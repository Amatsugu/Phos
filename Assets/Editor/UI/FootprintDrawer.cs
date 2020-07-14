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
			base.OnGUI(position, property, label);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			Debug.Log("test");
			var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Footprint.uxml");
			return template.CloneTree();
		}
	}
}
