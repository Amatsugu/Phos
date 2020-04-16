using System;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UI;

public class UIButtonHover : UIHover
{
	public float floatDist = 4;
	public Vector3 axis = Vector3.up;
	public float speed = 10;

	private float _curTime;
	private Vector3 _basePos;
	private Vector3 _lastPos;

#if DEBUG
	protected override void OnValidate()
	{
		base.OnValidate();
		axis = math.normalizesafe(axis);
	}
#endif

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			_curTime += Time.unscaledDeltaTime * speed;
		else
			_curTime -= Time.unscaledDeltaTime * speed;
		if (_lastPos != rTransform.localPosition)
			_basePos = rTransform.localPosition;
		_curTime = math.clamp(_curTime, 0, 1);
		var t = 1 - _curTime;
		t *= t;
		t = 1 - t;
		_lastPos = rTransform.localPosition = _basePos + (axis * math.lerp(0, floatDist, t));
	}

}