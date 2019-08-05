using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[HideInInspector]
	public bool isHovered;

	public event Action OnHover;
	public event Action OnBlur;
	[HideInInspector]
	public RectTransform rTransform;

	public bool IsActive => gameObject.activeInHierarchy;

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

	protected virtual void Awake()
	{
		rTransform = GetComponent<RectTransform>();
	}

	protected virtual void Start()
	{

	}

	protected virtual void Update()
	{

	}

	protected virtual void LateUpdate()
	{

	}

	public virtual void SetActive(bool active)
	{
		if (IsActive == active)
			return;
		gameObject.SetActive(active);
		if(!active)
			isHovered = false;
	}
}
