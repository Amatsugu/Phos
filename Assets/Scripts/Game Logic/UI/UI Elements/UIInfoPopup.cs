using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoPopup : UIExpandable
{
	[Header("Popup Settings")]
	public TMP_Text title;
	public TMP_Text desc;
	public Image image;


	public void Init(Sprite icon, string title, string message)
	{
		this.title.SetText(title);
		desc.SetText(message);
		image.sprite = icon;
		SetActive(true);
	}
}
