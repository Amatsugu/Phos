﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float moveSpeed = 2;
	public float minHeightFromGround = 4;
	public float maxHeight = 20;
	public float zoomAmmount = 1;
	public float zoomSpeed = 2;
	public MapRenderer mapRenderer;

	private float _targetHeight;
	private float _lastHeight;
	private float _anim = 0;
	private HexCoords _lastCoord;
	private Camera _cam;

	private Vector3 _lastClickPos;

    // Start is called before the first frame update
    void Awake()
    {
		_targetHeight = maxHeight;
		_cam = GetComponent<Camera>();
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
		if (Input.GetKey(KeyCode.LeftShift))
			moveVector *= 2;
		pos += moveVector * moveSpeed * Time.deltaTime;

		//Drag Panning
		Vector3 curPos;
		var mPos = Input.mousePosition;
		var ray = _cam.ScreenPointToRay(mPos);
		var height = pos.y - mapRenderer.map.SeaLevel;
		var d = height / Mathf.Sin(_cam.transform.eulerAngles.x * Mathf.Deg2Rad);
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			curPos = _lastClickPos = ray.GetPoint(d);
		}
		if(Input.GetKey(KeyCode.Mouse1))
		{
			curPos = ray.GetPoint(d);
			var delta = _lastClickPos - curPos;
			delta.y = 0;
			pos += delta;
		}

		//Zooming
		var scroll = Input.mouseScrollDelta.y * zoomAmmount;

		_targetHeight -= scroll;
		if (_targetHeight > maxHeight)
			_targetHeight = maxHeight;
		_lastCoord = HexCoords.FromPosition(pos, mapRenderer.map.TileEdgeLength);
		var minHeight = mapRenderer.GetHeight(_lastCoord, 2) + minHeightFromGround;

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
		pos.x = Mathf.Clamp(pos.x, mapRenderer.min.x, mapRenderer.max.x);
		pos.z = Mathf.Clamp(pos.z, mapRenderer.min.z, mapRenderer.max.z);
		transform.position = pos;



	}
}
