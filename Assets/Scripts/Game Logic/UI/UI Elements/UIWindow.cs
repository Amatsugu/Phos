using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIWindow : UIHover
{

	private RectTransform _rectTransform;

	void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
	}

	public void Close()
	{
		if(OnClosing())
		{
			_rectTransform.gameObject.SetActive(false);
		}
	}

	public virtual bool OnClosing()
	{
		return true;
	}

	public void Open()
	{
		_rectTransform.gameObject.SetActive(true);
		OnOpened();
	}

	public abstract void OnOpened();
}
