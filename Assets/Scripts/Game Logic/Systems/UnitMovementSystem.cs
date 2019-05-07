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
		Entities.ForEach((Entity e, ref Translation t, ref Heading h, ref UnitId id, ref MoveSpeed m, ref Destination d) =>
		{
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			var path = Map.ActiveMap.GetPath(curCoord, HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength));
			if (path == null)
				h.Value = math.normalize(d.Value - t.Value);
			else
				h.Value = math.normalizesafe(((float3)path[1].SurfacePoint) - t.Value);
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
			t.Value +=  h.Value * m.Value * Time.deltaTime;
			var tile = Map.ActiveMap[curCoord];
			t.Value.y = tile.Height;
			Map.ActiveMap.units[id.value].OccupyTile(tile);
		});


	}
}
