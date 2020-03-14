using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class GameRegistry : MonoBehaviour
{
	public static GameRegistry INST
	{
		get
		{
			if (_inst == null)
				_inst = FindObjectOfType<GameRegistry>();
			return _inst;
		}
	}

	private static GameRegistry _inst;

	public static BuildUI BuildUI => INST.buildUI;
	public static InteractionUI InteractionUI => INST.interactionUI;
	public static StatusUI StatusUI => INST.statusUI;
	public static BaseNameWindowUI BaseNameUI => INST.baseNameUI;
	public static ResearchTreeUI ResearchTreeUI => INST.researchTreeUI;
	public static Camera Camera => INST.mainCamera;
	public static CameraController CameraController => INST.cameraController;
	public static BuildingDatabase BuildingDatabase => INST._buildingDatabase;
	public static ResearchDatabase ResearchDatabase => INST.researchDatabase;
	public static ResourceSystem ResourceSystem => INST.resourceSystem;

	public BuildUI buildUI;
	public InteractionUI interactionUI;
	public StatusUI statusUI;
	public BaseNameWindowUI baseNameUI;
	public ResearchTreeUI researchTreeUI;
	public Camera mainCamera;
	public CameraController cameraController;
	public ResearchDatabase researchDatabase;
	public ResourceSystem resourceSystem;

	private BuildingDatabase _buildingDatabase;

	private HashSet<int> _unlockedBuildings;

	public static void SetBuildingDatabase(BuildingDatabase database)
	{
		INST._buildingDatabase = database;
		var buildings = database.buildings.Values.ToArray();
		INST._unlockedBuildings = new HashSet<int>();
		for (int i = 0; i < database.buildings.Count; i++)
		{
			if (buildings[i].info.tier == 1)
			{
				INST._unlockedBuildings.Add(buildings[i].id);
			}
		}
	}

	public static void UnlockBuilding(BuildingIdentifier building)
	{
		EventManager.InvokeEvent(GameEvent.OnBuildingUnlocked);
		if (!_inst._unlockedBuildings.Contains(building.id))
			_inst._unlockedBuildings.Add(building.id);
	}

	public static bool IsBuildingUnlocked(int id)
	{
		return _inst._unlockedBuildings.Contains(id);
	}

	public static class Cheats
	{
		public static bool INSTANT_RESEARCH;
		public static bool INSTANT_BUILD;
		public static bool NO_RESOURCE_COST;
	}
}