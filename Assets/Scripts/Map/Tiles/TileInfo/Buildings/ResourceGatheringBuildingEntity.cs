using Amatsugu.Phos.Tiles;

using System.Text;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Resoruce Gathering")]
	public class ResourceGatheringBuildingEntity : BuildingTileEntity
	{
		[Header("Gathering")]
		public ResourceIndentifier[] resourcesToGather;
		public int gatherRange = 3;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public override BuildingTile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return new GatheringBuildingTile(pos, height, map, this, rotation);
		}

		public override StringBuilder GetProductionString()
		{
			var gatherString = new StringBuilder();
			for (int i = 0; i < resourcesToGather.Length; i++)
			{
				var curRes = resourcesToGather[i];
				gatherString.Append($"+{curRes.ammount}{ResourceDatabase.GetResourceString(curRes.id)}/tile");
				if (i != resourcesToGather.Length - 1)
					gatherString.Append("\n");
			}
			var b = base.GetProductionString();
			if (b.Length > 0)
			{
				b.AppendLine();
				b.Append(gatherString);
				return b;
			}
			else
				return gatherString;
		}

	}
}