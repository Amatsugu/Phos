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
	None,
	UI,
	Tile,
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
		public HexCoords tile;
		public float duration;

		public PendingNotification(Sprite sprite, string title, string message, float duration)
		{
			this.sprite = sprite;
			this.title = title;
			this.message = message;
			this.duration = duration;
		}

		public PendingNotification(Sprite sprite, string title, string message, HexCoords tile, float duration = -1) : this(sprite, title, message, duration)
		{
			type = NotifTargetType.Tile;
			this.tile = tile;
		}

		public PendingNotification(Sprite sprite, string title, string message, UIPanel panel, float duration = -1) : this(sprite, title, message, duration)
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
#if DEBUG
	protected override void OnValidate()
	{
		base.OnValidate();
		if(Application.isPlaying)
			_notificationQueue = new Queue<PendingNotification>();
	}
#endif

	protected override void Update()
	{
		base.Update();
		//Create notif popups
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
				popup.Init(notif);
				popup.rectTransform.anchoredPosition = basePos;
				_animTime = 0;
			}
		}


		//Animate popups
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

	private void CreateNotification(Sprite icon, string title, string message, float duration = -1) => 
		_notificationQueue.Enqueue(new PendingNotification(icon, title, message, duration == -1 ? notifTime : duration));

	private void CreateNotification(Sprite icon, string title, string message, HexCoords tile, float duration = -1) => 
		_notificationQueue.Enqueue(new PendingNotification(icon, title, message, tile, duration == -1 ? notifTime : duration));

	private void CreateNotification(Sprite icon, string title, string message, UIPanel panel, float duration = -1) => 
		_notificationQueue.Enqueue(new PendingNotification(icon, title, message, panel, duration == -1 ? notifTime : duration));

	private void CreateNotification(NotifType type, string title, string message, float duration = -1)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message, duration);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message, duration);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message, duration);
				break;
		}
	}

	private void CreateNotification(NotifType type, string title, string message, HexCoords tile, float duration = -1)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message, tile, duration);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message, tile, duration);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message, tile, duration);
				break;
		}
	}

	private void CreateNotification(NotifType type, string title, string message, UIPanel panel, float duration = -1)
	{
		switch (type)
		{
			case NotifType.Info:
				CreateNotification(infoIcon, title, message, panel, duration);
				break;

			case NotifType.Warning:
				CreateNotification(warningIcon, title, message, panel, duration);
				break;

			case NotifType.Error:
				CreateNotification(errorIcon, title, message, panel, duration);
				break;
		}
	}

	public static void Notify(Sprite icon, string title, string message = null, float duration = -1) => INST.CreateNotification(icon, title, message, duration);

	public static void Notify(NotifType type, string title, string message = null, float duration = -1) => INST.CreateNotification(type, title, message, duration);

	public static void NotifyWithTarget(NotifType type, string title, HexCoords tile, string message = null, float duration = -1) => INST.CreateNotification(type, title, message, tile, duration);

	public static void NotifyWithTarget(NotifType type, string title, UIPanel panel, string message = null, float duration = -1) => INST.CreateNotification(type, title, message, panel, duration);

	public static void NotifyWithTarget(Sprite icon, string title, HexCoords tile, string message = null, float duration = -1) => INST.CreateNotification(icon, title, message, tile, duration);

	public static void NotifyWithTarget(Sprite icon, string title, UIPanel panel, string message = null, float duration = -1) => INST.CreateNotification(icon, title, message, panel, duration);
}