using AnimationSystem.AnimationData;
using AnimationSystem.Animations;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.ECS
{
	public class AnimatedMeshEntity : MeshEntityRotatable
	{
		public enum AnimationType
		{
			Slider,
			Rotate
		}

		public AnimationType animationType;
		public float duration;
		[ConditionalHide("animationType", 1)]
		public float3 rotationAxis;
		[ConditionalHide("animationType", 1)]
		public float rotationSpeed;
		public AnimationCurve animationCurve;
		[ConditionalHide("animationType", 0)]
		public float3 maxOffset;

		public override IEnumerable<ComponentType> GetComponents()
		{
			var comps = base.GetComponents();
			switch(animationType)
			{
				case AnimationType.Slider:
					return comps.Concat(new ComponentType[] { typeof(Slider), typeof(AnimationPhase), typeof(AnimStartPos), typeof(AnimEndPos) });
				default:
					return comps;
			}
		}

		public override void PrepareDefaultComponentData(Entity entity)
		{
			base.PrepareDefaultComponentData(entity);
			var em = Map.EM;
			switch (animationType)
			{
				case AnimationType.Slider:
					em.SetSharedComponentData(entity, new Slider
					{
						duration = duration,
						animationCurve = animationCurve
					});
					break;
			}
		}

		public override Entity Instantiate(float3 position, float3 scale)
		{
			var entity = base.Instantiate(position, scale);
			var em = Map.EM;
			switch (animationType)
			{
				case AnimationType.Slider:
					em.SetComponentData(entity, new AnimStartPos
					{
						Value = position
					});
					em.SetComponentData(entity, new AnimEndPos
					{
						Value = position + maxOffset
					});
					break;
			}
			return entity;
		}
	}
}
