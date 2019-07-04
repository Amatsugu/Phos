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
		public static Entity CreateLine(MeshEntity line, Vector3 a, Vector3 b)
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
	}
}
