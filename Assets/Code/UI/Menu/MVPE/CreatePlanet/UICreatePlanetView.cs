﻿using System;
using Snowship.NPlanet;
using Snowship.NUI.Generic;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.CreatePlanet {
	public class UICreatePlanetView : UIView {

		[SerializeField] private GridLayoutGroup planetViewGridLayoutGroup;

		[SerializeField] private Button backButton;

		[SerializeField] private InputField planetNameInputField;
		[SerializeField] private InputField planetSeedInputField;

		[SerializeField] private Slider planetSizeSlider;
		[SerializeField] private Text planetSizeText;

		[SerializeField] private Slider planetDistanceSlider;
		[SerializeField] private Text planetDistanceText;

		[SerializeField] private Slider temperatureRangeSlider;
		[SerializeField] private Text temperatureRangeText;

		[SerializeField] private Toggle randomOffsetsToggle;

		[SerializeField] private Slider windDirectionSlider;
		[SerializeField] private Text windDirectionText;

		[SerializeField] private GameObject planetTileInfoPanel;
		[SerializeField] private Image planetTileSpriteImage;
		[SerializeField] private Text planetTileBiomeText;
		[SerializeField] private Text planetTilePositionText;

		[SerializeField] private Button refreshPlanetButton;
		[SerializeField] private Button randomizePlanetButton;

		[SerializeField] private Button createPlanetButton;
		[SerializeField] private Image createPlanetButtonImage;

		public event Action OnBackButtonClicked;

		public event Action<string> OnPlanetNameChanged;
		public event Action<string> OnPlanetSeedChanged;

		public event Action<float> OnPlanetSizeSliderChanged;
		public event Action<float> OnPlanetDistanceSliderChanged;
		public event Action<float> OnTemperatureRangeSliderChanged;
		public event Action<bool> OnRandomOffsetsValueChanged;
		public event Action<float> OnWindDirectionSliderChanged;

		public event Action OnRefreshPlanetButtonClicked;
		public event Action OnRandomizePlanetButtonClicked;
		public event Action OnCreatePlanetButtonClicked;

		public override void OnOpen() {
			backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());

			planetNameInputField.onValueChanged.AddListener(planetName => OnPlanetNameChanged?.Invoke(planetName));
			planetSeedInputField.onValueChanged.AddListener(planetSeed => OnPlanetSeedChanged?.Invoke(planetSeed));

			planetSizeSlider.onValueChanged.AddListener(sliderValue => OnPlanetSizeSliderChanged?.Invoke(sliderValue));
			planetDistanceSlider.onValueChanged.AddListener(sliderValue => OnPlanetDistanceSliderChanged?.Invoke(sliderValue));
			temperatureRangeSlider.onValueChanged.AddListener(sliderValue => OnTemperatureRangeSliderChanged?.Invoke(sliderValue));
			randomOffsetsToggle.onValueChanged.AddListener(toggleValue => OnRandomOffsetsValueChanged?.Invoke(toggleValue));
			windDirectionSlider.onValueChanged.AddListener(sliderValue => OnWindDirectionSliderChanged?.Invoke(sliderValue));

			refreshPlanetButton.onClick.AddListener(() => OnRefreshPlanetButtonClicked?.Invoke());
			randomizePlanetButton.onClick.AddListener(() => OnRandomizePlanetButtonClicked?.Invoke());
			createPlanetButton.onClick.AddListener(() => OnCreatePlanetButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public GridLayoutGroup GetPlanetViewGridLayoutGroup() {
			return planetViewGridLayoutGroup;
		}

		public void SetPlanetNameInputField(string text) {
			planetNameInputField.text = text;
		}

		public void SetPlanetSeedInputField(string text) {
			planetSeedInputField.text = text;
		}

		public void SetPlanetSizeSlider(int minValue, int maxValue, int initialValue) {
			planetSizeSlider.minValue = minValue;
			planetSizeSlider.maxValue = maxValue;
			planetSizeSlider.value = initialValue;
		}

		public void SetPlanetSizeText(string text) {
			planetSizeText.text = text;
		}

		public void SetPlanetDistanceSlider(int minValue, int maxValue, int initialValue) {
			planetDistanceSlider.minValue = minValue;
			planetDistanceSlider.maxValue = maxValue;
			planetDistanceSlider.value = initialValue;
		}

		public void SetPlanetDistanceText(string text) {
			planetDistanceText.text = text;
		}

		public void SetTemperatureRangeSlider(int minValue, int maxValue, int initialValue) {
			temperatureRangeSlider.minValue = minValue;
			temperatureRangeSlider.maxValue = maxValue;
			temperatureRangeSlider.value = initialValue;
		}

		public void SetTemperatureRangeText(string text) {
			temperatureRangeText.text = text;
		}

		public void SetRandomOffsetsToggle(bool isOn) {
			randomOffsetsToggle.isOn = isOn;
		}

		public void SetWindDirectionSlider(int minValue, int maxValue, int initialValue) {
			windDirectionSlider.minValue = minValue;
			windDirectionSlider.maxValue = maxValue;
			windDirectionSlider.value = initialValue;
		}

		public void SetWindDirectionText(string text) {
			windDirectionText.text = text;
		}

		public void SetPlanetTileData(PlanetTile planetTile) {

			bool planetTileValid = planetTile != null;
			planetTileInfoPanel.SetActive(planetTileValid);
			planetTilePositionText.gameObject.SetActive(planetTileValid);

			if (!planetTileValid) {
				return;
			}

			planetTileSpriteImage.sprite = planetTile.sprite;
			planetTileBiomeText.text = planetTile.tile.biome.name;
			planetTilePositionText.text = $"({Mathf.FloorToInt(planetTile.tile.position.x)}, {Mathf.FloorToInt(planetTile.tile.position.y)})";
		}

		public void SetCreatePlanetButtonInteractable(bool interactable) {
			createPlanetButton.interactable = interactable;
			createPlanetButtonImage.color = interactable
				? ColourUtilities.GetColour(ColourUtilities.Colours.LightGrey220)
				: ColourUtilities.GetColour(ColourUtilities.Colours.Grey120);
		}
	}
}
