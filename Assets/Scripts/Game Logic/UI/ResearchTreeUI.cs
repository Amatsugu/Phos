using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ResearchTree;

[RequireComponent(typeof(UIPanel))]
public class ResearchTreeUI : MonoBehaviour
{
	public ResearchDatabase researchDatabase;
	public RectTransform nodeParent;
	[Header("Elements")]
	public RectTransform nodeOriginal;
	public RectTransform connector;
	[Header("Sidebar")]
	public TMP_Text activeTitle;
	public TMP_Text activeDesc;
	public RectTransform activeCostParent;
	public GameObject UIresource;

	[Header("Config")]
	public Vector3 offset = new Vector2();
	public Vector2 nodeSpacing = new Vector2(50, 50);

	public bool regen;


	private UIResearchNode[] _uiNodes;
	private RectTransform[] _uiNodeConnectors;
	private UIResearchResource[] _resources;
	private Vector2 _nodeSize;
	private int _curNodeElement = 0;
	private int _curConnectorElem = 0;

	private Vector2 _totalOffset;
	private ResearchTree _curTree;
	private UIPanel _thisPanel;
	private BuildingCategory _selectedCategory;

	void Awake()
	{
		GameRegistry.INST.researchTreeUI = this;
		GameRegistry.INST.researchDatabase = researchDatabase;
		_thisPanel = GetComponent<UIPanel>();
		_uiNodes = new UIResearchNode[0];
		_uiNodeConnectors = new RectTransform[0];
		_nodeSize = nodeOriginal.rect.size;
		_totalOffset = _nodeSize + nodeSpacing;
		_thisPanel.OnShow += () =>
		{
			GameRegistry.BuildUI.gameObject.SetActive(false);
			GameRegistry.InteractionUI.enabled = false;
			GameRegistry.CameraController.enabled = false;
		};
		_thisPanel.OnHide += () =>
		{
			GameRegistry.BuildUI.gameObject.SetActive(true);
			GameRegistry.InteractionUI.enabled = true;
			GameRegistry.CameraController.enabled = true;
		};
		
	}

	void Start()
	{
		_resources = new UIResearchResource[0];
		EventManager.AddEventListener("OnTick", () =>
		{
			if (!_thisPanel.IsOpen)
				return;
			var active = ResearchSystem.GetActiveResearchProgress(_selectedCategory);
			if (active == null)
				return;
			ShowActiveInfo();
			_curNodeElement = 0;
			_curConnectorElem = 0;
			DrawTree(_curTree.BaseNode);
			if (_resources.Length < active.resources.Length)
				Array.Resize(ref _resources, active.resources.Length);
			for (int i = 0; i < _resources.Length; i++)
			{
				if(_resources[i] == null)
				{
					var uiRes = Instantiate(UIresource, activeCostParent, false).GetComponent<UIResearchResource>();
					uiRes.gameObject.SetActive(true);
					_resources[i] = uiRes;
				}
				if(i >= active.resources.Length)
				{
					_resources[i].gameObject.SetActive(false);
					continue;
				}
				_resources[i].gameObject.SetActive(true);
				_resources[i].SetResource(active.resources[i].id);
				_resources[i].UpdateData(active.rProgress[i], (int)active.resources[i].ammount, active.lastTickProgress[i]);
			}
		});
	}

	public void ShowProductionTree() => ShowTree(BuildingCategory.Production);
	public void ShowDefenseTree() => ShowTree(BuildingCategory.Defense);
	public void ShowMilitaryTree() => ShowTree(BuildingCategory.Military);
	public void ShowResourcesTree() => ShowTree(BuildingCategory.Resources);
	public void ShowStructureTree() => ShowTree(BuildingCategory.Structure);
	public void ShowTechTree() => ShowTree(BuildingCategory.Tech);

