using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

using UnityEditor;

using UnityEngine;

[MaterialProperty("_BaseColor", MaterialPropertyFormat.Float4, (-1))]
public struct HDRPMateiralColor : IComponentData
{
	public float4 Value;
}
