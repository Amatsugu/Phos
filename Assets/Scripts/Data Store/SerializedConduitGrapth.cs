using DataStore.ConduitGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Rendering;

namespace Amatsugu.Phos.DataStore
{
	[Serializable]
	public class SerializedConduitGrapth
	{
		public int maxConnections;
		public ConduitNode[] nodes;
		public ConduitNode baseNode;
		public int curId;

		public ConduitGraph Deserialize()
		{
			var g = new ConduitGraph(baseNode, maxConnections);
			for (int i = 0; i < nodes.Length; i++)
			{
				var node = nodes[i];
				g.nodes.Add(node.id, node);
				g._coordMap.Add(node.conduitPos, node.id);
				g.SetID(curId);
			}
			return g;
		}
	}
}
