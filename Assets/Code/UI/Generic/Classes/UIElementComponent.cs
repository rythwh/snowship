using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI {
	public abstract class UIElementComponent : MonoBehaviour
	{
		private static RectTransform rectTransform = null;
		protected RectTransform RectTransform => rectTransform == null ? rectTransform = GetComponent<RectTransform>() : rectTransform;

		public virtual UniTask OnCreate() {
			return UniTask.CompletedTask;
		}

		protected virtual void OnClose() {
		}

		public void Close() => OnClose();
	}
}
