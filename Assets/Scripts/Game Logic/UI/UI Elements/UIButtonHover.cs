using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UI;

public class UIButtonHover : UIHover, ILayoutSelfController
{
	public float floatDist = -5;
	public Vector3 axis;
	public float speed = 1;

	private float _curTime;
	private Vector3 _basePos;

	public void SetLayoutHorizontal()
	{
		_basePos.x = rTransform.localPosition.x;
	}

	public void SetLayoutVertical()
	{
		_basePos.y = rTransform.localPosition.y;
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		axis = math.normalizesafe(axis);
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			_curTime += Time.unscaledDeltaTime * speed;
		else
			_curTime -= Time.unscaledDeltaTime * speed;
		_curTime = math.clamp(_curTime, 0, 1);
		var t = 1 - _curTime;
		t *= t;
		t = 1 - t;
		rTransform.localPosition = _basePos + (axis * math.lerp(0, floatDist, t));
	}
}