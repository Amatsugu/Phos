using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResearchTree))]
public class ResearchTreeUI : Editor
{
	private ResearchTreeEditorWindow _window;

	public override void OnInspectorGUI()
	{
		if(GUILayout.Button("Edit Tree"))
		{
			_window = EditorWindow.GetWindow<ResearchTreeEditorWindow>();
			_window.target = target as ResearchTree;
			_window.serializedTarget = serializedObject;
			_window.name = $"Research Tree: {_window.target.name}";
			_window.Show();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
			Undo.RecordObject(target, $"Research Tree {target.name}");
		}
	}
}


public class ResearchTreeEditorWindow : EditorWindow
{
	public ResearchTree target;
	public SerializedObject serializedTarget;

	private Vector2 _baseNodeSize = new Vector2(100, 130);
	private Vector2 _baseNodeSpacing = new Vector2(25, 25);

	private Vector2 _offset = new Vector2(10, 10);
	private Vector2 _scrollPos = new Vector2();
	private Vector2 _nodeSize = new Vector2(100, 100);
	private Vector2 _nodeSpacing = new Vector2(50, 50);
	private Vector2 _totalSize;

	private float _zoom = 1;
	private int _totalC = 100;

	void OnGUI()
	{
		if (serializedTarget == null)
		{
			Close();
			return;
		}
		_zoom = GUI.HorizontalSlider(new Rect(0, 0, 200, 20), _zoom, .5f, 1f);
		GUI.Label(new Rect(210, 0, 200, 20), $"{Mathf.Round(_zoom * 10)/10}x Node Count: {target.Count()}");
		_nodeSize = _baseNodeSize * _zoom;
		_nodeSpacing = _baseNodeSpacing * _zoom;
		_totalSize = _nodeSize + _nodeSpacing;
		GUI.Box(new Rect(0, 20, position.width, position.height - 20), GUIContent.none);
		var boxRect = new Rect(0, 20, position.width, position.height - 20);
		_scrollPos = GUI.BeginScrollView(boxRect, _scrollPos, new Rect(0, 0, (target.GetDepth()+1) * _totalSize.x, _totalC * _totalSize.y), true, true);
		_totalC = DrawTree(target.baseNode) + 1;
		
		GUI.EndScrollView();
		var curEvent = Event.current;
		if (curEvent.type == EventType.MouseMove)
			_scrollPos += curEvent.delta;
	}

	int DrawTree(ResearchTech curTech, int depth = 0, int c = 0)
	{
		var pos = new Vector2((depth * _totalSize.x), (c * _totalSize.y));
		pos += _offset;
		var nodeRect = new Rect(pos, _nodeSize);
		EditorGUI.BeginChangeCheck();
		GUI.BeginGroup(nodeRect);
		GUI.Box(new Rect(Vector2.zero, _nodeSize), GUIContent.none);
		curTech.name = EditorGUI.TextField(new Rect(depth == 0 ? 0 : 20, _nodeSize.y - 20, _nodeSize.x, 20), curTech.name);
		curTech.icon = EditorGUI.ObjectField(new Rect(0, 0, _nodeSize.x, _nodeSize.y - 20), curTech.icon, typeof(Sprite), false) as Sprite;
		GUI.Label(new Rect(_nodeSize.x - 20, _nodeSize.y - 20, 20,20), $"{curTech.id}");
		GUI.EndGroup();
		if(EditorGUI.EndChangeCheck())
			SaveObjectState();
		var lastC = c;
		var removeAt = -1;
		for (int i = 0; i < curTech.Count; i++)
		{
			var cPos = pos;
			cPos.x += (i == 0) ? _nodeSize.x : (_nodeSize.x + (_nodeSpacing.x / 2));
			cPos.y = ((i == 0 ? lastC : lastC + 1) * _totalSize.y) + (_nodeSize.y / 2);
			cPos.y += _offset.y;
			GUI.Box(new Rect(cPos, new Vector2(_nodeSpacing.x/(i == 0 ? 1 : 2), 1)), GUIContent.none);
			if (curTech.Count > 1 && i == curTech.Count - 1)
			{
				var hPos = cPos;
				//hPos.y -= 25;
				hPos.y = pos.y + (_nodeSize.y / 2);
				GUI.Box(new Rect(hPos, new Vector2(1, cPos.y - pos.y - (_nodeSize.y / 2))), GUIContent.none);
			}
			lastC = DrawTree(curTech.children[i], depth + 1, i == 0 ? lastC : lastC + 1);
			var minusPos = cPos;
			minusPos.x += (i == 0) ? _nodeSpacing.x : (_nodeSpacing.x / 2);
			minusPos.y += _nodeSize.y/2 - 20; 
			if (GUI.Button(new Rect(minusPos, new Vector2(20,20)), "-"))
			{
				removeAt = i;
			}
		}
		if(removeAt != -1)
		{
			curTech.RemoveChild(removeAt);
			SaveObjectState();
		}
		if(curTech.Count < ResearchTech.MAX_CHILDREN)
		{
			if(GUI.Button(new Rect(pos.x + _nodeSize.x, pos.y + (_nodeSize.y/2) - 10, 20, 20), "+"))
			{
				curTech.AddChild(new ResearchTech($"Node {curTech.Count}"));
				SaveObjectState();
			}
		}
		return lastC;
	}

	void SaveObjectState()
	{
		serializedTarget.ApplyModifiedProperties();
		EditorUtility.SetDirty(target);
		Undo.RecordObject(target, $"Research Tree {target.name}");
	}
}