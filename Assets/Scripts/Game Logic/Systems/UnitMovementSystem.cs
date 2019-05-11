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
				var dst = HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength);
				if (curCoord == dst)
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
					Debug.LogWarning($"Null Path From: {curCoord} To: {HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength)}");
					PostUpdateCommands.RemoveComponent<PathGroup>(e);
					PostUpdateCommands.RemoveComponent<Destination>(e);
					return;
				}
				paths.Add(pg.Value, path);
			}
#if DEBUG
			for (int i = 0; i < path.Value.Count - 1; i++)
			{
				Debug.DrawLine(path.Value[i].SurfacePoint, path.Value[i + 1].SurfacePoint, Color.HSVToRGB(MathUtils.Map((pg.Value * 45) % 360, 0, 360, 0, 1), 1, 1), 5);
			}
#endif
			PostUpdateCommands.AddSharedComponent(e, path);

		});


		Entities.WithAll<Rotation>().ForEach((Entity e, Path p, ref PathGroup pg, ref Translation t, ref Heading h, ref MoveSpeed m, ref UnitId id) =>
		{
			//TODO: Make sure units move into their positions on the destination tile
			pg.Progress = Mathf.Min(p.Value.Count - 1, pg.Progress);
			var nextTile = p.Value[pg.Progress+1];
			if(nextTile.IsFullyOccupied)
			{
				pg.Delay++;
				if(pg.Delay > 3)
				{
					PostUpdateCommands.RemoveComponent<Path>(e);
					paths.Remove(pg.Value);
					Debug.Log($"Group {pg.Value} Recaluclating");
					pg.Delay = 0;
					pg.Progress = 0;
				}
				return;
			}
			var curCoord = HexCoords.FromPosition(t.Value, Map.ActiveMap.tileEdgeLength);
			var curTile = Map.ActiveMap[curCoord];
			if(!Map.ActiveMap.units[id.Value].OccupyTile(curTile))
			{
				pg.Delay++;
				return;
			}
			pg.Delay = 0;
			var tOffset = curTile.GetOccipancyPos(id.Value);
			var ntPos = (float3)nextTile.SurfacePoint + tOffset;
			h.Value = math.normalize((ntPos) - t.Value);
			t.Value += h.Value * m.Value * Time.deltaTime;
			t.Value.y = curTile.Height;
			PostUpdateCommands.SetComponent(e, new Rotation { Value = Quaternion.LookRotation(h.Value, Vector3.up) });
			if (math.distancesq(t.Value, ntPos) < .05f)
			{
				pg.Progress++;
				if (pg.Progress + 1 >= p.Value.Count)
				{
					PostUpdateCommands.RemoveComponent<Path>(e);
					PostUpdateCommands.RemoveComponent<PathGroup>(e);
					PostUpdateCommands.RemoveComponent<Destination>(e);
				}
				return;
			}
		});

	}
}
