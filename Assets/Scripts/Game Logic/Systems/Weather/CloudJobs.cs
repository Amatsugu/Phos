using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[ExcludeComponent(typeof(ShadowOnlyTag))]
public struct CloudsJob : IJobChunk //IJobForEach<CloudData, Translation, NonUniformScale>
{
	public float size;

	[ReadOnly]
	public NativeArray<float2> field;
	[ReadOnly] public ComponentTypeHandle<CloudData> cloudType;
	public ComponentTypeHandle<Translation> translationType;
	public ComponentTypeHandle<NonUniformScale> scaleType;

	public float3 camPos;
	public float3 camCenteringOffset;
	public quaternion camRot;
	public float3 rawCamPos;
	public float disolveDist;
	public float disolveUpper;
	public float disolveLower;
	internal int gridSize;
	internal float innerRadius;

	public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
	{
		var cloudData = chunk.GetNativeArray(cloudType);
		var translations = chunk.GetNativeArray(translationType);
		var scales = chunk.GetNativeArray(scaleType);

		for (int i = 0; i < chunk.Count; i++)
		{
			var cloud = cloudData[i];
			var pos = translations[i];
			var scale = scales[i];

			pos.Value = math.rotate(camRot, cloud.pos) + HexCoords.SnapToGrid(camPos - camCenteringOffset, innerRadius, gridSize);

			var cloudSize = field[cloud.index].x;
			var cloudHeight = field[cloud.index].y;

			var vDist = pos.Value.y - rawCamPos.y;
			var disolve = Vector3.SqrMagnitude(new float3(pos.Value.x, 0, pos.Value.z) - new float3(rawCamPos.x, 0, rawCamPos.z)) / disolveDist;
			if (disolve <= 1)
			{
				if (vDist > -disolveLower && vDist < 0)
				{
					vDist = -vDist;
					var dT = vDist / disolveLower;
					dT = math.pow(dT, 8);
					disolve = math.lerp(disolve, 1, dT);
				}
				else if (vDist < disolveUpper && vDist >= 0)
				{
					var dT = (vDist / disolveUpper);
					dT = dT.Pow(8);
					disolve = math.lerp(disolve, 1, dT);
				}
				else
					disolve = 1;
				disolve -= .5f;
				disolve = math.clamp(disolve, 0, 1);
				disolve *= 2;
				disolve = disolve * disolve * disolve;
				cloudSize = math.lerp(0, cloudSize, disolve);
				cloudHeight = math.lerp(0, cloudHeight, disolve);
			}
			scale.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);

			translations[i] = pos;
			scales[i] = scale;

		}
	}
}

[RequireComponentTag(typeof(ShadowOnlyTag))]
public struct CloudShadowsJob : IJobChunk //IJobForEach<CloudData, Translation, NonUniformScale>
{
	public float size;

	[ReadOnly]
	public NativeArray<float2> field;

	public float3 camPos;
	public float3 camCenteringOffset;
	public quaternion camRot;

	[ReadOnly] public ComponentTypeHandle<CloudData> cloudType;
	public ComponentTypeHandle<Translation> translationType;
	public ComponentTypeHandle<NonUniformScale> scaleType;
	internal float innerRadius;
	internal int gridSize;

	public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
	{
		var shadows = chunk.GetNativeArray(cloudType);
		var translations = chunk.GetNativeArray(translationType);
		var scales = chunk.GetNativeArray(scaleType);

		for (int i = 0; i < chunk.Count; i++)
		{
			var c = shadows[i];
			var t = translations[i];
			var s = scales[i];
			t.Value = math.rotate(camRot, c.pos) + HexCoords.SnapToGrid(camPos - camCenteringOffset, innerRadius, gridSize);

			var cloudSize = field[c.index].x;
			var cloudHeight = field[c.index].y;
			s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);

			translations[i] = t;
			scales[i] = s;
		}
	}
}