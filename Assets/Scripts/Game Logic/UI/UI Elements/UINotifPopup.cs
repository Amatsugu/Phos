using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINotifPopup : MonoBehaviour
{
	public TMP_Text title;
	public TMP_Text message;
	public Image icon;

	public void Init(Sprite icon, string title)
	{
		this.title.SetText(title);
		this.icon.sprite = icon;
	}

	public void Init(Sprite icon, string title, string message)
	{
		Init(icon, title);
		this.message.SetText(message);
	}
}
