using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ResearchTree;

[CustomEditor(typeof(ResearchTreeInfo))]
public class ResearchTreeUI : Editor
{
	private ResearchTreeEditorWindow _window;
	private ResearchTreeInfo treeInfo;

	void OnEnable()
	{
		treeInfo = target as ResearchTreeInfo;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField($"{treeInfo.tree.BaseNode.name} Count:{treeInfo.tree.Count}");
		treeInfo.tree.category = (BuildingCategory)EditorGUILayout.EnumPopup("Category", treeInfo.tree.category);
		if(GUILayout.Button("Edit Tree"))
		{
			_window = EditorWindow.GetWindow<ResearchTreeEditorWindow>();
			_window.target = treeInfo;
			//_window.serializedTarget = serializedObject;
			_window.titleContent = new GUIContent($"Research Tree: {_window.target.name}");
			_window.Show();
		}

		if (GUILayout.Button("Reset"))
		{
			treeInfo.tree.Reset();
		}
	}
}


public class ResearchTreeEditorWindow : EditorWindow
{
	public ResearchTreeInfo target;
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
			serializedTarget = new SerializedObject(target);
			//Close();
			//return;
		}
		_zoom = GUI.HorizontalSlider(new Rect(0, 0, 200, 20), _zoom, .5f, 2f);
		GUI.Label(new Rect(210, 0, 500, 20), $"Zoom: {Mathf.Round(_zoom * 10)/10}x\t Node Count: {target.tree.Count}\t Tree Depth:{target.tree.GetDepth()}\t Tree Breath: {_totalC}");
		_nodeSize = _baseNodeSize * _zoom;
		_nodeSpacing = _baseNodeSpacing * _zoom;
		_totalSize = _nodeSize + _nodeSpacing;
		GUI.Box(new Rect(0, 20, position.width, position.height - 20), GUIContent.none);
		var boxRect = new Rect(0, 20, position.width - 400, position.height - 20);
		_scrollPos = GUI.BeginScrollView(boxRect, _scrollPos, new Rect(0, 0, (target.tree.GetDepth()+1) * _totalSize.x, _totalC * _totalSize.y), true, true);
		_totalC = DrawTree(target.tree.BaseNode) + 1;
		GUI.EndScrollView();
		
		//Side Bar
		GUI.BeginGroup(new Rect(boxRect.width, 20, 400, boxRect.height));
		GUI.Box(new Rect(0,0, 400, boxRect.height), GUIContent.none);
		if(_selectedNode != null)
		{
			serializedTarget.UpdateIfRequiredOrScript();
			var itemRect = new Rect(10,0, 400, EditorGUIUtility.singleLineHeight);
			GUI.Label(itemRect, $"Move To ");
			if (_selectedNode.id == 0)
				GUI.enabled = false;
			var nodes = target.tree.nodes.Where(t => t != null && t.id != _selectedNode.id && t.Count < ResearchTech.MAX_CHILDREN).Where(t => !target.tree.IsDeepChild(_selectedNode, t));
			using (var change = new EditorGUI.ChangeCheckScope())
			{
				var selection = EditorGUI.IntPopup(itemRect, "Move To", 0, nodes.Select(t => $"{t.name} [{t.id}]").Prepend("--Select Dst--").ToArray(), nodes.Select(t => t.id + 1).Prepend(0).ToArray()); 
				if(change.changed && selection != 0)
				{
					target.tree.MoveChild(selection - 1, _selectedNode);
				}
			}
			GUI.enabled = true;
			itemRect.y += 25;
			itemRect.width = 150;
			GUI.Label(itemRect, $"Id {_selectedNode.id}");
			itemRect.y += 25;
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
			itemRect.height = EditorGUIUtility.singleLineHeight;
			itemRect.width = 150;
			itemRect.x = 10;
			GUI.Label(itemRect, "Description");
			itemRect.width = 240;
			itemRect.x = 150;
			itemRect.height = 200;
			_selectedNode.description = EditorGUI.TextArea(itemRect, _selectedNode.description);
			itemRect.y += 205;
			itemRect.x = 10;
			itemRect.width = 400- 60;
			itemRect.height = EditorGUIUtility.singleLineHeight;
			var rewardSP = serializedTarget.FindProperty($"tree.nodes.Array.data[{_selectedNode.id}].reward");
			_selectedNode.reward = EditorGUI.ObjectField(itemRect, _selectedNode.reward, typeof(ResearchReward), false) as ResearchReward;
			itemRect.x += itemRect.width + 5;
			itemRect.width = 35;
			if(GUI.Button(itemRect, "New"))
			{
				var reward = ScriptableObject.CreateInstance<ResearchReward>();
				var assetPath = $"Assets/GameData/Tech Trees/Rewards/[{_selectedNode.id}]{_selectedNode.name} Reward.asset";
				AssetDatabase.CreateAsset(reward, assetPath);
				_selectedNode.reward = reward;
				serializedTarget.UpdateIfRequiredOrScript();
			}
			itemRect.y += itemRect.height;
			itemRect.x = 10;
			itemRect.width = 400;
			var costSP = serializedTarget.FindProperty($"tree.nodes.Array.data[{_selectedNode.id}].resourceCost");
			if(costSP != null)
			{
				EditorGUI.PropertyField(itemRect, costSP, new GUIContent("Resource Cost"), true);
				var res = new ResourceIndentifier[costSP.arraySize];
				for (int i = 0; i < res.Length; i++)
				{
					var r = costSP.GetArrayElementAtIndex(i);
					res[i] = new ResourceIndentifier
					{
						id = r.FindPropertyRelative("id").intValue,
						ammount = r.FindPropertyRelative("ammount").floatValue
					};
				}
				_selectedNode.resourceCost = res;
			}else
				_selectedNode = target.tree.BaseNode;
		}
		else
		{
			_selectedNode = target.tree.BaseNode;
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
		curTech.name = EditorGUI.TextField(new Rect(depth == 0 ? 0 : 20, _nodeSize.y - 20, _nodeSize.x - (depth == 0 ? 20 : 40), 20), curTech.name);
		curTech.icon = EditorGUI.ObjectField(new Rect(0, 0, _nodeSize.x, _nodeSize.y - 20), curTech.icon, typeof(Sprite), false) as Sprite;
		if(GUI.Button(new Rect(_nodeSize.x - 20, _nodeSize.y - 20, 20,20), $"{curTech.id}"))
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
			lastC = DrawTree(target.tree.GetChild(curTech.childrenIDs[i]), depth + 1, i == 0 ? lastC : lastC + 1);
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
			target.tree.RemoveChild(curTech, removeAt);
			SaveObjectState();
		}
		if(curTech.Count < ResearchTech.MAX_CHILDREN)
		{
			if(GUI.Button(new Rect(pos.x + _nodeSize.x, pos.y + (_nodeSize.y/2) - 10, 20, 20), "+"))
			{
				_selectedNode = target.tree.AddChild(curTech, $"Node {curTech.Count}");
				SaveObjectState();
			}
		}
		return lastC;
	}

	void SaveObjectState()
	{
		serializedTarget.ApplyModifiedProperties();
		EditorUtility.SetDirty(target);
		Undo.RecordObject(target, $"Research Tree {target.tree.name}");
	}
}