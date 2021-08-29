using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	[UpdateAfter(typeof(BuildingInstanceBufferSystem))]
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
			Entities.WithNone<BuildingInitTag>().WithAllReadOnly<BuildingId>().WithAll<BuildingBonusInitTag>().ForEach((Entity e, ref BuildingId building, ref HexPosition pos) =>
			{
				var neighbors = GameRegistry.GameMap.GetNeighbors(pos);
				var buffer = GameRegistry.GetTileInstanceBuffer();

				for (int i = 0; i < neighbors.Length; i++)
				{
					var n = neighbors[i];
					if (n is BuildingTile nb)
					{
						var nBid = _buildingDatabase[nb.buildingInfo];
						//Recevive Buff
						if (_adjDb.HasAdjacencyEffect((building.Value, nBid)))
						{
							var b = ((BuildingTile)GameRegistry.GameMap[pos]);
							b.AddBuff(nb.Coords, _adjDb.GetAdjancencyEffect(building.Value, nBid));
							var t = buffer[b.Coords.ToIndex(GameRegistry.GameMap.totalWidth)];
							b.ApplyBuffs(t, e, PostUpdateCommands);
						}
						//Give Buff
						else if (_adjDb.HasAdjacencyEffect((nBid, building.Value)))
						{
							nb.AddBuff(pos, _adjDb.GetAdjancencyEffect(building.Value, nBid));
							var nt = buffer[nb.Coords.ToIndex(GameRegistry.GameMap.totalWidth)];
							var ntb = EntityManager.GetComponentData<Building>(nt).Value;
							nb.ApplyBuffs(nt, ntb, PostUpdateCommands);
							Debug.Log($"Give buff to {nb.GetNameString()}");
						}
					}
				}
				PostUpdateCommands.RemoveComponent<BuildingBonusInitTag>(e);
			});
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
			_adjDb.Dispose();
		}


		protected override void OnDestroy()
		{
			base.OnDestroy();
			_adjDb.Dispose();
		}
	}
}
