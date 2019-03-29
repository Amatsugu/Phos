using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildUI : MonoBehaviour
{
	public UnitInfo[] Tech;
	public UnitInfo[] Resource;
	public UnitInfo[] Economy;
	public UnitInfo[] Structure;
	public UnitInfo[] Millitary;
	public UnitInfo[] Defense;

	public RectTransform buildWindow;
	public RectTransform unitUIPrefab;

	private RectTransform[] activeUnits;
	private UnitInfo selectedUnit;
	private bool placeMode;
	private Camera cam;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		activeUnits = new RectTransform[6];
		ShowBuildWindow(Tech);
		cam = Camera.main;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			placeMode = false;
		if (Input.GetKeyUp(KeyCode.Mouse0) && placeMode)
		{
			var mPos = Input.mousePosition;
			var t = Map.ActiveMap.GetTileFromRay(cam.ScreenPointToRay(mPos), cam.transform.position.y * 2);
			if (t != null && t.Height > Map.ActiveMap.SeaLevel)
			{
				Map.ActiveMap.HexFlatten(t.Coords, 1, 6, Map.FlattenMode.Average);
				//selector.transform.position = t.SurfacePoint;
				Map.ActiveMap.ReplaceTile(t, selectedUnit.tile);
			}

		}
	}

	public void ShowBuildWindow(UnitInfo[] units)
	{
		buildWindow.gameObject.SetActive(true);
		for (int i = 0; i < Tech.Length; i++)
		{
			if(activeUnits[i] == null)
			{
				activeUnits[i] = Instantiate(unitUIPrefab, buildWindow);
				activeUnits[i].anchoredPosition = new Vector2(5 + (i * 170), 5);
			}
			activeUnits[i].GetComponentInChildren<TMP_Text>().SetText(Tech[i].name);
			activeUnits[i].GetComponent<Button>().onClick.AddListener(() =>
			{
				selectedUnit = Tech[i];
				placeMode = true;
			});
		}
	}

	public void HideBuildWindow()
	{
		for (int i = 0; i < activeUnits.Length; i++)
		{
			activeUnits[i].gameObject.SetActive(false);
		}
	}
}
