using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using Unity.Entities;

using UnityEngine;


namespace Amatsugu.Phos.Tiles
{
	public class MetaTile : PoweredBuildingTile
	{
		public BuildingTile ParentTile { get; private set; }

		private PoweredBuildingTile _poweredParent;
		private bool _isPoweredTile;
		private bool _isConduitTile;

		public MetaTile(HexCoords coords, float height, Map map, TileEntity tInfo, BuildingTile parentTile) : base(coords, height, map, parentTile.buildingInfo, 0)
		{
			originalTile = tInfo;
			ParentTile = parentTile;
			if (ParentTile is PoweredBuildingTile powered)
			{
				_poweredParent = powered;
				_isPoweredTile = true;
			}
			_isConduitTile = ParentTile is ResourceConduitTile;
		}

		protected override void OnBuilt()
		{
		}

		public override void ApplyBuffs(Entity tileEntity, Entity buildingEntity, EntityCommandBuffer postUpdateCommands)
		{

		}

		public override void AddBuff(HexCoords src, StatsBuffs buff)
		{
			ParentTile.AddBuff(src, buff);
		}

		public override void RemoveBuff(HexCoords src)
		{
			ParentTile.RemoveBuff(src);
		}

		public override StringBuilder GetDescriptionString()
		{
			return ParentTile.GetDescriptionString();
		}

		public override StringBuilder GetNameString()
		{
#if UNITY_EDITOR
			return ParentTile.GetNameString().Append(" [Meta]");
#else
			return ParentTile.GetNameString();
#endif
		}

		//public override void Start()
		//{
		//	base.Start();
		//	FindConduitConnections();
		//}

		public override bool MetaTilesHasConnection()
		{
			return false;
		}

		//public override void HQConnected()
		//{
		//	if (_isPoweredTile && !_isConduitTile)
		//		_poweredParent.HQConnected();
		//}

		//public override void HQDisconnected()
		//{
		//	if (_isPoweredTile && !_isConduitTile)
		//		_poweredParent.HQDisconnected();
		//}

		public override void OnDisconnected()
		{
		}

		public override void OnConnected()
		{
		}

		public override void Deconstruct(DynamicBuffer<GenericPrefab> prefabs, Entity existingTileInstance, EntityCommandBuffer postUpdateCommands)
		{
			ParentTile.Deconstruct(prefabs, existingTileInstance, postUpdateCommands);
		}

		public override bool CanDeconstruct(Faction faction)
		{
			return ParentTile.CanDeconstruct(faction);
		}

		public override void OnPlaced()
		{
			
		}

		protected override void ApplyAdjacencyBonuses()
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
				_isPoweredTile = true;
				_poweredParent = powered;
			}
		}
	}
}