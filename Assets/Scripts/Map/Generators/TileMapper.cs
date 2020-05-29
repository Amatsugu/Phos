using Amatsugu.Phos.TileEntities;

using UnityEngine;

public abstract class TileMapper : ScriptableObject
{
	public abstract TileEntity GetTile(float sample, float seaLevel, float maxValue = 1);
}