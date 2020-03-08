using System.Linq;

using UnityEngine;

[ExecuteInEditMode]
public class UIStack : MonoBehaviour
{
	public RectTransform.Axis axis;
	public float padding = 0;
	public bool invertDirection;

	private RectTransform[] _children;

	// Start is called before the first frame update
	private void Start()
	{
		UpdateChildren();
	}

	public void UpdateChildren()
	{
		_children = GetComponentsInChildren<RectTransform>(true).Where(c => c.parent == transform).ToArray();
	}

	private void LateUpdate()
	{
		var curOffset = 0f;
		for (int i = 0; i < _children.Length; i++)
		{
			if (!_children[i].gameObject.activeInHierarchy)
				continue;
			if (axis == RectTransform.Axis.Horizontal)
			{
				curOffset += _children[i].rect.width + padding;
				_children[i].anchoredPosition = new Vector2(invertDirection ? -curOffset : curOffset, 0);
			}
			else
			{
				_children[i].anchoredPosition = new Vector2(0, invertDirection ? -curOffset : curOffset);
				curOffset += _children[i].rect.height + padding;
			}
		}
	}
}