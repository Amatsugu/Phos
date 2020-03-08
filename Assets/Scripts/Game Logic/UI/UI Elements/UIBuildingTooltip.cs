using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIBuildingTooltip : UIPanel
{
	public Image icon;
	public RectTransform panelBase;
	public TMP_Text toolTipTitle;
	public TMP_Text toolTipBody;
	public TMP_Text toolTipCost;
	public TMP_Text toolTipProd;
	public float speed = 2;

	private float _animTime;
	private bool _isClosing;

	public void ShowToolTip(Sprite icon, string title, string body, string costInfo, string productionInfo)
	{
		Show();
		this.icon.sprite = icon;
		toolTipTitle.SetText(title);
		toolTipBody.SetText(body);
		toolTipCost.SetText(costInfo);
		toolTipProd.SetText(productionInfo);
	}

	protected override void Update()
	{
		base.Update();
		if (_isClosing)
			_animTime -= Time.unscaledDeltaTime * speed;
		else
			_animTime += Time.unscaledDeltaTime * speed;
		_animTime = Mathf.Clamp(_animTime, 0, 1);
		var t = 1 - _animTime;
		t *= t * t * t;
		t = 1 - t;
		panelBase.anchoredPosition = Vector2.Lerp(new Vector2(0, -panelBase.rect.height), Vector2.zero, t);
		if (_isClosing && _animTime == 0)
			base.Hide();
	}

	public override void Show()
	{
		if (!IsOpen)
		{
			_animTime = 0;
		}
		_isClosing = false;
		base.Show();
	}

	public override void Hide()
	{
		_isClosing = true;
	}
}