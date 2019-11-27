using DataStore.ConduitGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PowerTransferEffectSystem : ComponentSystem
{
	public MeshEntityRotatable energyPacket;
	private ConduitGraph _conduitGraph;
	private Dictionary<int, List<Vector3>> _effectPaths;
	private HashSet<int> _removalList;
	private static PowerTransferEffectSystem _INST;

	protected override void OnCreate()
	{
		base.OnCreate();
		_INST = this;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		EventManager.AddEventListener("OnHQPlaced", OnHQ);
		_effectPaths = new Dictionary<int, List<Vector3>>();
		_removalList = new HashSet<int>();
	}

	private void OnHQ()
	{
		_conduitGraph = Map.ActiveMap.conduitGraph;
		_conduitGraph.OnNodeRemoved += OnNodeRemoved;
		EventManager.RemoveEventListener("OnHQPlaced", OnHQ);
	}

	private void OnNodeRemoved(ConduitNode node)
	{
		if (_effectPaths.ContainsKey(node.id))
		{
			_effectPaths.Remove(node.id);
			_removalList.Add(node.id);
		}
		var keys = _effectPaths.Keys.ToArray();
		for (int i = 0; i < keys.Length; i++)
		{
			OnNodeAdded(_conduitGraph.nodes[keys[i]]);
		}
	}

	public static void AddNode(ConduitNode node)
	{
		_INST.OnNodeAdded(node);
	}

	public static void RemoveNode(ConduitNode node)
	{
		_INST.OnNodeRemoved(node);
	}

	private void OnNodeAdded(ConduitNode node)
	{
		var path = _conduitGraph.GetPath(node);
		if (path == null)
			return;
		if (_effectPaths.ContainsKey(node.id))
			_effectPaths[node.id] = path;
		else
			_effectPaths.Add(node.id, path);
		for (int i = 1; i < path.Count; i++)
		{
			Debug.DrawLine(path[i-1], path[i], Color.magenta, 5);
		}
	}

	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, ref EnergyPacket ep, ref Translation t, ref Rotation r) =>
		{
			if (!_effectPaths.ContainsKey(ep.id))
			{
				if(_removalList.Contains(ep.id))
				{
					PostUpdateCommands.DestroyEntity(e);
					_removalList.Remove(ep.id);
				}
				return;
			}
			var path = _effectPaths[ep.id];
			if(ep.progress == -1 || ep.progress >= path.Count)
			{
				ep.progress = 0;
				t.Value = path[0];
			}
			t.Value = Vector3.MoveTowards(t.Value, path[ep.progress], 10 * Time.DeltaTime);
			if (ep.progress < path.Count)
			{
				if((Vector3)t.Value == path[ep.progress])
				{
					ep.progress++;
					if(ep.progress < path.Count)
						r.Value = Quaternion.LookRotation((Vector3)t.Value - path[ep.progress], Vector3.up);
				}
			}
			if(ep.progress >= path.Count)
			{
				ep.progress = 0;
				t.Value = path[0];
			}
		});
	}
}

public struct EnergyPacket : IComponentData
{
	public int id;
	public int progress;
}
