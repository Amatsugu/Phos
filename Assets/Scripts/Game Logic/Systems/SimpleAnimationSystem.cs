using AnimationSystem.AnimationData;
using AnimationSystem.Animations;
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AnimationSystem
{
	[BurstCompile]
	public class SimpleAnimationSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			//setup animations
			Entities.ForEach((Entity e, ref FallAnim f) =>
			{
				PostUpdateCommands.AddComponent(e, new Velocity { Value = f.startSpeed });
				PostUpdateCommands.RemoveComponent<FallAnim>(e);
			});

			var curTime = UnityEngine.Time.time;
			//Thumper
			Entities.ForEach((Slider th, ref Translation t) =>
			{
				var time = ((curTime + th.phase) % th.duration) / th.duration;
				var p = th.animationCurve.Evaluate(time);
				t.Value = math.lerp(th.basePos, th.maxPos, p);
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
	[BurstCompile]
	public class SimpleAnimationJobSystem : JobComponentSystem
	{
		[BurstCompile]
		public struct GravityJob : IJobForEach<Gravity, Velocity>
		{
			public float dt;
			public void Execute(ref Gravity g, ref Velocity v)
			{
				v.Value += new float3(0, -g.Value * dt, 0);
			}
		}

		[BurstCompile]
		public struct VelocityJob : IJobForEach<Velocity, Translation>
		{
			public float dt;
			public void Execute(ref Velocity v, ref Translation t)
			{
				t.Value += v.Value * dt;
			}
		}

		[BurstCompile]
		public struct FloorJob : IJobForEach<Floor, Translation> //TODO Optimize this
		{
			public void Execute(ref Floor f, ref Translation t)
			{
				if (t.Value.y <= f.Value)
					t.Value.y = f.Value;
			}
		}

		[BurstCompile]
		public struct RotateJob : IJobForEach<RotateAxis, RotateSpeed, Rotation>
		{
			public float dt;

			public void Execute(ref RotateAxis axis, ref RotateSpeed speed, ref Rotation rot)
			{
				rot.Value = Quaternion.Euler(((Quaternion)rot.Value).eulerAngles + (axis.Value * speed.Value * dt));
			}
		}

		[BurstCompile]
		public struct AccelerationJob : IJobForEach<Velocity, Acceleration>
		{
			public float dt;

			public void Execute(ref Velocity vel, ref Acceleration accel)
			{
				vel.Value += accel.Value * dt;
			}
		}

		[BurstCompile]
		public struct SeekTargetJob : IJobForEach<Translation, SeekTarget, Acceleration>
		{
			public float dt;

			public void Execute(ref Translation p, ref SeekTarget target, ref Acceleration a)
			{
				var moveDir = math.normalize(target.Value - p.Value);
				a.Value = moveDir * target.MaxAccel;
			}
		}

		[BurstCompile]
		public struct DragJob : IJobForEach<Drag, Velocity>
		{
			public DragJob(float dt)
			{
				this.dt = dt;
			}

			public readonly float dt;
			public const float airDensity = 1.2f;

			public void Execute(ref Drag drag, ref Velocity velocity)
			{
				
				velocity.Value -= drag.Value * velocity.Value * dt;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var gravityJob = new GravityJob { dt = Time.DeltaTime };
			var dep = gravityJob.Schedule(this, inputDeps);

			var seekJob = new SeekTargetJob();
			dep = seekJob.Schedule(this, dep);
			
			var accelJob = new AccelerationJob { dt = Time.DeltaTime };
			dep = accelJob.Schedule(this, dep);

			var dragJob = new DragJob(Time.DeltaTime);
			dep = dragJob.Schedule(this, dep);

			var velocityJob = new VelocityJob { dt = Time.DeltaTime };
			dep = velocityJob.Schedule(this, dep);

			var floorJob = new FloorJob();
			dep = floorJob.Schedule(this, dep);

			var rotJob = new RotateJob { dt = Time.DeltaTime };
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
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(Gravity left, Gravity right) => left.Equals(right);
		public static bool operator !=(Gravity left, Gravity right) => !(left == right);
	}

	public struct Velocity : IComponentData
	{
		public float3 Value;
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(Velocity left, Velocity right) => left.Equals(right);
		public static bool operator !=(Velocity left, Velocity right) => !(left == right);
	}

	public struct Drag : IComponentData
	{
		public float Value;
	}

	public struct Acceleration : IComponentData
	{
		public float3 Value;
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(Acceleration left, Acceleration right) => left.Equals(right);
		public static bool operator !=(Acceleration left, Acceleration right) => !(left == right);
	}

	public struct Fall : IComponentData
	{
		public float Value;
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(Fall left, Fall right) => left.Equals(right);
		public static bool operator !=(Fall left, Fall right) => !(left == right);
	}

	public struct Floor : IComponentData
	{
		public float Value;
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(Floor left, Floor right) => left.Equals(right);
		public static bool operator !=(Floor left, Floor right) => !(left == right);
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

	public struct SeekTarget : IComponentData
	{
		public float3 Value;
		public float MaxAccel;
	}

	public struct Slider : ISharedComponentData, IEquatable<Slider>
	{
		public float duration;
		public float phase;
		public AnimationCurve animationCurve;
		public float3 basePos;
		public float3 maxPos;

		public bool Equals(Slider other)
		{
			return duration == other.duration &&
				animationCurve.Equals(other.animationCurve) &&
				basePos.Equals(other.basePos) &&
				maxPos.Equals(other.maxPos) &&
				phase == other.phase;
		}

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + duration.GetHashCode();
			hash = hash * 31 + basePos.GetHashCode();
			hash = hash * 31 + maxPos.GetHashCode();
			hash = hash * 31 + phase.GetHashCode();
			hash = hash * 31 + animationCurve.GetHashCode();
			return hash;
		}
	}
}


