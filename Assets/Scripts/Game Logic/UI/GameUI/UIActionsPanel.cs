using System.Collections.Generic;

using UnityEngine.Events;
using UnityEngine.UI;

public class UIActionsPanel : UIPanel
{
	public UIBuildPanel actionButtonPrefab;

	public Button attackCommand;
	public Button moveCommand;
	public Button attackStateCommand;
	public Button guardCommand;
	public Button repairCommand;
	public Button deconstructCommand;
	public Button groundFireCommand;
	public Button patrolCommand;
	public Button haltCommand;

	private HashSet<Button> _activeButtons;

	protected override void Awake()
	{
		base.Awake();
		_activeButtons = new HashSet<Button>();
		attackCommand.gameObject.SetActive(false);
		moveCommand.gameObject.SetActive(false);
		attackStateCommand.gameObject.SetActive(false);
		groundFireCommand.gameObject.SetActive(false);
		guardCommand.gameObject.SetActive(false);
		repairCommand.gameObject.SetActive(false);
		deconstructCommand.gameObject.SetActive(false);
		patrolCommand.gameObject.SetActive(false);
		haltCommand.gameObject.SetActive(false);
	}

	public enum ActionState
	{
		Disabled,
		Unit,
		Building
	}

	public void UpdateState()
	{
	}

	public void ShowApplicableButtons(ICommandable[] units)
	{
		foreach (var item in _activeButtons)
		{
			item.gameObject.SetActive(false);
			item.onClick.RemoveAllListeners();
		}
		for (int i = 0; i < units.Length; i++)
			ShowButtons(units[i]);
		Show();
	}

	private void ShowButtons(ICommandable entity)
	{
		var supportedCommands = entity.GetSupportedCommands();
		if (supportedCommands.HasFlag(CommandActions.Attack))
			Showbutton(attackCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Move))
				Showbutton(moveCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.AttackState))
				Showbutton(attackStateCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Guard))
			Showbutton(guardCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Repair))
			Showbutton(repairCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Deconstruct))
			Showbutton(deconstructCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.GroundFire))
			Showbutton(groundFireCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Patrol))
			Showbutton(patrolCommand, () => { });

		if (supportedCommands.HasFlag(CommandActions.Halt))
			Showbutton(haltCommand, () => { });
	}

	private void Showbutton(Button button, UnityAction callback)
	{
		if (!_activeButtons.Contains(button))
			button.gameObject.SetActive(true);
		button.onClick.AddListener(callback);
	}

	public override void Hide()
	{
		base.Hide();
		foreach (var item in _activeButtons)
		{
			item.gameObject.SetActive(false);
			item.onClick.RemoveAllListeners();
		}
	}
}