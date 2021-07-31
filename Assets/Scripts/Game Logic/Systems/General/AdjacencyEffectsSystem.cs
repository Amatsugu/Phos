using System.Collections;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class AdjacencyEffectsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<BuildingId>().WithAll<BuildingBonusInitTag>().ForEach((Entity e, ref BuildingId building) =>
			{

			});
		}
	}
}
