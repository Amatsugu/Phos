using Amatsugu.Phos.TileEntities;

using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class FactoryBuildingTile : PoweredBuildingTile
	{
		public FactoryTileEntity factoryInfo;
		private MobileUnitEntity _curUnit;
		public FactoryBuildingTile(HexCoords coords, float height, Map map, FactoryTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			factoryInfo = tInfo;
		}

		public virtual void StartConstruction(MobileUnitEntity unitEntity)
		{
			Debug.Log($"Unit starting construction: {unitEntity.GetNameString()}");
			unitEntity.constructionMesh.Instantiate(SurfacePoint + new float3(0, .3f, 0), Quaternion.identity, unitEntity, 0.8f, unitEntity.buildTime);
			_curUnit = unitEntity;
		}

		public virtual void FinishConstruction()
		{

			var unit = GameRegistry.GameMap.AddUnit(_curUnit, this, factoryInfo.faction);
			var posX = UnityEngine.Random.Range(1, 10);
			var posZ = UnityEngine.Random.Range(1, 10);
			unit.MoveTo(Coords.TranslateOffset(posX, posZ).WorldPos);
			_curUnit = null;
			GameEvents.InvokeOnUnitBuilt(Coords);
		}

	}
}