using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Amatsugu.Phos.Editor.Window
{
	public class EditFootprintEditorWindow : EditorWindow
	{
		private SerializedProperty _prop;

		private void OnEnable()
		{
			AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI/Footprint.uxml").CloneTree(rootVisualElement);
		}

		public void Init(SerializedProperty prop)
		{
			var thisWindow = GetWindow<EditFootprintEditorWindow>();
			_prop = prop;
			var root = rootVisualElement;
			root.userData = prop.serializedObject;
			var size = root.Q("#Size");
			var centerBtn = root.Q("#center_button");
			/*var centerStyle = centerBtn.style;
			centerStyle.left = new StyleLength(position.width / 2 - 50);
			centerStyle.top = new StyleLength(position.height / 2 - 50);*/
			var tileBtn = root.Q("#tile_button");
			root.Remove(tileBtn);
			//var n = HexCoords.GetTileCount(prop.FindPropertyRelative("size").intValue);
			var coords = HexCoords.SpiralSelect(new HexCoords(0, 0, 100), prop.FindPropertyRelative("size").intValue, true);
			
			for (int i = 0; i < coords.Length; i++)
			{

			}
		}

		private void OnDisable()
		{
			_prop.serializedObject.ApplyModifiedProperties();
		}
	}
}
