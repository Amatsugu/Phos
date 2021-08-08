using Amatsugu.Phos.DataStore;

using System;

using Unity.Collections;

using UnityEngine;

namespace Amatsugu.Phos
{
	[CreateAssetMenu(menuName = "Game Data/Adjacency Effects")]
	public class AdjacenecyDatabase : ScriptableObject
	{
		public AdjacencyDefination[] definedBonuses;

		public Runtime ToRuntime()
		{
			var runtime = new Runtime(definedBonuses);

			return runtime;
		}

		public class Runtime : IDisposable
		{
			private NativeHashMap<Key, StatsBuffs> _adjancencyEffects;

			public Runtime(AdjacencyDefination[] definedBonuses)
			{
				_adjancencyEffects = new NativeHashMap<Key, StatsBuffs>(definedBonuses.Length, Allocator.Persistent);
				for (int i = 0; i < definedBonuses.Length; i++)
				{
					var curBonus = definedBonuses[i];
					for (int j = 0; j < curBonus.receivers.Length; j++)
						for (int k = 0; k < curBonus.providers.Length; k++)
							_adjancencyEffects.Add((curBonus.receivers[j], curBonus.providers[k]), curBonus.buff);
				}
			}

			public void Dispose()
			{
				if (!_adjancencyEffects.IsCreated)
					return;
				_adjancencyEffects.Dispose();
				_adjancencyEffects = default;
			}

			public StatsBuffs GetAdjancencyEffect(BuildingIdentifier a, BuildingIdentifier b)
			{
				if (_adjancencyEffects.ContainsKey((a, b)))
					return _adjancencyEffects[(a, b)];
				return StatsBuffs.Empty;
			}

			public StatsBuffs GetAdjancencyEffect(BuildingId a, BuildingId b)
			{
				if (_adjancencyEffects.ContainsKey((a, b)))
					return _adjancencyEffects[(a, b)];
				return StatsBuffs.Empty;
			}

			public StatsBuffs GetAdjancencyEffect(int a, int b)
			{
				if (_adjancencyEffects.ContainsKey((a, b)))
					return _adjancencyEffects[(a, b)];
				return StatsBuffs.Empty;
			}

			public struct Key : IEquatable<Key>
			{
				public int a;
				public int b;

				public static implicit operator Key((int a, int b) t) => new() { a = t.a, b = t.b };

				public static implicit operator Key((BuildingIdentifier a, BuildingIdentifier b) t) => new() { a = t.a.id, b = t.b.id };

				public static implicit operator Key((BuildingId a, BuildingId b) t) => new() { a = t.a.Value, b = t.b.Value };

				public bool Equals(Key other) => a == other.a && b == other.b;

				public override bool Equals(object obj)
				{
					if (obj is null)
						return false;
					if (obj is Key k)
						return Equals(k);
					return false;
				}

				public override int GetHashCode() => HashCode.Combine(a, b);
			}
		}
	}

	[Serializable]
	public struct AdjacencyDefination
	{
		public BuildingIdentifier[] receivers;
		public BuildingIdentifier[] providers;
		public StatsBuffs buff;
	}
}