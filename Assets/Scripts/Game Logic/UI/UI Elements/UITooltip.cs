using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITooltip : MonoBehaviour
{
	public bool ToolTipVisible { get; private set; }
	public TMP_Text toolTipTitle;
	public TMP_Text toolTipBody;
	public TMP_Text toolTipCost;
	public TMP_Text toolTipProd;
	public Vector2 tooltipOffset;

	private RectTransform _rTransform;
	private GameObject _thisGameObject;

	void Awake()
    {
		_rTransform = GetComponent<RectTransform>();
		_thisGameObject = gameObject;
    }

	private void LateUpdate()
	{
		if (ToolTipVisible)
			UpdatePos();
	}

	void UpdatePos()
	{
		var pos = Input.mousePosition + (Vector3)tooltipOffset;
		pos.x = Mathf.Clamp(pos.x, 0, Screen.width - _rTransform.rect.width);
		pos.y = Mathf.Clamp(pos.y, 0, Screen.height - _rTransform.rect.height);
		_rTransform.anchoredPosition = pos;
	}

	public void ShowToolTip(string title, string body, string costInfo, string productionInfo)
	{
		UpdatePos();
		_thisGameObject.SetActive(ToolTipVisible = true);
		toolTipTitle.SetText(title);
		toolTipBody.SetText(body);
		toolTipCost.SetText(costInfo);
		toolTipProd.SetText(productionInfo);
	}

	public void HideToolTip()
	{
		_thisGameObject.gameObject.SetActive(ToolTipVisible = false);
	}
}
