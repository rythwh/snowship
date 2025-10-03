using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UILoadingScreenPresenter : UIPresenter<UILoadingScreenView> {

		public UILoadingScreenPresenter(UILoadingScreenView view) : base(view) {
		}

		public override UniTask OnCreate() {
			UIEvents.OnLoadingScreenTextChanged += UpdateLoadingStateText;
			return UniTask.CompletedTask;
		}

		public override void OnClose() {
			UIEvents.OnLoadingScreenTextChanged -= UpdateLoadingStateText;
		}

		private void UpdateLoadingStateText(string state, string substate) {
			View.UpdateLoadingStateText(state, substate);
		}

	}
}
