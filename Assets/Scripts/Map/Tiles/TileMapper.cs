using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileMapper : ScriptableObject
{
	public abstract TileInfo GetTile(float sample, float seaLevel, float maxValue = 1);

}
