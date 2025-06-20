﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NPersistence;
using Snowship.NPlanet;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UILoadColonyPresenter : UIPresenter<UILoadColonyView> {

		private PlanetViewModule planetViewModule;
		private PersistenceColony selectedColony;
		private readonly PColony pColony = new PColony();

		public UILoadColonyPresenter(UILoadColonyView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;
			View.OnCreateColonyButtonClicked += OnCreateColonyButtonClicked;
			View.OnLoadColonyButtonClicked += OnLoadColonyButtonClicked;

			CreateColonyElements();

			CreatePlanetViewModule();
		}

		public override void OnClose() {
			View.OnBackButtonClicked -= OnBackButtonClicked;
			View.OnCreateColonyButtonClicked -= OnCreateColonyButtonClicked;
			View.OnLoadColonyButtonClicked -= OnLoadColonyButtonClicked;

			ClosePlanetViewModule();
		}

		private void CreatePlanetViewModule() {
			planetViewModule = new PlanetViewModule(View.PlanetViewGridLayoutGroup, View.PlanetTilePrefab);

			planetViewModule.OnPlanetTileClicked += OnPlanetTileClicked;
			planetViewModule.OnColonyTileClicked += OnColonyTileClicked;
		}

		private void ClosePlanetViewModule() {
			planetViewModule.OnPlanetTileClicked -= OnPlanetTileClicked;
			planetViewModule.OnColonyTileClicked -= OnColonyTileClicked;

			planetViewModule.DestroyPlanet();
		}

		private void OnBackButtonClicked() {
			GameManager.Get<UIManager>().CloseView(this);
		}

		private void OnCreateColonyButtonClicked() {
			GameManager.Get<UIManager>().OpenViewAsync<UICreateColony>(this, false).Forget();
		}

		private void OnColonyElementClicked(PersistenceColony colony) {
			SelectColony(colony);
		}

		private void OnLoadColonyButtonClicked() {
			pColony.ApplyLoadedColony(selectedColony);
		}

		private void CreateColonyElements() {
			List<PersistenceColony> colonies = pColony.GetPersistenceColonies();

			planetViewModule.DisplayPlanet(GameManager.Get<PlanetManager>().planet, colonies, true);

			foreach (PersistenceColony colony in colonies) {
				UILoadColonyElement loadColonyElement = new(colony);
				loadColonyElement.Open(View.ColonyElementsParent).Forget();
				loadColonyElement.OnLoadColonyElementClicked += OnColonyElementClicked;
			}
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			// Do nothing
		}

		private void OnColonyTileClicked(PersistenceColony colony) {
			SelectColony(colony);
		}

		private void SelectColony(PersistenceColony colony) {
			selectedColony = colony;

			bool colonyValid = selectedColony != null;
			string loadColonyButtonText = colonyValid ? $"Load {colony.name}" : "Select a Colony to Load";
			View.SetLoadColonyButtonInteractable(colonyValid, loadColonyButtonText);
		}
	}
}