using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIMultiClickButton : Button
	{
		public event Action<PointerEventData> OnClick;

		public override void OnPointerClick(PointerEventData eventData) {

			if (!IsActive() || !IsInteractable()) {
				return;
			}

			OnClick?.Invoke(eventData);
		}
	}
}