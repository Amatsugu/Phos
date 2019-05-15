using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
	public KeyCode key = KeyCode.F1;
	public GameObject uiBase;

    void LateUpdate()
    {
		if (Input.GetKeyUp(key))
			uiBase.SetActive(!uiBase.activeInHierarchy);
    }
}
