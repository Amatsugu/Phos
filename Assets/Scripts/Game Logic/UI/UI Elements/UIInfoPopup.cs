using Effects.Lines;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoPopup : UIExpandable
{
	[Header("Popup Settings")]
	public TMP_Text title;
	public TMP_Text desc;
	public Image image;
	public Vector3 offset = new Vector3(0, 1, 0);
	public MeshEntityRotatable line;

	private Vector3 _notifPos;
	private Entity _line;

	public void Init(HexCoords coords, Sprite icon, string title, string message)
	{
		this.title.SetText(title);
		_notifPos = Map.ActiveMap[coords].SurfacePoint + offset;
		desc.SetText(message);
		if(icon != null)
			image.sprite = icon;
		SetActive(true);
		//LateUpdate();
		rTransform.position = _notifPos;
		rTransform.rotation = GameRegistry.Camera.transform.rotation;
		_line = LineFactory.CreateStaticLine(line, Map.ActiveMap[coords].SurfacePoint, _notifPos);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Map.EM.DestroyEntity(_line);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		rTransform.rotation = GameRegistry.Camera.transform.rotation;
	}
}