	public void ShowTree(BuildingCategory category)
	{
		ClearTree();
		_selectedCategory = category;
		_curTree = researchDatabase[category];
		if (_uiNodes.Length < _curTree.Count)
			Array.Resize(ref _uiNodes, _curTree.Count);

		var c = DrawTree(_curTree.BaseNode) + 1;
		nodeParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, c * _totalOffset.y);
	}

	int DrawTree(ResearchTech curTech, int depth = 0, int c = 0, bool parentResearched = true)
	{
		var pos = new Vector3((depth * _totalOffset.x) + offset.x, (-c * _totalOffset.y) - offset.y);

		UIResearchNode uiNode = null;
		if (_uiNodes[_curNodeElement] == null)
			uiNode = _uiNodes[_curNodeElement] = Instantiate(nodeOriginal, pos, Quaternion.identity, nodeParent).GetComponent<UIResearchNode>();
		else
			uiNode = _uiNodes[_curNodeElement];
		_curNodeElement++;
		uiNode.nodeId = curTech.id;
		uiNode.gameObject.name = $"{curTech.name} [d: {depth} c:{c}]";
		uiNode.SetActive(true);
		uiNode.SetAnchoredPos(pos);
		uiNode.icon.sprite = curTech.icon;
		uiNode.titleText.SetText(curTech.name);
		uiNode.button.onClick.RemoveAllListeners();
		if (curTech != _curTree.BaseNode)
		{
			uiNode.button.onClick.AddListener(() =>
			{
				ResearchSystem.SetActiveResearch(new ResearchIdentifier
				{
					category = _selectedCategory,
					researchId = curTech.id
				});
			});
		}
		var prog = ResearchSystem.GetResearchProgress(new ResearchIdentifier
		{
			category = _selectedCategory,
			researchId = curTech.id
		});
		uiNode.SetSize(_nodeSize);
		uiNode.InitResources(curTech.resourceCost);
		uiNode.UpdateProgress(curTech.resourceCost, prog?.rProgress ?? new int[curTech.resourceCost.Length]);
		bool isResearched = ResearchSystem.IsResearchUnlocked(new ResearchIdentifier
		{
			category = _selectedCategory,
			researchId = curTech.id
		});

		if (!isResearched)
		{
			var active = ResearchSystem.GetActiveResearchProgress(_selectedCategory);
			if (active != null && active.identifier.researchId == curTech.id)
				uiNode.outline.effectColor = Color.cyan;
			else if (parentResearched)
				uiNode.outline.effectColor = Color.magenta;
			else
				uiNode.outline.effectColor = new Color(.2f, .2f, .2f);
		}else
		{
			uiNode.outline.effectColor = Color.clear;
		}
		var lastC = c;
		var expectedConnectorCount = curTech.Count + (curTech.Count > 1 ? 1 : 0);
		//TODO: Sort out this calculation
		if (_curConnectorElem + expectedConnectorCount >= _uiNodeConnectors.Length)
			Array.Resize(ref _uiNodeConnectors, _uiNodeConnectors.Length + expectedConnectorCount +1);
		for (int i = 0; i < curTech.Count; i++)
		{
			var cPos = pos;
			cPos.x += (i == 0) ? _nodeSize.x : (_nodeSize.x + (nodeSpacing.x/2));
			cPos.y = ((i == 0 ? lastC : lastC + 1) * -_totalOffset.y) - (_nodeSize.y/2);
			cPos.y -= offset.y;

			RectTransform vConnector = null;
			if (_uiNodeConnectors[_curConnectorElem] == null)
				vConnector = _uiNodeConnectors[_curConnectorElem] = Instantiate(connector, cPos, Quaternion.identity, nodeParent);
			else
				vConnector = _uiNodeConnectors[_curConnectorElem];
			_curConnectorElem++;
			vConnector.gameObject.SetActive(true);
			vConnector.anchoredPosition = cPos;
			vConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, i == 0 ? nodeSpacing.x : nodeSpacing.x/2);
			vConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);
			if (curTech.Count > 1 && i == curTech.Count-1)
			{
				var hPos = cPos;
				hPos.y = pos.y - (_nodeSize.y/2);

				RectTransform hConnector = null;
				if (_uiNodeConnectors[_curConnectorElem] == null)
					hConnector = _uiNodeConnectors[_curConnectorElem] = Instantiate(connector, hPos, Quaternion.identity, nodeParent);
				else
					hConnector = _uiNodeConnectors[_curConnectorElem];
				_curConnectorElem++;
				hConnector.gameObject.SetActive(true);
				hConnector.anchoredPosition = hPos;
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  pos.y - cPos.y - (_nodeSize.y/2));
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
			}
			lastC = DrawTree(_curTree.GetChild(curTech.childrenIDs[i]), depth + 1, i == 0 ? lastC : lastC + 1, isResearched);
		}
		return lastC;
	}

	public void Show(ResearchBuildingTile tile)
	{
		_thisPanel.Show();
		if (tile == null)
			ShowTree(BuildingCategory.Production);
		else
			ShowTree(tile.researchInfo.researchCategory);
		ShowActiveInfo();
	}

	public void ShowActiveInfo()
	{
		var curRes = ResearchSystem.GetActiveResearchProgress(_selectedCategory);
		if (curRes == null)
			return;
		activeTitle.text = GameRegistry.ResearchDatabase[curRes.identifier].name;
		activeDesc.text = GameRegistry.ResearchDatabase[curRes.identifier].description;
		activeDesc.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, activeDesc.preferredHeight);
	}

	void ClearTree()
	{
		
		for (int i = 0; i < _curNodeElement; i++)
			_uiNodes[i].SetActive(false);
		_curNodeElement = 0;
		for (int i = 0; i < _curConnectorElem; i++)
			_uiNodeConnectors[i].gameObject.SetActive(false);
		_curConnectorElem = 0;
	}


	public void Hide()
	{
		_thisPanel.Hide();
	}
}
