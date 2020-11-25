using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Units;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using TMPro;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amatsugu.Phos
{
    public class UITooltip : UIPanel
    {
		public TMP_Text bodyText;
		public GraphicRaycaster graphicRaycaster;
		public EventSystem eventSystem;

		public void Show(StringBuilder message, StringBuilder title = null)
		{
			if(title != null)
			{
				titleText.SetText(title);
				titleText.gameObject.SetActive(true);
			}else
				titleText.gameObject.SetActive(false);

			bodyText.SetText(message);
			rTransform.anchoredPosition = Input.mousePosition + new Vector3(5, 5);
			Show();
		}

		public void Show(BuildingTileEntity info)
		{
			var text = new StringBuilder(info.description);
			text.AppendLine(info.GetUpkeepString().ToString());
			text.AppendLine(info.GetProductionString().ToString());
			Show(text, info.GetNameString());
		}

		public void Show(MobileUnitEntity info)
		{
			var text = new StringBuilder(info.description);
			Show(text, info.GetNameString());
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			rTransform.anchoredPosition = Input.mousePosition + new Vector3(5, 5);
		}
	}
}
