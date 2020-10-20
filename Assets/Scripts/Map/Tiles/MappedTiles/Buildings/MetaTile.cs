using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;


namespace Amatsugu.Phos.Tiles
{
	public class MetaTile : PoweredBuildingTile
	{
		public BuildingTile ParentTile { get; private set; }

		private PoweredBuildingTile _poweredParent;
		private bool _isPowered;
		private bool _isConduit;

		public MetaTile(HexCoords coords, float height, Map map, TileEntity tInfo, BuildingTile parentTile) : base(coords, height, map, parentTile.buildingInfo, 0)
		{
			originalTile = tInfo;
			ParentTile = parentTile;
			if (ParentTile is PoweredBuildingTile powered)
			{
				_poweredParent = powered;
				_isPowered = true;
			}
			_isConduit = ParentTile is ResourceConduitTile;
		}

		protected override void StartConstruction()
		{
		}

		protected override void OnBuilt()
		{
		}

		public override void RenderBuilding()
		{
			hasBuilding = false;
		}

		public override void AddBuff(HexCoords src, StatsBuffs buff)
		{
			ParentTile.AddBuff(src, buff);
		}

		public override void RemoveBuff(HexCoords src)
		{
			ParentTile.RemoveBuff(src);
		}

		public override string GetDescription()
		{
			return ParentTile.GetDescription();
		}

		public override StringBuilder GetName()
		{
#if UNITY_EDITOR
			return ParentTile.GetName().Append(" Meta");
#else
			return ParentTile.GetName();
#endif
		}

		protected override void ApplyTileProperites()
		{
		}

		public override void HQConnected()
		{
			if (_isPowered && !_isConduit)
				_poweredParent.HQConnected();
		}

		public override void HQDisconnected()
		{
			if (_isPowered && !_isConduit)
				_poweredParent.HQDisconnected();
		}

		public override void OnDisconnected()
		{
		}

		public override void OnConnected()
		{
		}

		public override void Deconstruct()
		{
			ParentTile.Deconstruct();
		}

		public override bool CanDeconstruct(Faction faction)
		{
			return ParentTile.CanDeconstruct(faction);
		}

		protected override void DestroyBuilding()
		{

		}

		public override void OnPlaced()
		{
			
		}

		protected override void ApplyBonuses()
		{
			
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			ParentTile.TileUpdated(src, updateType);
		}

		public override void OnHeightChanged()
		{
			
		}

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			base.OnSerialize(tileData);
			tileData.Add("parent.X", ParentTile.Coords.X.ToString());
			tileData.Add("parent.Y", ParentTile.Coords.Y.ToString());
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			base.OnDeSerialized(tileData);
			var x = int.Parse(tileData["parent.X"]);
			var y = int.Parse(tileData["parent.Y"]);
			var coord = new HexCoords(x, y, map.tileEdgeLength);
			ParentTile = map[coord] as BuildingTile;
			if (ParentTile is PoweredBuildingTile powered)
			{
				_isPowered = true;
				_poweredParent = powered;
			}
		}
	}
}