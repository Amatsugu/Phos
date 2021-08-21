using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class AdjacencyEffectsSystem : ComponentSystem
	{
		private AdjacenecyDatabase.Runtime _adjDb;
		private BuildingDatabase _buildingDatabase;

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			_adjDb = GameRegistry.AdjacenecyDatabase.ToRuntime();
			_buildingDatabase = GameRegistry.BuildingDatabase;
		}

		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<BuildingId>().WithAll<BuildingBonusInitTag>().ForEach((Entity e, ref BuildingId building, ref HexPosition pos) =>
			{
				var neighbors = GameRegistry.GameMap.GetNeighbors(pos);

				for (int i = 0; i < neighbors.Length; i++)
				{
					var n = neighbors[i];
					if (n is BuildingTile nb)
					{
						var nBid = _buildingDatabase[nb.buildingInfo];
						//Recevive Buff
						if (_adjDb.HasAdjacencyEffect((building.Value, nBid)))
						{
							Debug.Log($"Has Buff from {nb.GetNameString()}");
						}
						//Give Buff
						else if (_adjDb.HasAdjacencyEffect((nBid, building.Value)))
						{
							Debug.Log($"Give buff to {nb.GetNameString()}");
						}
					}
				}
				PostUpdateCommands.RemoveComponent<BuildingBonusInitTag>(e);
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_adjDb.Dispose();
		}
	}
}
