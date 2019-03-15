using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float moveSpeed = 2;
	public float minHeightFromGround = 4;
	public float maxHeight = 20;
	public float zoomAmmount = 1;
	public float zoomSpeed = 2;
	public MapRenderer map;

	private float _targetHeight;
	private float _lastHeight;
	private float _anim = 0;
	private HexCoords _lastCoord;

    // Start is called before the first frame update
    void Awake()
    {
		_targetHeight = maxHeight;
    }

    // Update is called once per frame
    void Update()
    {
		//Panning
		var pos = transform.position;
		var moveVector = Vector3.zero;

		if (Input.GetKey(KeyCode.A))
			moveVector.x = -1;
		else if (Input.GetKey(KeyCode.D))
			moveVector.x = 1;

		if (Input.GetKey(KeyCode.W))
			moveVector.z = 1;
		else if (Input.GetKey(KeyCode.S))
			moveVector.z = -1;

		pos += moveVector * moveSpeed * Time.deltaTime;

		//Zooming
		var scroll = Input.mouseScrollDelta.y * zoomAmmount;

		_targetHeight -= scroll;
		if (_targetHeight > maxHeight)
			_targetHeight = maxHeight;
		var minHeight = map.map.SeaLevel + minHeightFromGround;
		if(Physics.Raycast(pos, Vector3.down, out var hit))
		{
			var tile = hit.collider.GetComponent<WorldTile>();
			if(tile != null)
			{
				_lastCoord = tile.coord;
				minHeight = map.GetHeight(_lastCoord, 2) + minHeightFromGround;
			}
		}

		if(minHeight != _lastHeight)
		{
			_lastHeight = minHeight;
			_anim = 0;
		}
		if(scroll != 0)
		{
			_anim = 0;
			if (_targetHeight < minHeight)
				_targetHeight = Mathf.Clamp(_targetHeight, minHeight, maxHeight);
		}
		var desHeight = (_targetHeight < minHeight) ? minHeight : _targetHeight;
		pos.y = Mathf.Lerp(pos.y, desHeight, _anim += Time.deltaTime * zoomSpeed);
		pos.x = Mathf.Clamp(pos.x, map.min.x, map.max.x);
		pos.z = Mathf.Clamp(pos.z, map.min.z, map.max.z);
		transform.position = pos;


	}
}
