using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NInput;

using Snowship.NState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UISimulationPresenter : UIPresenter<UISimulationView>
	{
		private InputManager InputM => GameManager.Get<InputManager>();
		private UIManager UIM => GameManager.Get<UIManager>();
		private HumanManager HumanM => GameManager.Get<HumanManager>();
		private StateManager StateM => GameManager.Get<StateManager>();

		public UISimulationPresenter(UISimulationView view) : base(view) {
		}

		public override async void OnCreate() {
			InputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			View.SetDisclaimerText($"Lumi Games (Snowship {GameManager.GameVersion.text})");

			await SetupChildUIs();

			HumanM.OnHumanSelected += async human => await OnHumanSelected(human);
		}

		public override void OnClose() {
			InputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private async void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			// TODO Once setup, de-select items from SelectionManager in a FIFO format, and if none remain, then open the PauseMenu
			if (UIM.OpenViewCount() > 0) {
			} else {
				await StateM.TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide);
			}
		}

		private async UniTask SetupChildUIs() {
			await UIM.OpenViewAsync<UIDateTime>(this);
			await UIM.OpenViewAsync<UIActionsPanel>(this);
		}

		private async UniTask OnHumanSelected(Human selectedHuman) {
			if (selectedHuman == null) {
				Debug.Log("Close UIHumanInfoPanel");
				UIM.CloseView<UIHumanInfoPanel>();
				return;
			}
			UIHumanInfoPanelParameters parameters = new(selectedHuman);
			await UIM.ReopenView<UIHumanInfoPanel>(this, parameters);
		}

	}
}
