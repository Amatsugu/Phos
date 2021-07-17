using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;
using System.Text;

using Unity.Entities;

using UnityEngine;
using UnityEngine.Profiling;

namespace Amatsugu.Phos.Tiles
{
	public class PoweredBuildingTile : BuildingTile
	{
		public bool HasHQConnection { get; protected set; }

		[Obsolete]
		protected bool connectionInit;

		private int _connectionNotif = -1;

		public PoweredBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			
		}

		public PoweredBuildingTile(HexCoords coords, float height, Map map, TileEntity tInfo) : base(coords, height, map, tInfo)
		{

		}

		public override void OnPlaced()
		{
			base.OnPlaced();
		}

		/// <summary>
		/// Callback for when this building is both powered and finished the build phase. Called after whichever happens last
		/// </summary>
		protected virtual void OnBuiltAndPowered()
		{

		}

		public override void BuildingStart(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
		{
			base.BuildingStart(buildingInst, postUpdateCommands);
		}

		public override void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			base.PrepareBuildingEntity(building, postUpdateCommands);
			postUpdateCommands.AddComponent<PoweredBuildingTag>(building);
		}


		/// <summary>
		/// Check if any of  this building's meta tiles has a connection to the HQ
		/// </summary>
		/// <returns></returns>
		public virtual bool MetaTilesHasConnection()
		{
			if (!buildingInfo.useMetaTiles)
				return false;
			for (int i = 0; i < metaTiles.Length; i++)
			{
				if (metaTiles[i].HasHQConnection)
					return true;
			}
			return false;
		}


		protected override void OnBuilt()
		{
			base.OnBuilt();
			if (HasHQConnection)
				OnBuiltAndPowered();
		}

		/// <summary>
		/// Callback for sucessful connection to the HQ
		/// </summary>
		public virtual void OnConnected()
		{
			HasHQConnection = true;
			if (IsBuilt)
				OnBuiltAndPowered();
			if (_connectionNotif != -1)
			{
				InfoPopupUI.RemovePopupNotif(Coords, _connectionNotif);
				_connectionNotif = -1;
			}
		}

		/// <summary>
		/// Callback for when this tile lost connection to the HQ and could not reconnect
		/// </summary>
		public virtual void OnDisconnected()
		{
			HasHQConnection = false;

			if (_connectionNotif == -1)
				_connectionNotif = InfoPopupUI.ShowPopupNotif(this, null, "No Power Connection", "This tile is not being powered by a Resource Conduit and cannot opperate");
		}

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			base.OnSerialize(tileData);
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			base.OnDeSerialized(tileData);
		}

		public override void OnRemoved()
		{
			base.OnRemoved();
			InfoPopupUI.HidePopup(Coords);
		}
	}
}