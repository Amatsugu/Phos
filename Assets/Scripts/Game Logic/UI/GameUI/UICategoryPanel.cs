using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICategoryPanel : UIPanel
{
	public Button tech;
	public Button gathering;
	public Button production;
	public Button structure;
	public Button military;
	public Button defense;

	public event Action<BuildingCategory> OnButtonClicked;
}
