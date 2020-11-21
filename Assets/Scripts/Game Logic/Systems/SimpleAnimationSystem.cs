using AnimationSystem.AnimationData;
using AnimationSystem.Animations;

using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
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
			Entities.WithNone<BuildingOffTag>().ForEach((Entity e, ref FallAnim f) =>
			{
				PostUpdateCommands.AddComponent(e, new Velocity { Value = f.startSpeed });
				PostUpdateCommands.RemoveComponent<FallAnim>(e);
			});

			var curTime = UnityEngine.Time.time;
			//Thumper
			Entities.WithNone<BuildingOffTag>().ForEach((Slider th, ref AnimationPhase phase, ref AnimStartPos start, ref AnimEndPos end,  ref Translation t) =>
			{
				var time = ((curTime + phase.Value) % th.duration) / th.duration;
				var p = th.animationCurve.Evaluate(time);
				t.Value = math.lerp(start.Value, end.Value, p);
			});

			//callbacks
			Entities.WithNone<BuildingOffTag>().ForEach((Entity e, ref Floor f, ref HitFloorCallback floorCallback, ref Translation t) =>
			{
				if (t.Value.y <= f.Value)
				{
					GameEvents.InvokeOnAnimationEvent(floorCallback.eventId);
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
		public struct GravityJob : IJobChunk
		{
			public ComponentTypeHandle<Velocity> velocityType;
			[ReadOnly] public ComponentTypeHandle<Gravity> gravityType;
			public float dt;

			public void Execute(ref Gravity g, ref Velocity v)
			{
				v.Value += new float3(0, -g.Value * dt, 0);
			}

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var vel = chunk.GetNativeArray(velocityType);
				var grav = chunk.GetNativeArray(gravityType);

				for (int i = 0; i < chunk.Count; i++)
				{
					var curVel = vel[i];
					curVel.Value += new float3(0, -grav[i].Value * dt, 0);
					vel[i] = curVel;
				}
			}
		}

		[BurstCompile]
		public struct VelocityJob : IJobChunk
		{
			public ComponentTypeHandle<Translation> translationType;
			[ReadOnly] public ComponentTypeHandle<Velocity> velocityType;
			public float dt;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var vel = chunk.GetNativeArray(velocityType);
				var trans = chunk.GetNativeArray(translationType);

				for (int i = 0; i < chunk.Count; i++)
				{
					var curT = trans[i];
					curT.Value += vel[i].Value * dt;
					trans[i] = curT;
				}
			}
		}

		[BurstCompile]
		public struct FloorJob : IJobChunk //IJobForEach<Floor, Translation> //TODO Optimize this
		{
			[ReadOnly]
			public ComponentTypeHandle<Floor> floorType;
			public ComponentTypeHandle<Translation> transType;

			public void Execute(ref Floor f, ref Translation t)
			{
				if (t.Value.y <= f.Value)
					t.Value.y = f.Value;
			}

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var f = chunk.GetNativeArray(floorType);
				var t = chunk.GetNativeArray(transType);
				for (int i = 0; i < chunk.Count; i++)
				{
					var p = t[i];
					var fl = f[i].Value;
					if (p.Value.y < fl)
					{
						p.Value.y = fl;
						t[i] = p;
					}
				}
			}
		}

		[BurstCompile]
		public struct RotateJob : IJobChunk //IJobForEach<RotateAxis, RotateSpeed, Rotation>
		{
			public float dt;
			[ReadOnly] public ComponentTypeHandle<RotateAxis> axisType;
			[ReadOnly] public ComponentTypeHandle<RotateSpeed> speedType;
			public ComponentTypeHandle<Rotation> rotationType;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var axis = chunk.GetNativeArray(axisType);
				var speed = chunk.GetNativeArray(speedType);
				var rot = chunk.GetNativeArray(rotationType);

				for (int i = 0; i < chunk.Count; i++)
				{
					var r = rot[i].Value;
					rot[i] = new Rotation
					{
						Value = math.mul(math.normalizesafe(r), quaternion.AxisAngle(axis[i].Value, speed[i].Value * dt))
					};
				}
			}
		}

		[BurstCompile]
		public struct AccelerationJob : IJobChunk//IJobForEach<Velocity, Acceleration>
		{

			public ComponentTypeHandle<Velocity> velocityType;
			[ReadOnly] public ComponentTypeHandle<Acceleration> accelerationType;
			public float dt;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var vel = chunk.GetNativeArray(velocityType);
				var accel = chunk.GetNativeArray(accelerationType);

				for (int i = 0; i < chunk.Count; i++)
				{
					var curVel = vel[i];
					curVel.Value += accel[i].Value * dt;
					vel[i] = curVel;
				}
			}
		}

		private EntityQuery _rotateQuery;
		private EntityQuery _accelQuery;
		private EntityQuery _gravityQuery;
		private EntityQuery _velQuery;
		private EntityQuery _floorQuery;

		protected override void OnCreate()
		{
			base.OnCreate();
			var rotDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<RotateAxis>(),
					ComponentType.ReadOnly<RotateSpeed>(),
					typeof(Rotation)
				},
				None = new ComponentType[]
				{
					typeof(Disabled),
					typeof(FrozenRenderSceneTag),
					typeof(BuildingOffTag),
				}
			};
			_rotateQuery = GetEntityQuery(rotDesc);
			var accelDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<Acceleration>(),
					typeof(Velocity)
				},
				None = new ComponentType[]
				{
					typeof(Disabled),
					typeof(FrozenRenderSceneTag),
					typeof(BuildingOffTag),
				}
			};
			_accelQuery = GetEntityQuery(accelDesc);
			var gravityDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<Gravity>(),
					typeof(Velocity)
				},
				None = new ComponentType[]
				{
					typeof(Disabled),
					typeof(FrozenRenderSceneTag),
					typeof(BuildingOffTag),
				}
			};
			_gravityQuery = GetEntityQuery(gravityDesc);
			var velDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<Velocity>(),
					typeof(Translation)
				},
				None = new ComponentType[]
				{
					typeof(Disabled),
					typeof(FrozenRenderSceneTag),
					typeof(BuildingOffTag),
				}
			};
			_velQuery = GetEntityQuery(velDesc);

			var floorDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<Floor>(),
					typeof(Translation)
				},
				None = new ComponentType[]
				{
					typeof(Disabled),
					typeof(FrozenRenderSceneTag),
					typeof(BuildingOffTag),
				}
			};
			_floorQuery = GetEntityQuery(floorDesc);
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var v = GetComponentTypeHandle<Velocity>(false);
			var gravityJob = new GravityJob 
			{ 
				dt = Time.DeltaTime,
				gravityType = GetComponentTypeHandle<Gravity>(true),
				velocityType = v
			};
			var dep = gravityJob.Schedule(_gravityQuery, inputDeps);

			var accelJob = new AccelerationJob 
			{ 
				dt = Time.DeltaTime,
				accelerationType = GetComponentTypeHandle<Acceleration>(true),
				velocityType = v
			};
			dep = accelJob.Schedule(_accelQuery, dep);

			var t = GetComponentTypeHandle<Translation>(false);
			var velocityJob = new VelocityJob 
			{
				dt = Time.DeltaTime,
				velocityType = GetComponentTypeHandle<Velocity>(true),
				translationType = t
			};
			dep = velocityJob.Schedule(_velQuery, dep);

			var floorJob = new FloorJob
			{
				floorType = GetComponentTypeHandle<Floor>(true),
				transType = t
			};
			dep = floorJob.Schedule(_floorQuery, dep);

			var rotJob = new RotateJob 
			{ 
				dt = Time.DeltaTime,
				axisType = GetComponentTypeHandle<RotateAxis>(true),
				speedType = GetComponentTypeHandle<RotateSpeed>(true),
				rotationType = GetComponentTypeHandle<Rotation>(false),
			};
			dep = rotJob.Schedule(_rotateQuery, dep);

			return dep;
		}
	}
}

namespace AnimationSystem.AnimationData
{

	public struct AnimationPhase : IComponentData
	{
		public float Value;
	}

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
		public AnimationCurve animationCurve;

		public bool Equals(Slider other)
		{
			return duration == other.duration &&
				animationCurve.Equals(other.animationCurve);
		}

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + duration.GetHashCode();
			hash = hash * 31 + animationCurve.GetHashCode();
			return hash;
		}
	}

	public struct AnimStartPos : IComponentData
	{
		public float3 Value;
	}

	public struct AnimEndPos : IComponentData
	{
		public float3 Value;
	}
}