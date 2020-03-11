using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Amatsugu.Phos.ECS.Jobs.Pathfinder
{
	[ExcludeComponent(typeof(PathProgress), typeof(Path))]
	public struct PathFinderJob : IJobForEachWithEntity<Translation, Destination, UnitId>
	{
		public const int MAX_PATH_LENGTH = 1024;
		public readonly float edgeLength;
		public readonly float innerRadius;

		[ReadOnly]
		public readonly NativeHashMap<HexCoords, float> navData;

		public NativeHashMap<int, Path> paths;
		private EntityCommandBuffer.Concurrent PostUpdateCommands;

		private NativeList<PathNode> open;
		private NativeHashMap<PathNode, float> closed;
		private NativeHashMap<PathNode, PathNode> nodePairs;

		public PathFinderJob(float edgeLength, float innerRadius, EntityCommandBuffer.Concurrent entityCommand, NativeHashMap<HexCoords, float> navData, NativeHashMap<int, Path> pathMap)
		{
			this.edgeLength = edgeLength;
			this.innerRadius = innerRadius;
			this.navData = navData;
			PostUpdateCommands = entityCommand;
			open = new NativeList<PathNode>(Allocator.TempJob);
			closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.TempJob);
			nodePairs = new NativeHashMap<PathNode, PathNode>();
			paths = pathMap;
		}

		public void Execute(Entity e, int index, [ReadOnly]ref Translation t, [ReadOnly]ref Destination d, [ReadOnly]ref UnitId id)
		{
			if (paths.ContainsKey(id.Value))
				return;
			paths.Add(id.Value, new Path { Value = GetPath(t.Value, d.Value) });
		}

		private NativeList<HexCoords> GetPath(float3 src, float3 dst)
		{
			var srcCoord = HexCoords.FromPosition(src);
			var dstCoord = HexCoords.FromPosition(dst);
			var srcNode = new PathNode(srcCoord, src.y, 0);
			var dstNode = new PathNode(dstCoord);
			open.Add(srcNode);
			int pathLen = 0;
			var neighbors = new NativeArray<HexCoords>(6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			while (open.Length > 0)
			{
				if (closed.Length > MAX_PATH_LENGTH)
					break;
				if (closed.ContainsKey(dstNode))
					break;
				var (best, bestIndex) = GetBestNode();
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
					if (UpdateOrAddOpen(newNode))
					{
						if (nodePairs.ContainsKey(newNode))
							nodePairs[newNode] = best;
						else
							nodePairs.Add(newNode, best);
					}
				}
				closed.Add(best, best.F);
			}

			if (closed.ContainsKey(dstNode))
			{
				var path = new NativeList<HexCoords>(pathLen, Allocator.Persistent);
				var curNode = dstNode;
				while (!curNode.Equals(srcNode))
				{
					path.Add(curNode.coords);
					curNode = nodePairs[curNode];
				}
				return path;
			}
			return default;
		}

		private bool UpdateOrAddOpen(PathNode newNode)
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

		private (PathNode best, int index) GetBestNode()
		{
			var (best, index) = (open[0], 0);
			for (int i = 1; i < open.Length; i++)
			{
				if (best.F > open[i].F)
					(best, index) = (open[i], i);
			}
			return (best, index);
		}

		private struct PathNode : IComparer<PathNode>, IEquatable<PathNode>
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
		}
	}
}