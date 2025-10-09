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
		private readonly InputManager inputM;
		private readonly UIManager uiM;
		private readonly IHumanEvents humanEvents;
		private readonly IHumanQuery humanQuery;
		private readonly StateManager stateM;

		private int openedHumanTabIndex = 0;

		public UISimulationPresenter(
			UISimulationView view,
			InputManager inputM,
			UIManager uiM,
			IHumanEvents humanEvents,
			IHumanQuery humanQuery,
			StateManager stateM
		) : base(view)
		{
			this.inputM = inputM;
			this.uiM = uiM;
			this.humanEvents = humanEvents;
			this.humanQuery = humanQuery;
			this.stateM = stateM;
		}

		public override async UniTask OnCreate() {
			inputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			View.SetDisclaimerText($"Lumi Games (Snowship {GameManager.GameVersion.text})");

			await SetupChildUIs();

			humanEvents.OnHumanSelected += async human => await OnHumanSelected(human);
		}

		public override void OnClose() {
			inputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private async void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			// TODO Once setup, de-select items from SelectionManager in a FIFO format, and if none remain, then open the PauseMenu
			if (uiM.OpenViewCount() > 0) {
			} else {
				await stateM.TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide);
			}
		}

		private async UniTask SetupChildUIs() {
			await uiM.OpenViewAsync<UIDateTime>(this);
			await uiM.OpenViewAsync<UIActionsPanel>(this);
		}

		private async UniTask OnHumanSelected(Human selectedHuman) {
			// Close existing info panel if new human is null
			if (selectedHuman == null) {
				Debug.Log("Close UIHumanInfoPanel");
				uiM.CloseView<UIHumanInfoPanel>();
				OnHumanTabIndexOpened(0);
				return;
			}

			// Open (or re-open if selecting new human) info panel
			UIHumanInfoPanelParameters parameters = new(
				selectedHuman,
				humanQuery.GetHumanView(selectedHuman),
				openedHumanTabIndex
			);
			if (await uiM.ReopenView<UIHumanInfoPanel>(this, parameters) is UIHumanInfoPanelPresenter presenter) {
				presenter.OnTabIndexOpened += OnHumanTabIndexOpened;
			}
		}

		private void OnHumanTabIndexOpened(int index) {
			openedHumanTabIndex = index;
		}
	}
}
