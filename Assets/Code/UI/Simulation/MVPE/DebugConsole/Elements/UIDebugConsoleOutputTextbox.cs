using Cysharp.Threading.Tasks;

namespace Snowship.NUI
{
	public class UIDebugConsoleOutputTextbox : UIElement<UIDebugConsoleOutputTextboxComponent> {
		private readonly string text;

		public UIDebugConsoleOutputTextbox(string text) {
			this.text = text;
		}

		protected override UniTask OnCreate() {
			Component.OutputToConsole(text);
			return UniTask.CompletedTask;
		}
	}
}
