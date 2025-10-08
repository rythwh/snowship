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
		private int openedHumanTabIndex = 0;

		private InputManager InputM => GameManager.Get<InputManager>();
		private UIManager UIM => GameManager.Get<UIManager>();
		private IHumanEvents HumanEvents => GameManager.Get<IHumanEvents>();
		private IHumanQuery HumanQuery => GameManager.Get<IHumanQuery>();
		private StateManager StateM => GameManager.Get<StateManager>();

		public UISimulationPresenter(UISimulationView view) : base(view) {
		}

		public override async UniTask OnCreate() {
			InputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			View.SetDisclaimerText($"Lumi Games (Snowship {GameManager.GameVersion.text})");

			await SetupChildUIs();

			HumanEvents.OnHumanSelected += async human => await OnHumanSelected(human);
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
			// Close existing info panel if new human is null
			if (selectedHuman == null) {
				Debug.Log("Close UIHumanInfoPanel");
				UIM.CloseView<UIHumanInfoPanel>();
				return;
			}

			// Open (or re-open if selecting new human) info panel
			UIHumanInfoPanelParameters parameters = new(
				selectedHuman,
				HumanQuery.GetHumanView(selectedHuman),
				openedHumanTabIndex
			);
			if (await UIM.ReopenView<UIHumanInfoPanel>(this, parameters) is UIHumanInfoPanelPresenter presenter) {
				presenter.OnTabIndexOpened += OnHumanTabIndexOpened;
			}
		}

		private void OnHumanTabIndexOpened(int index) {
			openedHumanTabIndex = index;
		}
	}
}
