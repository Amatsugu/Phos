using Steamworks;
using TMPro;

using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class BaseNameWindowUI : MonoBehaviour
{
	public TMP_InputField text;
	public TMP_Text baseNameText;

	[HideInInspector]
	public UIPanel panel;

	private void Awake()
	{
		panel = GetComponent<UIPanel>();
		GameRegistry.INST.baseNameUI = this;
		panel.OnShow += () =>
		{
			text.text = $"{SteamFriends.GetPersonaName()}'s Base";
			EventManager.InvokeEvent("nameWindowOpen");
		};
		panel.OnHide += () =>
		{
			baseNameText.text = text.text;
			EventManager.InvokeEvent("nameWindowClose");
		};
	}
}