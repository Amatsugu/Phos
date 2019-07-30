using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UIButtonHover : UIHover
{
	public float floatDist = -5;
	public float speed = 1;

	private float _curTime;
	protected RectTransform _rectTransform;

	protected override void Awake()
	{
		base.Awake();
		_rectTransform = GetComponent<RectTransform>();
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			_curTime += Time.deltaTime * speed;
		else
			_curTime -= Time.deltaTime * speed;
		_curTime = math.clamp(_curTime, 0, 1);
		var pos = _rectTransform.localPosition;
		var t = 1 - _curTime;
		t *= t;
		t = 1 - t;
		pos.z = math.lerp(0, floatDist, t);
		_rectTransform.localPosition = pos;
	}

}
