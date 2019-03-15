using UnityEngine;


[CreateAssetMenu(menuName = "Map Asset/Generator/Random")]
public class RandomGenerator : MapGenerator
{

	[System.Serializable]
	public class NoiseLayer
	{
		public bool enabled = true;
		public bool useFirstLayerAsMask = true;
		public NoiseSettings noiseSettings;
	}

	public float noiseScale = .5f;
	

	public NoiseLayer[] noiseLayers;
	public bool useSeed;
	public int seed = 11;

	private INoiseFilter[] noiseFilters;

	private void OnEnable()
	{
		noiseFilters = null;	
	}

	private float maxElevation = float.MinValue;

	public override Tile3D Generate(int x, int z, Transform parent = null)
	{
		float elevation = 0;
		float firstLayer = 0;
		var point = new Vector3(x, 0, z) / noiseScale;
		if (noiseFilters.Length > 0)
		{
			firstLayer = noiseFilters[0].Evaluate(point);
			if (noiseLayers[0].enabled)
				elevation = firstLayer;
		}
		for (int i = 1; i < noiseFilters.Length; i++)
		{
			if (noiseLayers[i].enabled)
			{
				float mask = (noiseLayers[i].useFirstLayerAsMask) ? firstLayer : 1;
				mask -= seaLevel;
				mask = Mathf.Max(0, mask);
				elevation += noiseFilters[i].Evaluate(point) * mask;
			}
		}
		if (maxElevation < elevation)
			maxElevation = elevation;
		return PaintTile(CreateTile(null, x, z, elevation, parent));
	}

	public override Tile3D PaintTile(Tile3D tile)
	{
		tile.info = tileMapper.GetTile(tile.Height, seaLevel, maxElevation);
		return tile;
	}

	public override Map<Tile3D> GenerateMap(Transform parent = null)
	{
		if (!useSeed)
			seed = (int)(new System.DateTime(1990, 1, 1) - System.DateTime.Now).TotalSeconds;
		Random.InitState(seed);
		maxElevation = float.MinValue;
		noiseFilters = new INoiseFilter[noiseLayers.Length];
		for (int i = 0; i < noiseLayers.Length; i++)
			noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings);
		Map<Tile3D> map = base.GenerateMap(parent);
		map.SeaLevel = seaLevel;
		return map;
	}
}
