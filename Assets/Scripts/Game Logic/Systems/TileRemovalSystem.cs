using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Collections;

using UnityEngine;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	[BurstCompatible]
	public class TileRemovalSystem : ComponentSystem
	{
		private NativeHashMap<HexCoords, float> _deletionQueue;

		protected override void OnCreate()
		{
			base.OnCreate();
			_deletionQueue = new NativeHashMap<HexCoords, float>(20, Allocator.Persistent);
			GameRegistry.INST.tileRemovalSystem = this;
		}

		protected override void OnUpdate()
		{
			if (_deletionQueue.Count() == 0)
				return;
			Entities.WithAllReadOnly<HexPosition, TileVersion, TileTag>().ForEach((Entity e, ref HexPosition pos, ref TileVersion version) =>
			{
				if(_deletionQueue.ContainsKey(pos.Value) && version.Value < _deletionQueue[pos.Value])
					PostUpdateCommands.DestroyEntity(e);
			});
			_deletionQueue.Clear();
		}

		public void MarkForRemoval(HexCoords coords, float age)
		{
			if (_deletionQueue.ContainsKey(coords))
				_deletionQueue[coords] = age;
			else
				_deletionQueue.Add(coords, age);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_deletionQueue.Dispose();
		}

		private struct DeletionOrder
		{
			public float age;
			public HexCoords coord;

			public DeletionOrder(HexCoords coords, float age) : this()
			{
				coord = coords;
				this.age = age;
			}
		}
	}
}
