using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NInput;

using Snowship.NState;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIPauseMenuPresenter : UIPresenter<UIPauseMenuView> {
		private readonly InputManager inputM;
		private readonly StateManager stateM;
		private readonly UIManager uiM;

		public UIPauseMenuPresenter(
			UIPauseMenuView view,
			InputManager inputM,
			StateManager stateM,
			UIManager uiM
		) : base(view)
		{
			this.inputM = inputM;
			this.stateM = stateM;
			this.uiM = uiM;
		}

		public override UniTask OnCreate() {

			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnSaveButtonClicked += OnSaveButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked += OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked += OnExitToDesktopButtonClicked;

			inputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			return UniTask.CompletedTask;
		}

		public override void OnClose() {
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnSaveButtonClicked -= OnSaveButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked -= OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked -= OnExitToDesktopButtonClicked;

			inputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private async void OnContinueButtonClicked() {
			await stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close);
		}

		private async void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			await stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close);
		}

		private void OnSaveButtonClicked() {
			try {
				// await persistenceM.CreateSave(GameManager.Get<ColonyManager>().colony);
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen));
			} catch (Exception e) {
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				throw e.InnerException ?? e;
			}
		}

		private async void OnSettingsButtonClicked() {
			await uiM.OpenViewAsync<UISettings>(this, false);
		}

		private async void OnExitToMenuButtonClicked() {
			await stateM.TransitionToState(EState.MainMenu, ETransitionUIAction.Close);
		}

		private void OnExitToDesktopButtonClicked() {
			Application.Quit();
		}
	}
}
