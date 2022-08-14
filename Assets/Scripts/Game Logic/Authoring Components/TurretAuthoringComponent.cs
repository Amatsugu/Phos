using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Amatsugu.Phos.ECS;

namespace Amatsugu.Phos
{
	[ExecuteInEditMode]
    public class TurretAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
		public GameObject head;
		public GameObject barrel;
		public Vector3 shotOffset;
		public GameObject projectile;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new Turret
			{
				Head = conversionSystem.GetPrimaryEntity(head),
				Barrel = conversionSystem.GetPrimaryEntity(barrel),
				projectile = conversionSystem.GetPrimaryEntity(projectile),
				shotOffset = shotOffset
			});
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			var basePose = head != null ? head.transform.localPosition: Vector3.zero;
			var fwd = head != null ? head.transform.forward : Vector3.zero;


			var pos = basePose + shotOffset;
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(pos + transform.position, 0.02f);

			if (head != null)
				pos = head.transform.localRotation * pos;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(pos + transform.position, 0.04f);

			if (barrel != null)
			{
				pos += barrel.transform.localPosition;
				fwd = barrel.transform.forward;
				pos = barrel.transform.localRotation * pos;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(pos + transform.position, 0.08f);

			pos += transform.position;

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(pos, 0.1f);
			Gizmos.DrawRay(pos, fwd);
		}
	}
}
