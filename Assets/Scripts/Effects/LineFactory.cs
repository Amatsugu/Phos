using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Effects.Lines
{
	public static class LineFactory
	{
		public static Entity CreateLine(MeshEntityRotatable line, Vector3 a, Vector3 b)
		{
			var e = line.Instantiate(a, Vector3.one);
			var em = World.Active.EntityManager;
			em.AddComponentData(e, new LineSegment
			{
				Start = a,
				End = b,
			});
			return e;
		}

		public static Entity CreateStaticLine(MeshEntityRotatable line, Vector3 a, Vector3 b, float thiccness = 0.1f)
		{
			var (t,s,r) = PrepareLine(a, b, thiccness);
			return line.Instantiate(t, s, r);
		}

		public static (float3 translation, float3 scale, Quaternion rotation) PrepareLine(float3 a, float3 b, float thiccness)
		{
			var dir = b - a;
			return (a, new float3(thiccness, thiccness, Vector3.Magnitude(dir)), Quaternion.LookRotation(dir, Vector3.up));
		}
	}
}
