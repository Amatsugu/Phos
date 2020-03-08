using UnityEngine;

public class Orbit : MonoBehaviour
{
	public Vector3 target;
	public float height = 40;
	public float distance = 40;
	public float speed = 1;

	private float _angle;

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			this.enabled = false;
			GetComponent<CameraController>().enabled = true;
		}
		_angle += Time.deltaTime * speed;
		var x = Mathf.Sin(_angle) * distance;
		var z = Mathf.Cos(_angle) * distance;
		transform.position = new Vector3(x, height, z) + target;
		transform.LookAt(target);
	}
}