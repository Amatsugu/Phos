using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public event Action OnHover;
	public event Action OnBlur;
	public bool isHovered;

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover?.Invoke();
		isHovered = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnBlur?.Invoke();
		isHovered = false;
	}

	public void ClearBlurEvents() => OnBlur = null;

	public void ClearHoverEvents() => OnHover = null;

	public virtual void ClearAllEvents()
	{
		ClearBlurEvents();
		ClearHoverEvents();
	}

	public void OnDisable()
	{
		OnBlur?.Invoke();
	}
}
