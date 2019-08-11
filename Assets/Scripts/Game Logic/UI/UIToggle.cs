using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
	public KeyCode key = KeyCode.F1;

	public GameObject[] ui;

	private bool _active = true;

    void LateUpdate()
    {
		if (Input.GetKeyUp(key))
		{
			_active = !_active;
			for (int i = 0; i < ui.Length; i++)
				ui[i].SetActive(_active);
		}
    }
}
