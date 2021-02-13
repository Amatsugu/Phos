using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;
using System.Text;

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

		protected virtual void OnBuiltAndPowered()
		{

		}

		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			FindConduitConnections();
		}

		public virtual void FindConduitConnections()
		{
			Profiler.BeginSample("Find Conduit Connections");
			var closestConduit = map.conduitGraph.GetClosestConduitNode(Coords);
			if (closestConduit == null)
			{
				if (MetaTilesHasConnection())
					HQConnected();
				else
					HQDisconnected();
			}
			else
			{
				
				var conduit = (map[closestConduit.conduitPos] as ResourceConduitTile);
				if (!conduit.HasHQConnection)
					HQDisconnected();
				else if (conduit.IsInPoweredRange(Coords))
					HQConnected();
				else
				{
					if (MetaTilesHasConnection())
						HQConnected();
					else
						HQDisconnected();
				}
			}
			connectionInit = true;
			Profiler.EndSample();
		}

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

		public virtual void HQConnected()
		{
			if (connectionInit)
			{
				if (HasHQConnection)
					return;
				if (!HasHQConnection)
				{
					Map.EM.RemoveComponent<BuildingOffTag>(GetBuildingEntity());
					Map.EM.RemoveComponent<BuildingOffTag>(subMeshes);
				}
			}
			HasHQConnection = true;
			OnConnected();
		}

		public virtual void HQDisconnected()
		{
			if (connectionInit)
			{
				if (HasHQConnection)
				{
					HasHQConnection = false;
					connectionInit = false;
					FindConduitConnections();
					return;
				}
				else
					return;
			}
			var e = GetBuildingEntity();
			if (!Map.EM.HasComponent<BuildingOffTag>(e))
			{
				Map.EM.AddComponent<BuildingOffTag>(e);
				Map.EM.AddComponent<BuildingOffTag>(subMeshes);
			}
			HasHQConnection = false;
			OnDisconnected();
		}

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