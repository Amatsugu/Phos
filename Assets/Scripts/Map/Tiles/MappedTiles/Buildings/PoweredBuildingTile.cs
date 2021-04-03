using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

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

		protected bool connectionInit;

		private int _connectionNotif = -1;

		public PoweredBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			
		}

		public override StringBuilder GetDescriptionString()
		{
			return base.GetDescriptionString()
				.AppendLine($"Has HQ Connection: {HasHQConnection}");
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

		protected override void BuildingStart(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
		{
			base.BuildingStart(buildingInst, postUpdateCommands);
			FindConduitConnections(buildingInst, postUpdateCommands);
		}

		/// <summary>
		/// Find a connection to nearby conduits
		/// </summary>
		public virtual void FindConduitConnections(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			Profiler.BeginSample("Find Conduit Connections");
			var closestConduit = map.conduitGraph.GetClosestConduitNode(Coords);
			if (closestConduit == null)
			{
				if (MetaTilesHasConnection())
					HQConnected(building, postUpdateCommands);
				else
					HQDisconnected(building, postUpdateCommands);
			}
			else
			{
				
				var conduit = (map[closestConduit.conduitPos] as ResourceConduitTile);
				if (!conduit.HasHQConnection)
					HQDisconnected(building, postUpdateCommands);
				else if (conduit.IsInPoweredRange(Coords))
					HQConnected(building, postUpdateCommands);
				else
				{
					if (MetaTilesHasConnection())
						HQConnected(building, postUpdateCommands);
					else
						HQDisconnected(building, postUpdateCommands);
				}
			}
			connectionInit = true;
			Profiler.EndSample();
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

		/// <summary>
		/// Callback for when this tile receives an HQ connection
		/// </summary>
		protected virtual void HQConnected(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
		{
			//TODO: Building powered state
			if (connectionInit)
			{
				if (HasHQConnection)
					return;
				if (!HasHQConnection)
					postUpdateCommands.RemoveComponent<BuildingOffTag>(buildingInst);
			}
			HasHQConnection = true;
			OnConnected();
		}

		/// <summary>
		/// Callback for the this tile loses it's HQ connection
		/// </summary>
		protected virtual void HQDisconnected(Entity buildingInst, EntityCommandBuffer postUpdateCommands)
		{
			//TODO: Building powered state
			if (connectionInit)
			{
				if (HasHQConnection)
				{
					HasHQConnection = false;
					connectionInit = false;
					FindConduitConnections(buildingInst, postUpdateCommands);
					return;
				}
				else
					return;
			}
			if(HasHQConnection)
				postUpdateCommands.AddComponent<BuildingOffTag>(buildingInst);
			HasHQConnection = false;
			OnDisconnected();
		}

		/// <summary>
		/// Callback for sucessful connection to the HQ
		/// </summary>
		public virtual void OnConnected()
		{
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
			if(_connectionNotif == -1)
				_connectionNotif = InfoPopupUI.ShowPopupNotif(this, null, "No Power Connection", "This tile is not being powered by a Resource Conduit and cannot opperate");
		}

		public override void OnSerialize(Dictionary<string, string> tileData)
		{
			base.OnSerialize(tileData);
			tileData.Add("connectionInit", null);
			tileData.Add("hasHQConnection", null);
		}

		public override void OnDeSerialized(Dictionary<string, string> tileData)
		{
			connectionInit = tileData.ContainsKey("connectionInit");
			HasHQConnection = tileData.ContainsKey("hasHQConnection");
			if (!HasHQConnection)
				OnDisconnected();
			else
				OnConnected();
			base.OnDeSerialized(tileData);
		}

		public override void OnRemoved()
		{
			base.OnRemoved();
			InfoPopupUI.HidePopup(Coords);
		}
	}
}