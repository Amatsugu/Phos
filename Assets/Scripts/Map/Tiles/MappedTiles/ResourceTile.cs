using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amatsugu.Phos.Tiles
{
	public class ResourceTile : Tile
	{
		public readonly ResourceTileInfo resInfo;
		public HexCoords gatherer;

		public ResourceTile(HexCoords coords, float height, Map map, ResourceTileInfo tInfo) : base(coords, height, map, tInfo)
		{
			resInfo = tInfo;
		}

		public override TileEntity GetMeshEntity()
		{
			return originalTile != null ? originalTile : info;
		}

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			base.OnSerialize(tileData);
			if (gatherer.isCreated)
			{
				tileData.Add($"{nameof(ResourceTile)}.gatherer.X", gatherer.X.ToString());
				tileData.Add($"{nameof(ResourceTile)}.gatherer.Y", gatherer.Y.ToString());
			}
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			if (tileData.ContainsKey($"{nameof(ResourceTile)}.gatherer.X"))
			{
				var x = int.Parse(tileData[$"{nameof(ResourceTile)}.gatherer.X"]);
				var y = int.Parse(tileData[$"{nameof(ResourceTile)}.gatherer.Y"]);
				gatherer = new HexCoords(x, y, map.tileEdgeLength);
			}
			base.OnDeSerialized(tileData);
		}
	}
}