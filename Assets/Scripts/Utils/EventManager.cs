using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	public static EventManager INST
	{
		get
		{
			if (_inst == null)
			{
				_inst = FindObjectOfType<EventManager>();
				if(_inst != null)
					_inst._events = new Dictionary<string, List<System.Action>>();
			}
			return _inst;
		}
	}

	private static EventManager _inst;

	private Dictionary<string, List<System.Action>> _events;

	public static void AddEventListener(string name, System.Action callback)
	{
		if (INST == null)
			return;
		if(!INST._events.ContainsKey(name))
			INST._events.Add(name, new List<System.Action>());
		INST._events[name].Add(callback);
	}

	public static void InvokeEvent(string name)
	{
		if (INST == null)
			return;
		if(INST._events.ContainsKey(name))
			for (int i = 0; i < INST._events[name].Count; i++)
				INST._events[name][i]?.Invoke();
	}

	public static void RemoveAllEventListeners(string name)
	{
		if(INST._events.ContainsKey(name))
			INST._events[name].Clear();
	}

	public static void RemoveEventListener(string name, System.Action callback)
	{
		if (INST._events.ContainsKey(name))
			INST._events[name].Remove(callback);
	}
}
