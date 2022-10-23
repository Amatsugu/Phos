using Amatsugu.Phos.TileEntities;

namespace Amatsugu.Phos.Tiles
{
	public class SubHQTile : PoweredBuildingTile
	{
		public SubHQTile(HexCoords coords, float height, Map map, SubHQTileEntity tInfo) : base(coords, height, map, tInfo, 0)
		{
			HasHQConnection = true;
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
		}

		protected override void OnBuilt()
		{
		}

		protected override void SendBuildNotification()
		{
		}

		public override bool CanDeconstruct(Faction faction) => false;

		public override void OnConnected()
		{
			
		}

		public override void OnDisconnected()
		{
			
		}

	}
}