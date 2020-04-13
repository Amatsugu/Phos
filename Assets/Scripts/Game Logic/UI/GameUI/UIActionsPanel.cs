using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


	private HashSet<Button> _activeButtons;

	public enum ActionState
	{
		Disabled,
		Unit,
		Building
	}

	public void UpdateState()
	{

	}

	private void ShowApplicableButtons(object[] units)
	{
		foreach (var item in _activeButtons)
		{
			item.gameObject.SetActive(false);
			item.onClick.RemoveAllListeners();
		}
		for (int i = 0; i < units.Length; i++)
			ShowButtons(units[i]);
	}



	private void ShowButtons(object entity)
	{
		if (entity is IAttack a)
			throw new System.NotImplementedException();
		if (entity is IMoveable m)
			throw new System.NotImplementedException();
		if (entity is IAttackState at)
			throw new System.NotImplementedException();
		if (entity is IGuard g)
			throw new System.NotImplementedException();
		if (entity is IRepairable r)
			throw new System.NotImplementedException();
		if (entity is IDeconstructable d)
			throw new System.NotImplementedException();
		if (entity is IGroundFire gf)
			throw new System.NotImplementedException();
		if (entity is IPartolable p)
			throw new System.NotImplementedException();
	}

	private void Showbutton(Button button, UnityAction callback)
	{
		if (!_activeButtons.Contains(button))
			button.gameObject.SetActive(true);
		button.onClick.AddListener(callback);
	}
}
