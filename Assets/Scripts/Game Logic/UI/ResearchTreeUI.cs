using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ResearchTree;

public class ResearchTreeUI : UIPanel, IBuildingUI
{
	public ResearchDatabase researchDatabase;
	public RectTransform nodeParent;
	[Header("Elements")]
	public RectTransform node;
	public RectTransform vertConnector;
	public RectTransform horizConnector;

	[Header("Config")]
	public Vector3 offset = new Vector2();
	public Vector2 nodeSize = new Vector2(100, 100);
	public Vector2 nodeSpacing = new Vector2(50, 50);

	public bool regen;


	private List<RectTransform> uiElements;

	private Vector2 _totalOffset;
	private ResearchTree _curTree;

	void Awake()
	{
		GameRegistry.INST.researchTreeUI = this;
		GameRegistry.INST.researchDatabase = researchDatabase;
	}

	void Start()
	{
		uiElements = new List<RectTransform>();
		_totalOffset = nodeSize + nodeSpacing;
		OnShow += OnOpened;
		OnHide += () => GameRegistry.BuildUI.gameObject.SetActive(false);
		Show();
	}

	int DrawTree(ResearchTech curTech, int depth = 0, int c = 0, bool parentResearched = true)
	{
		var pos = new Vector3((depth * _totalOffset.x) + offset.x, (-c * _totalOffset.y) - offset.y);
		var curNode = Instantiate(node, pos, Quaternion.identity, nodeParent);
		curNode.anchoredPosition = pos;
		curNode.gameObject.SetActive(true);
		curNode.gameObject.name = $"d: {depth} c:{c}";
		var uiNode = curNode.GetComponent<UIResearchNode>();
		uiNode.icon.sprite = curTech.icon;
		uiNode.titleText.SetText(curTech.name);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nodeSize.x);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nodeSize.y);
		if (!curTech.isResearched)
		{
			if (parentResearched)
				uiNode.icon.color = new Color(.5f, .5f, .5f);
			else
				uiNode.icon.color = new Color(.2f, .2f, .2f);
		}
		uiElements.Add(curNode);
		var lastC = c;
		for (int i = 0; i < curTech.Count; i++)
		{
			var cPos = pos;
			cPos.x += (i == 0) ? nodeSize.x : (nodeSize.x + (nodeSpacing.x/2));
			cPos.y = ((i == 0 ? lastC : lastC + 1) * -_totalOffset.y) - (nodeSize.y/2);
			cPos.y -= offset.y;
			var curConnector = Instantiate(horizConnector, cPos, Quaternion.identity, nodeParent);
			curConnector.gameObject.SetActive(true);
			curConnector.anchoredPosition = cPos;
			curConnector.transform.SetParent(curNode);
			curConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, i == 0 ? nodeSpacing.x : nodeSpacing.x/2);
			uiElements.Add(curConnector);
			if(curTech.Count > 1 && i == curTech.Count-1)
			{
				var hPos = cPos;
				//hPos.y -= 25;
				hPos.y = pos.y - (nodeSize.y/2);
				var hConnector = Instantiate(vertConnector, hPos, Quaternion.identity, nodeParent);
				hConnector.gameObject.SetActive(true);
				hConnector.anchoredPosition = hPos;
				hConnector.transform.SetParent(curNode);
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  pos.y - cPos.y - (nodeSize.y/2));
				uiElements.Add(hConnector);
			}
			lastC = DrawTree(_curTree.GetChild(curTech.childrenIDs[i]), depth + 1, i == 0 ? lastC : lastC + 1, curTech.isResearched);
		}
		return lastC;
	}

	public void Show(IteractiveBuildingTile target)
	{
		Show();
	}

	public void OnOpened()
	{
		GameRegistry.BuildUI.gameObject.SetActive(false);
		for (int i = 0; i < uiElements.Count; i++)
		{
			Destroy(uiElements[i].gameObject);
		}
		uiElements.Clear();
		var c = DrawTree((_curTree = researchDatabase[BuildingCategory.Hidden]).BaseNode) + 1;
		nodeParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, c * _totalOffset.y);
	}
}
