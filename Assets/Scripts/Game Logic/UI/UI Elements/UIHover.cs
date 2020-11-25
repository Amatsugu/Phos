using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHover : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[HideInInspector]
	public bool isHovered;

	public event Action OnHover;

	public event Action OnBlur;

	[HideInInspector]
	public RectTransform rTransform;

	//public bool IsActive => gameObject.activeInHierarchy;

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

	protected override void OnDisable()
	{
		base.OnDisable();
		OnBlur?.Invoke();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if(rTransform != null)
			rTransform = GetComponent<RectTransform>();
	}

	protected override void Awake()
	{
		base.Awake();
		rTransform = GetComponent<RectTransform>();
	}
	protected override void Start()
	{
		base.Start();
		rTransform = GetComponent<RectTransform>();
	}

	protected virtual void Update()
	{

	}

	protected virtual void LateUpdate()
	{

	}

#if !DEBUG
	protected virtual void OnValidate()
	{

	}
#endif

	public virtual void SetActive(bool active)
	{
		if(rTransform == null)
		rTransform = GetComponent<RectTransform>();
		gameObject.SetActive(active);
		if (!active)
			isHovered = false;
	}
}