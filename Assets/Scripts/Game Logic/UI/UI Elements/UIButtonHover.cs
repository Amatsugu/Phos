using Unity.Mathematics;

using UnityEngine;

public class UIButtonHover : UIHover
{
	public float floatDist = -5;
	public float speed = 1;

	private float _curTime;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			_curTime += Time.unscaledDeltaTime * speed;
		else
			_curTime -= Time.unscaledDeltaTime * speed;
		_curTime = math.clamp(_curTime, 0, 1);
		var pos = rTransform.localPosition;
		var t = 1 - _curTime;
		t *= t;
		t = 1 - t;
		pos.z = math.lerp(0, floatDist, t);
		rTransform.localPosition = pos;
	}
}