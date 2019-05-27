using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchTreeUI : MonoBehaviour
{
	public RectTransform node;
	public RectTransform vertConnector;
	public RectTransform horizConnector;

	public Vector3 offset = new Vector2();
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
		/*_researchTree = new ResearchTree(new ResearchTech("Base Node", isResearched: true));
		_researchTree.baseNode
			.AddChild(new ResearchTech("T1 Node", isResearched: true)
				.AddChild(new ResearchTech("T2 Node"))
				.AddChild(new ResearchTech("T2 Node", isResearched: true)
					.AddChild(new ResearchTech("T3 Node"))
					.AddChild(new ResearchTech("T3 Node")
						.AddChild(new ResearchTech("T4 Node")))))
			.AddChild(new ResearchTech("T1 Node")
				.AddChild(new ResearchTech("T2 Node"))
				.AddChild(new ResearchTech("T2 Node")));

		_totalOffset = nodeSize + nodeSpacing;

		DrawTree(_researchTree.baseNode);*/
	}

	int DrawTree(ResearchTech curTech, int depth = 0, int c = 0, bool parentResearched = true)
	{
		var pos = new Vector3((depth * _totalOffset.y) + offset.x, (-c * _totalOffset.x) - offset.y);
		var curNode = Instantiate(node, pos, Quaternion.identity, transform);
		curNode.anchoredPosition = pos;
		curNode.gameObject.SetActive(true);
		curNode.gameObject.name = $"d: {depth} c:{c}";
		curNode.GetComponentInChildren<TMP_Text>().SetText($"{curTech.name} [{curTech.id}]");
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nodeSize.x);
		curNode.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nodeSize.y);
		if (!curTech.isResearched)
		{
			if (parentResearched)
				curNode.GetComponent<Image>().color = new Color(.5f, .5f, .5f);
			else
				curNode.GetComponent<Image>().color = new Color(.2f, .2f, .2f);
		}
		uiElements.Add(curNode);
		var lastC = c;
		for (int i = 0; i < curTech.Count; i++)
		{
			var cPos = pos;
			cPos.x += (i == 0) ? nodeSize.x : (nodeSize.x + (nodeSpacing.x/2));
			cPos.y = ((i == 0 ? lastC : lastC + 1) * -_totalOffset.y) - (nodeSize.y/2);
			cPos.y -= offset.y;
			var curConnector = Instantiate(horizConnector, cPos, Quaternion.identity, transform);
			curConnector.anchoredPosition = cPos;
			curConnector.transform.SetParent(curNode);
			curConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, i == 0 ? nodeSpacing.x : nodeSpacing.x/2);
			uiElements.Add(curConnector);
			if(curTech.Count > 1 && i == curTech.Count-1)
			{
				var hPos = cPos;
				//hPos.y -= 25;
				hPos.y = pos.y - (nodeSize.y/2);
				var hConnector = Instantiate(vertConnector, hPos, Quaternion.identity, transform);
				hConnector.anchoredPosition = hPos;
				hConnector.transform.SetParent(curNode);
				hConnector.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  pos.y - cPos.y - (nodeSize.y/2));
				uiElements.Add(hConnector);
			}
			lastC = DrawTree(curTech.children[i], depth + 1, i == 0 ? lastC : lastC + 1, curTech.isResearched);
		}
		return lastC;
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
