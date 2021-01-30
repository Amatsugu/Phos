using System;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.UnitComponents
{
	public struct MoveSpeed : IComponentData
	{
		public float Value;
	}

	public struct Heading : IComponentData
	{
		public float3 Value;
	}

	public struct Destination : IComponentData
	{
		public float3 Value;
	}

	public struct UnitId : IComponentData
	{
		public int Value;
	}

	public struct UnitHead : IComponentData
	{
		public Entity Value;
	}

	public struct NextTile : IComponentData
	{
		public float3 Value;
	}

	public struct PathLine
	{
		public const float vLineGrad = 1e5f;

		public float gradient;
		public float yIntercept;
		public float gradientPerpendicular;

		private float3 _p1, _p2;

		private readonly bool _approachSide;

		public PathLine(float3 point, float3 perpendicular)
		{
			var d = point - perpendicular;

			if (d.x == 0)
				gradientPerpendicular = vLineGrad;
			else
				gradientPerpendicular = d.z / d.x;

			if (gradientPerpendicular == 0)
				gradient = vLineGrad;
			else
				gradient = -1 / gradientPerpendicular;

			yIntercept = point.z - gradient * point.x;

			_p1 = point;
			_p2 = point + new float3(1, 0, gradient);

			_approachSide = default;
			_approachSide = GetSide(perpendicular);
		}

		public bool GetSide(float3 p)
		{
			return (p.x - _p1.x) * (_p2.z - _p1.z) > (p.z - _p1.z) * (_p2.x - _p1.x);
		}

		public bool HasCrossedLine(float3 p)
		{
			return GetSide(p) != _approachSide;
		}

		public void DrawDebug(float length)
		{
			var dir = math.normalize(new float3(1, 0, gradient));
			var center = _p1 + math.up();
			center += new float3(0, GameRegistry.GameMap.GetHeight(center), 0);

			Debug.DrawLine(center - dir * length / 2f, center + dir * length / 3f, Color.magenta, 5);
		}
	}

	public struct Path : ISharedComponentData, IEquatable<Path>
	{
		public List<HexCoords> WayPoints;
		public PathLine[] turnBoundaries;
		public int finishIndex;

		private float3 _startPos;

		public Path(List<HexCoords> waypoints, float3 startPos, float turnDist)
		{
			WayPoints = waypoints;
			turnBoundaries = new PathLine[waypoints.Count];
			finishIndex = waypoints.Count - 1;
			_startPos = startPos;

			var prevPoint = startPos;
			for (int i = 0; i < waypoints.Count; i++)
			{
				var cPoint = waypoints[i].WorldPos;
				var dir = math.normalize(cPoint - prevPoint);
				var tBoundaryPoint = (i == finishIndex) ? cPoint : cPoint - dir * turnDist;
				turnBoundaries[i] = new PathLine(tBoundaryPoint, prevPoint - dir * turnDist);
				prevPoint = tBoundaryPoint;
			}
		}

		public void DrawDebug(float speed, float turnSpeed, float dt)
		{
			for (int i = 0; i < WayPoints.Count; i++)
			{
				var p = GameRegistry.GameMap.GetHeight(WayPoints[i].WorldPos);
				DebugUtilz.DrawCrosshair(WayPoints[i].WorldPos + new float3(0, p + 1, 0), .2f, Color.white, 5f);
				//turnBoundaries[i].DrawDebug(1);
			}
			var isSimulating = true;
			var pos = _startPos + math.up();
			var prevPoint = _startPos + math.up();
			var pathIndex = 0;
			var rot = WayPoints.Count >= 2 ? quaternion.LookRotation(WayPoints[1].WorldPos - WayPoints[0].WorldPos, math.up()) : quaternion.identity;
			while (isSimulating)
			{
				//pos.y = 0;
				while (turnBoundaries[pathIndex].HasCrossedLine(pos))
				{
					if (pathIndex == finishIndex)
					{
						isSimulating = false;
						break;
					}
					else
						pathIndex++;
				}

				var tgtPos = GameRegistry.GameMap[WayPoints[pathIndex]].SurfacePoint + math.up();
				var targetRot = quaternion.LookRotation(tgtPos - pos, math.up());

				var fwdRot = Quaternion.Lerp(rot, targetRot, dt * turnSpeed);
				var fwd = math.rotate(fwdRot, new float3(0, 0, 1));
				pos += fwd * speed * dt;
				//rot = math.mul(rot, quaternion.RotateY(math.radians(180)));
				//targetRot = math.mul(targetRot, q)

				//tgtPos.y = pos.y = 0;
				//targetRot = quaternion.LookRotation(tgtPos - pos, math.up());
				////targetRot = math.mul(targetRot, quaternion.RotateY(math.radians(180)));
				rot = Quaternion.Lerp(rot, targetRot, dt * turnSpeed);

				var tgtTile = GameRegistry.GameMap[HexCoords.FromPosition(pos)];
				if (tgtTile == null)
				{
					break;
				}
				var tgtHeight = tgtTile.SurfacePoint.y + 1;
				if (pos.y < tgtHeight)
					pos.y = tgtHeight;
				Debug.DrawLine(prevPoint, pos, Color.magenta, 5f);
				prevPoint = pos;
			}
		}

		public bool Equals(Path other)
		{
			return WayPoints == other.WayPoints;
		}

		public override int GetHashCode()
		{
			return WayPoints?.GetHashCode() ?? 0;
		}
	}

	public struct PathProgress : IComponentData
	{
		public int Delay;
		public int Progress;
	}
}