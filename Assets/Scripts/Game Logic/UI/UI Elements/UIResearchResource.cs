using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResearchResource : MonoBehaviour
{
	public TMP_Text resourceName;
	public TMP_Text resourceProgress;
	public Image icon;
	public Image progressBar;

	private int _rId;

	public void SetResource(int rId)
	{
		_rId = rId;
		if(resourceName != null)
			resourceName.text = ResourceDatabase.GetResourceName(rId);
		icon.sprite = ResourceDatabase.GetSprite(rId);
	}

	public void UpdateData(int curProgress, int total, int lastTickDelta)
	{
		var progress = (float)curProgress / total;
		progressBar.fillAmount = progress;
		if(resourceProgress != null)
			resourceProgress.text = $"{curProgress}/{total}";
		if (resourceName == null)
			return;
		var deltaText = "";
		if(lastTickDelta != 0)
			deltaText = $"[{ (lastTickDelta > 0 ? "+" : "")}{ lastTickDelta}]";
		resourceName.text = $"{ResourceDatabase.GetResourceName(_rId)} {deltaText}";
	}

	public void SetSize(float height, float width)
	{
		var rT = GetComponent<RectTransform>();
		rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
	}
}
