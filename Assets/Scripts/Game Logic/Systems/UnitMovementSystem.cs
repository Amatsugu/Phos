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
	private float _tileEdgeLength;
	private Camera _cam;
	private int _mapWidth;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		paths = new Dictionary<int, Path>();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
	}

	protected override void OnUpdate()
	{
		Entities.WithNone<Path>().ForEach((Entity e, ref PathGroup pg, ref Translation t, ref Destination d, ref UnitId id) =>
		{
			if (math.distancesq(t.Value, d.Value) < 0.005f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = HexCoords.FromPosition(t.Value, _tileEdgeLength);
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
					filter: ti => ti.Height > Map.ActiveMap.seaLevel && !(ti is BuildingTile))
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
			var c = MathUtils.Remap((pg.Value * 10) % 360, 0, 360, 0, 1);
			var off = new Vector3(0, c, 0);
			for (int i = 0; i < path.Value.Count - 1; i++)
			{
				Debug.DrawLine(path.Value[i].SurfacePoint + off, path.Value[i + 1].SurfacePoint + off, Color.HSVToRGB(c, 1, 1), 5);
			}
#endif
			PostUpdateCommands.AddSharedComponent(e, path);
			pg.Progress = pg.Delay = 0;

		});


		Entities.WithAll<Rotation>().ForEach((Entity e, ref UnitId id) =>
		{
			if (!Map.ActiveMap.units.ContainsKey(id.Value))
				return;
			if (!Input.GetKey(KeyCode.LeftShift))
				return;
			var unit = Map.ActiveMap.units[id.Value];
			var target = Map.ActiveMap.GetTileFromRay(_cam.ScreenPointToRay(Input.mousePosition)).SurfacePoint;
			var dir = (target - unit.Position).normalized;
			var newPos = unit.Position += dir * Time.deltaTime * unit.info.moveSpeed;
			newPos.y = Map.ActiveMap[HexCoords.FromPosition(newPos)].Height;
			target.y = unit.Position.y;
			dir = (target - unit.Position);
			unit.Position = newPos;
			EntityManager.SetComponentData(e, new Rotation { Value = Quaternion.LookRotation(dir) });
		});

	}
}
