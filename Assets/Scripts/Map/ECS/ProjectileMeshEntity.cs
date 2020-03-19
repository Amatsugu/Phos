﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Assets.Scripts.Map.ECS
{
	public class ProjectileMeshEntity : DynamicMeshEntity
	{
		public Faction faction;

		public Entity Instantiate(float3 position, float scale, float3 velocity = default, float3 angularVelocity = default)
		{
			var rot = (velocity.Equals(default) ? quaternion.identity : quaternion.LookRotation(velocity, new float3(0,1,0)));
			var e = Instantiate(position, rot, scale, velocity, angularVelocity);
			return e;
		}

		public Entity BufferedInstantiate(EntityCommandBuffer cmb, float3 position, float scale, float3 velocity = default, float3 angularVelocity = default)
		{
			var rot = (velocity.Equals(default) ? quaternion.identity : quaternion.LookRotation(velocity, new float3(0, 1, 0)));
			var e = BufferedInstantiate(cmb, position, rot, scale, velocity, angularVelocity);
			return e;
		}

		protected override PhysicsMass GetMass()
		{
			return PhysicsMass.CreateKinematic(MassProperties.UnitSphere);
		}

		protected override CollisionFilter GetFilter() => new CollisionFilter
		{
			BelongsTo = (1u << (int)faction),
			CollidesWith = ~((1u << (int)Faction.PhosProjectile) | (1u << (int)Faction.PlayerProjectile)),
			GroupIndex = 0
		};
	}
}