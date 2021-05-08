using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class ConduitLineRendererSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<RecalculateConduitsTag, MapTag>().ForEach((Entity e) =>
			{
				var visitedItems = new HashSet<int>();
				var nodes = GameRegistry.GameMap.conduitGraph.nodes.Values.ToArray();
				for (int i = 0; i < nodes.Length; i++)
				{
					var curNode = nodes[i];
					for (int j = 0; j < curNode.ConnectionCount; j++)
					{
						var curCID = curNode._connections[j];
						if (visitedItems.Contains(curCID))
							continue;
						visitedItems.Add(curCID);
					}
				}
			});
		}
	}
}
