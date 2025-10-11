using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NColony;
using Snowship.NPlanet;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UICreateColonyPresenter : UIPresenter<UICreateColonyView>
	{
		private readonly PlanetManager planetM;
		private readonly UIManager uiM;
		private readonly ColonyManager colonyM;

		private PlanetViewModule planetViewModule;
		private readonly CreateColonyData createColonyData = new CreateColonyData();

		public UICreateColonyPresenter(
			UICreateColonyView view,
			PlanetManager planetM,
			UIManager uiM,
			ColonyManager colonyM
		) : base(view)
		{
			this.planetM = planetM;
			this.uiM = uiM;
			this.colonyM = colonyM;
		}

		public override UniTask OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;

			View.OnColonyNameChanged += OnColonyNameChanged;
			View.OnRandomizeColonyNameButtonClicked += OnRandomizeColonyNameButtonClicked;
			View.OnMapSizeSliderChanged += OnMapSizeSliderChanged;
			View.OnMapSeedChanged += OnMapSeedChanged;

			View.OnCreateColonyButtonClicked += OnCreateColonyButtonClicked;

			SetColonyNameInputField();
			SetMapSeedInputField();
			SetMapSizeSlider();
			SetCreateColonyButtonInteractable();

			CreatePlanetViewModule();
			CreatePlanetPreview();

			return UniTask.CompletedTask;
		}

		public override void OnClose() {
			View.OnBackButtonClicked -= OnBackButtonClicked;

			View.OnColonyNameChanged -= OnColonyNameChanged;
			View.OnRandomizeColonyNameButtonClicked -= OnRandomizeColonyNameButtonClicked;
			View.OnMapSizeSliderChanged -= OnMapSizeSliderChanged;
			View.OnMapSeedChanged -= OnMapSeedChanged;

			View.OnCreateColonyButtonClicked -= OnCreateColonyButtonClicked;

			ClosePlanetViewModule();
		}

		private void CreatePlanetViewModule() {
			planetViewModule = new PlanetViewModule(View.PlanetViewGridLayoutGroup, View.PlanetTilePrefab);
			planetViewModule.DisplayPlanet(
				planetM.planet,
				// pColony.GetPersistenceColonies(),
				true
			);

			planetViewModule.OnPlanetTileClicked += OnPlanetTileClicked;
			// planetViewModule.OnColonyTileClicked += OnColonyTileClicked;
		}

		private void ClosePlanetViewModule() {
			planetViewModule.OnPlanetTileClicked -= OnPlanetTileClicked;
			// planetViewModule.OnColonyTileClicked -= OnColonyTileClicked;

			planetViewModule.DestroyPlanet();
		}

		private void CreatePlanetPreview() {
			Planet planet = planetM.planet;
			planetViewModule.DisplayPlanet(
				planet,
				// pColony.GetPersistenceColonies(),
				false
			);
			OnPlanetTileClicked(planetM.selectedPlanetTile);
		}

		private void SetColonyNameInputField() {
			View.SetColonyNameInputField($"{createColonyData.Name}");
		}

		private void SetMapSeedInputField() {
			View.SetMapSeedInputField($"{createColonyData.Seed}");
		}

		private void SetMapSizeSlider() {
			View.SetupMapSizeSlider(
				CreateColonyData.MapSizeIndexRange.Min,
				CreateColonyData.MapSizeIndexRange.Max,
				createColonyData.SizeIndex
			);
		}

		private void SetCreateColonyButtonInteractable() {
			View.SetSaveColonyButtonInteractable(createColonyData.CanCreateColony());
		}

		private void OnBackButtonClicked() {
			uiM.GoBack(this);
		}

		private void OnColonyNameChanged(string colonyName) {
			bool validColonyName = createColonyData.SetName(colonyName);
			if (!validColonyName) {
				View.SetColonyNameInputField(string.Empty);
			}
			SetCreateColonyButtonInteractable();
		}

		private void OnRandomizeColonyNameButtonClicked() {
			View.SetColonyNameInputField(CreateColonyData.GenerateRandomColonyName());
		}

		private void OnMapSeedChanged(string mapSeed) {
			createColonyData.Seed = StringUtilities.ParseSeed(mapSeed);
		}

		private void OnMapSizeSliderChanged(float sliderValue) {
			createColonyData.SetSize(Mathf.FloorToInt(sliderValue));
			View.SetMapSizeText($"{createColonyData.Size}²");
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			bool planetTileValid = createColonyData.SetPlanetTile(planetTile);
			SetCreateColonyButtonInteractable();
			View.SetPlanetTileData(planetTile, planetTileValid);
		}

		private void OnCreateColonyButtonClicked() {
			colonyM.CreateColony(createColonyData);
		}

		// private void OnColonyTileClicked(PersistenceColony persistenceColony) {
		// 	// TODO Popup asking to load colony?
		// }
	}
}
