using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : ComponentSystem
{

	private Dictionary<int, Path> paths;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		paths = new Dictionary<int, Path>();
	}

	protected override void OnUpdate()
	{
		Entities.WithNone<Path>().ForEach((Entity e, ref PathGroup pg, ref Translation t, ref Destination d) =>
		{
			if (math.distancesq(t.Value, d.Value) < 1f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			Path path;
			if (paths.ContainsKey(pg.Value))
				path = paths[pg.Value];
			else
			{
				path = new Path
				{
					Value = Map.ActiveMap.GetPath(curCoord, HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength),
					filter: ti => ti.Height > Map.ActiveMap.seaLevel && !ti.IsFullyOccupied && !(ti is BuildingTile))
				};
				paths.Add(pg.Value, path);
			}
			pg.Delay = 0;
			PostUpdateCommands.AddSharedComponent(e, path);

		});


		Entities.WithAll<Rotation>().ForEach((Entity e, Path p, ref PathGroup pg, ref Translation t, ref Heading h, ref MoveSpeed m, ref UnitId id) =>
		{
			if(pg.Delay > 60)
			{
				PostUpdateCommands.RemoveComponent<Path>(e);
				paths.Remove(pg.Value);
				return;
			}
			if(pg.Progress >= p.Value?.Count)
			{
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<PathGroup>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var nextTile = p.Value[pg.Progress];
			if(nextTile.IsFullyOccupied)
			{
				pg.Delay++;
				return;
			}
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			var curTtile = Map.ActiveMap[curCoord];
			if(!Map.ActiveMap.units[id.Value].OccupyTile(curTtile))
			{
				pg.Delay++;
				return;
			}
			pg.Delay = 0;
			var tOffset = curTtile.GetOccipancyPos(id.Value);
			var nt = (float3)nextTile.SurfacePoint + tOffset;
			if (math.distancesq(t.Value, nt) < .2f)
			{
				pg.Progress++;
				return;
			}
			h.Value = math.normalize((nt) - t.Value);
			t.Value += h.Value * m.Value * Time.deltaTime;
			t.Value.y = curTtile.Height;
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
		});

	}
}
