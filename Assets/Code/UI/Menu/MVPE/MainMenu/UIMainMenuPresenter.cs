using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIMainMenuPresenter : UIPresenter<UIMainMenuView> {

		// private readonly PLastSave pLastSave = new PLastSave();

		public UIMainMenuPresenter(UIMainMenuView view) : base(view) {
		}

		public override UniTask OnCreate() {

			View.OnNewButtonClicked += OnNewButtonClicked;
			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnLoadButtonClicked += OnLoadButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitButtonClicked += OnExitButtonClicked;

			View.SetDisclaimerText($"Lumi Games (Snowship {GameManager.GameVersion.text})");

			/*PLastSave.LastSaveProperties lastSaveProperties = pLastSave.GetLastSaveProperties();
			if (lastSaveProperties != null) {
				View.SetupContinueButton(
					pLastSave.IsLastSaveUniverseLoadable(),
					PersistenceUtilities.LoadSaveImageFromSaveDirectoryPath(lastSaveProperties.lastSaveSavePath)
				);
			} else {*/
			View.DisableContinueButton();
			// }

			List<Sprite> backgroundImages = Resources.LoadAll<Sprite>(@"UI/Backgrounds/SingleMap").ToList();
			View.SetBackground(backgroundImages[Random.Range(0, backgroundImages.Count)]);

			return UniTask.CompletedTask;
		}

		public override void OnClose() {
			View.OnNewButtonClicked -= OnNewButtonClicked;
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnLoadButtonClicked -= OnLoadButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitButtonClicked -= OnExitButtonClicked;
		}

		private async void OnNewButtonClicked() {
			await GameManager.Get<UIManager>().OpenViewAsync<UICreatePlanet>(this, false);
		}

		private void OnContinueButtonClicked() {
			// GameManager.Get<PersistenceManager>().ContinueFromMostRecentSave();
		}

		private async void OnLoadButtonClicked() {
			await GameManager.Get<UIManager>().OpenViewAsync<UILoadColony>(this, false);
		}

		private async void OnSettingsButtonClicked() {
			await GameManager.Get<UIManager>().OpenViewAsync<UISettings>(this, false);
		}

		private void OnExitButtonClicked() {
			Application.Quit();
		}
	}
}
