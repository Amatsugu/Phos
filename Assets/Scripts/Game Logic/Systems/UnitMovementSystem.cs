using Amatsugu.Phos;
using Amatsugu.Phos.UnitComponents;

using Unity.Collections;
using Unity.Entities;
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
	private Map _map;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnMapLoaded += Init;
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
	}

	private void Init()
	{
		_map = GameRegistry.GameMap;
		_navData = _map.GenerateNavData();
		_tileEdgeLength = _map.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = _map.width;
		_innerRadius = _map.innerRadius;
		//_paths = new Dictionary<int, NativeList<HexCoords>>();
		_open = new NativeList<PathNode>(Allocator.Persistent);
		_closed = new NativeHashMap<PathNode, float>(MAX_PATH_LENGTH, Allocator.Persistent);
		_nodePairs = new NativeHashMap<PathNode, PathNode>(_navData.Count(), Allocator.Persistent);
		GameEvents.OnMapChanged += OnMapChanged;
		_ready = true;
	}

	private void OnMapChanged()
	{
		_map.GenerateNavData(ref _navData);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!_ready)
			return;
		_open.Dispose();
		_closed.Dispose();
		_nodePairs.Dispose();
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
		Entities.WithAllReadOnly<UnitDomain.Land>().WithNone<PathProgress, Path>().ForEach((Entity e, ref Translation t, ref Destination d, ref UnitId id) =>
		{
			var p = GetPath(t.Value, d.Value, ref _navData, _map.innerRadius, ref _open, ref _closed, ref _nodePairs);
			if (p == null)
			{
				Debug.LogWarning("Path Null");
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			PostUpdateCommands.AddSharedComponent(e, new Path { Value = p });
			PostUpdateCommands.AddComponent(e, new PathProgress
			{
				Progress = p.Count - 1
			});
		});
		Entities.WithAllReadOnly<UnitDomain.Naval>().WithNone<PathProgress, Path>().ForEach((Entity e, ref Translation t, ref Destination d, ref UnitId id) =>
		{
			var p = GetPath(t.Value, d.Value, ref _navData, _map.innerRadius, ref _open, ref _closed, ref _nodePairs, -1);
			if (p == null)
			{
				Debug.LogWarning("Path Null");
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			PostUpdateCommands.AddSharedComponent(e, new Path { Value = p });
			PostUpdateCommands.AddComponent(e, new PathProgress
			{
				Progress = p.Count - 1
			});
		});

		//Move to Target
		Entities.WithNone<Path, Destination>().WithAllReadOnly<MoveToTarget>().ForEach((Entity e, ref Translation c, ref AttackTarget t, ref AttackRange range) =>
		{
			var tPos = EntityManager.GetComponentData<CenterOfMass>(t.Value);
			if (!range.IsInRange(c.Value, tPos.Value))
				return;
			PostUpdateCommands.AddComponent(e, new Destination { Value = tPos.Value });
		});

		//Move to Target
		Entities.WithNone<Path>().WithAllReadOnly<MoveToTarget, Destination>().ForEach((Entity e, ref Translation c, ref AttackTarget t, ref AttackRange range) =>
		{
			var tPos = EntityManager.GetComponentData<CenterOfMass>(t.Value);
			if (!range.IsInRange(c.Value, tPos.Value))
				return;
			PostUpdateCommands.SetComponent(e, new Destination { Value = tPos.Value });
		});

		//Follow path to Target
		Entities.WithAllReadOnly<MoveToTarget>().WithAnyReadOnly<UnitDomain.Land, UnitDomain.Naval>().ForEach((Entity e, ref Translation t, ref AttackRange range, ref AttackTarget tgt, ref PathProgress pathId, ref Destination d) =>
		{
			var tPos = EntityManager.GetComponentData<CenterOfMass>(tgt.Value);
			if (!tPos.Value.Equals(d.Value))
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.SetComponent(e, new Destination { Value = tPos.Value });
				Debug.Log($"<b>{nameof(UnitMovementSystem)}</b>: Target Moved, Recalculating Path");
				return;
			}
			var dst = math.length(t.Value - tPos.Value);
			if (range.IsInRange(dst))
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				Debug.Log($"<b>{nameof(UnitMovementSystem)}</b>: Target in Range");
			}
		});

		//Follow Paths
		Entities.WithAnyReadOnly<UnitDomain.Land, UnitDomain.Naval>().ForEach((Entity e, Path path, ref UnitId id, ref Rotation rot, ref Translation t, ref PathProgress pathId, ref MoveSpeed speed) =>
		{
			//Path Complete
			var dst = _map[path.Value[pathId.Progress]].SurfacePoint;

			dst.y = t.Value.y;
			t.Value = Vector3.MoveTowards(t.Value, dst, Time.DeltaTime * speed.Value);
			if (pathId.Progress > 0)
			{
				var pointA = _map[path.Value[pathId.Progress - 1]].SurfacePoint;
				var pointB = dst;
				var dir = math.normalizesafe(pointB - pointA);
				dir.y = 0;
				var targetRot = quaternion.LookRotation(dir, math.up());
				rot.Value = Quaternion.RotateTowards(rot.Value, targetRot, 360 * Time.DeltaTime);
				//PostUpdateCommands.SetComponent(unit.HeadEntity, rot);
			}
			else
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			t.Value.y = _map[HexCoords.FromPosition(t.Value)].SurfacePoint.y;
			//Next Point
			if (t.Value.Equals(dst))
				pathId.Progress--;
		});

		//Follow Path Air
		//Entities.WithAnyReadOnly<UnitDomain.Air>().ForEach((ref Rotation rot, ref Translation t, ref MoveSpeed speed, ref Destination dst) =>
		//{
		//	var pos = Vector3.MoveTowards(t.Value, dst.Value, speed.Value * Time.DeltaTime);
		//	var p2 = pos;
		//	p2.y = _map[HexCoords.FromPosition(t.Value)].SurfacePoint.y + 6f;
		//	pos = Vector3.MoveTowards(pos, p2, speed.Value * Time.DeltaTime);
		//	t.Value = pos;

		//	var dir = (t.Value - dst.Value);
		//	dir.y = 0;
		//	rot.Value = quaternion.LookRotation(dir, math.up());
		//});


		Entities.ForEach((ref UnitHead head, ref Translation t) =>
		{
			PostUpdateCommands.SetComponent(head.Value, t);
		});

		Entities.WithNone<AttackTarget>().ForEach((ref UnitHead head, ref Rotation r) =>
		{
			PostUpdateCommands.SetComponent(head.Value, r);
		});

		Entities.ForEach((ref Translation t, ref UnitHead head, ref AttackTarget target) =>
		{
			var targetPos = EntityManager.GetComponentData<CenterOfMass>(target.Value);
			var dir = (t.Value - targetPos.Value);
			dir.y = 0;
			var rot = quaternion.LookRotation(dir, math.up());
			PostUpdateCommands.SetComponent(head.Value, new Rotation { Value = rot });
		});
	}
}