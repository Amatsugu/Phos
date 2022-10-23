using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Amatsugu.Phos
{
    public class UIDeconstructionOverlay : UIBehaviour
	{
		public RectTransform top;
		public RectTransform bottom;
		public RectTransform left;
		public RectTransform right;
		public RectTransform text;
		
		override protected void Start()
        {
			GameEvents.OnEnterDeconstructionMode += Show;
			GameEvents.OnExitDeconstructionMode += Hide;
		}

		public void Hide()
		{
			top.gameObject.SetActive(false);
			bottom.gameObject.SetActive(false);
			left.gameObject.SetActive(false);
			right.gameObject.SetActive(false);
			text.gameObject.SetActive(false);
		}

		public void Show()
		{
			top.gameObject.SetActive(true);
			bottom.gameObject.SetActive(true);
			left.gameObject.SetActive(true);
			right.gameObject.SetActive(true);
			text.gameObject.SetActive(true);
		}
    }
}
