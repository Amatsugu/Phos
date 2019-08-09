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
	public float endTime;

	[HideInInspector]
	public float opacity = 0f;

	public bool isShown = true;

	public GameObject GameObject { get; private set; }
	public RectTransform rectTransform;

	private Image _bg;
	private Button _button;

	private Color _iC, _bC, _tC, _mC;


	void Start()
	{
		GameObject = gameObject;
		Show(false);
		rectTransform = GetComponent<RectTransform>();
		_button = GetComponent<Button>();
		_bg = GetComponent<Image>();
		_iC = icon.color;
		_bC = _bg.color;
		_tC = title.color;
		if(message != null)
			_mC = message.color;
	}

	public void Init(NotificationsUI.PendingNotification notification, float notifTime)
	{
		Show(true);
		SetOpacity(0);
		icon.sprite = notification.sprite;
		title.SetText(notification.title);
		endTime = Time.time + notifTime;
		_button.onClick.RemoveAllListeners();
		if(notification.type == NotifTargetType.Tile)
		{
			var tile = notification.tile;
			if(tile != null)
				_button.onClick.AddListener(() => CameraController.FocusOnTile(tile));
		}else
		{
			var panel = notification.panel;
			_button.onClick.AddListener(() => panel.Show());
		}
		message?.SetText(notification.message);
	}

	public void Show(bool isShown)
	{
		if (this.isShown != isShown)
			GameObject.SetActive(this.isShown = isShown);
	}

	public void SetOpacity(float opacity)
	{
		//Icon
		var col = _iC;
		col.a *= this.opacity = opacity;
		icon.color = col;
		//BG
		col = _bC;
		col.a *= opacity;
		_bg.color = col;
		//Title
		col = _tC;
		col.a *= opacity;
		title.color = col;
		//Message
		if (message == null)
			return;
		col = _mC;
		col.a *= opacity;
		message.color = col;

	}
}
