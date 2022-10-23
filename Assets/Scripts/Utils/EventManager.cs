using System.Collections.Generic;

using UnityEngine;

public enum GameEvent
{
	OnResearchComplete = -1721594843,
}

public class EventManager : MonoBehaviour
{
	public static EventManager INST
	{
		get
		{
			if (_inst == null)
			{
				_inst = FindObjectOfType<EventManager>();
				if (_inst != null)
					_inst._events = new Dictionary<int, List<System.Action>>();
			}
			return _inst;
		}
	}

	private static EventManager _inst;

	private Dictionary<int, List<System.Action>> _events;

	public static void AddEventListener(string name, System.Action callback) => AddEventListener(Animator.StringToHash(name), callback);

	public static void AddEventListener(int eventID, System.Action callback)
	{
		if (!INST._events.ContainsKey(eventID))
			INST._events.Add(eventID, new List<System.Action>());
		INST._events[eventID].Add(callback);
	}

	public static void InvokeEvent(string name) => InvokeEvent(name.GetHashCode());

	public static void InvokeEvent(int eventID)
	{
		if (INST == null)
			return;
		if (INST._events.ContainsKey(eventID))
			for (int i = 0; i < INST._events[eventID].Count; i++)
			{
				INST._events[eventID][i]?.Invoke();
			}
	}

	public static void RemoveAllEventListeners(string name) => RemoveAllEventListeners(name.GetHashCode());

	public static void RemoveAllEventListeners(int eventID)
	{
		if (INST._events.ContainsKey(eventID))
			INST._events[eventID].Clear();
	}

	public static void RemoveEventListener(string name, System.Action callback) => RemoveEventListener(name.GetHashCode(), callback);

	public static void RemoveEventListener(int eventID, System.Action callback)
	{
		if (INST._events.ContainsKey(eventID))
			INST._events[eventID].Remove(callback);
	}
}
