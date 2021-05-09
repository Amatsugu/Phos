using DataStore.ConduitGraph;

using Effects.Lines;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{

	[UpdateAfter(typeof(MapConversionSystem))]
	public class ConduitLineRendererConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ConduitLinesAuthoring lines) =>
			{
				var entity = GetPrimaryEntity(lines);
				var prefabs = GameRegistry.PrefabDatabase;
				PostUpdateCommands.AddComponent(entity, new ConduitLinePrefabs
				{
					active = GetPrimaryEntity(lines.activeLine),
					inactive = GetPrimaryEntity(lines.inactiveLine)
				});
			});
		}
	}

	[UpdateAfter(typeof(PowerTransferSystem))]
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class ConduitLineRendererSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<RenderConduitLinesTag, MapTag, ConduitLinePrefabs>().ForEach((Entity e, ref ConduitLinePrefabs lines) =>
			{
				var existingConnections = new HashSet<ConduitConnection>();
				var nodesDict = GameRegistry.GameMap.conduitGraph.nodes;
				var nodes = GameRegistry.GameMap.conduitGraph.nodes.Values.ToArray();
				for (int i = 0; i < nodes.Length; i++)
				{
					var curNode = nodes[i];
					for (int j = 0; j < curNode.ConnectionCount; j++)
					{
						var curCID = curNode._connections[j];	
						var bNode = nodesDict[curCID];
						var thisConnection = new ConduitConnection(curNode, bNode);
						if (existingConnections.Contains(thisConnection))
							continue;
						existingConnections.Add(thisConnection);
						var a = curNode.conduitPos.WorldPos;
						a.y = curNode.height;
						var b = bNode.conduitPos.WorldPos;
						b.y = bNode.height;
						Entity line = default;
						if (curNode.IsConnected)
							line = PostUpdateCommands.Instantiate(lines.active);
						else
							line = PostUpdateCommands.Instantiate(lines.inactive);
						LineFactory.UpdateStaticLine(PostUpdateCommands, line, a, b);
					}
				}
				PostUpdateCommands.RemoveComponent<RenderConduitLinesTag>(e);
			});

		}
	}



	public struct ConduitConnection : IEquatable<ConduitConnection>
	{
		public int a, b;

		public ConduitConnection(ConduitNode a, ConduitNode b)
		{
			this.a = a.id;
			this.b = b.id;
		}

		public override bool Equals(object obj)
		{
			if (obj is ConduitConnection other)
				return other.Equals(this);
			return false;
		}

		public bool Equals(ConduitConnection other) => ((a == other.a) && (b == other.b)) || ((a == other.b) && (b == other.a));

		public override int GetHashCode()
		{
			int hashCode = 2118541809;
			hashCode ^= a;
			hashCode ^= b;
			return hashCode;
		}

		public static bool operator ==(ConduitConnection left, ConduitConnection right) => left.Equals(right);

		public static bool operator !=(ConduitConnection left, ConduitConnection right) => !(left == right);
	}

	public struct ConduitLinePrefabs : IComponentData
	{
		public Entity active;
		public Entity inactive;
	}
}