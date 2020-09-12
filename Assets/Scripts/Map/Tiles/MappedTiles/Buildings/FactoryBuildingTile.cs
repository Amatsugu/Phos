using Amatsugu.Phos.TileEntities;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class FactoryBuildingTile : PoweredBuildingTile
	{
		public FactoryTileEntity factoryInfo;
		private MobileUnitEntity _curUnit;
		public FactoryBuildingTile(HexCoords coords, float height, Map map, FactoryTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			factoryInfo = tInfo;
		}

		public virtual void StartConstruction(MobileUnitEntity unitEntity)
		{
			Debug.Log($"Unit starting construction: {unitEntity.GetNameString()}");
			_curUnit = unitEntity;
		}

		public virtual void FinishConstruction()
		{

			var unit = GameRegistry.GameMap.AddUnit(_curUnit, this, factoryInfo.faction);
			var posX = Random.Range(1, 10);
			var posZ = Random.Range(1, 10);
			unit.MoveTo(Coords.TranslateOffset(posX, posZ).WorldPos);
			_curUnit = null;
			GameEvents.InvokeOnUnitBuilt(Coords);
		}

	}
}