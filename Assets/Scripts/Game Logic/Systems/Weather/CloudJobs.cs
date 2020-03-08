using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[ExcludeComponent(typeof(ShadowOnlyTag))]
public struct CloudsJob : IJobForEach<CloudData, Translation, NonUniformScale>
{
	public float size;

	[ReadOnly]
	public NativeArray<float2> field;

	public float3 camPos;
	public float3 rawCamPos;
	public float disolveDist;
	public float disolveUpper;
	public float disolveLower;

	public void Execute(ref CloudData c, ref Translation t, ref NonUniformScale s)
	{
		t.Value = c.pos + camPos;

		var cloudSize = field[c.index].x;
		var cloudHeight = field[c.index].y;

		var vDist = t.Value.y - rawCamPos.y;
		var disolve = Vector3.SqrMagnitude(new float3(t.Value.x, 0, t.Value.z) - new float3(rawCamPos.x, 0, rawCamPos.z)) / disolveDist;
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
		s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
	}
}

[RequireComponentTag(typeof(ShadowOnlyTag))]
public struct CloudShadowsJob : IJobForEach<CloudData, Translation, NonUniformScale>
{
	public float size;

	[ReadOnly]
	public NativeArray<float2> field;

	public float3 camPos;

	public void Execute(ref CloudData c, ref Translation t, ref NonUniformScale s)
	{
		t.Value = c.pos + camPos;

		var cloudSize = field[c.index].x;
		var cloudHeight = field[c.index].y;
		s.Value = new float3(size * cloudSize, cloudHeight, size * cloudSize);
	}
}