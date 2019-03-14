using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator : ScriptableObject
{
	public Vector2 Size = new Vector2(20, 20);
	public TileMapper tileMapper;
	public float seaLevel = 4;
	public float edgeLength = 1;
	public FeatureGenerator[] featureGenerators;
	[HideInInspector]
	public bool Regen;

	public float InnerRadius => Mathf.Sqrt(3f) / 2f * edgeLength;
	public abstract Tile3D Generate(int x, int y, Transform parent = null);
	
	public Tile3D CreateTile(int x, int z, float height, Transform parent)
	{
		return CreateTile(tileMapper.GetTile(0, seaLevel), x, z, height, parent);
	}

	public Tile3D CreateTile(TileInfo tileInfo , int x, int z, float height, Transform parent)
	{
		var tile = new Tile3D(HexCoords.FromOffsetCoords(x, z, edgeLength), height, tileInfo);
		return tile;
		//var g = Instantiate(t, GetPosition(x, y), Quaternion.identity, parent);
		//return g.GetComponent<Tile>().SetPos(x, y);
	}

	public void GenerateFeatures(Map<Tile3D> map)
	{
		if (featureGenerators == null)
			return;
		foreach (var fg in featureGenerators)
		{
			if (fg != null)
			{
				Debug.Log("Running Feature Generator: " + fg.GetType().Name);
				fg.Generate(map);
			}
		}
	}

	public abstract void PaintTile(Tile3D tile);

	public virtual Map<Tile3D> GenerateMap(Transform parent = null)
	{
		Map<Tile3D> map = new Map<Tile3D>((int)Size.y, (int)Size.x, parent, edgeLength);
		for (int z = 0; z < map.Height; z++)
		{
			for (int x = 0; x < map.Width; x++)
			{
				var coords = HexCoords.FromOffsetCoords(x, z, edgeLength);
				map[coords] = Generate(x, z, parent);
			}
		}
		return map;
	}
}
