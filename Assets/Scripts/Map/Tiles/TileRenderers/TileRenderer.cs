using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class TileRenderer : ScriptableObject
{
	public MeshEntity entity;

	public abstract void Render(Tile tile, Entity parent);
	public abstract void UpdateHeight(Tile tile, Entity parent);
}
