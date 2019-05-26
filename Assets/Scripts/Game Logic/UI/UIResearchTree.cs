using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIResearchTree : MonoBehaviour
{
	public RectTransform node;
	public RectTransform vertConnector;
	public RectTransform horizConnector;

	public Vector2 nodeSize = new Vector2(100, 100);
	public Vector2 nodeSpacing = new Vector2(50, 50);

	public bool regen;


	private ResearchTree _researchTree;
	private List<RectTransform> uiElements;

	private Vector2 _totalOffset;

	void Start()
	{
		uiElements = new List<RectTransform>();
		//Test research
		_researchTree = new ResearchTree(new ResearchTech("Base Node"));
		_researchTree.baseNode
			.AddChild(new ResearchTech("T1 Node")
				.AddChild(new ResearchTech("T2 Node"))
				.AddChild(new ResearchTech("T2 Node")
					.AddChild(new ResearchTech("T3 Node"))
					.AddChild(new ResearchTech("T3 Node")
						.AddChild(new ResearchTech("T4 Node")))))
			.AddChild(new ResearchTech("T1 Node")
				.AddChild(new ResearchTech("T2 Node"))
					.AddChild(new ResearchTech("T3 Node")));

		_totalOffset = nodeSize + nodeSpacing;

		DrawTree(_researchTree.baseNode);
	}

	int DrawTree(ResearchTech curTech, int depth = 0, int c = 0)
	{
		var pos = new Vector3((c * _totalOffset.x), (-depth * _totalOffset.y));
		var curNode = Instantiate(node, pos, Quaternion.identity, transform);
		curNode.anchoredPosition = pos;
		curNode.gameObject.SetActive(true);
		curNode.gameObject.name = $"d: {depth} c:{c}";
		curNode.GetComponentInChildren<TMP_Text>().SetText(curTech.name);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nodeSize.x);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nodeSize.y);
		uiElements.Add(curNode);
		var lastWidth = c;
		for (int i = 0; i < curTech.Count; i++)
		{
			var cPos = pos;
			cPos.y -= (i == 0) ? nodeSize.y : (nodeSize.y + (nodeSpacing.y/2));
			cPos.x = ((lastWidth + i) * _totalOffset.x) + (nodeSize.x/2);
			var curConnector = Instantiate(vertConnector, cPos, Quaternion.identity, transform);
			curConnector.anchoredPosition = cPos;
			curConnector.transform.SetParent(curNode);
			curConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, i == 0 ? nodeSpacing.y : nodeSpacing.y/2);
			uiElements.Add(curConnector);
			if(curTech.Count > 1 && i == curTech.Count-1)
			{
				var hPos = cPos;
				//hPos.y -= 25;
				hPos.x = pos.x + (nodeSize.x/2);
				var hConnector = Instantiate(horizConnector, hPos, Quaternion.identity, transform);
				hConnector.anchoredPosition = hPos;
				hConnector.transform.SetParent(curNode);
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cPos.x - pos.x - (nodeSize.x/2));
				uiElements.Add(hConnector);
			}
			lastWidth = DrawTree(curTech.children[i], depth + 1, lastWidth + i);
		}
		return lastWidth;
	}

	void OnValidate()
	{
		if (!Application.isPlaying)
			return;
		if (uiElements == null)
			return;
		for (int i = 0; i < uiElements.Count; i++)
		{
			Destroy(uiElements[i].gameObject);
		}
		uiElements.Clear();
		_totalOffset = nodeSize + nodeSpacing;
		DrawTree(_researchTree.baseNode);
	}
}
