using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.Units;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using TMPro;
using UnityEngine;

public class UIInfoPanel : UIPanel
{
	public TMP_Text descriptionText;
	public TMP_Text productionText;

	public void ShowInfo(BuildingTileEntity building)
	{
		titleText.SetText(building.GetNameString());
		var upkeep = building.GetUpkeepString();
		descriptionText.SetText($"{building.description}\n{(upkeep.Length > 0 ? $"Upkeep: {upkeep}" : "")}");
		productionText.SetText(building.GetProductionString());
		SetActive(true);
	}

	//public void ShowInfo(Tile tile)
	//{
	//	titleText.SetText(tile.GetNameString());
	//	//var upkeep = tile..GetUpkeepString();
	//	//descriptionText.SetText($"{building.description}\n{(upkeep.Length > 0 ? $"Upkeep: {upkeep}" : "")}");
	//	descriptionText.SetText(tile.GetDescriptionString());
	//	//productionText.SetText(building.GetProductionString());
	//	SetActive(true);
	//}

	public void ShowInfo(BuildingTile tile)
	{
		titleText.SetText(tile.GetNameString());
		//var upkeep = tile..GetUpkeepString();
		//descriptionText.SetText($"{building.description}\n{(upkeep.Length > 0 ? $"Upkeep: {upkeep}" : "")}");
		descriptionText.SetText(tile.GetDescriptionString());
		productionText.SetText(tile.GetProductionString());
		SetActive(true);
	}

	public void ShowInfo(ResourceTile tile)
	{
		titleText.SetText(tile.GetNameString());
		descriptionText.SetText(tile.GetDescriptionString());
		productionText.SetText(tile.GetProductionString());
		SetActive(true);
	}

	public void ShowInfo(MobileUnitEntity unitEntity)
	{
		titleText.SetText(unitEntity.GetNameString());

		SetActive(true);
	}
}
