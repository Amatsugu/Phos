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
	public RectTransform node;
	public RectTransform vertConnector;
	public RectTransform horizConnector;
	[Header("Sidebar")]
	public TMP_Text activeTitle;
	public TMP_Text activeDesc;
	public RectTransform activeCostParent;
	public GameObject UIresource;

	[Header("Config")]
	public Vector3 offset = new Vector2();
	public Vector2 nodeSize = new Vector2(100, 100);
	public Vector2 nodeSpacing = new Vector2(50, 50);

	public bool regen;


	private List<RectTransform> _uiElements;
	private List<UIResearchResource> _resources;

	private Vector2 _totalOffset;
	private ResearchTree _curTree;
	private UIPanel _thisPanel;
	private BuildingCategory _selectedCategory;

	void Awake()
	{
		GameRegistry.INST.researchTreeUI = this;
		GameRegistry.INST.researchDatabase = researchDatabase;
		_thisPanel = GetComponent<UIPanel>();
		_uiElements = new List<RectTransform>();
		_totalOffset = nodeSize + nodeSpacing;
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
		_resources = new List<UIResearchResource>();
		EventManager.AddEventListener("OnTick", () =>
		{
			if (!_thisPanel.IsOpen)
				return;
			var active = ResearchSystem.GetActiveResearchProgress(_selectedCategory);
			if (active == null)
				return;
			for (int i = 0; i < active.resources.Length; i++)
			{
				if(_resources.Count == i)
				{
					var uiRes = Instantiate(UIresource, activeCostParent, false).GetComponent<UIResearchResource>();
					_resources.Add(uiRes);
				}
				_resources[i].UpdateData(active.rProgress[i], (int)active.resources[i].ammount, active.lastTickProgress[i]);
			}
		});
	}

	public void ShowEconomyTree() => ShowTree(BuildingCategory.Economy);
	public void ShowDefenseTree() => ShowTree(BuildingCategory.Defense);
	public void ShowMilitaryTree() => ShowTree(BuildingCategory.Military);
	public void ShowResourcesTree() => ShowTree(BuildingCategory.Resources);
	public void ShowStructureTree() => ShowTree(BuildingCategory.Structure);
	public void ShowTechTree() => ShowTree(BuildingCategory.Tech);

	public void ShowTree(BuildingCategory category)
	{
		ClearTree();
		_selectedCategory = category;
		var c = DrawTree((_curTree = researchDatabase[category]).BaseNode) + 1;
		nodeParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, c * _totalOffset.y);
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
		uiNode.button.onClick.RemoveAllListeners();
		uiNode.button.onClick.AddListener(() =>
		{
			ResearchSystem.SetActiveResearch(new ResearchIdentifier
			{
				category = _selectedCategory,
				researchId = curTech.id
			});
		});
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nodeSize.x);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nodeSize.y);
		
		if (!curTech.isResearched)
		{
			if (parentResearched)
				uiNode.icon.color = new Color(.5f, .5f, .5f);
			else
				uiNode.icon.color = new Color(.2f, .2f, .2f);
		}
		_uiElements.Add(curNode);
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
			_uiElements.Add(curConnector);
			if(curTech.Count > 1 && i == curTech.Count-1)
			{
				var hPos = cPos;
				hPos.y = pos.y - (nodeSize.y/2);
				var hConnector = Instantiate(vertConnector, hPos, Quaternion.identity, nodeParent);
				hConnector.gameObject.SetActive(true);
				hConnector.anchoredPosition = hPos;
				hConnector.transform.SetParent(curNode);
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  pos.y - cPos.y - (nodeSize.y/2));
				_uiElements.Add(hConnector);
			}
			lastC = DrawTree(_curTree.GetChild(curTech.childrenIDs[i]), depth + 1, i == 0 ? lastC : lastC + 1, curTech.isResearched);
		}
		return lastC;
	}

	public void Show(ResearchBuildingTile tile)
	{
		_thisPanel.Show();
		ShowTree(BuildingCategory.Tech);
	}

	void ClearTree()
	{
		for (int i = 0; i < _uiElements.Count; i++)
		{
			Destroy(_uiElements[i].gameObject);
		}
		_uiElements.Clear();
	}


	public void Hide()
	{
		_thisPanel.Hide();
	}
}
