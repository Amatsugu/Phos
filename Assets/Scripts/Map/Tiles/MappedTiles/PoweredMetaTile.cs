using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	public class PoweredMetaTile : PoweredBuildingTile
	{
		private PoweredBuildingTile _parent;

		public PoweredMetaTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, PoweredBuildingTile parentTile) : base(coords, height, map, tInfo)
		{
			originalTile = tInfo;
			_parent = parentTile;
		}

		protected override void OnBuilt()
		{
			
		}

		public override void RenderBuilding()
		{
			
		}

		protected override void ApplyBonuses()
		{
			_parent.AddBuff(buffs);
		}

		public override void AddConsumptionMulti(float ammount)
		{
			_parent.AddConsumptionMulti(ammount);
		}

		public override void AddProductionMulti(float ammount)
		{
			_parent.AddProductionMulti(ammount);
		}

		public override string GetDescription()
		{
			return _parent.GetDescription();
		}

		protected override void ApplyTileProperites()
		{
		}

		public override string GetName()
		{
			return _parent.GetName();
		}

		public override void OnDisconnected()
		{
			_parent.HQDisconnected();
		}

		public override void OnConnected()
		{
			_parent.HQConnected();
		}

		public override bool CanDeconstruct(Faction faction)
		{
			return _parent.CanDeconstruct(faction);
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			_parent.TileUpdated(src, updateType);
		}
	}
}
