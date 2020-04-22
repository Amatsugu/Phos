using Unity.Mathematics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float moveSpeed = 2;
	public float minHeightFromGround = 4;
	public float maxHeight = 20;
	public float zoomAmmount = 1;
	public float zoomSpeed = 2;
	public float edgePanSize = 100;
	public float highAngle = 60;
	public float lowAngle = 80;
	public AnimationCurve angleCurve;
	public bool edgePan = false;
	public MapRenderer mapRenderer;

	private float _targetHeight;
	private float _lastHeight;
	private float _anim = 0;
	private Vector3 _lastCoord;
	private Camera _cam;

	private Vector3 _lastClickPos;
	private Transform _thisTransform;
	private bool _isFocusing;
	private float _focusTime;
	private Vector3 _focusPos;
	private bool _canRotate = true;
	private float _rotationTime = 0;

	private CameraMode _state = 0;

	public enum CameraMode
	{
		Locked = 0,
		Panning,
		Free,
		Cenematic
	}

	private void Awake()
	{
		GameRegistry.INST.cameraController = this;
		GameRegistry.INST.mainCamera = _cam = GetComponent<Camera>();
		_thisTransform = transform;
	}

	private void Start()
	{
		_targetHeight = maxHeight;
		/*if (Application.isEditor)
			maxHeight = 500;*/
		EventManager.AddEventListener("nameWindowOpen", () =>
		{
			enabled = false;
		});
		EventManager.AddEventListener("nameWindowClose", () =>
		{
			enabled = true;
		});
		_state = CameraMode.Panning;
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.F3))
		{
			_state = CameraMode.Free;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		if (Input.GetKeyUp(KeyCode.F2))
		{
			_state = CameraMode.Panning;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		switch (_state)
		{
			case CameraMode.Locked:
				break;
			case CameraMode.Panning:
				PanningCam();
				break;
			case CameraMode.Free:
				FreeCam();
				break;
		}
		
	}

	private void FreeCam()
	{
		var camRot = _thisTransform.eulerAngles;
		var camPos = _thisTransform.position;

		var move = Vector3.zero;

		if (Input.GetKey(KeyCode.W))
			move.z += moveSpeed;
		else if (Input.GetKey(KeyCode.S))
			move.z -= moveSpeed;

		if (Input.GetKey(KeyCode.A))
			move.x -= moveSpeed;
		else if (Input.GetKey(KeyCode.D))
			move.x += moveSpeed;

		if (Input.GetKey(KeyCode.LeftShift))
			move.y -= moveSpeed;
		else if (Input.GetKey(KeyCode.Space))
			move.y += moveSpeed;

		var mX = Input.GetAxis("Mouse X");
		var mY = Input.GetAxis("Mouse Y");

		camRot.y += mX;
		camRot.x -= mY;

		if (camRot.x > 90 && camRot.x < 180)
			camRot.x = 90;
		if (camRot.x < 270 && camRot.x > 180)
			camRot.x = 270;

		var camQ = Quaternion.Euler(camRot);
		camPos += camQ * move * Time.deltaTime;
		_thisTransform.position = camPos;
		_thisTransform.rotation = camQ;

	}

	private void PanningCam()
	{
		_canRotate = true;
		if (Input.GetKeyDown(KeyCode.X))
		{
			this.enabled = false;
			GetComponent<Orbit>().enabled = true;
		}
		//Panning
		var pos = _thisTransform.position;
		var rot = _thisTransform.eulerAngles;
		var moveVector = Vector3.zero;
		var mPos = Input.mousePosition;

		if (!Input.GetKey(KeyCode.Mouse1))
		{
			if (Input.GetKey(KeyCode.A))
				moveVector.x = -1;
			else if (Input.GetKey(KeyCode.D))
				moveVector.x = 1;

			if (Input.GetKey(KeyCode.W))
				moveVector.z = 1;
			else if (Input.GetKey(KeyCode.S))
				moveVector.z = -1;

			if (edgePan)
			{
				//Edge Panning
				if (mPos.x < edgePanSize)
					moveVector.x = -1;
				else if (mPos.x > Screen.width - edgePanSize)
					moveVector.x = 1;
				if (mPos.y < edgePanSize)
					moveVector.z = -1;
				else if (mPos.y > Screen.height - edgePanSize)
					moveVector.z = 1;
			}

			if (moveVector.sqrMagnitude != 0)
			{
				_isFocusing = false;
				_canRotate = false;
			}

			if (Input.GetKey(KeyCode.LeftShift))
				moveVector *= 2;
			pos += moveVector * moveSpeed * Time.deltaTime;
		}

		if (true) //Todo do raycast on ui
		{
			//Drag Panning
			Vector3 curPos;
			var ray = _cam.ScreenPointToRay(mPos);
			var height = pos.y - mapRenderer.map.seaLevel;
			var d = height / Mathf.Sin(_cam.transform.localEulerAngles.x * Mathf.Deg2Rad);
			if (Input.GetKeyDown(KeyCode.Mouse2))
				_lastClickPos = ray.GetPoint(d);
			if (Input.GetKey(KeyCode.Mouse2))
			{
				_isFocusing = false;
				_canRotate = false;
				curPos = ray.GetPoint(d);
				var delta = _lastClickPos - curPos;
				delta.y = 0;
				pos += delta;
			}

			//Zooming
			var scroll = Input.mouseScrollDelta.y * zoomAmmount;
			if (scroll != 0)
				_isFocusing = false;

			_targetHeight -= scroll;
			if (_targetHeight > maxHeight)
				_targetHeight = maxHeight;
			_lastCoord = pos;
			var minHeight = Map.ActiveMap.GetHeight(_lastCoord, 2) + minHeightFromGround;

			if (minHeight != _lastHeight)
			{
				_lastHeight = minHeight;
				_anim = 0;
				_rotationTime = 0;
			}
			if (scroll != 0)
			{
				_anim = 0;
				_rotationTime = 0;
				if (_targetHeight < minHeight)
					_targetHeight = Mathf.Clamp(_targetHeight, minHeight, maxHeight);
			}
			var desHeight = (_targetHeight < minHeight) ? minHeight : _targetHeight;
			pos.y = Mathf.Lerp(pos.y, desHeight, _anim += Time.unscaledDeltaTime * zoomSpeed);
			var t = pos.y.Remap(minHeight, maxHeight, 0, 1);
			if (_canRotate)
			{
				_rotationTime += Time.unscaledDeltaTime * zoomSpeed;
				_rotationTime = Mathf.Clamp(_rotationTime, 0, 1);
				var angle = lowAngle.Lerp(highAngle, angleCurve.Evaluate(t));
				rot.x = rot.x.Lerp(angle, _rotationTime);
			}
		}
		if (_isFocusing)
		{
			_focusTime += Time.unscaledDeltaTime;
			pos = Vector3.Lerp(pos, _focusPos, _focusTime);
			if (_focusTime >= 1)
				_isFocusing = false;
		}
		pos.x = Mathf.Clamp(pos.x, mapRenderer.min.x, mapRenderer.max.x);
		pos.z = Mathf.Clamp(pos.z, mapRenderer.min.z, mapRenderer.max.z);
		_thisTransform.position = pos;
		_thisTransform.rotation = Quaternion.Euler(rot);
	}

	public void FocusPoint(float3 targetPos)
	{
		var height = _targetHeight - targetPos.y;
		var d = height / Mathf.Sin(_thisTransform.localEulerAngles.x * Mathf.Deg2Rad);
		var ray = new Ray(targetPos, _thisTransform.forward);

		targetPos = ray.GetPoint(-d);
		_focusPos = targetPos;
		_isFocusing = true;
		_focusTime = 0;
	}

	public static void FocusOnTile(HexCoords tile) => GameRegistry.CameraController.FocusPoint(Map.ActiveMap[tile].SurfacePoint);
	public static void FocusOnTile(float3 position) => GameRegistry.CameraController.FocusPoint(position);
}