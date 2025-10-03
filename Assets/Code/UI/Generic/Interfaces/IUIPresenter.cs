using Cysharp.Threading.Tasks;

namespace Snowship.NUI
{
	public interface IUIPresenter {
		UniTask OnCreate();
		void OnPostCreate();
		void OnClose();
	}
}
