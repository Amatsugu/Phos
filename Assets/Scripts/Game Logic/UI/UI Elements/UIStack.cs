using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStack : MonoBehaviour
{
	public RectTransform.Axis axis;
	public float padding = 0;

	private RectTransform[] _children;

    // Start is called before the first frame update
    void Start()
    {
		UpdateChildren();
    }

	public void UpdateChildren()
	{
		_children = GetComponentsInChildren<RectTransform>(true).Where(c => c.parent == transform).ToArray();
	}

	void LateUpdate()
    {
		var curOffset = 0f;
		for (int i = 0; i < _children.Length; i++)
		{
			if (!_children[i].gameObject.activeInHierarchy)
				continue;
			if (axis == RectTransform.Axis.Horizontal)
			{
				_children[i].anchoredPosition = new Vector2(curOffset, 0);
			}else
			{
				_children[i].anchoredPosition = new Vector2(0, curOffset);
			}
			curOffset += _children[i].rect.width + padding;
		}
    }
}
