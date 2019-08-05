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

	private Vector3 _notifPos;

	public void Init(HexCoords coords, Sprite icon, string title, string message)
	{
		this.title.SetText(title);
		_notifPos = Map.ActiveMap[coords].SurfacePoint;
		desc.SetText(message);
		if(icon != null)
			image.sprite = icon;
		SetActive(true);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		var pos = GameRegistry.Camera.WorldToScreenPoint(_notifPos);
		rTransform.anchoredPosition = new Vector2(pos.x, pos.y);
	}
}
