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
		private ConduitNode _baseNode;

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
		}

		public bool ContainsNode(HexCoords nodePos) => _coordMap.ContainsKey(nodePos);

		public void AddNode(HexCoords nodePos, ConduitNode connectTo)
		{
			var newNode = CreateNode(nodePos);
			newNode.ConnectTo(connectTo);
		}

		ConduitNode CreateNode(HexCoords nodePos)
		{
			var id = _curId++;
			var newNode = new ConduitNode(id, nodePos, maxConnections);
			nodes.Add(id, newNode);
			_coordMap.Add(nodePos, id);
			return newNode;
		}

		public void AddNodeDisconected(HexCoords nodePos) => CreateNode(nodePos);

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

		public ConduitNode[] GetNodesInRange(HexCoords nodePos, float rangeSq, bool excludeFull = true)
		{
			var nodesInRange = new ConduitNode[maxConnections];
			var pos = _coordMap.Keys.ToArray();
			var j = 0;
			for (int i = 0; i < _coordMap.Count; i++)
			{
				if (j >= maxConnections)
					break;
				var n = nodes[_coordMap[pos[i]]];
				if (excludeFull && n.IsFull)
					continue;
				if (n.conduitPos.DistanceToSq(nodePos) <= rangeSq)
					nodesInRange[j++] = n;
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
