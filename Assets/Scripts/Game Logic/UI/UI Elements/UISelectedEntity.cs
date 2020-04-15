using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISelectedEntity : UIButtonHover
{
	public Image icon;
	public TMP_Text nameText;
	public TMP_Text countText;

	public void Show(string name, Sprite icon, int count)
	{
		nameText.SetText(name);
		this.icon.sprite = icon;
		countText.SetText(count.ToString());
	}
}
