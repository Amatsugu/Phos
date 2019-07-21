using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsUI : MonoBehaviour
{
	[Header("Prefabs")]
	public RectTransform notifBig;
	public RectTransform notifMin;
	[Header("Icons")]
	public Sprite infoIcon;
	public Sprite warningIcon;
	public Sprite errorIcon;
	[Header("Config")]
	public float notifTime = 2;
	public int maxNotifs = 6;

	public static NotificationsUI INST
	{
		get
		{
			if (_inst == null)
				return _inst = FindObjectOfType<NotificationsUI>();
			return _inst;
		}
	}

	public enum NotifType
	{
		Info,
		Warning,
		Error
	}

	private UINotifPopup[] _notifsMin;
	private UINotifPopup[] _notifsBig;
	private static NotificationsUI _inst;

	private void CreateNotification(Sprite icon, string title, string message = null)
	{
		if(message == null) //Min Notif
		{

		}else //Big Notif
		{

		}
	}

	private void CreateNotification(NotifType type, string title, string message = null)
	{
		switch(type)
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

	public static void Notify(Sprite icon, string title, string message = null) => INST.CreateNotification(icon, title, message);

	public static void Notify(NotifType type, string title, string message = null) => INST.CreateNotification(type, title, message);
}
