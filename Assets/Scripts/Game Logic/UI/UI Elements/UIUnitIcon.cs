using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnitIcon : UIButtonHover, IPointerClickHandler
{
	public event System.Action OnClick;

	public TMP_Text titleText;
	public TMP_Text costText;
	public Button button;
	public Image icon;

	protected override void Awake()
	{
		base.Awake();
		GameEvents.OnGameTick += OnTick;
	}

	public void OnTick()
	{
		if (IsActive() && isHovered)
			Hover();
	}

	public Vector3 AnchoredPosition
	{
		get => rTransform.anchoredPosition;
		set => rTransform.anchoredPosition = value;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick?.Invoke();
	}

	public void ClearClickEvents() => OnClick = null;

	public override void ClearAllEvents()
	{
		base.ClearAllEvents();
		ClearClickEvents();
	}
}