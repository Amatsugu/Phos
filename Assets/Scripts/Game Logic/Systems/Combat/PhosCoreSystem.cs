using AnimationSystem.AnimationData;
using Effects.Lines;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PhosCoreSystem : ComponentSystem
{

	private MeshEntityRotatable _bullet;
	private bool _isReady = false;
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
			_isReady = true;
		};
	}

	protected override void OnUpdate()
	{
		if (!_isReady)
			return;
		Entities.WithNone<Disabled>().ForEach((Entity e, ref PhosCore core, ref HexPosition p) =>
		{
			var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2);
			PostUpdateCommands.SetComponent(core.ring, new Rotation { Value = quaternion.AxisAngle(Vector3.up, baseAngle + (math.PI * 2) / 12f) });
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{

				var t = Map.ActiveMap[p.coords];
				var targetPoint = t.SurfacePoint + UnityEngine.Random.insideUnitSphere * core.targetingRange;
				targetPoint = Map.ActiveMap[HexCoords.FromPosition(targetPoint)].SurfacePoint;
				for (int i = 0; i < 6; i++)
				{
					var curAngle = baseAngle + (math.PI / 3) * i;
					var dir = math.rotate(quaternion.RotateY(curAngle), Vector3.forward);
					Debug.DrawRay(t.SurfacePoint + new Vector3(0, 10, 0), dir, Color.magenta);
					var proj = _bullet.BufferedInstantiate(PostUpdateCommands, (float3)t.SurfacePoint + (dir * 1.9f) + new float3(0, 2.25916f, 0), Vector3.one * .4f);
					dir.y = .4f;
					PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 5 });
					PostUpdateCommands.AddComponent(proj, new Velocity { Value = dir * core.projectileSpeed });
					//PostUpdateCommands.AddComponent(proj, new Gravity { Value = 9.8f });
					PostUpdateCommands.AddComponent(proj, new PhosProjectile
					{
						targetTime = Time.ElapsedTime + core.targetDelay + (i * (1/12f)),
						target = targetPoint,
						flightSpeed = core.projectileSpeed * 10
					});

				}
				core.nextVolleyTime = Time.ElapsedTime + core.fireRate;
			}
		});
	}

	public class PhosProjectileSystem : JobComponentSystem
	{
		struct PhosProjectileJob : IJobForEachWithEntity<PhosProjectile, Velocity, Translation>
		{
			public EntityCommandBuffer.Concurrent CMB;
			public double curTime;

			public void Execute(Entity entity, int index, ref PhosProjectile proj, ref Velocity vel, ref Translation t)
			{
				if (curTime >= proj.targetTime)
				{
					CMB.RemoveComponent(index, entity, typeof(PhosProjectile));
					CMB.RemoveComponent(index, entity, typeof(Gravity));
					vel.Value = math.normalize(proj.target - t.Value) * proj.flightSpeed;
					LineFactory.UpdateStaticLine(CMB, index, entity, t.Value, proj.target);

				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new PhosProjectileJob 
			{ 
				CMB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent(),
				curTime = Time.ElapsedTime
			};
			inputDeps = job.Schedule(this, inputDeps);
			inputDeps.Complete();
			return inputDeps;
		}
	}


	public struct PhosCore : IComponentData
	{
		public float spinRate;
		public float fireRate;
		public double nextVolleyTime;
		public float projectileSpeed;
		public float targetDelay;
		public float targetingRange;
		public Entity ring;
	}

	public struct PhosProjectile : IComponentData
	{
		public double targetTime;
		public float flightSpeed;
		public float3 target;
	}
}