using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Amatsugu.Phos.ECS
{
	public class TurretSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackRange range) =>
			{
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				r = math.mul(math.normalizesafe(r), quaternion.AxisAngle(math.up(), 10 * Time.DeltaTime));
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = r
				});

				var fwd = math.rotate(r, new float3(0, 0, 1));



			});
		}
	}

	public struct Turret : IComponentData
	{
		internal Entity Head;
	}
}
