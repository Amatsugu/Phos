using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public abstract class MapGenerator : ScriptableObject
{
	public Vector2 Size = new Vector2(20, 20);
	public TileMapper tileMapper;
	public float seaLevel = 4;
	public float edgeLength = 1;
	public FeatureGenerator[] featureGenerators;
	public bool useJobs = true;
	[HideInInspector]
	public bool Regen;

	public float InnerRadius => Mathf.Sqrt(3f) / 2f * edgeLength;
	public abstract Tile3D Generate(int x, int y);
	
	public Tile3D CreateTile(int x, int z, float height)
	{
		return CreateTile(tileMapper.GetTile(0, seaLevel), x, z, height);
	}

	public Tile3D CreateTile(TileInfo tileInfo , int x, int z, float height)
	{
		var tile = new Tile3D(HexCoords.FromOffsetCoords(x, z, edgeLength), height, tileInfo);
		return tile;
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

	public abstract Tile3D PaintTile(Tile3D tile);

	public virtual Map<Tile3D> GenerateMap(Transform parent = null)
	{
		Map<Tile3D> map = new Map<Tile3D>((int)Size.y, (int)Size.x, parent, edgeLength);
		var chunkSize = Map<Tile3D>.Chunk.SIZE;
		if(useJobs)
		{
			//TODO : Rewrite Generator for Jobs
			var job = new GeneratorJob(map, Generate);
			var mapSize = map.Width * map.Height * chunkSize;
			var handle = job.Schedule(mapSize, 1);
			handle.Complete();
			return map;
		}
		for (int z = 0; z < map.Height * chunkSize; z++)
		{
			for (int x = 0; x < map.Width * chunkSize; x++)
			{
				var coords = HexCoords.FromOffsetCoords(x, z, edgeLength);
				map[coords] = Generate(x, z);
			}
		}
		return map;
	}

	public struct GeneratorJob : IJobParallelFor
	{
		private Map<Tile3D> _map;
		private const int _chunkSize = Map<Tile3D>.Chunk.SIZE;
		private readonly float _edgeLenth;
		private readonly Func<int, int, Tile3D> _generate;

		public GeneratorJob(Map<Tile3D> map, Func<int, int, Tile3D> generateFunc)
		{
			_map = map;
			_edgeLenth = map.TileEdgeLength;
			_generate = generateFunc;
		}

		public void Execute(int z)
		{
			for (int x = 0; x < _map.Width * _chunkSize; x++)
			{
				var coords = HexCoords.FromOffsetCoords(x, z, _map.TileEdgeLength);
				_map[coords] = _generate(x, z);
			}
		}
	}
}
