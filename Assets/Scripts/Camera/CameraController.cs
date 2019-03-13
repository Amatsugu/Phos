using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float moveSpeed = 2;
	public float minHeight = 2;
	public float maxHeight = 20;
	public float zoomSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

		var scroll = Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;

		pos += Vector3.down * scroll;

		pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
		transform.position = pos;

	}
}
