using Amatsugu.Phos.UnitComponents;

using AnimationSystem.AnimationData;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Amatsugu.Phos
{
	//[BurstCompile]
	public class BoidUnitMovementSystem : JobComponentSystem
	{
		private struct BoidJob : IJobChunk
		{
			[ReadOnly] internal ComponentTypeHandle<MoveSpeed> moveSpeedType;
			[ReadOnly] internal ComponentTypeHandle<Destination> destinationType;
			public ComponentTypeHandle<Translation> translationType;
			public ComponentTypeHandle<Velocity> velocityType;
			public ComponentTypeHandle<Rotation> rotationType;
			[ReadOnly] public NativeHashMap<HexCoords, float> navData;
			public float dt;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var translations = chunk.GetNativeArray(translationType);
				var velocities = chunk.GetNativeArray(velocityType);
				var rotations = chunk.GetNativeArray(rotationType);
				var speeds = chunk.GetNativeArray(moveSpeedType);
				var destinations = chunk.GetNativeArray(destinationType);

				var flightHeight = new float3(0, 6, 0);

				for (int i = 0; i < chunk.Count; i++)
				{
					var v = velocities[i];

					var totalVelocity = v.Value;

					translations[i] = GroupColision(translations[i]);

					totalVelocity += Flocking(i, ref translations, chunk.Count) * dt;

					totalVelocity += CollisionAvoidance(i, ref translations, chunk.Count) * dt;

					//totalVelocity += MatchSpeed(i, ref velocities, chunk.Count) * dt;

					//totalVelocity += ForwardMovement(speeds[i], rotations[i]) * dt;
					totalVelocity += TendToPoint(translations[i], destinations[i].Value + flightHeight) * dt;

					totalVelocity += TendToAltitude(translations[i], 6) * dt;
					if (!totalVelocity.Equals(float3.zero))
						totalVelocity = LimitVelocity(totalVelocity, speeds[i].Value);

					v.Value = totalVelocity;

					velocities[i] = v;

					if (!totalVelocity.Equals(0))
					{
						rotations[i] = new Rotation
						{
							Value = quaternion.LookRotationSafe(-totalVelocity, math.up())
						};
					}
				}
			}

			private Translation GroupColision(Translation translation)
			{
				var coord = HexCoords.FromPosition(translation.Value);
				var ground = navData.ContainsKey(coord) ? navData[coord] : translation.Value.y;
				if (translation.Value.y < ground)
					translation.Value.y = ground;
				return translation;
			}

			private float3 ForwardMovement(MoveSpeed speed, Rotation rotation)
			{
				var fwd = new float3(0, 0, 1);
				fwd = math.rotate(rotation.Value, fwd);

				fwd *= speed.Value * 1;
				return fwd;
			}

			private float3 TendToAltitude(Translation t, float altitude)
			{
				var coord = HexCoords.FromPosition(t.Value);
				var ground = navData.ContainsKey(coord) ? navData[coord] : altitude;
				var tAlt = new float3(t.Value.x, altitude + ground, t.Value.z);
				if (t.Value.y > tAlt.y)
					return (tAlt - t.Value) * 2f;
				else if (t.Value.y < tAlt.y)
					return (tAlt - t.Value) * .5f;
				else
					return 0;
			}

			private float3 LimitVelocity(float3 velocity, float limit)
			{
				if (math.lengthsq(velocity) > limit * limit)
					return (math.normalizesafe(velocity) * limit);
				else
					return velocity;
			}

			private float3 TendToPoint(Translation translation, float3 center)
			{
				var dir = (center - translation.Value);
				if (math.lengthsq(dir) > 2 * 2)
					return dir * .3f;
				else
					return 0;
			}

			private float3 MatchSpeed(int j, ref NativeArray<Velocity> velocities, int count)
			{
				float3 avgVel = float3.zero;

				for (int i = 0; i < count; i++)
				{
					if (i != j)
						avgVel += velocities[i].Value;
				}

				avgVel /= count - 1;
				return (avgVel - velocities[j].Value) / 8;
			}

			private float3 CollisionAvoidance(int j, ref NativeArray<Translation> translations, int count)
			{
				var c = float3.zero;
				for (int i = 0; i < count; i++)
				{
					if (i != j)
					{
						if (math.lengthsq(translations[i].Value - translations[j].Value) < 3 * 3)
							c -= (translations[i].Value - translations[j].Value);
					}
				}
				return c;
			}

			private float3 Flocking(int j, ref NativeArray<Translation> translations, int count)
			{
				float3 avgPos = float3.zero;

				if (count <= 1)
					return avgPos;

				for (int i = 0; i < count; i++)
				{
					if (i != j)
						avgPos += translations[i].Value;
				}

				avgPos /= count - 1;
				return (avgPos - translations[j].Value) * .1f;
			}
		}

		private EntityQuery _entityQuery;
		private NativeHashMap<HexCoords, float> _navData;

		protected override void OnCreate()
		{
			base.OnCreate();
			var desc = new EntityQueryDesc
			{
				All = new[]
				{
					ComponentType.ReadOnly<Translation>(),
					ComponentType.ReadOnly<MoveSpeed>(),
					ComponentType.ReadOnly<Destination>(),
					ComponentType.ReadOnly<UnitDomain.Air>(),
					typeof(Rotation),
					typeof(Velocity),
				},
			};

			_entityQuery = GetEntityQuery(desc);
			//_navData = new NativeHashMap<HexCoords, float>(GameRegistry.GameMap.totalWidth * GameRegistry.GameMap.totalWidth, Allocator.Persistent);
			GameEvents.OnMapLoaded += OnMapChanged;
			GameEvents.OnMapChanged += OnMapChanged;
		}

		private void OnMapChanged()
		{
			if (_navData.IsCreated)
				_navData.Dispose();
			_navData = GameRegistry.GameMap.GenerateNavData(false);
		}

		protected override void OnDestroy()
		{
			if (_navData.IsCreated)
				_navData.Dispose();
			GameEvents.OnMapLoaded -= OnMapChanged;
			GameEvents.OnMapChanged -= OnMapChanged;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var boidsJob = new BoidJob
			{
				rotationType = GetComponentTypeHandle<Rotation>(false),
				translationType = GetComponentTypeHandle<Translation>(false),
				velocityType = GetComponentTypeHandle<Velocity>(false),
				moveSpeedType = GetComponentTypeHandle<MoveSpeed>(true),
				destinationType = GetComponentTypeHandle<Destination>(true),
				navData = _navData,
				dt = Time.DeltaTime
			};
			inputDeps = boidsJob.Schedule(_entityQuery, inputDeps);

			return inputDeps;
		}
	}
}