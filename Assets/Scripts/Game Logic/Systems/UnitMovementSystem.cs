using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : ComponentSystem
{
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
	}

	protected override void OnUpdate()
	{
		Entities.WithNone<NextTile>().ForEach((Entity e, ref Translation t, ref Heading h, ref UnitId id, ref MoveSpeed m, ref Destination d) =>
		{
			if (math.distancesq(t.Value, d.Value) < Map.ActiveMap.innerRadius * Map.ActiveMap.innerRadius)
			{
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			var path = Map.ActiveMap.GetPath(curCoord, HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength), filter: ti => !ti.IsFullyOccupied && !(ti is BuildingTile));
			if (path == null)
			{
				h.Value = math.normalize(d.Value - t.Value);
				PostUpdateCommands.AddComponent(e, new NextTile { Value = d.Value });
			}
			else
			{
				h.Value = math.normalizesafe(((float3)path[1].SurfacePoint) - t.Value);
				PostUpdateCommands.AddComponent(e, new NextTile { Value = path[1].SurfacePoint });
			}
			
		});


		Entities.ForEach((Entity e, ref Translation t, ref Heading h, ref MoveSpeed m, ref NextTile nt, ref UnitId id) =>
		{
			if (math.distancesq(t.Value, nt.Value) < Map.ActiveMap.innerRadius * Map.ActiveMap.innerRadius)
			{
				PostUpdateCommands.RemoveComponent<NextTile>(e);
				return;
			}
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
			t.Value += h.Value * m.Value * Time.deltaTime;
			var tile = Map.ActiveMap[curCoord];
			if (tile != null)
			{
				t.Value.y = tile.Height;
				Map.ActiveMap.units[id.Value].OccupyTile(tile);
			}
		});

	}
}
