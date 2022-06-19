using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using DataStore.ConduitGraph;
using Effects.Lines;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class IndicatorManager : IDisposable
{
	public TMP_Text floatingText;

	private Dictionary<MeshEntity, List<Entity>> _indicatorEntities;
	private Dictionary<MeshEntity, int> _renderedEntities;
	private Dictionary<HexCoords, int> _renderedIndicators;
	private NativeArray<Entity> _entities;
	private EntityManager _EM;
	private float3 _offset;
	private int _nextEntityIndex = 0;
	private List<string> _errors;
	private bool disposedValue;

	public IndicatorManager(EntityManager entityManager, float offset, TMP_Text floatingText, int maxIndicator = 1024)
	{
		_EM = entityManager;
		_indicatorEntities = new Dictionary<MeshEntity, List<Entity>>();
		_renderedEntities = new Dictionary<MeshEntity, int>();
		_renderedIndicators = new Dictionary<HexCoords, int>();
		_errors = new List<string>();
		_offset = new float3(0, offset, 0);
		_entities = new NativeArray<Entity>(maxIndicator, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		this.floatingText = floatingText;
	}


	private void GrowIndicators(MeshEntity indicatorMesh, int count)
	{
		List<Entity> entities;
		if (!_indicatorEntities.ContainsKey(indicatorMesh))
		{
			_indicatorEntities.Add(indicatorMesh, entities = new List<Entity>());
			_renderedEntities.Add(indicatorMesh, 0);
		}
		else
		{
			entities = _indicatorEntities[indicatorMesh];
			if (count <= entities.Count)
				return;
		}
		var curSize = entities.Count;
		for (int i = curSize; i < count; i++)
		{
			Entity curEntity;
			entities.Add(curEntity = indicatorMesh.Instantiate(Vector3.zero, Vector3.one * .9f));
			GameRegistry.EntityManager.AddComponent(curEntity, typeof(DisableRendering));
		}
	}

	public static void ShowHexRange(Tile center, int range, MeshEntityRotatable border)
	{
		var ring = HexCoords.SelectRing(center.Coords, range);
		var neighbors = new Tile[6];
		for (int i = 0; i < ring.Length; i++)
		{
			var p = ring[i];
			var s = center.map[p].SurfacePoint;
			center.map.GetNeighbors(p, ref neighbors);
			for (int n = 0; n < 6; n++)
			{
				if (neighbors[n].Coords.Distance(center.Coords) <= range)
					continue;
				var e = border.Instantiate(s, 1, quaternion.RotateY(math.radians((60 * n) + 180)));
				GameRegistry.EntityManager.AddComponentData(e, new DeathTime { Value = Time.time + .01f });
			}
		}
	}

	public static void ShowRangeSphere(Tile tile, float attackRange, MeshEntity rangeSphere)
	{
		var e = rangeSphere.Instantiate(tile.SurfacePoint, attackRange);
		GameRegistry.EntityManager.AddComponentData(e, new DeathTime { Value = Time.time + 0.01f });
	}

	public void SetIndicator(Tile tile, MeshEntity indicator)
	{
		if (_renderedIndicators.ContainsKey(tile.Coords))
		{
			var i = _renderedIndicators[tile.Coords];
			_EM.DestroyEntity(_entities[i]);
			_entities[i] = indicator.Instantiate(tile.SurfacePoint + _offset, 0.9f);
		}
		_renderedIndicators[tile.Coords] = _nextEntityIndex;
		_entities[_nextEntityIndex++] = indicator.Instantiate(tile.SurfacePoint + _offset, 0.9f);
	}

	public void SetIndicator(Tile tile, GameObject indicator)
	{
		var entityId = GameRegistry.PrefabDatabase[indicator];
		var buffer = GameRegistry.GetGenericPrefabBuffer();
		var prefabEntity = buffer[entityId];
		var curInstance = GameRegistry.EntityManager.Instantiate(prefabEntity.value);
		GameRegistry.EntityManager.SetComponentData(curInstance, new Translation
		{
			Value = tile.SurfacePoint + _offset
		});
		GameRegistry.EntityManager.AddComponentData(curInstance, new Scale
		{
			Value = 0.9f
		});

		if (_renderedIndicators.ContainsKey(tile.Coords))
		{
			var i = _renderedIndicators[tile.Coords];
			_EM.DestroyEntity(_entities[i]);
			_entities[i] = curInstance;
		}
		_renderedIndicators[tile.Coords] = _nextEntityIndex;
		_entities[_nextEntityIndex++] = curInstance;
	}

	/*public void UnSetIndicator(HexCoords tilePos)
	{
		if (_renderedIndicators.ContainsKey(tilePos))
			_EM.DestroyEntity(_renderedIndicators[tilePos]);
	}*/

	public void UnSetAllIndicators()
	{
		if(_nextEntityIndex != 0)
		{
			for (int i = 0; i < _nextEntityIndex; i++)
			{
				if(_EM.Exists(_entities[i]))
					_EM.DestroyEntity(_entities[i]);
			}
		}
		_renderedIndicators.Clear();
		_errors.Clear();
		_nextEntityIndex = 0;
	}

	public void LogError(string errorMessage) => _errors.Add(errorMessage);

	public void PublishAndClearErrors()
	{
		for (int i = 0; i < _errors.Count; i++)
			NotificationsUI.Notify(NotifType.Error, _errors[i]);
		_errors.Clear();
	}

	public void ShowIndicators(MeshEntity indicatorMesh, List<Tile> tiles)
	{
		GrowIndicators(indicatorMesh, tiles.Count);
		for (int i = 0; i < _indicatorEntities[indicatorMesh].Count; i++)
		{
			if (i < tiles.Count)
			{
				if (i >= _renderedEntities[indicatorMesh])
					_EM.RemoveComponent<DisableRendering>(_indicatorEntities[indicatorMesh][i]);

				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint + _offset });
			}
			else
			{
				if (i >= _renderedEntities[indicatorMesh])
					break;
				if (i < _renderedEntities[indicatorMesh])
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(DisableRendering));
			}
		}
		_renderedEntities[indicatorMesh] = tiles.Count;
	}

	public void ShowLines(MeshEntityRotatable line, Vector3 src, List<ConduitNode> nodes, float thiccness = 0.1f)
	{
		GrowIndicators(line, nodes.Count);
		int c = 0;
		for (int i = 0, j = 0; i < _indicatorEntities[line].Count; i++, j++)
		{
			if (j < nodes.Count)
			{
				if (i >= _renderedEntities[line])
					_EM.RemoveComponent<DisableRendering>(_indicatorEntities[line][i]);

				var pos = nodes[j].conduitPos.WorldPos + new float3(0, nodes[j].height, 0);
				LineFactory.UpdateStaticLine(_indicatorEntities[line][i], src, pos, thiccness);
				c++;
			}
			else
			{
				if (i >= _renderedEntities[line])
					break;
				if (i < _renderedEntities[line])
					_EM.AddComponent(_indicatorEntities[line][i], typeof(DisableRendering));
			}
		}
		_renderedEntities[line] = c;
	}


	public void HideIndicator(MeshEntity indicator)
	{
		if (!_indicatorEntities.ContainsKey(indicator))
			return;
		for (int i = 0; i < _renderedEntities[indicator]; i++)
		{
			_EM.AddComponent(_indicatorEntities[indicator][i], typeof(DisableRendering));
		}
		_renderedEntities[indicator] = 0;
	}

	public void HideAllIndicators()
	{
		foreach (var indicators in _indicatorEntities)
		{
			HideIndicator(indicators.Key);
		}
		floatingText.gameObject.SetActive(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)

			}
			if(_entities.IsCreated)
				_entities.Dispose();
			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~IndicatorManager()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
