using Amatsugu.Phos.ECS.Jobs.Pathfinder;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

public class UnitMovementSystem : ComponentSystem
{
	private float _tileEdgeLength;
	private float _innerRadius;
	private Camera _cam;
	private int _mapWidth;
	private EntityQuery EntityQuery;

	private NativeHashMap<HexCoords, float> _navData;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		if (Map.ActiveMap == null)
			return;
		_navData = Map.ActiveMap.GenerateNavData();
		_tileEdgeLength = Map.ActiveMap.tileEdgeLength;
		_cam = GameRegistry.Camera;
		_mapWidth = Map.ActiveMap.width;
		_innerRadius = Map.ActiveMap.innerRadius;
	}

	protected override void OnStopRunning()
	{
		_navData.Dispose();
	}

	/*protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		if (!_navData.IsCreated)
			return inputDeps;
		var buffer = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent();
		var job = new PathFinderJob(_tileEdgeLength, _innerRadius, buffer, _navData, paths);
		inputDeps = job.Schedule(this, inputDeps);
		handle.Complete();
		paths = job.paths;
		return inputDeps;
	}*/

	protected override void OnUpdate()
	{
		Entities.ForEach((Entity e, Path path, ref UnitId id, ref Rotation rot, ref Translation t, ref PathProgress pathId, ref MoveSpeed speed) =>
		{
			if (pathId.Progress >= path.Value.Length)
			{
				PostUpdateCommands.RemoveComponent<PathProgress>(e);
				path.Value.Dispose();
				PostUpdateCommands.RemoveComponent<Path>(e);
				PostUpdateCommands.RemoveComponent<Destination>(e);
				return;
			}
			var dst = Map.ActiveMap[path.Value[pathId.Progress]].SurfacePoint;

			dst.y = t.Value.y;
			t.Value = Vector3.MoveTowards(t.Value, dst, Time.DeltaTime * speed.Value);
			var unit = Map.ActiveMap.units[id.Value];
			t.Value.y = Map.ActiveMap[unit.Coords].Height;
			unit.UpdatePos(t.Value);
			if (t.Value.Equals(dst))
			{
				pathId.Progress++;
				return;
			}
			rot.Value = quaternion.LookRotation(t.Value - dst, new float3(0, 1, 0));
		});
	}
}