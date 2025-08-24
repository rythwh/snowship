using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI.UITab
{
	public interface IUITabElement
	{
		void SetActive(bool active);
		UniTask Open(Transform parent);
		void Close();
	}
}
