using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResearchNode : UIHover
{
	public TMP_Text titleText;
	public TMP_Text descText;
	public Image icon;
	public RectTransform costDisplay;
	public Button button;
	public Outline outline;
	public RectTransform resourceCostPrefab;
	[HideInInspector]
	public int nodeId;

	[Header("Sections")]
	public RectTransform info;
	public RectTransform extraInfo;


	private UIResearchResource[] _uIResearchResources;
	private Vector2 _curSize;

	protected override void Awake()
	{
		base.Awake();
		_curSize = rTransform.rect.size;
		descText.enabled = false;
		OnHover += () =>
		{
			rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _curSize.y + descText.preferredHeight);
			rTransform.SetAsLastSibling();
			descText.enabled = true;
		};
		OnBlur += () =>
		{
			rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _curSize.y);
			descText.enabled = false;
		};
	}

	public void SetAnchoredPos(Vector3 pos)
	{
		rTransform.anchoredPosition = pos;
	}

	public void SetSize(Vector2 size)
	{
		_curSize = size;
		rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		if(isHovered)
			rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _curSize.y + descText.preferredHeight);
		else
			rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.y);
		icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		info.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		info.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x - size.y);
	}

	public void InitResources(ResourceIndentifier[] resources)
	{
		if (_uIResearchResources == null)
			_uIResearchResources = new UIResearchResource[resources.Length];
		if (_uIResearchResources.Length < resources.Length)
			Array.Resize(ref _uIResearchResources, resources.Length);
		for (int i = 0; i < resources.Length; i++)
		{
			if (_uIResearchResources[i] != null)
				continue;
			var uiRR = _uIResearchResources[i] = Instantiate(resourceCostPrefab, costDisplay).GetComponent<UIResearchResource>();
			uiRR.SetSize(costDisplay.rect.height, costDisplay.rect.height);
			uiRR.icon.sprite = ResourceDatabase.GetSprite(resources[i].id);
		}
	}

	public void UpdateProgress(ResourceIndentifier[] resources, int[] progress)
	{
		for (int i = 0; i < _uIResearchResources.Length; i++)
		{
			if(i >= resources.Length)
			{
				_uIResearchResources[i]?.gameObject.SetActive(false);
				continue;
			}
			_uIResearchResources[i].gameObject.SetActive(true);
			_uIResearchResources[i].UpdateData(progress[i], (int)resources[i].ammount, 0);
		}
	}
}
