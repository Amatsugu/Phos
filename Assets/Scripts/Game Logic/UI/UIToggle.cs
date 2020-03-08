using UnityEngine;

public class UIToggle : MonoBehaviour
{
	public KeyCode key = KeyCode.F1;

	public GameObject[] ui;

	private bool _active = true;

	private void LateUpdate()
	{
		if (Input.GetKeyUp(key))
		{
			_active = !_active;
			for (int i = 0; i < ui.Length; i++)
				ui[i].SetActive(_active);
		}
	}
}