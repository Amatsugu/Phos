using System.Collections;
using System.Collections.Generic;

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
		public void Show(RectTransform anchor, string message, string title = null)
		{
			if(title != null)
			{
				titleText.SetText(title);
				titleText.gameObject.SetActive(true);
			}else
				titleText.gameObject.SetActive(false);

			bodyText.SetText(message);
			var pos = anchor.position;

			var offset = anchor.pivot * anchor.rect.size;

			rTransform.anchoredPosition = pos + new Vector3(offset.x, offset.y);
			Show();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			var hits = new List<RaycastResult>();
			graphicRaycaster.Raycast(new PointerEventData(eventSystem), hits);
			for (int i = 0; i < hits.Count; i++)
			{
				var r = hits[i].gameObject.GetComponent<RectTransform>();
				if (r != null)
					Show(r, "Test");
			}
		}
	}
}
