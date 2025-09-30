using System;
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

		public UIPauseMenuPresenter(UIPauseMenuView view) : base(view) {
		}

		public override void OnCreate() {

			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnSaveButtonClicked += OnSaveButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked += OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked += OnExitToDesktopButtonClicked;

			GameManager.Get<InputManager>().InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;
		}

		public override void OnClose() {
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnSaveButtonClicked -= OnSaveButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked -= OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked -= OnExitToDesktopButtonClicked;

			GameManager.Get<InputManager>().InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private async void OnContinueButtonClicked() {
			await GameManager.Get<StateManager>().TransitionToState(EState.Simulation, ETransitionUIAction.Close);
		}

		private async void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			await GameManager.Get<StateManager>().TransitionToState(EState.Simulation, ETransitionUIAction.Close);
		}

		private async void OnSaveButtonClicked() {
			try {
				// await GameManager.Get<PersistenceManager>().CreateSave(GameManager.Get<ColonyManager>().colony);
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen));
			} catch (Exception e) {
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				throw e.InnerException ?? e;
			}
		}

		private async void OnSettingsButtonClicked() {
			await GameManager.Get<UIManager>().OpenViewAsync<UISettings>(this, false);
		}

		private async void OnExitToMenuButtonClicked() {
			await GameManager.Get<StateManager>().TransitionToState(EState.MainMenu, ETransitionUIAction.Close);
		}

		private void OnExitToDesktopButtonClicked() {
			Application.Quit();
		}
	}
}
