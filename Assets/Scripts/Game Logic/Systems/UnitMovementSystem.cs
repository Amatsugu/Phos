using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		Entities.WithNone<Path>().ForEach((Entity e, ref PathGroup pg, ref Translation t, ref Destination d, ref UnitId id) =>
		{
			if (math.distancesq(t.Value, d.Value) < 0.0005f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = unit.occupiedTile;
			Path path;
			if (paths.ContainsKey(pg.Value))
				path = paths[pg.Value];
			else
			{
				var dst = HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength);
				if (curCoord == dst || Map.ActiveMap[dst].IsFullyOccupied)
				{
					PostUpdateCommands.RemoveComponent<PathGroup>(e);
					PostUpdateCommands.RemoveComponent<Destination>(e);
					return;
				}
				path = new Path
				{
					Value = Map.ActiveMap.GetPath(curCoord, dst,
					filter: ti => ti.Height > Map.ActiveMap.seaLevel && !ti.IsFullyOccupied && !(ti is BuildingTile))
				};
				if(path.Value == null)
				{
					Debug.LogWarning($"Null Path From: {curCoord} To: {dst}");
					PostUpdateCommands.RemoveComponent<PathGroup>(e);
					PostUpdateCommands.RemoveComponent<Destination>(e);
					return;
				}
				paths.Add(pg.Value, path);
			}
#if DEBUG
			var c = MathUtils.Map((pg.Value * 10) % 360, 0, 360, 0, 1);
			var off = new Vector3(0, c, 0);
			for (int i = 0; i < path.Value.Count - 1; i++)
			{
				Debug.DrawLine(path.Value[i].SurfacePoint + off, path.Value[i + 1].SurfacePoint + off, Color.HSVToRGB(c, 1, 1), 5);
			}
#endif
			PostUpdateCommands.AddSharedComponent(e, path);
			pg.Progress = pg.Delay = 0;

		});


		Entities.WithAll<Rotation>().ForEach((Entity e, Path p, ref PathGroup pg, ref Translation t, ref Heading h, ref MoveSpeed m, ref UnitId id) =>
		{
			pg.Progress = Mathf.Min(p.Value.Count - 1, pg.Progress);
			var nextTile = p.Value[pg.Progress+1];
			if(nextTile.IsFullyOccupied && !nextTile.Equals(p.Value.Last()))
			{
				pg.Delay++;
				if(pg.Delay > 3)
				{
					PostUpdateCommands.RemoveComponent<Path>(e);
					paths.Remove(pg.Value);
				}
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			var curTile = Map.ActiveMap[curCoord];
			if(!unit.OccupyTile(curTile))
			{
				pg.Delay++;
				if (pg.Delay > 3)
				{
					PostUpdateCommands.RemoveComponent<Path>(e);
					paths.Remove(pg.Value);
				}
				return;
			}
			pg.Delay = 0;
			var tOffset = curTile.GetOccipancyPos(id.Value);
			var ntPos = (float3)nextTile.SurfacePoint + tOffset;
			h.Value = math.normalize((ntPos) - t.Value);
			t.Value += h.Value * m.Value * Time.deltaTime;
			t.Value.y = curTile.Height;
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
			if (math.distancesq(t.Value, ntPos) < .0005f)
			{
				pg.Progress++;
				if (pg.Progress + 1 >= p.Value.Count)
				{
					PostUpdateCommands.RemoveComponent<Path>(e);
					PostUpdateCommands.RemoveComponent<PathGroup>(e);
					PostUpdateCommands.RemoveComponent<Destination>(e);
					paths.Remove(pg.Value);

				}
				return;
			}
		});

	}
}
