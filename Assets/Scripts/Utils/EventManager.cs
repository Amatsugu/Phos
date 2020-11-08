using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.Units;

using System;
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

public class GameEvents
{
	private static GameEvents INST => _inst ?? (_inst = new GameEvents());

	private static GameEvents _inst;

	public static event Action OnMapLoaded
	{
		add => INST._onMapLoaded += value;
		remove => INST._onMapLoaded -= value;
	}

	public static void InvokeOnMapLoaded() => INST._onMapLoaded?.Invoke();

	public static event Action OnMapChanged
	{
		add => INST._onMapChanged += value;
		remove => INST._onMapChanged -= value;
	}

	public static void InvokeOnMapChanged() => INST._onMapChanged?.Invoke();

	public static event Action OnGameReady
	{
		add => INST._onGameReady += value;
		remove => INST._onGameReady -= value;
	}

	public static void InvokeOnGameReady() => INST._onGameReady?.Invoke();

	public static event Action OnMapRegen
	{
		add => INST._onMapRegen += value;
		remove => INST._onMapRegen -= value;
	}

	public static void InvokeOnMapRegen() => INST._onMapRegen?.Invoke();

	public static event Action OnMapDestroyed
	{
		add => INST._onMapDestroyed += value;
		remove => INST._onMapDestroyed -= value;
	}

	public static void InvokeOnMapDestroyed() => INST._onMapDestroyed?.Invoke();

	public static event Action OnWeatherInit
	{
		add => INST._onWeatherInit += value;
		remove => INST._onWeatherInit -= value;
	}

	public static void InvokeOnWeatherInit() => INST._onWeatherInit?.Invoke();

	public static event Action OnHQPlaced
	{
		add => INST._onHQPlaced += value;
		remove => INST._onHQPlaced -= value;
	}

	public static void InvokeOnHQPlaced() => INST._onHQPlaced?.Invoke();

	public static event Action OnGameTick
	{
		add => INST._onGameTick += value;
		remove => INST._onGameTick -= value;
	}

	public static void InvokeOnGameTick() => INST._onGameTick?.Invoke();

	public static event Action OnGameSaving
	{
		add => INST._onGameSaving += value;
		remove => INST._onGameSaving -= value;
	}

	public static void InvokeOnGameSaving() => INST._onGameSaving?.Invoke();

	public static event Action OnGameLoaded
	{
		add => INST._onGameGameLoaded += value;
		remove => INST._onGameGameLoaded -= value;
	}

	public static void InvokeOnGameLoaded() => INST._onGameGameLoaded?.Invoke();

	public static event Action<BuildingTileEntity> OnBuildingUnlocked
	{
		add => INST._onBuildingUnlocked += value;
		remove => INST._onBuildingUnlocked -= value;
	}

	public static void InvokeOnBuildingUnlocked(BuildingTileEntity unlockedBuilding) => INST._onBuildingUnlocked?.Invoke(unlockedBuilding);

	public static event Action OnDevConsoleOpen
	{
		add => INST._onDevConsoleOpen += value;
		remove => INST._onDevConsoleOpen -= value;
	}

	public static void InvokeOnDevConsoleOpen() => INST._onDevConsoleOpen?.Invoke();

	public static event Action OnDevConsoleClose
	{
		add => INST._onDevConsoleClose += value;
		remove => INST._onDevConsoleClose -= value;
	}

	public static void InvokeOnDevConsoleClose() => INST._onDevConsoleClose?.Invoke();

	public static event Action OnCameraFreeze
	{
		add => INST._onCameraFreeze += value;
		remove => INST._onCameraFreeze -= value;
	}

	public static void InvokeOnCameraFreeze() => INST._onCameraFreeze?.Invoke();

	public static event Action OnCameraUnFreeze
	{
		add => INST._onCameraUnFreeze += value;
		remove => INST._onCameraUnFreeze -= value;
	}

	public static void InvokeOnCameraUnFreeze() => INST._onCameraUnFreeze?.Invoke();

	public static event Action<int> OnAnimationEvent
	{
		add => INST._onAnimationEvent += value;
		remove => INST._onAnimationEvent -= value;
	}

	public static void InvokeOnAnimationEvent(int eventID) => INST._onAnimationEvent?.Invoke(eventID);

	public static event Action<MobileUnit> OnUnitDied
	{
		add => INST._onUnitDied += value;
		remove => INST._onUnitDied -= value;
	}

	public static void InvokeOnUnitDied(MobileUnit unit) => INST._onUnitDied?.Invoke(unit);

	public static event Action<BuildingTile> OnBuildingDied
	{
		add => INST._onBuildingDied += value;
		remove => INST._onBuildingDied -= value;
	}

	public static void InvokeOnUnitBuilt(HexCoords factoryCoords) => INST._onUnitBuilt?.Invoke(factoryCoords);

	public static event Action<HexCoords> OnUnitBuilt
	{
		add => INST._onUnitBuilt += value;
		remove => INST._onUnitBuilt -= value;
	}

	public static void InvokeOnUnitQueued(BuildOrder order) => INST._onUnitQueued?.Invoke(order);

	public static event Action<BuildOrder> OnUnitQueued
	{
		add => INST._onUnitQueued += value;
		remove => INST._onUnitQueued -= value;
	}

	public static void InvokeOnUnitConstructionStart(ConstructionOrder order) => INST._onUnitConstructionStart?.Invoke(order);

	public static event Action<ConstructionOrder> OnUnitConstructionStart
	{
		add => INST._onUnitConstructionStart += value;
		remove => INST._onUnitConstructionStart -= value;
	}

	public static void InvokeOnBuildingDied(BuildingTile building) => INST._onBuildingDied?.Invoke(building);

	private event Action _onMapLoaded;

	private event Action _onGameReady;

	private event Action _onMapRegen;

	private event Action _onMapDestroyed;

	private event Action _onMapChanged;

	private event Action _onWeatherInit;

	private event Action _onHQPlaced;

	private event Action _onGameTick;

	private event Action _onGameSaving;

	private event Action _onGameGameLoaded;

	private event Action _onDevConsoleOpen;

	private event Action _onDevConsoleClose;

	private event Action _onCameraFreeze;

	private event Action _onCameraUnFreeze;

	private event Action<BuildingTileEntity> _onBuildingUnlocked;

	private event Action<int> _onAnimationEvent;

	private event Action<MobileUnit> _onUnitDied;

	private event Action<BuildingTile> _onBuildingDied;

	private event Action<HexCoords> _onUnitBuilt;

	private event Action<BuildOrder> _onUnitQueued;

	private event Action<ConstructionOrder> _onUnitConstructionStart;
}