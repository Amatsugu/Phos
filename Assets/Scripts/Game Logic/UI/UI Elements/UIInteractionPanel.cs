using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionPanel : UIHover
{
	public Vector3 AnchoredPosition
	{
		get => _rectTransform.anchoredPosition;
		set => _rectTransform.anchoredPosition = value;
	}
	public bool PanelVisible { get; private set; }
	public float Width => _rectTransform.rect.width;
	public float Height => _rectTransform.rect.height;
	public TMP_Text titleText;
	public TMP_Text descText;
	public TMP_Text upgradeBtnText;
	public Button upgradeBtn;
	public TMP_Text destroyBtnText;
	public Button destroyBtn;

	public event Action OnUpgradeHover;
	public event Action OnUpgradeBlur;

	public event Action OnDestroyHover;
	public event Action OnDestroyBlur;

	public event Action OnUpgradeClick;
	public event Action OnDestroyClick;


	private RectTransform _rectTransform;
	private GameObject _gameObject;
    void Awake()
    {
		_rectTransform = GetComponent<RectTransform>();
		_gameObject = gameObject;

		upgradeBtn.onClick.AddListener(() => OnUpgradeClick?.Invoke());
		destroyBtn.onClick.AddListener(() => OnDestroyClick?.Invoke());
		var upgradeHover = upgradeBtn.GetComponent<UIHover>();
		upgradeHover.OnBlur += () => OnUpgradeBlur?.Invoke();
		upgradeHover.OnHover += () => OnUpgradeHover?.Invoke();
		var destroyHover = destroyBtn.GetComponent<UIHover>();
		destroyHover.OnBlur += () => OnDestroyBlur?.Invoke();
		destroyHover.OnHover += () => OnDestroyHover?.Invoke();
	}


	public void ShowPanel(string title, string desc, bool showUpgradeBtn = true, bool showDestroyBtn = true,  string upgradeText = "Upgrade", string destroyText = "Destroy")
	{
		_gameObject.SetActive(PanelVisible = true);
		titleText.SetText(title);
		descText.SetText(desc);
		upgradeBtn.gameObject.SetActive(showUpgradeBtn);
		upgradeBtnText.SetText(upgradeText);
		destroyBtn.gameObject.SetActive(showDestroyBtn);
		destroyBtnText.SetText(destroyText);
	}

	public void HidePanel()
	{
		_gameObject.SetActive(PanelVisible = false);
	}
}
