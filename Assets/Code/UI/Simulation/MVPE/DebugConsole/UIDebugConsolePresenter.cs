using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIDebugConsolePresenter : UIPresenter<UIDebugConsoleView>
	{
		private readonly DebugManager debugM;

		public UIDebugConsolePresenter(UIDebugConsoleView view, DebugManager debugM) : base(view) {
			this.debugM = debugM;
		}

		public override UniTask OnCreate() {

			debugM.OnConsoleOutputProduced += OnDebugOutputReceived;
			debugM.OnConsoleClearRequested += ClearConsole;

			View.SelectDebugInputField();

			View.OnDebugCommandSent += OnDebugCommandSent;

			return UniTask.CompletedTask;
		}

		public override void OnClose() {

			debugM.OnConsoleOutputProduced -= OnDebugOutputReceived;
			debugM.OnConsoleClearRequested -= ClearConsole;

			View.OnDebugCommandSent -= OnDebugCommandSent;
		}

		private void OnDebugCommandSent(string text) {
			debugM.ParseCommandInput(text);
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
