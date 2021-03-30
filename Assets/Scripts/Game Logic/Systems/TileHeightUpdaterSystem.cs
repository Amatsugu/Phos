using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class TileHeightUpdaterSystem : ComponentSystem
	{
		private NativeHashMap<HexCoords, float> _tilesToUpdate;

		protected override void OnCreate()
		{
			base.OnCreate();
			GameRegistry.INST.tileHeightUpdaterSystem = this;
			_tilesToUpdate = new NativeHashMap<HexCoords, float>(128, Allocator.Persistent);
		}

		protected override void OnUpdate()
		{
			if (_tilesToUpdate.Count() == 0)
				return;
			Entities.WithAllReadOnly<HexPosition, TileTag>().ForEach((Entity e, ref Translation pos, ref HexPosition hexPos) =>
			{
				if (_tilesToUpdate.ContainsKey(hexPos.Value))
					pos.Value = new float3(pos.Value.x, _tilesToUpdate[hexPos.Value], pos.Value.z);
			});

			_tilesToUpdate.Clear();
		}

		public void UpdateTileHeight(HexCoords pos, float height)
		{
			if (_tilesToUpdate.ContainsKey(pos))
				_tilesToUpdate[pos] = height;
			else
				_tilesToUpdate.Add(pos, height);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_tilesToUpdate.Dispose();
		}
	}
}
