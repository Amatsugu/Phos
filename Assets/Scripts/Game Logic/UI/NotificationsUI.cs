using System.Collections.Generic;

using UnityEngine;

public enum NotifType
{
	Info,
	Warning,
	Error
}

public enum NotifTargetType
{
	Tile,
	UI
}

public class NotificationsUI : UIHover
{
	[Header("Prefabs")]
	public UINotifPopup notifBig;

	public UINotifPopup notifMin;

	[Header("Icons")]
	public Sprite infoIcon;

	public Sprite warningIcon;
	public Sprite errorIcon;

	[Header("Config")]
	public float notifTime = 2;

	public float animSpeed = 2;
	public int maxNotifs = 6;
	public float spacing = 10;
	public Vector2 basePos;

	public static NotificationsUI INST
	{
		get
		{
			if (_inst == null)
				return _inst = FindObjectOfType<NotificationsUI>();
			return _inst;
		}
	}

	private int[] _activeNotifs;
	private int _activeCount;
	private UINotifPopup[] _notifsMin;
	private UINotifPopup[] _notifsBig;
	private static NotificationsUI _inst;
	private Queue<PendingNotification> _notificationQueue;
	private float _animTime = 1;

	public class PendingNotification
	{
		public Sprite sprite;
		public string title;
		public string message;
		public NotifTargetType type;
		public UIPanel panel;
		public Tile tile;

		public PendingNotification(Sprite sprite, string title, string message)
		{
			this.sprite = sprite;
			this.title = title;
			this.message = message;
		}

		public PendingNotification(Sprite sprite, string title, string message, Tile tile) : this(sprite, title, message)
		{
			type = NotifTargetType.Tile;
			this.tile = tile;
		}

		public PendingNotification(Sprite sprite, string title, string message, UIPanel panel) : this(sprite, title, message)
		{
			type = NotifTargetType.UI;
			this.panel = panel;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_notificationQueue = new Queue<PendingNotification>();
		_activeNotifs = new int[2 * maxNotifs];
		for (int i = 0; i < _activeNotifs.Length; i++)
		{
			_activeNotifs[i] = -1;
		}
		_notifsBig = new UINotifPopup[2 * maxNotifs];
		_notifsMin = new UINotifPopup[2 * maxNotifs];
		for (int i = 0; i < _notifsMin.Length; i++)
		{
			_notifsBig[i] = Instantiate(notifBig, rTransform);
			_notifsMin[i] = Instantiate(notifMin, rTransform);
		}
	}

	private void OnValidate()
	{
		_notificationQueue = new Queue<PendingNotification>();
	}

	protected override void Update()
	{
		base.Update();

		for (int i = 0; i < _activeNotifs.Length; i++)
		{
			if (_notificationQueue.Count == 0 || _activeCount == maxNotifs)
				break;
			if (_activeNotifs[i] == -1)
			{
				var notif = _notificationQueue.Dequeue();
				UINotifPopup popup;
				int id;
				if (notif.message == null)
				{
					(id, popup) = GetNotifMin();
					id += _notifsMin.Length;
				}
				else
				{
					(id, popup) = GetNotifBig();
				}
				_activeNotifs[i] = id;
				_activeCount++;
				popup.Init(notif, notifTime);
				popup.rectTransform.anchoredPosition = basePos;
				_animTime = 0;
			}
		}

		var curOffset = 0f;
		if (_animTime < 1)
		{
			_animTime += Time.unscaledDeltaTime * animSpeed;
			if (_animTime > 1)
				_animTime = 1;
		}
		for (int i = 0, j = 0; i < _activeNotifs.Length && j < _activeCount; i++)
		{
			if (_activeNotifs[i] != -1)
			{
				var curNotif = _activeNotifs[i] >= _notifsMin.Length ? _notifsMin[_activeNotifs[i] - _notifsMin.Length] : _notifsBig[_activeNotifs[i]];
				curNotif.rectTransform.anchoredPosition = Vector2.Lerp(curNotif.rectTransform.anchoredPosition, basePos + new Vector2(0, curOffset), _animTime);
				curOffset += spacing + curNotif.rectTransform.rect.height;
				if (Time.unscaledTime > curNotif.endTime)
				{
					if (curNotif.opacity <= 0)
					{
						_activeNotifs[i] = -1;
						_activeCount--;
						curNotif.Show(false);
						_animTime = 0;
					}
					else
					{
						curNotif.SetOpacity(curNotif.opacity - Time.unscaledDeltaTime * animSpeed);
					}
				}
				else if (curNotif.opacity < 1)
				{
					curNotif.SetOpacity(curNotif.opacity + Time.unscaledDeltaTime * animSpeed);
				}
				j++;
			}
		}
	}

