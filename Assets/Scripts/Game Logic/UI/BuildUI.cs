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

	//UI
	public RectTransform buildWindow;
	public RectTransform toolTip;
	public TMP_Text toolTipTitle;
	public TMP_Text toolTipBody;
	public TMP_Text toolTipCost;
	public TMP_Text toolTipProd;
	public Vector2 tooltipOffset;

	public RectTransform unitUIPrefab;

	private RectTransform[] activeUnits;
	private UnitInfo selectedUnit;
	private bool placeMode;
	private Camera cam;
	private bool toolTipVisible;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		activeUnits = new RectTransform[6];
		ShowBuildWindow(Tech);
		ShowToolTip(Tech[0].name, Tech[0].description, Tech[0].GetCostString(), Tech[0].GetProductionString());
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

		if(toolTipVisible)
		{
			var pos = Input.mousePosition + (Vector3)tooltipOffset;
			pos.x = Mathf.Clamp(pos.x, 0, Screen.width - toolTip.rect.width);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height - toolTip.rect.height);
			toolTip.anchoredPosition = pos;
		}
	}

	public void ShowToolTip(string title, string body, string costInfo, string productionInfo)
	{
		toolTip.gameObject.SetActive(toolTipVisible = true);
		toolTipTitle.SetText(title);
		toolTipBody.SetText(body);
		toolTipCost.SetText(costInfo);
		toolTipProd.SetText(productionInfo);
	}

	public void HideToolTip()
	{
		toolTip.gameObject.SetActive(toolTipVisible = false);
	}

	public void ShowBuildWindow(UnitInfo[] units)
	{
		buildWindow.gameObject.SetActive(true);
		for (int i = 0; i < units.Length; i++)
		{
			var unit = units[i];
			if(activeUnits[i] == null)
			{
				activeUnits[i] = Instantiate(unitUIPrefab, buildWindow);
				activeUnits[i].anchoredPosition = new Vector2(5 + (i * 170), 5);
			}
			activeUnits[i].GetComponentInChildren<TMP_Text>().SetText(unit.name);
			var btn = activeUnits[i].GetComponent<Button>();
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() =>
			{
				ShowToolTip(unit.name, unit.description, unit.GetCostString(), unit.GetProductionString());
				selectedUnit = unit;
				placeMode = true;
			});
		}
	}

	public void HideBuildWindow()
	{
		buildWindow.gameObject.SetActive(false);
		for (int i = 0; i < activeUnits.Length; i++)
		{
			activeUnits[i]?.gameObject.SetActive(false);
		}
	}
}
