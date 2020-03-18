using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
			/*var path = GetPath(t.Value, d.Value);
			if (!path.IsCreated)
				return;
			var p = new Path { Value =  path };
			PostUpdateCommands.AddSharedComponent(index, e, p);*/
		}


		public static (NativeList<PathNode> open, NativeHashMap<PathNode, float> closed, NativeHashMap<PathNode, PathNode> nodePairs) PrepareCollections(int navDataLen)
		{
			NativeList<PathNode> open = new NativeList<PathNode>(Allocator.Persistent);
			NativeHashMap<PathNode, float> closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.Persistent);
			NativeHashMap<PathNode, PathNode> nodePairs = new NativeHashMap<PathNode, PathNode>(navDataLen, Allocator.Persistent);
			return (open, closed, nodePairs);
		}

		[BurstCompile]
		public static List<HexCoords> GetPath(
			float3 src,
			float3 dst,
			ref NativeHashMap<HexCoords, float> navData,
			float innerRadius,
			ref NativeList<PathNode> open,
			ref NativeHashMap<PathNode, float> closed,
			ref NativeHashMap<PathNode, PathNode> nodePairs)
		{
			var srcCoord = HexCoords.FromPosition(src);
			var dstCoord = HexCoords.FromPosition(dst);
			var srcNode = new PathNode(srcCoord, src.y, 0);
			var dstNode = new PathNode(dstCoord, dst.y, 0);
			open.Clear();
			closed.Clear();
			nodePairs.Clear();

			//NativeList<PathNode> open = new NativeList<PathNode>(Allocator.Temp);
			//NativeHashMap<PathNode, float> closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.Temp);
			//NativeHashMap<PathNode, PathNode> nodePairs = new NativeHashMap<PathNode, PathNode>(navData.Length, Allocator.Temp);
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
				closed.Add(best, best.F);
				for (int i = 0; i < 6; i++)
				{
					var curNeighbor = neighbors[i];
					if (!navData.ContainsKey(curNeighbor))
						continue;

					var newNode = new PathNode(curNeighbor, navData[curNeighbor], best.G + 1);
					//Debug.DrawLine(best.surfacePoint, newNode.surfacePoint, Color.blue, 1);
					if (closed.ContainsKey(newNode))
						continue;
					if (navData[curNeighbor] < 0)
					{
						closed.Add(newNode, 0);
						break;
					}

					if (curNeighbor == dstCoord)
					{
						closed.Add(dstNode = newNode, 0);
						nodePairs.Add(newNode, best);
						pathLen = newNode.G;
						break;
					}
					newNode.CacheF(dst);
					UpdateOrAddOpen(newNode, ref open, ref closed);
					if (!nodePairs.ContainsKey(newNode))
						nodePairs.Add(newNode, best);
				}
			}
			//open.Dispose();
			if (closed.ContainsKey(dstNode))
			{
				//closed.Dispose();
				var path = new List<HexCoords>();
				var curNode = dstNode;
				while (!curNode.Equals(srcNode))
				{
					path.Add(curNode.coords);
					curNode = nodePairs[curNode];
				}
				//nodePairs.Dispose();
				return path;
			}
			UnityEngine.Debug.LogWarning("No Path");
			//nodePairs.Dispose();
			return default;
		}

		private static bool UpdateOrAddOpen(PathNode newNode, ref NativeList<PathNode> open, ref NativeHashMap<PathNode, float> closed)
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

		private static (PathNode best, int index) GetBestNode(ref NativeList<PathNode> open)
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