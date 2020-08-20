using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Mathematics;

using UnityEditor;

using UnityEngine;

namespace Amatsugu.Phos.Assets.Editor.UI
{
	public class FootprintEditorWindow : EditorWindow
	{
		public SerializedProperty footprint;

		public void OnGUI()
		{
			var initGUIDepth = GUI.depth;
			var size = footprint.FindPropertyRelative("size");
			var fp = footprint.FindPropertyRelative("footprint");
			GUILayout.Label($"Size: {size.intValue}");
			EditorGUILayout.PropertyField(size);
			var sizeInt = math.clamp(size.intValue, 0, 3);
			size.intValue = sizeInt;
			
			var windowCenterOffset = (float2)position.size/2;
			var btnSize = 30;
			var ir = HexCoords.CalculateInnerRadius(btnSize);
			windowCenterOffset.x -= HexCoords.CalculateShortDiagonal(btnSize) / 2;
			var cellSize = new float2(ir * 1.9f, btnSize * 1.9f);

			var active = new List<int2>();
			var curActive = new HashSet<int2>();
			for (int i = 0; i < fp.arraySize; i++)
			{
				var p = fp.GetArrayElementAtIndex(i);
				var x = p.FindPropertyRelative("x").intValue;
				var y = p.FindPropertyRelative("y").intValue;
				curActive.Add(new int2(x,y));
			}
			if (curActive.Count == 0)
			{
				curActive.Add(0);
				active.Add(0);
			}
			var centerCoord = HexCoords.Zero(btnSize);
			var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Cell.psd");
			var inactiveTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/WorldSpace/Tile Fill 50.png");
			var coords = HexCoords.SpiralSelect(centerCoord, sizeInt);
			//Render Hexes
			for (int i = 0; i < coords.Length; i++)
			{
				var curPos = new int2(coords[i].X, coords[i].Y);
				var guiPos = coords[i].WorldPos.xz;
				guiPos.y *= -1;
				guiPos += windowCenterOffset;
				var posRect = new Rect(guiPos, cellSize);
				GUI.depth = initGUIDepth;
				GUI.DrawTexture(posRect, curActive.Contains(curPos) ? tex : inactiveTex);
			}
			//Render Checkboxes
			for (int i = 0; i < coords.Length; i++)
			{
				var curPos = new int2(coords[i].X, coords[i].Y);
				var guiPos = coords[i].WorldPos.xz;
				guiPos.y *= -1;
				guiPos += windowCenterOffset;
				var posRect = new Rect(guiPos, new Vector2(15, 15));
				posRect.position += (Vector2)(cellSize/2);
				posRect.position -= posRect.size/2;
				if (curPos.Equals(int2.zero))
					GUI.enabled = false;
				if (GUI.Toggle(posRect, curActive.Contains(curPos), GUIContent.none))
					active.Add(curPos);
				GUI.enabled = true;
			}


			GUI.depth = initGUIDepth;
			fp.arraySize = active.Count;
			for (int i = 0; i < active.Count; i++)
			{
				var p = fp.GetArrayElementAtIndex(i);
				p.FindPropertyRelative("x").intValue = active[i].x;
				p.FindPropertyRelative("y").intValue = active[i].y;
			}
			fp.serializedObject.ApplyModifiedProperties();
			size.serializedObject.ApplyModifiedProperties();
		}
	}
}
