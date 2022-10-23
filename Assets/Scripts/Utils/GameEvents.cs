using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.Units;

using System;

public class GameEvents
{
	private static GameEvents INST => _inst ??= new GameEvents();

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

	public static void InvokeOnUnitQueued(QueuedUnit order) => INST._onUnitQueued?.Invoke(order);

	public static event Action<QueuedUnit> OnUnitQueued
	{
		add => INST._onUnitQueued += value;
		remove => INST._onUnitQueued -= value;
	}

	public static void InvokeOnUnitConstructionStart(PendingUnitBuildOrder order) => INST._onUnitConstructionStart?.Invoke(order);

	public static event Action<PendingUnitBuildOrder> OnUnitConstructionStart
	{
		add => INST._onUnitConstructionStart += value;
		remove => INST._onUnitConstructionStart -= value;
	}

	public static void InvokeOnUnitConstructionEnd(int orderId) => INST._onUnitConstructionEnd?.Invoke(orderId);

	public static event Action<int> OnUnitConstructionEnd
	{
		add => INST._onUnitConstructionEnd += value;
		remove => INST._onUnitConstructionEnd -= value;
	}

	public static void InvokeOnUnitDequeued(int orderId) => INST._onUnitDequeued?.Invoke(orderId);

	public static event Action<int> OnUnitDequeued
	{
		add => INST._onUnitDequeued += value;
		remove => INST._onUnitDequeued -= value;
	}
	
	public static void InvokeOnEnterDeconstructionMode() => INST._onEnterDeconstructionMode?.Invoke();

	public static event Action OnEnterDeconstructionMode
	{
		add => INST._onEnterDeconstructionMode += value;
		remove => INST._onEnterDeconstructionMode -= value;
	}

	public static void InvokeOnExitDeconstructionMode() => INST._onExitDeconstructionMode?.Invoke();

	public static event Action OnExitDeconstructionMode
	{
		add => INST._onExitDeconstructionMode += value;
		remove => INST._onExitDeconstructionMode -= value;
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
	private event Action _onEnterDeconstructionMode;
	private event Action _onExitDeconstructionMode;

	private event Action<BuildingTileEntity> _onBuildingUnlocked;

	private event Action<int> _onAnimationEvent;

	private event Action<MobileUnit> _onUnitDied;

	private event Action<BuildingTile> _onBuildingDied;

	private event Action<HexCoords> _onUnitBuilt;

	private event Action<QueuedUnit> _onUnitQueued;

	private event Action<PendingUnitBuildOrder> _onUnitConstructionStart;
	private event Action<int> _onUnitConstructionEnd;
	private event Action<int> _onUnitDequeued;
}