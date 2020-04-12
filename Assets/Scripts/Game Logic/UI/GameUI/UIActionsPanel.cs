using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionsPanel : UIPanel
{
	public UIBuildPanel actionButtonPrefab;


	private UIBuildPanel[] _buttons;

	public enum ActionState
	{
		Disabled,
		Unit,
		Building
	}

	public void UpdateState()
	{

	}

	private void ShowApplicableUnitButtons(MobileUnit[] units)
	{
		for (int i = 0; i < units.Length; i++)
		{
			ShowButtons(units[i]);
		}
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
	}

	private void ShowApplicableTileButtons(Tile[] tiles)
	{
		for (int i = 0; i < tiles.Length; i++)
		{
			ShowButtons(tiles[i]);
		}
	}
}
