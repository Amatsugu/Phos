using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

using UnityEngine;

[MaterialProperty("_ConstructProgress", MaterialPropertyFormat.Float)]
public struct ConstructionProgress : IComponentData
{
	public float Value;
}

[MaterialProperty("_ConstructOffset", MaterialPropertyFormat.Float)]
public struct ConstructionOffset : IComponentData
{
	public float Value;
}

[MaterialProperty("_ConstructHeight", MaterialPropertyFormat.Float)]
public struct ConstructionHeight : IComponentData
{
	public float Value;
}

[MaterialProperty("_StartTime", MaterialPropertyFormat.Float)]
public struct ConstructionStart : IComponentData
{
	public float Value;
}

[MaterialProperty("_ConstructDuration", MaterialPropertyFormat.Float)]
public struct ConstructionDuration : IComponentData
{
	public float Value;
}
