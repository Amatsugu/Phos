using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataStore.ConduitGraph
{
	public class ConduitGraph
	{
		public readonly int maxConnections;
		public Dictionary<int, ConduitNode> nodes;
		public int Count => nodes.Count;
		public Dictionary<HexCoords, int> _coordMap;
		private readonly ConduitNode _baseNode;

		public event Action<ConduitNode> OnNodeRemoved;
		public event Action<ConduitNode> OnNodeAdded;

		private int _curId = 0;

		public ConduitGraph(HexCoords baseNode, int maxConnections = 6)
		{
			this.maxConnections = maxConnections;
			nodes = new Dictionary<int, ConduitNode>();
			_coordMap = new Dictionary<HexCoords, int>();
			_baseNode = CreateNode(baseNode);
		}

		public ConduitNode GetNode(HexCoords nodePos) => nodes[_coordMap[nodePos]];

		public void ConnectNode(HexCoords nodePos, ConduitNode connectTo) => nodes[_coordMap[nodePos]].ConnectTo(connectTo);

		public void RemoveNode(HexCoords nodePos)
		{
			var node = nodes[_coordMap[nodePos]];
			for (int i = 0; i < maxConnections; i++)
			{
				var id = node._connections[i];
				if (id != -1)
					nodes[id].DisconnectFrom(node);
			}
			nodes.Remove(node.id);
			_coordMap.Remove(nodePos);
			OnNodeRemoved?.Invoke(node);
		}

		public bool ContainsNode(HexCoords nodePos) => _coordMap.ContainsKey(nodePos);

		public void AddNode(HexCoords nodePos, ConduitNode connectTo)
		{
			var newNode = CreateNode(nodePos);
			newNode.ConnectTo(connectTo);
			OnNodeAdded?.Invoke(newNode);
		}

		ConduitNode CreateNode(HexCoords nodePos)
		{
			var id = _curId++;
			var newNode = new ConduitNode(id, nodePos, maxConnections);
			nodes.Add(id, newNode);
			_coordMap.Add(nodePos, id);
			return newNode;
		}

		public void AddNodeDisconected(HexCoords nodePos)
		{
			var node = CreateNode(nodePos);
			OnNodeAdded?.Invoke(node);
		}

		public ConduitNode GetClosestNode(HexCoords nodePos, bool excludeFull = true)
		{
			var closest = _baseNode;
			var bestDist = _baseNode.conduitPos.DistanceToSq(nodePos);
			var pos = _coordMap.Keys.ToArray();
			for (int i = 0; i < _coordMap.Count; i++)
			{
				var n = nodes[_coordMap[pos[i]]];
				if (excludeFull && n.IsFull)
					continue;
				var dist = nodePos.DistanceToSq(pos[i]);
				if(dist < bestDist)
				{
					bestDist = dist;
					closest = n;
				}
			}
			return closest.IsFull ? default : closest;
		}


		/// <summary>
		/// Gets the closest node and excludes the base node
		/// </summary>
		/// <param name="nodePos">Coord to compare distance to</param>
		/// <returns>Closest non base node</returns>
		public ConduitNode GetClosestConduitNode(HexCoords nodePos)
		{
			ConduitNode closest = null;
			var bestDist = float.MaxValue;
			var pos = _coordMap.Keys.ToArray();
			for (int i = 0; i < _coordMap.Count; i++)
			{
				var n = nodes[_coordMap[pos[i]]];
				if (n.Equals(_baseNode))
					continue;
				var dist = nodePos.DistanceToSq(pos[i]);
				if (dist < bestDist)
				{
					bestDist = dist;
					closest = n;
				}
			}
			return closest;
		}

		public ConduitNode[] GetDisconectedNodes()
		{
			var visited = new HashSet<ConduitNode>();
			TraverseGraph(_baseNode, visited);
			if(visited.Count == Count)
				return new ConduitNode[0];

			var nodesArr = nodes.Values.ToArray();
			var disconected = new ConduitNode[Count-visited.Count];
			var j = 0;
			for (int i = 0; i < Count; i++)
			{
				if (!visited.Contains(nodesArr[i]))
					disconected[j++] = nodesArr[i];
			}
			return disconected;
		}

		public HashSet<ConduitNode> GetDisconectedNodesSet()
		{
			var visited = new HashSet<ConduitNode>();
			TraverseGraph(_baseNode, visited);
			if (visited.Count == Count)
				return new HashSet<ConduitNode>();

			var nodesArr = nodes.Values.ToArray();
			var disconected = new HashSet<ConduitNode>();
			for (int i = 0; i < Count; i++)
			{
				if (!visited.Contains(nodesArr[i]))
					disconected.Add(nodesArr[i]);
			}
			return disconected;
		}

		public void TraverseGraph(ConduitNode node, HashSet<ConduitNode> visited)
		{
			if (!visited.Contains(node))
				visited.Add(node);
			else
				return;
			if (node.IsEmpty)
				return;
			for (int i = 0; i < node.maxConnections; i++)
			{
				var id = node._connections[i];
				if (id == -1)
					continue;
				var cNode = nodes[id];
				TraverseGraph(cNode, visited);
			}
		}

		public List<ConduitNode> GetNodesInRange(HexCoords nodePos, float rangeSq, bool excludeFull = true)
		{
			var nodesInRange = new List<ConduitNode>();
			var pos = _coordMap.Keys.ToArray();
			for (int i = 0; i < _coordMap.Count; i++)
			{
				var n = nodes[_coordMap[pos[i]]];
				if (excludeFull && n.IsFull)
					continue;
				if (n.conduitPos.DistanceToSq(nodePos) <= rangeSq)
					nodesInRange.Add(n);
			}
			return nodesInRange;
		}

		public ConduitNode[] GetConnections(HexCoords nodePos) => GetConnections(nodes[_coordMap[nodePos]]);

		public ConduitNode[] GetConnections(ConduitNode node)
		{
			if (node.IsEmpty)
				return null;
			var connections = new ConduitNode[node.ConnectionCount];
			var j = 0;
			for (int i = 0; i < maxConnections; i++)
			{
				if (node._connections[i] != -1)
					connections[j++] = nodes[node._connections[i]];
			}
			return connections;
		}

		private class PathNode
		{
			public int G;
			public ConduitNode node;
			public PathNode src;
			public float F;

			public PathNode(ConduitNode node, int g, PathNode src = null)
			{
				this.node = node;
				G = g;
				this.src = src;
			}

			public void CacheF(HexCoords b)
			{
				F = CalculateF(b);
			}

			public float CalculateF(HexCoords b)
			{
				var d = node.conduitPos.DistanceToSq(b);
				return G + d;
			}

			public override bool Equals(object obj)
			{
				if (obj is PathNode n)
				{
					return n.node.Equals(n.node);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return node.GetHashCode();
			}
		}

		public List<Vector3> GetPath(ConduitNode dst)
		{
			PathNode BestFScore(HashSet<PathNode> nodes)
			{
				PathNode best = nodes.First();
				foreach (var node in nodes)
				{
					if (best.F > node.F)
						best = node;
				}
				return best;
			}

			var open = new HashSet<PathNode> { new PathNode(_baseNode, 1) };
			var closed = new HashSet<PathNode>();
			var dstNode = new PathNode(dst, 1);
			PathNode last = null;
			while(open.Count > 0)
			{
				if (closed.Contains(dstNode))
				{
					last = new PathNode(dst, 0, last);
					break;
				}
				PathNode curNode = BestFScore(open);
				open.Remove(curNode);
				closed.Add(curNode);
				last = curNode;
				var neighbors = GetConnections(curNode.node);
				if (neighbors == null)
					break;
				for (int i = 0; i < neighbors.Length; i++)
				{
					var neighbor = neighbors[i];
					var adj = new PathNode(neighbor, curNode.G + 1, curNode);
					if (closed.Contains(adj))
						continue;
					adj.CacheF(dst.conduitPos);
					if (!open.Contains(adj))
						open.Add(adj);
					else
					{
						var o = open.First(oAdj => oAdj.Equals(adj));
						if(adj.F < o.F)
						{
							open.Remove(o);
							open.Add(adj);
						}
					}
				}
				if(open.Count > 512)
				{
					Debug.LogWarning("Big Path");
					break;
				}
			}
			if (last.node != dst)
				return null;
			var cur = last;
			if (cur == null)
				return null;
			List<Vector3> path = new List<Vector3>();
			do
			{
				path.Add(Map.ActiveMap[cur.node.conduitPos].SurfacePoint);
				cur = cur.src;
			} while (cur != null);
			path.Reverse();
			return path;
		}
	}

	public class ConduitNode
	{
		public readonly int maxConnections;
		public readonly int id;
		public HexCoords conduitPos;

		internal int[] _connections;

		public int ConnectionCount { get; private set; }
		public bool IsEmpty => ConnectionCount == 0;
		public bool IsFull => ConnectionCount == maxConnections;


		public ConduitNode(int id, HexCoords pos, int maxConnections = 6)
		{
			this.id = id;
			conduitPos = pos;
			this.maxConnections = maxConnections;
			_connections = new int[maxConnections];
			for (int i = 0; i < maxConnections; i++)
				_connections[i] = -1;
			ConnectionCount = 0;
		}

		internal void ConnectTo(ConduitNode node)
		{
			if (IsFull || node.IsFull)
				throw new Exception("One of the nodes are full");
			AddConnection(node.id);
			node.AddConnection(id);
		}

		public bool IsConnectedTo(ConduitNode node)
		{
			if (IsEmpty)
				return false;
			for (int i = 0; i < maxConnections; i++)
			{
				if (_connections[i] == node.id)
					return true;
			}
			return false;
		}

		public void DisconnectFrom(ConduitNode node)
		{
			if (IsEmpty)
				throw new Exception($"This node is empty");
			if (!IsConnectedTo(node))
				throw new Exception($"This node[{id}] is not connected to [{node.id}]");
			RemoveConnection(node.id);
			node.RemoveConnection(id);
		}

		private void RemoveConnection(int nodeId)
		{
			for (int i = 0; i < maxConnections; i++)
			{
				if(_connections[i] == nodeId)
				{
					_connections[i] = -1;
					ConnectionCount--;
					break;
				}
			}
		}

		private void AddConnection(int nodeId)
		{
			for (int i = 0; i < maxConnections; i++)
			{
				if (_connections[i] == -1)
				{
					_connections[i] = nodeId;
					ConnectionCount++;
					break;
				}
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is ConduitNode r)
				return r.id == id;
			return false;
		}

		public override int GetHashCode()
		{
			return id;
		}
	}
}
