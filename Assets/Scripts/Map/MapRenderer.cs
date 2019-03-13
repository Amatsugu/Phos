using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
	public TileInfo tile;
	public MapGenerator generator;
	public GameObject oceanPlane;

	private Map<Tile3D> _map;
	private GameObject _ocean;
	private Camera _cam;

	private void Start()
	{
		Init();
		_cam = FindObjectOfType<Camera>();
	}

	public void Init()
	{
		_map = generator.GenerateMap(transform);
		_map.Render(transform);
		var pos = oceanPlane.transform.localScale;
		pos *= 2;
		pos.y = _map.SeaLevel;
		_ocean = Instantiate(oceanPlane, pos, Quaternion.identity);

		//var coords = HexCoords.FromPosition(new Vector3(20, 0, 70), _map.InnerRadius);

		//_map.CircularFlatten(coords, 3, 6);
	}

	private void Update()
	{
		var min = _cam.ScreenPointToRay(new Vector3(0, _map.SeaLevel, 0));
		var max = _cam.ScreenPointToRay(new Vector3(Screen.width, _map.SeaLevel, Screen.height));
		var l = min.GetPoint(_cam.transform.position.y);
		var r = max.GetPoint(_cam.transform.position.y);
		var ratio = (float)Screen.width / Screen.height;
		var h = (r.x - l.x);
		var view = new Rect(_cam.transform.position.x - (h * ratio * .5f), _cam.transform.position.z - 1, h * ratio, h);

		_map.UpdateView(view);
		if(generator.Regen)
		{
			generator.Regen = false;
			_map.Destroy();
			Destroy(_ocean);
			Init();
		}
		
	}

}
