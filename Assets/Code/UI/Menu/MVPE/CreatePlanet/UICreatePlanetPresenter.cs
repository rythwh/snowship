﻿using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UICreatePlanetPresenter : UIPresenter<UICreatePlanetView> {

		private PlanetViewModule planetViewModule;
		private readonly CreatePlanetData createPlanetData = new CreatePlanetData();
		private readonly PPlanet pPlanet = new PPlanet();
		private readonly PColony pColony = new PColony();

		public UICreatePlanetPresenter(UICreatePlanetView view) : base(view) {
		}

		public override void OnCreate() {

			View.OnBackButtonClicked += OnBackButtonClicked;

			View.OnPlanetNameChanged += OnPlanetNameChanged;
			View.OnPlanetSeedChanged += OnPlanetSeedChanged;

			View.OnPlanetSizeSliderChanged += OnPlanetSizeSliderChanged;
			View.OnPlanetDistanceSliderChanged += OnPlanetDistanceSliderChanged;
			View.OnTemperatureRangeSliderChanged += OnTemperatureRangeSliderChanged;
			View.OnWindDirectionSliderChanged += OnWindDirectionSliderChanged;

			View.OnRefreshPlanetButtonClicked += OnRefreshPlanetButtonClicked;
			View.OnRandomizePlanetButtonClicked += OnRandomizePlanetButtonClicked;
			View.OnCreatePlanetButtonClicked += OnCreatePlanetButtonClicked;

			SetPlanetNameInputField();
			SetPlanetSeedInputField();
			SetPlanetSizeSlider();
			SetPlanetDistanceSlider();
			SetTemperatureRangeSlider();
			SetWindDirectionSlider();

			CreatePlanetViewModule();
			CreatePlanetPreview();

			Debug.Log(createPlanetData.ToString());
		}

		public override void OnClose() {

			View.OnBackButtonClicked -= OnBackButtonClicked;

			View.OnPlanetNameChanged -= OnPlanetNameChanged;
			View.OnPlanetSeedChanged -= OnPlanetSeedChanged;

			View.OnPlanetSizeSliderChanged -= OnPlanetSizeSliderChanged;
			View.OnPlanetDistanceSliderChanged -= OnPlanetDistanceSliderChanged;
			View.OnTemperatureRangeSliderChanged -= OnTemperatureRangeSliderChanged;
			View.OnWindDirectionSliderChanged -= OnWindDirectionSliderChanged;

			View.OnRefreshPlanetButtonClicked -= OnRefreshPlanetButtonClicked;
			View.OnRandomizePlanetButtonClicked -= OnRandomizePlanetButtonClicked;
			View.OnCreatePlanetButtonClicked -= OnCreatePlanetButtonClicked;

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

		private void SetPlanetNameInputField() {
			View.SetPlanetNameInputField($"{createPlanetData.Name}");
		}

		private void SetPlanetSeedInputField() {
			View.SetPlanetSeedInputField($"{createPlanetData.Seed}");
		}

		private void SetPlanetSizeSlider() {
			View.SetPlanetSizeSlider(
				0,
				CreatePlanetData.GetNumPlanetSizes() - 1,
				createPlanetData.SizeIndex
			);
		}

		private void SetPlanetDistanceSlider() {
			View.SetPlanetDistanceSlider(
				CreatePlanetData.PlanetDistanceIndexRange.Min,
				CreatePlanetData.PlanetDistanceIndexRange.Max,
				createPlanetData.DistanceIndex
			);
		}

		private void SetTemperatureRangeSlider() {
			View.SetTemperatureRangeSlider(
				CreatePlanetData.PlanetTemperatureIndexRange.Min,
				CreatePlanetData.PlanetTemperatureIndexRange.Max,
				createPlanetData.TemperatureRangeIndex
			);
		}

		private void SetWindDirectionSlider() {
			View.SetWindDirectionSlider(
				CreatePlanetData.PlanetWindDirectionIndexRange.Min,
				CreatePlanetData.PlanetWindDirectionIndexRange.Max - 1,
				createPlanetData.WindDirectionIndex
			);
		}

		private void SetCreatePlanetButtonInteractable(bool interactable) {
			View.SetCreatePlanetButtonInteractable(interactable);
		}

		private void OnBackButtonClicked() {
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void OnPlanetNameChanged(string planetName) {
			bool validPlanetName = createPlanetData.SetName(planetName);
			SetCreatePlanetButtonInteractable(validPlanetName);
		}

		private void OnPlanetSeedChanged(string planetSeed) {
			createPlanetData.Seed = StringUtilities.ParseSeed(planetSeed);
		}

		private void OnPlanetSizeSliderChanged(float sliderValue) {
			createPlanetData.SetSize(Mathf.FloorToInt(sliderValue));
			View.SetPlanetSizeText($"{createPlanetData.Size}");
		}

		private void OnPlanetDistanceSliderChanged(float sliderValue) {
			createPlanetData.SetDistance(Mathf.FloorToInt(sliderValue));
			View.SetPlanetDistanceText($"{createPlanetData.Distance} AU");
		}

		private void OnTemperatureRangeSliderChanged(float sliderValue) {
			createPlanetData.SetTemperatureRange(Mathf.FloorToInt(sliderValue));
			View.SetTemperatureRangeText($"{createPlanetData.TemperatureRange}°C");
		}

		private void OnWindDirectionSliderChanged(float sliderValue) {
			createPlanetData.SetWindDirection(Mathf.FloorToInt(sliderValue));
			View.SetWindDirectionText($"{CreatePlanetData.GetWindCardinalDirectionByIndex(createPlanetData.WindDirectionIndex)}");
		}

		private void OnRefreshPlanetButtonClicked() {
			Debug.Log(createPlanetData.ToString());
			CreatePlanetPreview();
		}

		private void OnRandomizePlanetButtonClicked() {
			RandomizePlanetSettings();
		}

		private void OnCreatePlanetButtonClicked() {
			pPlanet.CreatePlanet(GameManager.Get<PlanetManager>().planet);
			GameManager.Get<UIManager>().OpenViewAsync<UICreateColony>(this, false).Forget();
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			GameManager.Get<PlanetManager>().SetSelectedPlanetTile(planetTile);
			View.SetPlanetTileData(planetTile);
		}

		private void OnColonyTileClicked(PersistenceColony persistenceColony) {
			Debug.LogError("Clicked loaded colony on new planet, this should not be possible!");
		}

		private void CreatePlanetPreview() {
			Planet planet = GameManager.Get<PlanetManager>().CreatePlanet(createPlanetData);
			planetViewModule.DisplayPlanet(
				planet,
				pColony.GetPersistenceColonies(),
				true
			);
		}

		private void RandomizePlanetSettings() {
			createPlanetData.Seed = CreatePlanetData.GenerateRandomPlanetSeed();
			View.SetPlanetSeedInputField($"{createPlanetData.Seed}");
			CreatePlanetPreview();
		}
	}
}