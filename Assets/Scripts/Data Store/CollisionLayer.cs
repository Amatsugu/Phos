using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos
{
	[Flags]
    public enum CollisionLayer
	{
		Default = 0,
		Player = 1 << 1,
		Phos = 1 << 2,
		Projectile = 1 << 3,
		Tile = 1 << 4,
		Unit = 1 << 5,
		Building = 1 << 6,
		Ground = 1 << 7,
		Air = 1 << 8,
		Naval = 1 << 9
	}
}
