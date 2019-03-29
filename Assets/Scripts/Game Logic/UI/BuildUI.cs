using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		activeUnits = new RectTransform[6];
		ShowBuildWindow(Tech);
	}

	// Update is called once per frame
	void Update()
	{

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
