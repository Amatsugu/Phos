using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Amatsugu.Phos.ECS.Jobs.Pathfinder
{
	[BurstCompile]
	public class PathFinderSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var cmb = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent();
			var path = new PathFinderJob(Map.ActiveMap.tileEdgeLength, Map.ActiveMap.innerRadius, cmb, Map.ActiveMap.GenerateNavData());
			Entities.WithNone<PathProgress, Path>().ForEach((Entity e, ref Translation t, ref Destination d, ref UnitId id) =>
			{
				var p = new Path { Value = path.GetPath(t.Value, t.Value) };
				PostUpdateCommands.AddSharedComponent(e, p);
				PostUpdateCommands.AddComponent<PathProgress>(e);
			});
			/*inputDeps = path.Schedule(this, inputDeps);
			return inputDeps;*/
		}
	}

	[ExcludeComponent(typeof(PathProgress), typeof(Path))]
	public struct PathFinderJob : IJobForEachWithEntity<Translation, Destination, UnitId>
	{
		public const int MAX_PATH_LENGTH = 1024;
		public readonly float edgeLength;
		public readonly float innerRadius;

		[ReadOnly]
		public readonly NativeHashMap<HexCoords, float> navData;

		private EntityCommandBuffer.Concurrent PostUpdateCommands;

		public PathFinderJob(float edgeLength, float innerRadius, EntityCommandBuffer.Concurrent entityCommand, NativeHashMap<HexCoords, float> navData)
		{
			this.edgeLength = edgeLength;
			this.innerRadius = innerRadius;
			this.navData = navData;
			PostUpdateCommands = entityCommand;
		}

		public void Execute(Entity e, int index, [ReadOnly]ref Translation t, [ReadOnly]ref Destination d, [ReadOnly]ref UnitId id)
		{
			var path = GetPath(t.Value, d.Value);
			if (!path.IsCreated)
				return;
			var p = new Path { Value =  path };
			PostUpdateCommands.AddSharedComponent(index, e, p);
		}

		public NativeList<HexCoords> GetPath(float3 src, float3 dst)
		{
			var srcCoord = HexCoords.FromPosition(src);
			var dstCoord = HexCoords.FromPosition(dst);
			var srcNode = new PathNode(srcCoord, src.y, 0);
			var dstNode = new PathNode(dstCoord);
			NativeList<PathNode> open = new NativeList<PathNode>(Allocator.Temp);
			NativeHashMap<PathNode, float> closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.Temp);
			NativeHashMap<PathNode, PathNode> nodePairs = new NativeHashMap<PathNode, PathNode>(navData.Length, Allocator.Temp);
			open.Add(srcNode);
			int pathLen = 0;
			var neighbors = new NativeArray<HexCoords>(6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			while (open.Length > 0)
			{
				if (closed.Length > MAX_PATH_LENGTH)
					break;
				if (closed.ContainsKey(dstNode))
					break;
				var (best, bestIndex) = GetBestNode(ref open);
				open.RemoveAtSwapBack(bestIndex);
				best.GetNeighbors(ref neighbors, innerRadius, navData);
				for (int i = 0; i < 6; i++)
				{
					var curNeighbor = neighbors[i];
					if (!curNeighbor.isCreated)
						continue;

					var newNode = new PathNode(curNeighbor, navData[curNeighbor], best.G + 1);
					if (navData[curNeighbor] < 0)
					{
						closed.Add(newNode, 0);
						break;
					}

					if (curNeighbor == dstCoord)
					{
						closed.Add(dstNode = newNode, 0);
						pathLen = newNode.G;
						break;
					}
					newNode.CacheF(dst);
					if (UpdateOrAddOpen(newNode, ref open, ref closed))
					{
						if (nodePairs.ContainsKey(newNode))
							nodePairs[newNode] = best;
						else
							nodePairs.Add(newNode, best);
					}
				}
				closed.Add(best, best.F);
			}
			open.Dispose();
			if (closed.ContainsKey(dstNode))
			{
				closed.Dispose();
				var path = new NativeList<HexCoords>(pathLen, Allocator.Persistent);
				var curNode = dstNode;
				while (!curNode.Equals(srcNode))
				{
					path.Add(curNode.coords);
					curNode = nodePairs[curNode];
				}
				nodePairs.Dispose();
				return path;
			}
			nodePairs.Dispose();
			return default;
		}

		private bool UpdateOrAddOpen(PathNode newNode, ref NativeList<PathNode> open, ref NativeHashMap<PathNode, float> closed)
		{
			for (int i = 0; i < open.Length; i++)
			{
				if (open[i].Equals(newNode))
				{
					var n = open[i];
					if (n.F < newNode.F)
						return false;
					else
					{
						if (closed.ContainsKey(newNode) && closed[newNode] < newNode.F)
							return false;
						open[i] = newNode;
						return true;
					}
				}
			}
			open.Add(newNode);
			return true;
		}

		private (PathNode best, int index) GetBestNode(ref NativeList<PathNode> open)
		{
			var (best, index) = (open[0], 0);
			for (int i = 1; i < open.Length; i++)
			{
				if (best.F > open[i].F)
					(best, index) = (open[i], i);
			}
			return (best, index);
		}

		public struct PathNode : IComparer<PathNode>, IEquatable<PathNode>
		{
			public HexCoords coords;
			public float3 surfacePoint;
			public int G;
			public float F;
			public bool isCreated;

			public PathNode(HexCoords coords, float height, int g, float f = 0)
			{
				this.coords = coords;
				surfacePoint = coords.worldXZ;
				surfacePoint.y = height;
				G = g;
				isCreated = true;
				F = f;
			}

			public PathNode(HexCoords coords) : this(coords, 0, default, 0)
			{
			}

			public void GetNeighbors(ref NativeArray<HexCoords> neighbors, float innerRadius, NativeHashMap<HexCoords, float> navData)
			{
				for (int i = 0; i < 6; i++)
				{
					var n = coords.GetNeighbor(i, innerRadius);
					if (navData.ContainsKey(n))
						neighbors[i] = n;
					else
						neighbors[i] = default;
				}
			}

			public void CacheF(float3 b) => F = CalculateF(b);

			public float CalculateF(float3 b) => G + math.lengthsq(surfacePoint - b);

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

			public override bool Equals(object obj) => obj is PathNode n ? n.coords.Equals(n.coords) : false;

			public bool Equals(PathNode other) => coords.Equals(other.coords);

			public override int GetHashCode() => coords.GetHashCode();

			public static bool operator ==(PathNode left, PathNode right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(PathNode left, PathNode right)
			{
				return !(left == right);
			}
		}
	}
}