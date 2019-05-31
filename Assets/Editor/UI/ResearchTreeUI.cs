using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ResearchTree;

[CustomEditor(typeof(ResearchTree))]
public class ResearchTreeUI : Editor
{
	private ResearchTreeEditorWindow _window;
	private ResearchTree tree;

	void OnEnable()
	{
		tree = target as ResearchTree;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField($"{tree.BaseNode.name} Count:{tree.Count}");
		if(GUILayout.Button("Edit Tree"))
		{
			_window = EditorWindow.GetWindow<ResearchTreeEditorWindow>();
			_window.target = tree;
			_window.serializedTarget = serializedObject;
			_window.titleContent = new GUIContent($"Research Tree: {_window.target.name}");
			_window.Show();
		}

		if (GUILayout.Button("Reset"))
		{
			tree.Reset();
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
	private ResearchTech _selectedNode;

	void OnGUI()
	{
		if (serializedTarget == null)
		{
			Close();
			return;
		}
		_zoom = GUI.HorizontalSlider(new Rect(0, 0, 200, 20), _zoom, .5f, 1f);
		GUI.Label(new Rect(210, 0, 200, 20), $"{Mathf.Round(_zoom * 10)/10}x Node Count: {target.Count}");
		_nodeSize = _baseNodeSize * _zoom;
		_nodeSpacing = _baseNodeSpacing * _zoom;
		_totalSize = _nodeSize + _nodeSpacing;
		GUI.Box(new Rect(0, 20, position.width, position.height - 20), GUIContent.none);
		var boxRect = new Rect(0, 20, position.width - 400, position.height - 20);
		_scrollPos = GUI.BeginScrollView(boxRect, _scrollPos, new Rect(0, 0, (target.GetDepth()+1) * _totalSize.x, _totalC * _totalSize.y), true, true);
		_totalC = DrawTree(target.BaseNode) + 1;
		
		GUI.EndScrollView();
		var curEvent = Event.current;
		if (curEvent.type == EventType.MouseMove)
			_scrollPos += curEvent.delta;

		//Side Bar
		GUI.BeginGroup(new Rect(boxRect.width, 20, 400, boxRect.height));
		GUI.Box(new Rect(0,0, 400, boxRect.height), GUIContent.none);
		if(_selectedNode != null)
		{
			var itemRect = new Rect(10,0, 150, 20);
			GUI.Label(itemRect, $"Id {_selectedNode.id}");
			itemRect.y = 25;
			GUI.Label(itemRect, "Name");
			itemRect.x = 150;
			itemRect.width = 240;
			_selectedNode.name = EditorGUI.TextField(itemRect, _selectedNode.name);
			itemRect.y += 25;
			itemRect.x = 10;
			itemRect.width = 150;
			GUI.Label(itemRect, "Icon");
			itemRect.width = 140;
			itemRect.height = 140;
			itemRect.x = 250;
			_selectedNode.icon = EditorGUI.ObjectField(itemRect, _selectedNode.icon, typeof(Sprite), false) as Sprite;
			itemRect.y += 145;
			itemRect.height = 20;
			itemRect.width = 150;
			itemRect.x = 10;
			GUI.Label(itemRect, "Description");
			itemRect.width = 240;
			itemRect.x = 150;
			itemRect.height = 200;
			_selectedNode.description = EditorGUI.TextArea(itemRect, _selectedNode.description);

		}
		else
		{
			_selectedNode = target.BaseNode;
		}
		GUI.EndGroup();
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
		var cEvent = Event.current;
		if (cEvent.isMouse && cEvent.button == 0 && nodeRect.Contains(cEvent.mousePosition))
		{
			_selectedNode = curTech;
		}
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
			lastC = DrawTree(target.GetChild(curTech.childrenIDs[i]), depth + 1, i == 0 ? lastC : lastC + 1);
			var minusPos = cPos;
			minusPos.x += (i == 0) ? _nodeSpacing.x : (_nodeSpacing.x / 2);
			minusPos.y += _nodeSize.y/2 - 20;
			if (GUI.Button(new Rect(minusPos, new Vector2(20, 20)), "-"))
			{
				removeAt = i;
			}
		}
		if(removeAt != -1)
		{
			target.RemoveChild(curTech, removeAt);
			SaveObjectState();
		}
		if(curTech.Count < ResearchTech.MAX_CHILDREN)
		{
			if(GUI.Button(new Rect(pos.x + _nodeSize.x, pos.y + (_nodeSize.y/2) - 10, 20, 20), "+"))
			{
				target.AddChild(curTech, new ResearchTech($"Node {curTech.Count}"));
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