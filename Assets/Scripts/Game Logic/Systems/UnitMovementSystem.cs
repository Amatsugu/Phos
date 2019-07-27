using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMovementSystem : JobComponentSystem
{

	private float _tileEdgeLength;
	private Camera _cam;
	private int _mapWidth;
	private EntityQuery EntityQuery;
	public static ConcurrentDictionary<int, Path> paths;
	private EntityCommandBuffer buffer;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		//paths = new Dictionary<int, Path>();
		paths = new ConcurrentDictionary<int, Path>();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
	}

	public struct PathFinderJob : IJobForEachWithEntity<PathId, Translation, Destination, UnitId>
	{
		public readonly EntityCommandBuffer PostUpdateCommands;
		public readonly float edgeLength;

		public PathFinderJob(float edgeLength, EntityCommandBuffer entityCommand)
		{
			this.edgeLength = edgeLength;
			PostUpdateCommands = entityCommand;
		}

		public void Execute(Entity e, int index, ref PathId pg, ref Translation t, ref Destination d, ref UnitId id)
		{
			if (paths.ContainsKey(id.Value))
				return;
			if (math.distancesq(t.Value, d.Value) < 0.005f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = HexCoords.FromPosition(t.Value, edgeLength);
			Path path;
			var dst = HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength);
			if (curCoord == dst)
			{
				PostUpdateCommands.RemoveComponent<PathId>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
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
				PostUpdateCommands.RemoveComponent<PathId>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}

			//PostUpdateCommands.AddSharedComponent(index, e, path);
			paths.TryAdd(id.Value, path);
		}
	}

	public struct PathFollowJob : IJobForEachWithEntity<PathId, UnitId, Translation, Rotation>
	{
		public void Execute(Entity entity, int index, ref PathId pathId, ref UnitId id, ref Translation t, ref Rotation r)
		{
			if (!paths.ContainsKey(id.Value))
				return;
			var path = paths[id.Value];
			if (pathId.Progress >= path.Value.Count)
			{
				paths.TryRemove(id.Value, out path);
				return;
			}
			t.Value = path.Value[pathId.Progress].SurfacePoint;
			Map.ActiveMap.units[id.Value].UpdatePos(t.Value);
			pathId.Progress++;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		buffer = World.GetExistingSystem<EntityCommandBufferSystem>().CreateCommandBuffer();
		var job = new PathFinderJob(_tileEdgeLength, buffer);
		var handle = job.Schedule(this, inputDeps);
		handle.Complete();
		var moveJob = new PathFollowJob();
		handle = moveJob.Schedule(this, handle);
		return handle;
	}
}
