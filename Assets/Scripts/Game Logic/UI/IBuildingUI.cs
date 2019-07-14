using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingUI
{
	void Show(InteractiveBuildingTile target);

	void Hide();
}
