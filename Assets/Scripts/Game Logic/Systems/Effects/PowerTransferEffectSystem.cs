using DataStore.ConduitGraph;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PowerTransferEffectSystem : ComponentSystem
{
	private ConduitGraph _conduitGraph;
	private Dictionary<int, List<Vector3>> _effectPaths;
	private int progress = 0;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		EventManager.AddEventListener("OnHQPlaced", OnHQ);
		_effectPaths = new Dictionary<int, List<Vector3>>();
	}

	private void OnHQ()
	{
		_conduitGraph = Map.ActiveMap.conduitGraph;
		_conduitGraph.OnNodeAdded += OnNodeAdded;
		_conduitGraph.OnNodeRemoved += OnNodeRemoved;
		EventManager.RemoveEventListener("OnHQPlaced", OnHQ);
	}

	private void OnNodeRemoved(ConduitNode node)
	{
		_effectPaths.Remove(node.id);
	}

	private void OnNodeAdded(ConduitNode node)
	{
		_effectPaths.Add(node.id, _conduitGraph.GetPath(node));
	}

	protected override void OnUpdate()
	{
		bool needReset = true;
		foreach (var path in _effectPaths)
		{
			if (path.Value == null)
				continue;
			if(progress < path.Value.Count)
			{
				needReset = false;
				Debug.DrawRay(path.Value[progress], Vector3.up * 5, Color.red);
				progress++;
			}
		}
		if (needReset)
			progress = 0;
		Entities.ForEach((Entity e, ref HexPosition c) =>
		{

		});
	}
}
