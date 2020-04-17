using Amatsugu.Phos.ECS.Jobs.Pathfinder;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using static Amatsugu.Phos.ECS.Jobs.Pathfinder.PathFinder;

public class UnitMovementSystem : ComponentSystem
{
	private float _tileEdgeLength;
	private float _innerRadius;
	private Camera _cam;
	private int _mapWidth;
	private EntityQuery EntityQuery;

	private NativeHashMap<HexCoords, float> _navData;
	//private Dictionary<int, NativeList<HexCoords>> _paths;
	private NativeList<PathNode> _open;
	private NativeHashMap<PathNode, float> _closed;
	private NativeHashMap<PathNode, PathNode> _nodePairs;
	private bool _ready;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnMapLoaded += Init;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
	}

	void Init()
	{
		_ready = true;
		_navData = Map.ActiveMap.GenerateNavData();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
		_innerRadius = Map.ActiveMap.innerRadius;
		//_paths = new Dictionary<int, NativeList<HexCoords>>();
		_open = new NativeList<PathNode>(Allocator.Persistent);
		_closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.Persistent);
		_nodePairs = new NativeHashMap<PathNode, PathNode>(_navData.Count(), Allocator.Persistent);
		GameEvents.OnMapLoaded -= Init;
		GameEvents.OnMapChanged += OnMapChanged;
	}

	void OnMapChanged()
	{
		Map.ActiveMap.GenerateNavData(ref _navData);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameEvents.OnMapChanged -= OnMapChanged;
		_navData.Dispose();
	}

	protected override void OnStopRunning()
	{
	}

	protected override void OnUpdate()
	{
		if (!_ready)
			return;
		///Caluclate Paths
		Entities.WithNone<PathProgress, Path>().ForEach((Entity e, ref Translation t, ref Destination d, ref UnitId id) =>
		{
			var p = GetPath(t.Value, d.Value, ref _navData, Map.ActiveMap.innerRadius, ref _open, ref _closed, ref _nodePairs);
			if (p == null)
			{
				Debug.LogWarning("Path Null");
				return;
			}
			PostUpdateCommands.AddSharedComponent(e, new Path { Value = p });
			PostUpdateCommands.AddComponent(e, new PathProgress
			{
				Progress = p.Count - 1
			});
		});

		//Move to Target
		Entities.WithNone<Path>().ForEach((Entity e, ref Translation c, ref MoveToTarget m, ref AttackTarget t, ref AttackRange range) =>
		{
			var tPos = EntityManager.GetComponentData<CenterOfMass>(t.Value);
			var distSq = math.lengthsq(c.Value - tPos.Value);
			if (distSq <= range.ValueSq)
				return;
			if(EntityManager.HasComponent<Destination>(e))
			{
				PostUpdateCommands.SetComponent(e, new Destination { Value = tPos.Value });
			}else
				PostUpdateCommands.AddComponent(e, new Destination { Value = tPos.Value });
		});

		//Follow path to Target
		Entities.WithAll<MoveToTarget>().ForEach((Entity e, ref Translation t, ref AttackRange range, ref AttackTarget tgt, ref PathProgress pathId, ref Destination d) =>
		{
			var tPos = EntityManager.GetComponentData<CenterOfMass>(tgt.Value);
			if(!tPos.Value.Equals(d.Value))
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.SetComponent(e, new Destination { Value = tPos.Value });
				Debug.Log($"<b>{nameof(UnitMovementSystem)}</b>: Target Moved, Recalculating Path");
				return;
			}
			var dst = math.lengthsq(t.Value - tPos.Value);
			Debug.Log(dst);
			if (dst <= range.ValueSq)
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				Debug.Log($"<b>{nameof(UnitMovementSystem)}</b>: Target in Range");
			}
		});

		//Follow Paths
		Entities.ForEach((Entity e, Path path, ref UnitId id, ref Rotation rot, ref Translation t, ref PathProgress pathId, ref MoveSpeed speed) =>
		{

			//Path Complete
			if (pathId.Progress < 0)
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var dst = Map.ActiveMap[path.Value[pathId.Progress]].SurfacePoint;

			dst.y = t.Value.y;
			t.Value = Vector3.MoveTowards(t.Value, dst, Time.DeltaTime * speed.Value);

			var unit = Map.ActiveMap.units[id.Value];
			PostUpdateCommands.SetComponent(unit.HeadEntity, t);
			t.Value.y = Map.ActiveMap[HexCoords.FromPosition(t.Value)].Height;
			//Next Point
			if (t.Value.Equals(dst))
			{
				pathId.Progress--;
				if (pathId.Progress >= 0)
				{
					var lc = Map.ActiveMap[path.Value[pathId.Progress]].SurfacePoint;
					lc.y = t.Value.y;
					rot.Value = quaternion.LookRotation(t.Value - lc, new float3(0, 1, 0));
					PostUpdateCommands.SetComponent(unit.HeadEntity, rot);
				}
			}
		});
	}
}