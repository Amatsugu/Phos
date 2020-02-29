using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DealDamage : IComponentData
{
	public float damage;
	public Faction src;
}
