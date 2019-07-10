using System.Collections;
using System.Collections.Generic;
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
	public static BuildingDatabase BuildingDatabase => INST.buildingDatabase;
	public static ResearchDatabase ResearchDatabase => INST.researchDatabase;


	public BuildUI buildUI;
	public InteractionUI interactionUI;
	public StatusUI statusUI;
	public BaseNameWindowUI baseNameUI;
	public ResearchTreeUI researchTreeUI;
	public Camera mainCamera;
	public CameraController cameraController;
	public BuildingDatabase buildingDatabase;
	public ResearchDatabase researchDatabase;

}
