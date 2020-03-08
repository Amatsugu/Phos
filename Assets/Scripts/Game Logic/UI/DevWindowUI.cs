using UnityEngine;

public class DevWindowUI : UIPanel
{
	public GameObject UI;

	protected override void Start()
	{
		GameRegistry.CameraController.enabled = false;
		UI.SetActive(false);
		OnHide += () =>
		{
			GameRegistry.CameraController.enabled = true;
			UI.SetActive(true);
		};
		base.Start();
	}
}