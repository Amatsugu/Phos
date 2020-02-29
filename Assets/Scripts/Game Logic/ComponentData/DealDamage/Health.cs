using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Health : IComponentData
{
	public float Value;
	public float maxHealth;
}

public struct Damage : IComponentData
{
	public float Value;
}