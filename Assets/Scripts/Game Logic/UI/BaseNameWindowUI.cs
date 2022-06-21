using TMPro;

using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class BaseNameWindowUI : MonoBehaviour
{
	public TMP_InputField text;
	public TMP_Text baseNameText;

	[HideInInspector]
	public UIPanel panel;
	private bool _firstSet = true;

	private void Awake()
	{
		panel = GetComponent<UIPanel>();
		GameRegistry.INST.baseNameUI = this;
		panel.OnShow += () =>
		{
			Debug.Log("Show");
			GameEvents.InvokeOnCameraFreeze();
		};
		panel.OnHide += () =>
		{
			if(string.IsNullOrWhiteSpace(text.text))
			{
				panel.SetActive(true);
				return;
			}
			baseNameText.text = text.text;
			GameRegistry.SetBaseName(text.text);
			GameEvents.InvokeOnCameraUnFreeze();
		};

		GameEvents.OnMapLoaded += () =>
		{
			baseNameText.text = string.IsNullOrWhiteSpace(GameRegistry.GameState.baseName) ? "Base Name" : GameRegistry.GameState.baseName;
		};
	}
}