using Amatsugu.Phos.TileEntities;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class SmartTile : Tile
	{
		public SmartTile(HexCoords coords, float height, Map map, TileEntity tInfo = null) : base(coords, height, map, tInfo)
		{

		}

		public override void Destroy()
		{
			base.Destroy();
		}

		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
		}

		public override void OnHide()
		{
			base.OnHide();
		}

		public override void OnShow()
		{
			base.OnShow();
		}

		public override Entity Render()
		{
			return base.Render();
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
		}
	}
}