using AnimationSystem.AnimationData;
using AnimationSystem.Animations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AnimationSystem
{
	public class SimpleAnimationSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			//settup animations
			Entities.ForEach((Entity e, ref FallAnim f) =>
			{
				PostUpdateCommands.AddComponent(e, new Velocity { Value = f.startSpeed });
				PostUpdateCommands.RemoveComponent<FallAnim>(e);
			});

			//callbacks
			Entities.ForEach((Entity e, ref Floor f, ref HitFloorCallback floorCallback, ref Translation t) =>
			{
				if (t.Value.y <= f.Value)
				{
					EventManager.InvokeEvent(floorCallback.eventId.ToString());
					PostUpdateCommands.DestroyEntity(e);
				}
			});
		}
	}


	[UpdateBefore(typeof(SimpleAnimationSystem))]
	public class SimpleAnimationJobSystem : JobComponentSystem
	{
		public struct GravityJob : IJobForEach<Gravity, Velocity>
		{
			public float dt;
			public void Execute(ref Gravity g, ref Velocity v)
			{
				v.Value += new float3(0, -g.Value * dt, 0);
			}
		}

		public struct VelocityJob : IJobForEach<Velocity, Translation>
		{
			public float dt;
			public void Execute(ref Velocity v, ref Translation t)
			{
				t.Value += v.Value * dt;
			}
		}

		public struct FloorJob : IJobForEach<Floor, Translation> //TODO Optimize this
		{
			public void Execute(ref Floor f, ref Translation t)
			{
				if (t.Value.y <= f.Value)
					t.Value.y = f.Value;
			}
		}

		public struct RotateJob : IJobForEach<RotateAxis, RotateSpeed, Rotation>
		{
			public float dt;

			public void Execute(ref RotateAxis axis, ref RotateSpeed speed, ref Rotation rot)
			{
				rot.Value = Quaternion.Euler(((Quaternion)rot.Value).eulerAngles + (axis.Value * speed.Value));
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var gravityJob = new GravityJob
			{
				dt = Time.deltaTime
			};
			var dep = gravityJob.Schedule(this, inputDeps);
			var velocityJob = new VelocityJob
			{
				dt = Time.deltaTime
			};
			dep = velocityJob.Schedule(this, dep);
			var floorJob = new FloorJob();
			dep = floorJob.Schedule(this, dep);

			var rotJob = new RotateJob { dt = Time.deltaTime };
			dep = rotJob.Schedule(this, dep);

			return dep;
		}
	}
}

namespace AnimationSystem.AnimationData
{
	public struct Gravity : IComponentData
	{
		public float Value;
	}

	public struct Velocity : IComponentData
	{
		public float3 Value;
	}

	public struct Fall : IComponentData
	{
		public float Value;
	}

	public struct Floor : IComponentData
	{
		public float Value;
	}

	public struct HitFloorCallback : IComponentData
	{
		public int eventId;
	}

	public struct AnimEndCallback : IComponentData
	{
		public int eventId;
	}

	public struct AnimEndTag : IComponentData
	{
	}
}

namespace AnimationSystem.Animations
{
	public struct FallAnim : IComponentData
	{
		public float3 startSpeed;
	}

	public struct RotateAxis : IComponentData
	{
		public Vector3 Value;
	}

	public struct RotateSpeed : IComponentData
	{
		public float Value;
	}
}


