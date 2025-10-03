using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIDebugConsolePresenter : UIPresenter<UIDebugConsoleView>
	{

		private DebugManager DebugM => GameManager.Get<DebugManager>();

		public UIDebugConsolePresenter(UIDebugConsoleView view) : base(view) {
		}

		public override UniTask OnCreate() {

			DebugM.OnConsoleOutputProduced += OnDebugOutputReceived;
			DebugM.OnConsoleClearRequested += ClearConsole;

			View.SelectDebugInputField();

			View.OnDebugCommandSent += OnDebugCommandSent;

			return UniTask.CompletedTask;
		}

		public override void OnClose() {

			DebugM.OnConsoleOutputProduced -= OnDebugOutputReceived;
			DebugM.OnConsoleClearRequested -= ClearConsole;

			View.OnDebugCommandSent -= OnDebugCommandSent;
		}

		private void OnDebugCommandSent(string text) {
			DebugM.ParseCommandInput(text);
		}

		private async void OnDebugOutputReceived(string text) {
			await View.OutputToConsole(text);
			View.SelectDebugInputField();
		}

		private void ClearConsole() {
			View.ClearConsole();
			View.SelectDebugInputField();
		}

	}
}
