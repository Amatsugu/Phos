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

	public void Blur()
	{
		OnBlur?.Invoke();
	}

	public void Hover()
	{
		OnHover?.Invoke();
	}

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

	public virtual void OnDisable()
	{
		OnBlur?.Invoke();
	}

	public virtual void OnEnable()
	{
		rTransform = rTransform ?? GetComponent<RectTransform>();
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
		rTransform = rTransform ?? GetComponent<RectTransform>();
		gameObject.SetActive(active);
		if(!active)
			isHovered = false;
	}
}
