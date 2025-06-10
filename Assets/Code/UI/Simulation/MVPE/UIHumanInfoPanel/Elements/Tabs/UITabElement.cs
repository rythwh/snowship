using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI.UITab
{
	public abstract class UITabElement<T> : UIElement<T>, IUITabElement where T : UITabElementComponent
	{
		public void SetActive(bool active) {
			Component.gameObject.SetActive(active);
		}
	}

	public interface IUITabElement
	{
		void SetActive(bool active);
		UniTask Open(Transform parent);
	}
}
