using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine.AddressableAssets;

namespace Amatsugu.Phos.ECS
{
	public class TurretSystem : ComponentSystem
	{

		private bool _isReady = false;
		private ProjectileMeshEntity _bullet;

		protected override void OnCreate()
		{
			base.OnCreate();
			var op = Addressables.LoadAssetAsync<ProjectileMeshEntity>("PlayerProjectile");
			op.Completed += e =>
			{
				if (e.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				{
					_bullet = e.Result;
					_isReady = true;
				}
			};
		}

		protected override void OnUpdate()
		{
			if (!_isReady)
				return;

			Entities.ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackRange range) =>
			{
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				r = math.mul(math.normalizesafe(r), quaternion.AxisAngle(math.up(), 10 * Time.DeltaTime));
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = r
				});

				var fwd = -math.rotate(r, new float3(0, 0, 1));


				var b  = _bullet.BufferedInstantiate(PostUpdateCommands, pos.Value + fwd * 2f, 0.1f, fwd * 10);

				PostUpdateCommands.AddComponent(b, new DeathTime { Value = Time.ElapsedTime + 5 });

			});
		}
	}

	public struct Turret : IComponentData
	{
		internal Entity Head;
	}
}
