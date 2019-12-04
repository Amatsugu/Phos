using NGenerics.DataStructures.General;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
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

	private NativeArray<float> _navData;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		//paths = new Dictionary<int, Path>();
		paths = new ConcurrentDictionary<int, Path>();
		_navData = Map.ActiveMap.GenerateNavData();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
	}

	[ExcludeComponent(typeof(PathProgress), typeof(Path))]
	public struct PathFinderJob : IJobForEachWithEntity<Translation, Destination, UnitId>
	{
		public EntityCommandBuffer.Concurrent PostUpdateCommands;
		public readonly float edgeLength;
		[ReadOnly]
		public readonly NativeArray<float> navData;

		public PathFinderJob(float edgeLength, EntityCommandBuffer.Concurrent entityCommand, NativeArray<float> navData)
		{
			this.edgeLength = edgeLength;
			this.navData = navData;
			PostUpdateCommands = entityCommand;
		}

		public void Execute(Entity e, int index, ref Translation t, ref Destination d, ref UnitId id)
		{
			if (math.distancesq(t.Value, d.Value) < 0.005f)
			{
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}
			var unit = Map.ActiveMap.units[id.Value];
			var curCoord = unit.Coords;
			Path path;
			var dst = HexCoords.FromPosition(d.Value, Map.ActiveMap.tileEdgeLength);
			if (curCoord == dst)
			{
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}
			path = new Path
			{
				Value = Map.ActiveMap.GetPath(curCoord, dst,
				include: ti => !ti.IsUnderwater && ti.info.isTraverseable)
			};
			if (path.Value == null)
			{
				Debug.LogWarning($"Null Path From: {curCoord} To: {dst}");
				PostUpdateCommands.RemoveComponent<Destination>(index, e);
				return;
			}

			//PostUpdateCommands.AddSharedComponent(e, path);
			PostUpdateCommands.AddComponent(index, e, new PathProgress());
			paths.AddOrUpdate(id.Value, path, (i, p) => path);
			//paths.TryAdd(id.Value, path);
		}

		[BurstCompile]
		void GetPath(HexCoords src, HexCoords dst, int w, int h)
		{
			Heap<PathNode> open = new Heap<PathNode>(HeapType.Minimum);
			open.Add(new PathNode(null, 1));
			HashSet<PathNode> closed = new HashSet<PathNode>();

			var dstNode = new PathNode(null, 1);
			PathNode last = null;
			
			while(true)
			{
				if (closed.Contains(dstNode))
					break;

				PathNode curTileNode = open.RemoveRoot();
				closed.Add(curTileNode);

				last = curTileNode;

				//var neighbors = HexCoords.GetNeighbors
			}
		}

		private class PathNode : IComparer<PathNode>
		{
			public HexCoords coords;
			public Vector3 surfacePoint;
			public int G;
			public PathNode src;
			public float F;

			public PathNode(Tile tile, int g, PathNode src = null)
			{
				coords = tile.Coords;
				surfacePoint = tile.SurfacePoint;
				G = g;
				this.src = src;
			}


			public void CacheF(Vector3 b)
			{
				F = CalculateF(b);
			}

			public float CalculateF(Vector3 b)
			{
				var d = surfacePoint - b;
				return G + (d.sqrMagnitude);
			}

			public int Compare(PathNode x, PathNode y)
			{
				var diff = x.F - y.F;
				if (diff == 0)
					return 0;
				if (diff < 0)
					return -1;
				else
					return 1;
			}

			public override bool Equals(object obj)
			{
				if (obj is PathNode n)
				{
					return n.coords.Equals(n.coords);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return coords.GetHashCode();
			}
		}
	}

	public struct PathFollowJob : IJobForEachWithEntity<PathProgress, UnitId, Translation, Rotation, MoveSpeed>
	{

		public EntityCommandBuffer.Concurrent PostUpdateCommands;

		public PathFollowJob(float deltaTime, EntityCommandBuffer.Concurrent commandBuffer)
		{
			this.deltaTime = deltaTime;
			PostUpdateCommands = commandBuffer;
		}

		public readonly float deltaTime;

		public void Execute(Entity entity, int index, ref PathProgress pathId, ref UnitId id, ref Translation t, ref Rotation r, ref MoveSpeed speed)
		{
			if (!paths.ContainsKey(id.Value))
				return;
			var path = paths[id.Value];
			if (pathId.Progress >= path.Value.Count)
			{
				paths.TryRemove(id.Value, out path);
				PostUpdateCommands.RemoveComponent<PathProgress>(index*2, entity);
				//PostUpdateCommands.RemoveComponent<Path>(entity);
				PostUpdateCommands.RemoveComponent<Destination>((int)((index + .5f) * 2), entity);
				return;
			}
			var dst = path.Value[pathId.Progress].SurfacePoint;
			dst.y = t.Value.y;
			t.Value = Vector3.MoveTowards(t.Value, dst, deltaTime * speed.Value);
			var unit = Map.ActiveMap.units[id.Value];
			t.Value.y = Map.ActiveMap[unit.Coords].Height;
			unit.UpdatePos(t.Value);
			if((Vector3)t.Value == dst)
				pathId.Progress++;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var buffer = World.GetExistingSystem<EntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent();
		var job = new PathFinderJob(_tileEdgeLength, buffer, _navData);
		var handle = job.Schedule(this, inputDeps);
		handle.Complete();
		var buffer2 = World.GetExistingSystem<EntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent();
		var moveJob = new PathFollowJob(Time.DeltaTime, buffer2);
		handle = moveJob.Schedule(this, handle);
		handle.Complete();
		Map.ActiveMap.UpdateUnitChunks();
		return handle;
	}
}
