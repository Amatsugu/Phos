using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class InitializeWeather : MonoBehaviour
{
	public MeshEntity cloudMesh;
	public MeshEntity cloudShadowMesh;
	public Light sun;
	public VolumeProfile volumeProfile;
	public VisualEffect rainVfx;
	public WeatherDefination[] weatherDefinations;
	public float clouldHeight;
	public int fieldHeight = 200;
	public int fieldWidth = 100;
	public NoiseSettings noiseSettings;

	[Range(1, 100)]
	public float camDisolveDist = 10;

	[Range(1, 50)]
	public float disolveOffsetDist = 20;

	public float disolveUpperBound = 0;
	public float disolveLowerBound = 0;

	private NativeArray<Entity> _clouds;
	private NativeArray<Entity> _cloudShadows;

	private void Awake()
	{
		EventManager.AddEventListener("OnMapLoaded", InitWather);
	}

	private void InitWather()
	{
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		_clouds = new NativeArray<Entity>(fieldHeight * fieldWidth, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_cloudShadows = new NativeArray<Entity>(fieldHeight * fieldWidth, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		cloudMesh.Instantiate(_clouds);
		cloudShadowMesh.Instantiate(_cloudShadows);
		em.AddComponent(_clouds, typeof(CloudData));
		em.AddComponent(_cloudShadows, typeof(CloudData));
		em.AddComponent(_cloudShadows, typeof(ShadowOnlyTag));
		var innerR = HexCoords.CalculateInnerRadius(2);
		for (int z = 0; z < fieldHeight; z++)
		{
			for (int x = 0; x < fieldWidth; x++)
			{
				var pos = HexCoords.OffsetToWorldPosXZ(x, z, innerR, 2);
				pos.y = clouldHeight;
				em.SetComponentData(_clouds[x + z * fieldWidth], new Translation { Value = pos });
				em.SetComponentData(_clouds[x + z * fieldWidth], new NonUniformScale { Value = Vector3.one * 4 });
				em.SetComponentData(_clouds[x + z * fieldWidth], new CloudData { pos = pos, index = x + z * fieldWidth });
				em.SetComponentData(_cloudShadows[x + z * fieldWidth], new Translation { Value = pos });
				em.SetComponentData(_cloudShadows[x + z * fieldWidth], new NonUniformScale { Value = Vector3.one * 4 });
				em.SetComponentData(_cloudShadows[x + z * fieldWidth], new CloudData { pos = pos, index = x + z * fieldWidth });
			}
		}
		EventManager.RemoveEventListener("OnMapLoaded", InitWather);
	}

	private void OnDestroy()
	{
		if (enabled)
		{
			if (_clouds.IsCreated)
				_clouds.Dispose();
			if (_cloudShadows.IsCreated)
				_cloudShadows.Dispose();
		}
	}
}