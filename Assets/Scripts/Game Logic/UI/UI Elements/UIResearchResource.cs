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
	public RectTransform progressBar;

	private int _rId;

	public void Init(int rId)
	{
		_rId = rId;
		resourceName.text = ResourceDatabase.GetResourceName(rId);
		icon.sprite = ResourceDatabase.GetSprite(rId);
	}

	public void UpdateData(int curProgress, int total, int lastTickDelta)
	{
		var progress = (float)curProgress / total;
		progressBar.anchorMax = new Vector2(progress, 1);
		resourceProgress.text = $"{curProgress}/{total}";
		resourceName.text = $"{ResourceDatabase.GetResourceName(_rId)} [{(lastTickDelta > 0 ? "+" : "")}{lastTickDelta}]";
	}
}
