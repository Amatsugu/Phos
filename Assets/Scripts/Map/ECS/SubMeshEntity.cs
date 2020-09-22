using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.ECS
{
	[Serializable]
	public struct SubMeshEntry
	{
		public float3 offset;
		public MeshEntityRotatable mesh;
	}

	[Serializable]
	public struct SubMeshIdentifier
	{
		public int id;
	}
}
