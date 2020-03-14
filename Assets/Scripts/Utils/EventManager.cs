using System.Collections.Generic;

using UnityEngine;

public enum GameEvent
{
	OnMapLoaded = 1805809122,
	OnGameReady = 1859452432,
	OnMapRegen = -1290680596,
	OnWeatherInit = -1795000521,
	OnHQPlaced = 347355883,
	OnMapDestroyed = -904577914,
	OnGameSaving = -43491477,
	OnGameStart = -89447245,
	OnGameTick = -998220040,
	OnBuildingUnlocked = 2117215534,
	OnBuildWindowOpen = 1968727591,
	OnBuildWindowClose = -1653997983,
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

	public static void AddEventListener(GameEvent gameEvent, System.Action callback) => AddEventListener(gameEvent, callback);

	public static void AddEventListener(int eventID, System.Action callback)
	{
		if (!INST._events.ContainsKey(eventID))
			INST._events.Add(eventID, new List<System.Action>());
		INST._events[eventID].Add(callback);
		Debug.Log($"Register [{eventID}]: {callback.Method.Name}");
	}

	public static void InvokeEvent(string name) => InvokeEvent(name.GetHashCode());

	public static void InvokeEvent(GameEvent gameEvent) => InvokeEvent(Animator.StringToHash(gameEvent.ToString()));

	public static void InvokeEvent(int eventID)
	{
		if (INST == null)
			return;
		if (INST._events.ContainsKey(eventID))
			for (int i = 0; i < INST._events[eventID].Count; i++)
			{
				Debug.Log($"Invoke [{eventID}]: {INST._events[eventID][i]?.Method.Name}");
				INST._events[eventID][i]?.Invoke();
			}
	}

	public static void RemoveAllEventListeners(string name) => RemoveAllEventListeners(name.GetHashCode());

	public static void RemoveAllEventListeners(GameEvent gameEvent) => RemoveAllEventListeners((int)gameEvent);

	public static void RemoveAllEventListeners(int eventID)
	{
		if (INST._events.ContainsKey(eventID))
			INST._events[eventID].Clear();
	}

	public static void RemoveEventListener(string name, System.Action callback) => RemoveEventListener(name.GetHashCode(), callback);

	public static void RemoveEventListener(GameEvent gameEvent, System.Action callback) => RemoveEventListener((int)gameEvent, callback);

	public static void RemoveEventListener(int eventID, System.Action callback)
	{
		if (INST._events.ContainsKey(eventID))
			INST._events[eventID].Remove(callback);
		//Debug.Log($"UnRegister [{eventID}]: {callback.Method.ToString()}");
	}
}