using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Snowship.NUtilities
{
	public class HandleClickScript : MonoBehaviour, IPointerClickHandler
	{

		private Action leftClickDelegate = null;
		private Action middleClickDelegate = null;
		private Action rightClickDelegate = null;

		public void Initialize(
			Action leftClickDelegate,
			Action middleClickDelegate,
			Action rightClickDelegate
		) {
			this.leftClickDelegate = leftClickDelegate;
			this.middleClickDelegate = middleClickDelegate;
			this.rightClickDelegate = rightClickDelegate;
		}

		public void OnPointerClick(PointerEventData pointerEventData) {
			switch (pointerEventData.button) {
				case PointerEventData.InputButton.Left:
					if (leftClickDelegate != null) {
						leftClickDelegate.Invoke();
					}
					break;
				case PointerEventData.InputButton.Middle:
					if (middleClickDelegate != null) {
						middleClickDelegate.Invoke();
					}
					break;
				case PointerEventData.InputButton.Right:
					if (rightClickDelegate != null) {
						rightClickDelegate.Invoke();
					}
					break;
			}
		}
	}
}
