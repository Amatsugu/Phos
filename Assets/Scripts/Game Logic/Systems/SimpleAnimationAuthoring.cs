using AnimationSystem.AnimationData;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace AnimationSystem.Animations
{
	public class SimpleAnimationAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		public enum AnimationType
		{
			Rotation,
			Slider
		}

		public AnimationType animationType;

		//Rotation
		public float3 axis = Vector3.up;

		public float speed = 1;

		//Slider
		public AnimationCurve animationCurve;

		public float duration;
		public float animationPhase;
		public bool timeRelativePhase;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			switch (animationType)
			{
				case AnimationType.Rotation:
					dstManager.AddComponentData(entity, new RotateSpeed { Value = speed });
					dstManager.AddComponentData(entity, new RotateAxis { Value = axis });
					break;

				case AnimationType.Slider:
					dstManager.AddSharedComponentData(entity, new Slider { duration = duration, animationCurve = animationCurve });
					dstManager.AddComponentData(entity, new AnimStartPos { Value = float3.zero });
					dstManager.AddComponentData(entity, new AnimEndPos { Value = new float3(0, .5f, 0) });
					if (timeRelativePhase)
						dstManager.AddComponentData(entity, new AnimationPhase { Value = Time.time + animationPhase });
					else
						dstManager.AddComponentData(entity, new AnimationPhase { Value = animationPhase });
					break;
			}
		}
	}
}