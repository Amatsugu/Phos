using Amatsugu.Phos.Tiles;

using DataStore.ConduitGraph;
using Effects.Lines;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class IndicatorManager
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
			Map.EM.AddComponent(curEntity, typeof(FrozenRenderSceneTag));
		}
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

	/*public void UnSetIndicator(HexCoords tilePos)
	{
		if (_renderedIndicators.ContainsKey(tilePos))
			_EM.DestroyEntity(_renderedIndicators[tilePos]);
	}*/

	public void UnSetAllIndicators()
	{
		_EM.DestroyEntity(_entities);
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
					_EM.RemoveComponent<FrozenRenderSceneTag>(_indicatorEntities[indicatorMesh][i]);

				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint + _offset });
			}
			else
			{
				if (i >= _renderedEntities[indicatorMesh])
					break;
				if (i < _renderedEntities[indicatorMesh])
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(FrozenRenderSceneTag));
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
					_EM.RemoveComponent<FrozenRenderSceneTag>(_indicatorEntities[line][i]);

				var pos = nodes[j].conduitPos.world + new float3(0, nodes[j].height, 0);
				LineFactory.UpdateStaticLine(_indicatorEntities[line][i], src, pos, thiccness);
				c++;
			}
			else
			{
				if (i >= _renderedEntities[line])
					break;
				if (i < _renderedEntities[line])
					_EM.AddComponent(_indicatorEntities[line][i], typeof(FrozenRenderSceneTag));
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
			_EM.AddComponent(_indicatorEntities[indicator][i], typeof(FrozenRenderSceneTag));
		}
		_renderedEntities[indicator] = 0;
	}

	public void HideAllIndicators()
	{
		foreach (var indicators in _indicatorEntities)
		{
			HideIndicator(indicators.Key);
		}
	}

}
