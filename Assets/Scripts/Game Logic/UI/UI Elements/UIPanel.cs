using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UIPanel : UIHover
{

	public RectTransform PanelBase { get; private set; }
	public Button closeButton;
	public TMP_Text titleText;
	public bool hideOnStart = true;
	public bool hideOnBlur = true;
	public event System.Action OnShow;
	public event System.Action OnHide;

	public bool IsOpen
	{
		get
		{
			return gameObject.activeInHierarchy;
		}
	}

	private void Start()
	{
		PanelBase = GetComponent<RectTransform>();
		if(hideOnStart)
			Hide();
		if(closeButton != null)
		{
			var click = new Button.ButtonClickedEvent();
			click.AddListener(Hide);
			closeButton.onClick = click;
		}
	}

	private void Update()
	{
		if (!hideOnBlur)
			return;
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			var rect = PanelBase.rect;
			rect.x = PanelBase.position.x;
			rect.y = PanelBase.position.y - rect.height;
			if (!rect.Contains(Input.mousePosition))
				Hide();

		}
	}

	public virtual void Show()
	{
		gameObject.SetActive(true);
		OnShow?.Invoke();
	}

	public virtual void Hide()
	{
		gameObject.SetActive(false);
		OnHide?.Invoke();
	}

	public static void DestroyChildren(Transform transform)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}

	}

	public static Vector3 ContrainToScreen(Vector3 pos, Rect rect)
	{
		if (pos.x < 0)
			pos.x = 0;
		if (pos.x + rect.width > Screen.width)
			pos.x = Screen.width - rect.width;
		if (pos.y - rect.height < 0)
			pos.y = rect.height;
		if (pos.y > Screen.height)
			pos.y = Screen.height;
		return pos;
	}
}
