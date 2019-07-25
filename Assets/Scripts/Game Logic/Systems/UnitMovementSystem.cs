using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : JobComponentSystem
{

	private Dictionary<int, Path> paths;
	private float _tileEdgeLength;
	private Camera _cam;
	private int _mapWidth;
	private EntityQuery EntityQuery;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		paths = new Dictionary<int, Path>();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
	}

	public struct PathFinderJob : IJobForEachWithEntity<PathGroup, Translation, Destination, UnitId>
	{


		public readonly EntityCommandBuffer.Concurrent PostUpdateCommands;
		public readonly float edgeLength;

		public PathFinderJob(float edgeLength, EntityCommandBuffer.Concurrent entityCommand)
		{
			this.edgeLength = edgeLength;
			PostUpdateCommands = entityCommand;
		}

		public void Execute(Entity e, int index, ref PathGroup pg, ref Translation t, ref Destination d, ref UnitId id)
		{
			if (math.distancesq(t.Value, d.Value) < 0.005f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = HexCoords.FromPosition(t.Value, edgeLength);
			Path path;
			var dst = HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength);
			if (curCoord == dst)
			{
				PostUpdateCommands.RemoveComponent<PathGroup>(index, e);
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}
			path = new Path
			{
				Value = Map.ActiveMap.GetPath(curCoord, dst,
				filter: ti => ti.Height > Map.ActiveMap.seaLevel && !(ti is BuildingTile))
			};
			if (path.Value == null)
			{
				Debug.LogWarning($"Null Path From: {curCoord} To: {dst}");
				PostUpdateCommands.RemoveComponent<PathGroup>(index, e);
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}
#if DEBUG
			/*var c = MathUtils.Remap((pg.Value * 10) % 360, 0, 360, 0, 1);
			var off = new Vector3(0, c, 0);
			for (int i = 0; i < path.Value.Count - 1; i++)
			{
				Debug.DrawLine(path.Value[i].SurfacePoint + off, path.Value[i + 1].SurfacePoint + off, Color.HSVToRGB(c, 1, 1), 5);
				if (i == path.Value.Count - 2)
					Debug.DrawRay(path.Value[i + 1].SurfacePoint + off, Vector3.up, Color.HSVToRGB(c, 1, 1), 5);
			}*/
#endif
			PostUpdateCommands.AddSharedComponent(index, e, path);
			pg.Progress = pg.Delay = 0;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		
		var job = new PathFinderJob(_tileEdgeLength, new EntityCommandBuffer.Concurrent());
		var handle = job.Schedule(this, inputDeps);
		handle.Complete();
		
		return handle;
	}
}
