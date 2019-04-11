using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public event Action OnHover;
	public event Action OnBlur;

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnBlur?.Invoke();
	}

	public void OnDisable()
	{
		OnBlur?.Invoke();
	}
}