	private void AddNotif(int id)
	{
		for (int i = 0; i < _activeNotifs.Length; i++)
		{
			if (_activeNotifs[i] == -1)
			{
				_activeNotifs[i] = id;
				_activeCount++;
				break;
			}
		}
	}

	private (int id, UINotifPopup notif) GetNotifMin() => GetNotif(_notifsMin);

	private (int id, UINotifPopup notif) GetNotifBig() => GetNotif(_notifsBig);

	private (int id, UINotifPopup notif) GetNotif(UINotifPopup[] popups)
	{
		for (int i = 0; i < popups.Length; i++)
		{
			if (!popups[i].isShown)
			{
				return (i, popups[i]);
			}
		}
		throw new System.Exception("Somehow requesting more notifications than possible");
	}

	/*void OnEnable()
	{
		ShowAllActive(true);
	}

	void OnDisable()
	{
		ShowAllActive(false);
	}
	*/

	private void ShowAllActive(bool isShown)
	{
		for (int i = 0, j = 0; i < _activeNotifs.Length && j < _activeCount; i++)
		{
			var nId = _activeNotifs[i];
			if (nId == -1)
				continue;
			j++;
			if (nId >= _notifsMin.Length)
			{
				nId -= _notifsMin.Length;
				_notifsMin[nId].Show(isShown);
			}
			else
				_notifsBig[nId].Show(isShown);
		}
	}

	private void CreateNotification(Sprite icon, string title, string message) => _notificationQueue.Enqueue(new PendingNotification(icon, title, message));

	private void CreateNotification(Sprite icon, string title, string message, Tile tile) => _notificationQueue.Enqueue(new PendingNotification(icon, title, message, tile));

	private void CreateNotification(Sprite icon, string title, string message, UIPanel panel) => _notificationQueue.Enqueue(new PendingNotification(icon, title, message, panel));

	private void CreateNotification(NotifType type, string title, string message)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message);
				break;
		}
	}

	private void CreateNotification(NotifType type, string title, string message, Tile tile)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message, tile);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message, tile);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message, tile);
				break;
		}
	}

	private void CreateNotification(NotifType type, string title, string message, UIPanel panel)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message, panel);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message, panel);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message, panel);
				break;
		}
	}

	public static void Notify(Sprite icon, string title, string message = null) => INST.CreateNotification(icon, title, message);

	public static void Notify(NotifType type, string title, string message = null) => INST.CreateNotification(type, title, message);

	public static void NotifyWithTarget(NotifType type, string title, Tile tile, string message = null) => INST.CreateNotification(type, title, message, tile);

	public static void NotifyWithTarget(NotifType type, string title, UIPanel panel, string message = null) => INST.CreateNotification(type, title, message, panel);

	public static void NotifyWithTarget(Sprite icon, string title, Tile tile, string message = null) => INST.CreateNotification(icon, title, message, tile);

	public static void NotifyWithTarget(Sprite icon, string title, UIPanel panel, string message = null) => INST.CreateNotification(icon, title, message, panel);
}