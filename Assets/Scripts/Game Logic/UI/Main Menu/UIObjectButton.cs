using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class UIObjectButton : MonoBehaviour
{
	public event Action OnClick;

	public MeshRenderer buttonMesh;
	public TMP_Text text;
	public Color normalColor;
	public Color hoverColor;

#if UNITY_EDITOR
	private void OnValidate()
	{
		if(text != null)
			text.color = normalColor;
	}
#endif

	private void OnMouseUp()
	{
		OnClick?.Invoke();
	}

	private void OnMouseEnter()
	{
		text.color = hoverColor;
	}

	private void OnMouseExit()
	{
		text.color = normalColor;
	}
}
