using UnityEngine;

namespace Snowship.NUI {
	public abstract class UIElementComponent : MonoBehaviour
	{
		private static RectTransform rectTransform = null;
		public RectTransform RectTransform => rectTransform == null ? rectTransform = GetComponent<RectTransform>() : rectTransform;

		public virtual void OnCreate() {
		}

		protected virtual void OnClose() {
		}

		public void Close() => OnClose();
	}
}