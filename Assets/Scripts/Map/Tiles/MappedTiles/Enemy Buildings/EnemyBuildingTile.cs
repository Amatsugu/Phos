using Amatsugu.Phos.TileEntities;

namespace Amatsugu.Phos.Tiles
{
	public class EnemyBuildingTile : BuildingTile
	{
		public EnemyBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			isBuilt = true;
		}

		protected override void OnBuilt()
		{
			//base.OnBuilt();
		}

	}
}