using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : BaseManager {

	private GameManager startCoroutineReference;

	public void SetStartCoroutineReference(GameManager startCoroutineReference) {
		this.startCoroutineReference = startCoroutineReference;
	}

	public static readonly string gameVersionString = "Snowship " + PersistenceManager.gameVersion.Value;
	public static readonly string disclaimerText = "<size=20>" + gameVersionString + "</size>\nSnowship by Ryan White - flizzehh.itch.io/snowship\nThis game is a work in progress and subject to major changes.";

	public bool playerTyping = false;

	public static string SplitByCapitals(string combinedString) {
		var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
				 RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString, " ");
	}

	public static Color HexToColor(string hexString) {
		int r = int.Parse("" + hexString[0] + hexString[1], System.Globalization.NumberStyles.HexNumber);
		int g = int.Parse("" + hexString[2] + hexString[3], System.Globalization.NumberStyles.HexNumber);
		int b = int.Parse("" + hexString[4] + hexString[5], System.Globalization.NumberStyles.HexNumber);
		return new Color(r, g, b, 255f) / 255f;
	}

	public static string RemoveNonAlphanumericChars(string removeFromString) {
		return new Regex("[^a-zA-Z0-9 -]").Replace(removeFromString, string.Empty);
	}

	public enum Colours { DarkRed, DarkGreen, LightRed, LightGreen, LightGrey220, LightGrey200, LightGrey180, Grey150, Grey120, DarkGrey50, LightBlue, LightOrange, DarkOrange, White, DarkYellow, LightYellow, LightPurple, DarkPurple };

	private static readonly Dictionary<Colours, Color> colourMap = new Dictionary<Colours, Color>() {
		{ Colours.DarkRed, new Color(192f, 57f, 43f, 255f) / 255f },
		{ Colours.DarkGreen, new Color(39f, 174f, 96f, 255f) / 255f },
		{ Colours.LightRed, new Color(231f, 76f, 60f, 255f) / 255f },
		{ Colours.LightGreen, new Color(46f, 204f, 113f, 255f) / 255f },
		{ Colours.LightGrey220, new Color(220f, 220f, 220f, 255f) / 255f },
		{ Colours.LightGrey200, new Color(200f, 200f, 200f, 255f) / 255f },
		{ Colours.LightGrey180, new Color(180f, 180f, 180f, 255f) / 255f },
		{ Colours.Grey150, new Color(150f, 150f, 150f, 255f) / 255f },
		{ Colours.Grey120, new Color(120f, 120f, 120f, 255f) / 255f },
		{ Colours.DarkGrey50, new Color(50f, 50f, 50f, 255f) / 255f },
		{ Colours.LightBlue, new Color(52f, 152f, 219f, 255f) / 255f },
		{ Colours.LightOrange, new Color(230f, 126f, 34f, 255f) / 255f },
		{ Colours.DarkOrange, new Color(211f, 84f, 0f, 255f) / 255f },
		{ Colours.White, new Color(255f, 255f, 255f, 255f) / 255f },
		{ Colours.DarkYellow, new Color(216f, 176f, 15f, 255f) / 255f },
		{ Colours.LightYellow, new Color(241f, 196f, 15f, 255f) / 255f },
		{ Colours.LightPurple, new Color(155f, 89f, 182f, 255f) / 255f },
		{ Colours.DarkPurple, new Color(142f, 68f, 173f, 255f) / 255f }
	};

	public static Color GetColour(Colours colourKey) {
		return colourMap[colourKey];
	}

	public static bool IsAlphanumericWithSpaces(string text) {
		return Regex.IsMatch(text, @"^[A-Za-z0-9 ]*[A-Za-z0-9][A-Za-z0-9 ]*$");
	}

	public int screenWidth = 0;
	public int screenHeight = 0;

	public GameObject canvas;

	public void SetupUI() {

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		canvas = GameObject.Find("Canvas");

		SetupMainMenu();

		SetupLoadUniverseUI();
		SetupCreateUniverseUI();
		SetupLoadPlanetUI();
		SetupCreatePlanetUI();
		SetupLoadColonyUI();
		SetupCreateColonyUI();
		SetupLoadSaveUI();

		SetupLoadingScreen();

		SetupGameUI();

		SetupPauseMenu();

		SetLoadUniverseActive(false);
		SetCreateUniverseActive(false);
		SetLoadPlanetActive(false);
		SetCreatePlanetActive(false);
		SetLoadColonyActive(false);
		SetCreateColonyActive(false);
		SetLoadSaveActive(false);

		SetSettingsMenuActive(false);

		SetPauseMenuActive(false);

		InitializeTileInformation();
		InitializeSelectedContainerIndicator();
		InitializeSelectedManufacturingTileObjectIndicator();

		GetMainMenuContinueFile();

		CreateBottomLeftMenus();
		CreateProfessionsList();
		CreateClothesList();
		CreateResourcesList();

		selectedMTOFuelPanel = new MTOPanel(mtoFuelPanelObj, true);
		selectedMTONoFuelPanel = new MTOPanel(mtoNoFuelPanelObj, false);

		SetSelectedColonistInformation(false);
		SetSelectedTraderMenu();
		SetSelectedContainerInfo();

		SetCaravanElements();
		SetJobElements();

		SetLoadingScreenActive(false);
	}

	public GameObject mainMenu;
	private GameObject mainMenuBackground;
	private GameObject mainMenuButtonsPanel;
	private GameObject snowshipLogo;
	private GameObject darkBackground;

	private GameObject settingsPanel;

	private void SetupMainMenu() {
		mainMenu = canvas.transform.Find("MainMenu").gameObject;

		mainMenu.transform.Find("Disclaimer-Text").GetComponent<Text>().text = disclaimerText;

		mainMenuBackground = mainMenu.transform.Find("MainMenuBackground-Image").gameObject;
		snowshipLogo = mainMenu.transform.Find("SnowshipLogo-Image").gameObject;
		SetMainMenuBackground();

		darkBackground = mainMenu.transform.Find("DarkBackground-Image").gameObject;

		mainMenuButtonsPanel = mainMenu.transform.Find("MainMenuButtons-Panel").gameObject;

		settingsPanel = canvas.transform.Find("Settings-Panel").gameObject;
		settingsPanel.transform.Find("SettingsCancel-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(false); });
		settingsPanel.transform.Find("SettingsApply-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.persistenceM.ApplySettings(); });
		settingsPanel.transform.Find("SettingsAccept-Button").GetComponent<Button>().onClick.AddListener(delegate {
			GameManager.persistenceM.ApplySettings();
			SetSettingsMenuActive(false);
		});

		mainMenuButtonsPanel.transform.Find("New-Button").GetComponent<Button>().onClick.AddListener(delegate { SetCreateUniverseActive(true); });

		mainMenuButtonsPanel.transform.Find("Continue-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.persistenceM.ContinueFromMostRecentSave(); });
		mainMenuButtonsPanel.transform.Find("Continue-Button").GetComponent<HoverToggleScript>().Initialize(mainMenu.transform.Find("MainMenuButtons-Panel/Continue-Button/LoadFilePanelParent-Panel").gameObject);

		mainMenuButtonsPanel.transform.Find("Load-Button").GetComponent<Button>().onClick.AddListener(delegate { SetLoadUniverseActive(true); });
		mainMenuButtonsPanel.transform.Find("Settings-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(true); });
		mainMenuButtonsPanel.transform.Find("Exit-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });
	}

	private enum PlanetPreviewState {
		CreatePlanet, CreateColony, LoadColony, Nothing
	}

	private GameObject loadUniversePanel;

	private Transform universesListPanel;

	private Button loadUniverseButton;

	private List<UniverseElement> universeElements = new List<UniverseElement>();

	private UniverseElement selectedUniverseElement;

	private class UniverseElement {
		public PersistenceManager.PersistenceUniverse persistenceUniverse;

		public GameObject obj;

		public UniverseElement(PersistenceManager.PersistenceUniverse persistenceUniverse, Transform parent) {
			this.persistenceUniverse = persistenceUniverse;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/UniverseElement-Panel"), parent, false);

			obj.transform.Find("UniverseName-Text").GetComponent<Text>().text = persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name];

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.LastSaveDateTime];

			string saveVersion = persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion];
			string gameVersion = persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion];

			string saveVersionColour = ColorUtility.ToHtmlStringRGB(GetColour(Colours.DarkGrey50));
			string gameVersionColour = ColorUtility.ToHtmlStringRGB(GetColour(Colours.DarkGrey50));

			if (saveVersion == PersistenceManager.saveVersion.Value && gameVersion == PersistenceManager.gameVersion.Value) {
				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedUniverseElement(this); });
			} else {
				obj.GetComponent<Image>().color = GetColour(Colours.DarkRed);
				obj.GetComponent<Button>().interactable = false;

				if (saveVersion != PersistenceManager.saveVersion.Value) {
					saveVersionColour = ColorUtility.ToHtmlStringRGB((GetColour(Colours.DarkRed) + GetColour(Colours.DarkGrey50)) / 2f);
				}
				if (gameVersion != PersistenceManager.gameVersion.Value) {
					gameVersionColour = ColorUtility.ToHtmlStringRGB((GetColour(Colours.DarkRed) + GetColour(Colours.DarkGrey50)) / 2f);
				}
			}

			obj.transform.Find("LastSaveData-Panel/VersionGameSave-Text").GetComponent<Text>().text = string.Format(
				"{0}{1}{2}{3} {4}{5}{6}{7}",
				"<color=#" + gameVersionColour + ">",
				"G",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion],
				"</color>",
				"<color=#" + saveVersionColour + ">",
				"S",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion],
				"</color>"
			);
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedUniverseElement(UniverseElement universeElement) {
		selectedUniverseElement = universeElement;

		loadUniverseButton.interactable = selectedUniverseElement != null;
		if (selectedUniverseElement != null) {
			loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedUniverseElement.persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name];
		} else {
			loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = "Select a Universe to Load";
		}
	}

	private void SetupLoadUniverseUI() {
		loadUniversePanel = mainMenu.transform.Find("LoadUniverse-Panel").gameObject;

		Button backButton = loadUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.universeM.SetUniverse(null);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedUniverseElement(null);

			SetLoadUniverseActive(false);
		});

		universesListPanel = loadUniversePanel.transform.Find("Universes-ScrollPanel/UniversesList-Panel");

		loadUniverseButton = loadUniversePanel.transform.Find("LoadUniverse-Button").GetComponent<Button>();
		loadUniverseButton.onClick.AddListener(delegate {
			if (selectedUniverseElement != null) {
				GameManager.persistenceM.ApplyLoadedConfiguration(selectedUniverseElement.persistenceUniverse);
				GameManager.persistenceM.ApplyLoadedUniverse(selectedUniverseElement.persistenceUniverse);

				SetSelectedUniverseElement(null);

				SetLoadUniverseActive(false);
				SetLoadPlanetActive(true);
			}
		});
		SetSelectedUniverseElement(null);
	}

	private void SetLoadUniverseActive(bool active) {
		loadUniversePanel.SetActive(active);
		ToggleMainMenuButtons(loadUniversePanel);

		foreach (UniverseElement universeElement in universeElements) {
			universeElement.Delete();
		}
		universeElements.Clear();

		if (loadUniversePanel.activeSelf) {
			foreach (PersistenceManager.PersistenceUniverse persistenceUniverse in GameManager.persistenceM.GetPersistenceUniverses()) {
				universeElements.Add(new UniverseElement(persistenceUniverse, universesListPanel));
			}
		}
	}

	private GameObject createUniversePanel;

	private InputField universeNameInputField;

	private void SetupCreateUniverseUI() {
		createUniversePanel = mainMenu.transform.Find("CreateUniverse-Panel").gameObject;

		Button backButton = createUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.universeM.SetUniverse(null);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			universeNameInputField.text = string.Empty;

			SetCreateUniverseActive(false);
		});

		universeNameInputField = createUniversePanel.transform.Find("UniverseName-Panel/InputField").GetComponent<InputField>();

		Button saveUniverseButton = createUniversePanel.transform.Find("SaveUniverse-Button").GetComponent<Button>();
		saveUniverseButton.onClick.AddListener(delegate {
			GameManager.universeM.CreateUniverse(universeNameInputField.text);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			universeNameInputField.text = string.Empty;

			SetCreateUniverseActive(false);
			SetCreatePlanetActive(true);
		});
		saveUniverseButton.interactable = false;
		saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = GetColour(Colours.Grey120);

		universeNameInputField.onValueChanged.AddListener(delegate {
			bool validUniverseName = !string.IsNullOrEmpty(universeNameInputField.text) && IsAlphanumericWithSpaces(universeNameInputField.text);
			saveUniverseButton.interactable = validUniverseName;
			saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = validUniverseName ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
		});
	}

	private void SetCreateUniverseActive(bool active) {
		createUniversePanel.SetActive(active);
		ToggleMainMenuButtons(createUniversePanel);
	}

	private GameObject loadPlanetPanel;

	private Transform planetsListPanel;

	private Button loadPlanetButton;

	private List<PlanetElement> planetElements = new List<PlanetElement>();

	private PlanetElement selectedPlanetElement;

	private class PlanetElement {
		public PersistenceManager.PersistencePlanet persistencePlanet;

		public GameObject obj;

		public PlanetElement(PersistenceManager.PersistencePlanet persistencePlanet, Transform parent) {
			this.persistencePlanet = persistencePlanet;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/PlanetElement-Panel"), parent, false);

			obj.transform.Find("PlanetName-Text").GetComponent<Text>().text = persistencePlanet.name;

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistencePlanet.lastSaveDateTime;

			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedPlanetElement(this); });
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedPlanetElement(PlanetElement planetElement) {
		selectedPlanetElement = planetElement;

		loadPlanetButton.interactable = selectedPlanetElement != null;
		if (selectedPlanetElement != null) {
			loadPlanetButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedPlanetElement.persistencePlanet.name;
		} else {
			loadPlanetButton.transform.Find("Text").GetComponent<Text>().text = "Select a Planet to Load";
		}
	}

	private void SetupLoadPlanetUI() {
		loadPlanetPanel = mainMenu.transform.Find("LoadPlanet-Panel").gameObject;

		Button backButton = loadPlanetPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetElement(null);

			SetLoadPlanetActive(false);
			SetLoadUniverseActive(true);
		});

		Button createPlanetButton = loadPlanetPanel.transform.Find("CreatePlanet-Button").GetComponent<Button>();
		createPlanetButton.onClick.AddListener(delegate {
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetElement(null);

			SetLoadPlanetActive(false);
			SetCreatePlanetActive(true);
		});

		planetsListPanel = loadPlanetPanel.transform.Find("Planets-ScrollPanel/PlanetsList-Panel");

		loadPlanetButton = loadPlanetPanel.transform.Find("LoadPlanet-Button").GetComponent<Button>();
		loadPlanetButton.onClick.AddListener(delegate {
			if (selectedPlanetElement != null) {
				GameManager.persistenceM.ApplyLoadedPlanet(selectedPlanetElement.persistencePlanet);
				GameManager.colonyM.SetColony(null);

				SetLoadPlanetActive(false);
				SetLoadColonyActive(true);
			}
		});
		SetSelectedPlanetElement(null);
	}

	private void SetLoadPlanetActive(bool active) {
		loadPlanetPanel.SetActive(active);
		ToggleMainMenuButtons(loadPlanetPanel);

		foreach (PlanetElement planetElement in planetElements) {
			planetElement.Delete();
		}
		planetElements.Clear();

		if (loadPlanetPanel.activeSelf) {
			foreach (PersistenceManager.PersistencePlanet persistencePlanet in GameManager.persistenceM.GetPersistencePlanets()) {
				planetElements.Add(new PlanetElement(persistencePlanet, planetsListPanel));
			}
		}
	}

	private GameObject createPlanetPanel;

	private InputField planetRegenerationCodeInputField;
	private Button planetRegenerationCodeButton;

	private RectTransform createPlanetPlanetTilesRectTransform;

	private InputField planetNameInputField;
	private InputField planetSeedInputField;
	private Slider planetSizeSlider;
	private Slider planetDistanceSlider;
	private Slider temperatureRangeSlider;
	private Toggle randomOffsetsToggle;
	private Slider windDirectionSlider;

	private GameObject createPlanetSelectedPlanetTileInfoPanel;
	private Image createPlanetSelectedPlanetTileSpriteImage;
	private Text createPlanetSelectedPlanetTileBiomeText;
	private Text createPlanetSelectedPlanetTilePositionText;

	private void SetupCreatePlanetUI() {
		createPlanetPanel = mainMenu.transform.Find("CreatePlanet-Panel").gameObject;

		createPlanetPlanetTilesRectTransform = createPlanetPanel.transform.Find("PlanetPreview-Panel").GetComponent<RectTransform>();

		Button backButton = createPlanetPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);

			SetCreatePlanetDefaultSettings();

			SetCreatePlanetActive(false);
			SetLoadUniverseActive(true);
		});

		planetRegenerationCodeInputField = createPlanetPanel.transform.Find("PlanetRegenerationCode-Panel/InputField").GetComponent<InputField>();
		planetRegenerationCodeButton = createPlanetPanel.transform.Find("PlanetRegenerationCode-Panel/LoadPlanetCode-Button").GetComponent<Button>();

		planetNameInputField = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetName-Panel/InputField").GetComponent<InputField>();

		planetSeedInputField = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSeed-Panel/InputField").GetComponent<InputField>();

		planetSizeSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSize-Slider").GetComponent<Slider>();
		planetSizeSlider.minValue = 0;
		planetSizeSlider.maxValue = PlanetManager.GetNumPlanetSizes() - 1;
		Text planetSizeText = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSizeValue-Text").GetComponent<Text>();
		planetSizeText.text = PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)).ToString();
		planetSizeSlider.onValueChanged.AddListener(delegate {
			planetSizeText.text = PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)).ToString();
		});

		planetDistanceSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistance-Slider").GetComponent<Slider>();
		planetDistanceSlider.minValue = PlanetManager.GetMinPlanetDistance();
		planetDistanceSlider.maxValue = PlanetManager.GetMaxPlanetDistance();
		Text planetDistanceText = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistanceValue-Text").GetComponent<Text>();
		planetDistanceText.text = PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)).ToString() + " AU";
		planetDistanceSlider.onValueChanged.AddListener(delegate {
			planetDistanceText.text = PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)).ToString() + " AU";
		});

		temperatureRangeSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRange-Slider").GetComponent<Slider>();
		temperatureRangeSlider.minValue = 0;
		temperatureRangeSlider.maxValue = 10;
		Text temperatureRangeText = createPlanetPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRangeValue-Text").GetComponent<Text>();
		temperatureRangeText.text = PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)) + "°C";
		temperatureRangeSlider.onValueChanged.AddListener(delegate {
			temperatureRangeText.text = PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)) + "°C";
		});

		randomOffsetsToggle = createPlanetPanel.transform.Find("PlanetSettings-Panel/RandomOffsets-Toggle").GetComponent<Toggle>();

		windDirectionSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirection-Slider").GetComponent<Slider>();
		windDirectionSlider.minValue = 0;
		windDirectionSlider.maxValue = PlanetManager.GetNumWindDirections() - 1;
		Text windDirectionText = createPlanetPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirectionValue-Text").GetComponent<Text>();
		windDirectionText.text = PlanetManager.GetWindCardinalDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value));
		windDirectionSlider.onValueChanged.AddListener(delegate {
			windDirectionText.text = PlanetManager.GetWindCardinalDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value));
		});

		SetCreatePlanetDefaultSettings();

		Button refreshPreviewButton = createPlanetPanel.transform.Find("PlanetSettings-Panel/RefreshAndRandomize-Panel/RefreshPreview-Button").GetComponent<Button>();
		refreshPreviewButton.onClick.AddListener(delegate {
			CreateNewPlanetFromSettings(true);
		});

		Button randomizeButton = createPlanetPanel.transform.Find("PlanetSettings-Panel/RefreshAndRandomize-Panel/Randomize-Button").GetComponent<Button>();
		randomizeButton.onClick.AddListener(delegate {
			RandomizePlanetSettings();
		});

		createPlanetSelectedPlanetTileInfoPanel = createPlanetPanel.transform.Find("SelectedPlanetTileInfo-Panel").gameObject;

		createPlanetSelectedPlanetTileSpriteImage = createPlanetSelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileSprite-Image").GetComponent<Image>();
		createPlanetSelectedPlanetTileBiomeText = createPlanetSelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileBiome-Panel/BiomeValue-Text").GetComponent<Text>();
		createPlanetSelectedPlanetTilePositionText = createPlanetPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>();

		Button savePlanetButton = createPlanetPanel.transform.Find("SavePlanet-Button").GetComponent<Button>();
		savePlanetButton.onClick.AddListener(delegate {
			GameManager.persistenceM.CreatePlanet(CreateNewPlanetFromSettings(false));
			GameManager.colonyM.SetColony(null);

			SetCreatePlanetActive(false);
			SetCreateColonyActive(true);
		});

		planetNameInputField.onValueChanged.AddListener(delegate {
			bool validPlanetName = IsAlphanumericWithSpaces(planetNameInputField.text);
			savePlanetButton.interactable = validPlanetName;
			savePlanetButton.transform.Find("Image").GetComponent<Image>().color = validPlanetName ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
		});
	}

	private void SetCreatePlanetDefaultSettings() {
		planetNameInputField.text = PlanetManager.GetRandomPlanetName();

		planetSeedInputField.text = PlanetManager.GetRandomPlanetSeed().ToString();

		planetSizeSlider.value = Mathf.FloorToInt((planetSizeSlider.minValue + planetSizeSlider.maxValue) / 2f) + 2; // 60

		planetDistanceSlider.value = Mathf.FloorToInt((planetDistanceSlider.minValue + planetDistanceSlider.maxValue) / 2f); // 1 AU

		temperatureRangeSlider.value = Mathf.FloorToInt((temperatureRangeSlider.minValue + temperatureRangeSlider.maxValue) / 2f) + 2; // 70°C

		randomOffsetsToggle.isOn = true;

		windDirectionSlider.value = UnityEngine.Random.Range(windDirectionSlider.minValue, windDirectionSlider.maxValue);
	}

	private PlanetManager.Planet CreateNewPlanetFromSettings(bool displayPlanet) {
		PlanetManager.Planet planet = GameManager.planetM.CreatePlanet(
			planetNameInputField.text, 
			ParseSeed(planetSeedInputField.text),
			PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)),
			PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)),
			PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)),
			randomOffsetsToggle.isOn,
			PlanetManager.GetWindCircularDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value))
		);

		if (displayPlanet) {
			DisplayPlanet(
				planet,
				createPlanetPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
				createColonyPlanetTilesRectTransform,
				PlanetPreviewState.CreatePlanet
			);
		}

		return planet;
	}

	private void RandomizePlanetSettings() {
		planetSeedInputField.text = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
	}

	private void SetCreatePlanetActive(bool active) {
		createPlanetPanel.SetActive(active);
		ToggleMainMenuButtons(createPlanetPanel);

		SetCreatePlanetDefaultSettings();

		if (createPlanetPanel.activeSelf) {
			RandomizePlanetSettings();
			CreateNewPlanetFromSettings(true);
			SetSelectedPlanetTile(null, PlanetPreviewState.CreatePlanet);
		} else {
			ClearPlanet();
		}
	}

	private GameObject loadColonyPanel;

	private RectTransform loadColonyPlanetTilesRectTransform;

	private Transform coloniesListPanel;

	private Button loadColonyButton;

	private List<ColonyElement> colonyElements = new List<ColonyElement>();

	private ColonyElement selectedColonyElement;

	private class ColonyElement {
		public PersistenceManager.PersistenceColony persistenceColony;

		public GameObject obj;

		public ColonyElement(PersistenceManager.PersistenceColony persistenceColony, Transform parent) {
			this.persistenceColony = persistenceColony;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonyElement-Panel"), parent, false);

			obj.transform.Find("ColonyName-Panel/ColonyName-Text").GetComponent<Text>().text = persistenceColony.name;

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistenceColony.lastSaveDateTime;

			if (persistenceColony.lastSaveImage != null) {
				obj.transform.Find("ColonyLastSave-Image").GetComponent<Image>().sprite = persistenceColony.lastSaveImage;
			}

			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedColonyElement(this); });
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedColonyElement(ColonyElement colonyElement) {
		selectedColonyElement = colonyElement;

		loadColonyButton.interactable = selectedColonyElement != null;
		if (selectedColonyElement != null) {
			loadColonyButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedColonyElement.persistenceColony.name;
		} else {
			loadColonyButton.transform.Find("Text").GetComponent<Text>().text = "Select a Colony to Load";
		}
	}

	private void SetupLoadColonyUI() {
		loadColonyPanel = mainMenu.transform.Find("LoadColony-Panel").gameObject;

		Button backButton = loadColonyPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.colonyM.SetColony(null);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetLoadPlanetActive(true);
		});

		Button createColonyButton = loadColonyPanel.transform.Find("CreateColony-Button").GetComponent<Button>();
		createColonyButton.onClick.AddListener(delegate {
			GameManager.colonyM.SetColony(null);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetCreateColonyActive(true);
		});

		loadColonyPlanetTilesRectTransform = loadColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<RectTransform>();

		coloniesListPanel = loadColonyPanel.transform.Find("Colonies-ScrollPanel/ColoniesList-Panel");

		loadColonyButton = loadColonyPanel.transform.Find("LoadColony-Button").GetComponent<Button>();
		loadColonyButton.onClick.AddListener(delegate {
			GameManager.persistenceM.ApplyLoadedColony(selectedColonyElement.persistenceColony);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetLoadSaveActive(true);
		});
		SetSelectedColonyElement(null);
	}

	private void SetLoadColonyActive(bool active) {
		loadColonyPanel.SetActive(active);
		ToggleMainMenuButtons(loadColonyPanel);

		foreach (ColonyElement colonyElement in colonyElements) {
			colonyElement.Delete();
		}
		colonyElements.Clear();

		if (loadColonyPanel.activeSelf) {
			DisplayPlanet(
				GameManager.planetM.planet,
				loadColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
				loadColonyPlanetTilesRectTransform,
				PlanetPreviewState.LoadColony
			);

			foreach (PersistenceManager.PersistenceColony persistenceColony in GameManager.persistenceM.GetPersistenceColonies()) {
				colonyElements.Add(new ColonyElement(persistenceColony, coloniesListPanel));
			}
		} else {
			ClearPlanet();
		}
	}

	private GameObject createColonyPanel;

	private InputField mapRegenerationCodeInputField;
	private Button mapRegenerationCodeButton;

	private RectTransform createColonyPlanetTilesRectTransform;

	private InputField colonyNameInputField;
	private InputField mapSeedInputField;
	private Slider mapSizeSlider;

	private GameObject createColonySelectedPlanetTileInfoPanel;
	private Image createColonySelectedPlanetTileSpriteImage;
	private Text createColonySelectedPlanetTileBiomeText;
	private Text createColonySelectedPlanetTileAverageTemperatureText;
	private Text createColonySelectedPlanetTileAveragePrecipitationText;
	private Text createColonySelectedPlanetTileAltitudeText;
	private Text createColonySelectedPlanetTilePositionText;

	private Button saveColonyButton;

	private void SetupCreateColonyUI() {
		createColonyPanel = mainMenu.transform.Find("CreateColony-Panel").gameObject;

		Button backButton = createColonyPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			SetCreateColonyDefaultSettings();

			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);

			SetCreateColonyActive(false);
			SetLoadPlanetActive(true);
		});

		mapRegenerationCodeInputField = createColonyPanel.transform.Find("MapRegenerationCode-Panel/InputField").GetComponent<InputField>();
		mapRegenerationCodeButton = createColonyPanel.transform.Find("MapRegenerationCode-Panel/LoadMapCode-Button").GetComponent<Button>();

		createColonyPlanetTilesRectTransform = createColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<RectTransform>();

		colonyNameInputField = createColonyPanel.transform.Find("MapSettings-Panel/ColonyName-Panel/InputField").GetComponent<InputField>();

		mapSeedInputField = createColonyPanel.transform.Find("MapSettings-Panel/MapSeed-Panel/InputField").GetComponent<InputField>();
		
		mapSizeSlider = createColonyPanel.transform.Find("MapSettings-Panel/MapSize-Panel/MapSize-Slider").GetComponent<Slider>();
		mapSizeSlider.minValue = 0;
		mapSizeSlider.maxValue = ColonyManager.GetNumMapSizes() - 1;
		Text mapSizeText = createColonyPanel.transform.Find("MapSettings-Panel/MapSize-Panel/MapSizeValue-Text").GetComponent<Text>();
		mapSizeText.text = ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)).ToString();
		mapSizeSlider.onValueChanged.AddListener(delegate {
			mapSizeText.text = ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)).ToString();
		});

		SetCreateColonyDefaultSettings();

		createColonySelectedPlanetTileInfoPanel = createColonyPanel.transform.Find("SelectedPlanetTileInfo-Panel").gameObject;

		createColonySelectedPlanetTileSpriteImage = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileSprite-Image").GetComponent<Image>();
		createColonySelectedPlanetTileBiomeText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileBiome-Panel/BiomeValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAverageTemperatureText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAverageTemperature-Panel/TemperatureValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAveragePrecipitationText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAveragePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAltitudeText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTilePositionText = createColonyPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>();

		saveColonyButton = createColonyPanel.transform.Find("SaveColony-Button").GetComponent<Button>();
		saveColonyButton.onClick.AddListener(delegate {
			ColonyManager.Colony colony = GameManager.colonyM.CreateColony(
				colonyNameInputField.text,
				GameManager.planetM.selectedPlanetTile.tile.position,
				ParseSeed(mapSeedInputField.text), 
				ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)),
				GameManager.planetM.selectedPlanetTile.averageTemperature,
				GameManager.planetM.selectedPlanetTile.tile.GetPrecipitation(),
				GameManager.planetM.selectedPlanetTile.terrainTypeHeights,
				GameManager.planetM.selectedPlanetTile.surroundingPlanetTileHeightDirections,
				GameManager.planetM.selectedPlanetTile.isRiver,
				GameManager.planetM.selectedPlanetTile.surroundingPlanetTileRivers
			);

			GameManager.colonyM.SetupNewColony(colony, false);

			SetCreateColonyActive(false);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);
		});

		colonyNameInputField.onValueChanged.AddListener(delegate {
			SetSaveColonyButtonInteractable();
		});
	}

	public void SetCreateColonyDefaultSettings() {
		colonyNameInputField.text = ColonyManager.GetRandomColonyName();

		mapSeedInputField.text = TileManager.Map.GetRandomMapSeed().ToString();

		mapSizeSlider.value = 1;
	}

	private void SetCreateColonyActive(bool active) {
		createColonyPanel.SetActive(active);
		ToggleMainMenuButtons(createColonyPanel);

		SetCreateColonyDefaultSettings();

		if (createColonyPanel.activeSelf) {
			DisplayPlanet(
					GameManager.planetM.planet,
					createColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
					createColonyPlanetTilesRectTransform,
					PlanetPreviewState.CreateColony
				);

			SetSelectedPlanetTile(GameManager.planetM.selectedPlanetTile, PlanetPreviewState.CreateColony);
		} else {
			ClearPlanet();
		}
	}

	private GameObject loadSavePanel;

	private Transform savesListPanel;

	private Button loadSaveButton;

	private List<SaveElement> saveElements = new List<SaveElement>();

	private SaveElement selectedSaveElement;

	private class SaveElement {
		public PersistenceManager.PersistenceSave persistenceSave;

		public GameObject obj;

		public SaveElement(PersistenceManager.PersistenceSave persistenceSave, Transform parent) {
			this.persistenceSave = persistenceSave;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SaveElement-Panel"), parent, false);

			if (persistenceSave.loadable) {
				if (persistenceSave.image != null) {
					obj.transform.Find("Save-Image").GetComponent<Image>().sprite = persistenceSave.image;
				}

				obj.transform.Find("SaveData-Panel/SavedDateTime-Text").GetComponent<Text>().text = persistenceSave.saveDateTime;

				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedSaveElement(this);
				});
			} else {
				obj.transform.Find("SaveData-Panel/Saved-Text").GetComponent<Text>().text = "Error while reading save.";
				obj.transform.Find("SaveData-Panel/SavedDateTime-Text").GetComponent<Text>().text = string.Empty;
				obj.GetComponent<Image>().color = GetColour(Colours.LightRed);
			}
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedSaveElement(SaveElement saveElement) {
		selectedSaveElement = saveElement;

		loadSaveButton.interactable = selectedSaveElement != null;
		if (selectedSaveElement != null) {
			loadSaveButton.transform.Find("Text").GetComponent<Text>().text = "Load Save (" + selectedSaveElement.persistenceSave.saveDateTime + ")";
		} else {
			loadSaveButton.transform.Find("Text").GetComponent<Text>().text = "Select a Save to Load";
		}
	}

	private void SetupLoadSaveUI() {
		loadSavePanel = mainMenu.transform.Find("LoadSave-Panel").gameObject;

		Button backButton = loadSavePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			SetSelectedSaveElement(null);

			SetLoadSaveActive(false);
			SetLoadColonyActive(true);
		});

		savesListPanel = loadSavePanel.transform.Find("Saves-ScrollPanel/SavesList-Panel");

		loadSaveButton = loadSavePanel.transform.Find("LoadSave-Button").GetComponent<Button>();
		loadSaveButton.onClick.AddListener(delegate {
			if (selectedSaveElement != null) {
				startCoroutineReference.StartCoroutine(GameManager.persistenceM.ApplyLoadedSave(selectedSaveElement.persistenceSave));

				SetSelectedSaveElement(null);

				SetLoadSaveActive(false);
			}
		});
		SetSelectedSaveElement(null);
	}

	private void SetLoadSaveActive(bool active) {
		loadSavePanel.SetActive(active);
		ToggleMainMenuButtons(loadSavePanel);

		foreach (SaveElement saveElement in saveElements) {
			saveElement.Delete();
		}
		saveElements.Clear();

		if (loadSavePanel.activeSelf) {
			foreach (PersistenceManager.PersistenceSave persistenceSave in GameManager.persistenceM.GetPersistenceSaves()) {
				saveElements.Add(new SaveElement(persistenceSave, savesListPanel));
			}
		}
	}

	private List<GameObject> planetTileObjs = new List<GameObject>();

	private void ClearPlanet() {
		foreach (GameObject planetTileObj in planetTileObjs) {
			MonoBehaviour.Destroy(planetTileObj);
		}
		planetTileObjs.Clear();
	}

	private void DisplayPlanet(PlanetManager.Planet planet, GridLayoutGroup planetGrid, RectTransform planetRectTransform, PlanetPreviewState planetPreviewState) {

		ClearPlanet();

		planetGrid.cellSize = new Vector2(
			createPlanetPlanetTilesRectTransform.sizeDelta.x / planet.mapData.mapSize,
			createPlanetPlanetTilesRectTransform.sizeDelta.y / planet.mapData.mapSize
		);
		planetGrid.constraintCount = planet.mapData.mapSize;

		foreach (PlanetManager.Planet.PlanetTile planetTile in planet.planetTiles) {
			GameObject planetTileObj = MonoBehaviour.Instantiate(GameManager.resourceM.planetTilePrefab, planetGrid.transform, false);
			planetTileObj.name = "Planet Tile: " + planetTile.tile.position;
			planetTileObj.GetComponent<Image>().sprite = planetTile.sprite;
			planetTileObj.GetComponent<Button>().onClick.AddListener(delegate {
				SetSelectedPlanetTile(planetTile, planetPreviewState);
			});

			planetTileObjs.Add(planetTileObj);
		}
	}

	public static int ParseSeed(string seedString) {
		if (string.IsNullOrEmpty(seedString)) {
			seedString = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
		}
		int seed = 0;
		if (!int.TryParse(seedString, out seed)) {
			int seedCharacterIndex = 1;
			foreach (char seedCharacter in seedString) {
				seed += seedCharacter * seedCharacterIndex;
				seedCharacterIndex += 1;
			}
		}
		return seed;
	}

	private void SetSelectedPlanetTile(PlanetManager.Planet.PlanetTile selectedPlanetTile, PlanetPreviewState planetPreviewState) {
		GameManager.planetM.SetSelectedPlanetTile(selectedPlanetTile);
		DisplaySelectedPlanetTileInfo(selectedPlanetTile, planetPreviewState);

		SetSaveColonyButtonInteractable();
	}

	public void SetSaveColonyButtonInteractable() {
		bool selectedPlanetTileNotNull = GameManager.planetM.selectedPlanetTile != null;

		bool validColonyName = IsAlphanumericWithSpaces(colonyNameInputField.text);

		bool interactable = selectedPlanetTileNotNull && validColonyName;
		saveColonyButton.interactable = interactable;
		saveColonyButton.transform.Find("Image").GetComponent<Image>().color = interactable ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
	}

	private void DisplaySelectedPlanetTileInfo(PlanetManager.Planet.PlanetTile selectedPlanetTile, PlanetPreviewState planetPreviewState) {
		createPlanetSelectedPlanetTileInfoPanel.SetActive(false);
		createPlanetSelectedPlanetTilePositionText.gameObject.SetActive(false);
		createColonySelectedPlanetTileInfoPanel.SetActive(false);
		createColonySelectedPlanetTilePositionText.gameObject.SetActive(false);

		if (planetPreviewState == PlanetPreviewState.CreatePlanet) {
			createPlanetSelectedPlanetTileInfoPanel.SetActive(selectedPlanetTile != null);
			createPlanetSelectedPlanetTilePositionText.gameObject.SetActive(createPlanetSelectedPlanetTileInfoPanel.activeSelf);
			if (createPlanetSelectedPlanetTileInfoPanel.activeSelf) {
				createPlanetSelectedPlanetTileSpriteImage.sprite = selectedPlanetTile.sprite;
				createPlanetSelectedPlanetTileBiomeText.text = selectedPlanetTile.tile.biome.name;
				createPlanetSelectedPlanetTilePositionText.text = "(" + Mathf.FloorToInt(selectedPlanetTile.tile.position.x) + ", " + Mathf.FloorToInt(selectedPlanetTile.tile.position.y) + ")";
			}
		} else if (planetPreviewState == PlanetPreviewState.CreateColony) {
			createColonySelectedPlanetTileInfoPanel.SetActive(selectedPlanetTile != null);
			createColonySelectedPlanetTilePositionText.gameObject.SetActive(createColonySelectedPlanetTileInfoPanel.activeSelf);
			if (createColonySelectedPlanetTileInfoPanel.activeSelf) {
				createColonySelectedPlanetTileSpriteImage.sprite = selectedPlanetTile.sprite;
				createColonySelectedPlanetTileBiomeText.text = selectedPlanetTile.tile.biome.name;
				createColonySelectedPlanetTileAverageTemperatureText.text = Mathf.RoundToInt(selectedPlanetTile.tile.temperature).ToString() + "°C";
				createColonySelectedPlanetTileAveragePrecipitationText.text = Mathf.RoundToInt(selectedPlanetTile.tile.GetPrecipitation() * 100f).ToString() + "%";
				createColonySelectedPlanetTileAltitudeText.text = selectedPlanetTile.altitude;
				createColonySelectedPlanetTilePositionText.text = "(" + Mathf.FloorToInt(selectedPlanetTile.tile.position.x) + ", " + Mathf.FloorToInt(selectedPlanetTile.tile.position.y) + ")";
			}
		} else if (planetPreviewState == PlanetPreviewState.LoadColony) {

		}
	}

	private Text loadingStateText;
	private Text subLoadingStateText;

	private void SetupLoadingScreen() {
		loadingStateText = canvas.transform.Find("LoadingScreen/LoadingState-Text").GetComponent<Text>();
		subLoadingStateText = canvas.transform.Find("LoadingScreen/SubLoadingState-Text").GetComponent<Text>();
		SetLoadingScreenActive(false);
	}

	private GameObject gameUI;

	private Vector2 mousePosition;
	public TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	private GameObject colonistsPanel;
	private GameObject colonistListPanel;

	private GameObject caravansPanel;
	private GameObject caravanListPanel;

	private GameObject jobsPanel;
	private GameObject jobListPanel;

	private GameObject selectedColonistInformationPanel;

	private GameObject selectedColonistNeedsSkillsTabButton;
	private GameObject selectedColonistNeedsSkillsTabButtonLinkPanel;
	private GameObject selectedColonistInventoryTabButton;
	private GameObject selectedColonistInventoryTabButtonLinkPanel;
	private GameObject selectedColonistClothingTabButton;
	private GameObject selectedColonistClothingTabButtonLinkPanel;

	private GameObject selectedColonistNeedsSkillsPanel;
	private GameObject selectedColonistHappinessModifiersPanel;
	private GameObject selectedColonistInventoryPanel;
	private GameObject selectedColonistClothingPanel;
	private GameObject selectedColonistClothingSelectionPanel;
	private KeyValuePair<GameObject, GameObject> availableClothingTitleAndList;
	private KeyValuePair<GameObject, GameObject> takenClothingTitleAndList;

	private GameObject selectedTraderMenu;
	private GameObject tradeMenu;

	private GameObject dateTimeInformationPanel;

	private GameObject selectionSizeCanvas;
	private GameObject selectionSizePanel;

	private GameObject selectedContainerIndicator;
	private GameObject selectedContainerInventoryPanel;

	private GameObject professionMenuButton;
	private GameObject professionsList;

	private GameObject objectsMenuButton;
	private GameObject objectPrefabsList;

	private GameObject clothesMenuButton;
	private GameObject clothesMenuPanel;
	private InputField clothesSearchInputField;
	private GameObject clothesList;

	private GameObject resourcesMenuButton;
	private GameObject resourcesMenuPanel;
	private InputField resourcesSearchInputField;
	private GameObject resourcesList;

	private GameObject selectedMTOIndicator;
	private GameObject mtoNoFuelPanelObj;
	private GameObject mtoFuelPanelObj;
	private MTOPanel selectedMTOFuelPanel;
	private MTOPanel selectedMTONoFuelPanel;
	public MTOPanel selectedMTOPanel;

	private void SetupGameUI() {
		gameUI = canvas.transform.Find("Game-BackgroundPanel").gameObject;

		gameUI.transform.Find("Disclaimer-Text").GetComponent<Text>().text = gameVersionString;

		tileInformation = gameUI.transform.Find("TileInformation-Panel").gameObject;

		colonistsPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Colonists-Panel").gameObject;
		colonistListPanel = colonistsPanel.transform.Find("ColonistList-Panel").gameObject;
		colonistsPanel.transform.Find("ColonistsTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(colonistListPanel, colonistsPanel.transform.Find("ColonistsTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

		caravansPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Caravans-Panel").gameObject;
		caravanListPanel = caravansPanel.transform.Find("CaravanList-Panel").gameObject;
		caravansPanel.transform.Find("CaravansTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(caravanListPanel, caravansPanel.transform.Find("CaravansTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

		jobsPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Jobs-Panel").gameObject;
		jobListPanel = jobsPanel.transform.Find("JobList-Panel").gameObject;
		jobsPanel.transform.Find("JobsTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(jobListPanel, jobsPanel.transform.Find("JobsTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

		selectedColonistInformationPanel = gameUI.transform.Find("SelectedColonistInfo-Panel").gameObject;

		selectedColonistNeedsSkillsPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/NeedsSkills-Panel").gameObject;
		selectedColonistHappinessModifiersPanel = selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessModifier-Panel").gameObject;
		selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessModifiers-Button").GetComponent<Button>().onClick.AddListener(delegate { selectedColonistHappinessModifiersPanel.SetActive(!selectedColonistHappinessModifiersPanel.activeSelf); });
		selectedColonistHappinessModifiersPanel.transform.Find("Return-Button").GetComponent<Button>().onClick.AddListener(delegate { selectedColonistHappinessModifiersPanel.SetActive(!selectedColonistHappinessModifiersPanel.activeSelf); });
		selectedColonistHappinessModifiersPanel.SetActive(false);

		selectedColonistInventoryPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/Inventory-Panel").gameObject;

		selectedColonistClothingPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/Clothing-Panel").gameObject;
		selectedColonistClothingSelectionPanel = selectedColonistClothingPanel.transform.Find("ClothingSelection-Panel").gameObject;
		selectedColonistClothingSelectionPanel.transform.Find("Return-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistClothingSelectionPanelActive(!selectedColonistClothingSelectionPanel.activeSelf); });
		availableClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailableTitle-Panel").gameObject,
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailable-Panel").gameObject
		);
		takenClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTakenTitle-Panel").gameObject,
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTaken-Panel").gameObject
		);

		selectedColonistNeedsSkillsTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/NeedsSkills-Button").gameObject;
		selectedColonistNeedsSkillsTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistNeedsSkillsTabButton); });
		selectedColonistNeedsSkillsTabButtonLinkPanel = selectedColonistNeedsSkillsTabButton.transform.Find("Link-Panel").gameObject;
		selectedColonistInventoryTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/Inventory-Button").gameObject;
		selectedColonistInventoryTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistInventoryTabButton); });
		selectedColonistInventoryTabButtonLinkPanel = selectedColonistInventoryTabButton.transform.Find("Link-Panel").gameObject;
		selectedColonistClothingTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/Clothing-Button").gameObject;
		selectedColonistClothingTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistClothingTabButton); });
		selectedColonistClothingTabButtonLinkPanel = selectedColonistClothingTabButton.transform.Find("Link-Panel").gameObject;

		SetSelectedColonistTab(selectedColonistNeedsSkillsTabButton);

		selectedTraderMenu = gameUI.transform.Find("SelectedTraderMenu-Panel").gameObject;
		tradeMenu = gameUI.transform.Find("TradeMenu-Panel").gameObject;
		tradeMenu.transform.Find("Cancel-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeMenuActive(false); });
		tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().onClick.AddListener(delegate { ConfirmTrade(); });
		SetTradeMenu();

		dateTimeInformationPanel = gameUI.transform.Find("DateTimeInformation-Panel").gameObject;

		selectionSizeCanvas = GameObject.Find("SelectionSize-Canvas");
		selectionSizeCanvas.GetComponent<Canvas>().sortingOrder = 100; // Selection Area Size Canvas
		selectionSizePanel = selectionSizeCanvas.transform.Find("SelectionSize-Panel/Content-Panel").gameObject;

		selectedContainerInventoryPanel = gameUI.transform.Find("SelectedContainerInventory-Panel").gameObject;

		professionMenuButton = gameUI.transform.Find("AdminMenu-Panel/ProfessionsMenu-Button").gameObject;
		professionsList = professionMenuButton.transform.Find("ProfessionsList-Panel").gameObject;
		professionMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetProfessionsList(); });

		objectsMenuButton = gameUI.transform.Find("AdminMenu-Panel/ObjectsMenu-Button").gameObject;
		objectPrefabsList = objectsMenuButton.transform.Find("ObjectPrefabsList-ScrollPanel").gameObject;
		objectsMenuButton.GetComponent<Button>().onClick.AddListener(delegate {
			ToggleObjectPrefabsList(true);
		});
		objectPrefabsList.SetActive(false);

		clothesMenuButton = gameUI.transform.Find("AdminMenu-Panel/ClothesMenu-Button").gameObject;
		clothesMenuPanel = clothesMenuButton.transform.Find("ClothesMenu-Panel").gameObject;
		clothesSearchInputField = clothesMenuPanel.transform.Find("ClothesSearch-InputField").GetComponent<InputField>();
		clothesSearchInputField.onValueChanged.AddListener(delegate { FilterClothesList(clothesSearchInputField.text); });
		clothesList = clothesMenuPanel.transform.Find("ClothesList-ScrollPanel").gameObject;
		clothesMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetClothesList(); });
		clothesMenuPanel.SetActive(false);

		resourcesMenuButton = gameUI.transform.Find("AdminMenu-Panel/ResourcesMenu-Button").gameObject;
		resourcesMenuPanel = resourcesMenuButton.transform.Find("ResourcesMenu-Panel").gameObject;
		resourcesSearchInputField = resourcesMenuPanel.transform.Find("ResourcesSearch-InputField").GetComponent<InputField>();
		resourcesSearchInputField.onValueChanged.AddListener(delegate { FilterResourcesList(resourcesSearchInputField.text); });
		resourcesList = resourcesMenuPanel.transform.Find("ResourcesList-ScrollPanel").gameObject;
		resourcesMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetResourcesList(); });
		resourcesMenuPanel.SetActive(false);

		mtoNoFuelPanelObj = gameUI.transform.Find("SelectedManufacturingTileObjectNoFuel-Panel").gameObject;
		mtoFuelPanelObj = gameUI.transform.Find("SelectedManufacturingTileObjectFuel-Panel").gameObject;
	}

	public GameObject pauseMenu;
	private GameObject pauseMenuButtons;
	private GameObject pauseLabel;

	private void SetupPauseMenu() {
		pauseMenu = canvas.transform.Find("PauseMenu-BackgroundPanel").gameObject;
		pauseMenuButtons = pauseMenu.transform.Find("ButtonsList-Panel").gameObject;
		pauseLabel = pauseMenu.transform.Find("PausedLabel-Text").gameObject;

		pauseMenuButtons.transform.Find("PauseContinue-Button").GetComponent<Button>().onClick.AddListener(delegate { SetPauseMenuActive(false); });

		pauseMenuButtons.transform.Find("PauseSave-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.persistenceM.CreateSave(GameManager.colonyM.colony); });

		pauseMenuButtons.transform.Find("PauseSettings-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(true); });

		pauseMenuButtons.transform.Find("PauseExitToMainMenu-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToMenu(); });

		pauseMenuButtons.transform.Find("PauseExitToDesktop-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });
	}

	public ResourceManager.Container selectedContainer;

	public override void Update() {
		if (GameManager.tileM.mapState == TileManager.MapState.Generated) {
			mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile newMouseOverTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
			if (newMouseOverTile != mouseOverTile) {
				mouseOverTile = newMouseOverTile;
				if (!pauseMenu.activeSelf) {
					UpdateTileInformation();
				}
			}

			if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				if (GameManager.jobM.firstTile != null) {
					GameManager.jobM.StopSelection();
				} else {
					if (GameManager.jobM.GetSelectedPrefab() != null) {
						GameManager.jobM.SetSelectedPrefab(null);
					} else {
						if (!playerTyping) {
							if (!Input.GetMouseButtonDown(1)) {
								SetPauseMenuActive(!pauseMenu.activeSelf);
							}
						}
					}
				}
			}

			UpdateSelectedColonistInformation();
			UpdateSelectedTraderMenu();
			UpdateTradeMenu();

			UpdateColonistElements();
			UpdateCaravanElements();
			UpdateJobElements();

			if (selectedContainer != null) {
				if (Input.GetMouseButtonDown(1)) {
					SetSelectedContainer(null);
				}
				UpdateSelectedContainerInfo();
			}

			if (selectedMTO != null) {
				selectedMTOPanel.Update(selectedMTO);
				if (Input.GetMouseButtonDown(1)) {
					SetSelectedManufacturingTileObject(null);
				}
			}

			if (Input.GetMouseButtonDown(0) && !IsPointerOverUI()) {
				ResourceManager.Container container = GameManager.resourceM.containers.Find(findContainer => findContainer.tile == newMouseOverTile || findContainer.additionalTiles.Contains(newMouseOverTile));
				if (container != null) {
					SetSelectedManufacturingTileObject(null);
					SetSelectedContainer(container);
				}
				ResourceManager.ManufacturingTileObject mto = GameManager.resourceM.manufacturingTileObjectInstances.Find(mtoi => mtoi.tile == newMouseOverTile || mtoi.additionalTiles.Contains(newMouseOverTile));
				if (mto != null) {
					SetSelectedContainer(null);
					SetSelectedManufacturingTileObject(mto);
				}
			}

			if (professionsList.activeSelf) {
				UpdateProfessionsList();
			}

			if (clothesMenuPanel.activeSelf) {
				UpdateClothesList();
			}

			if (resourcesMenuPanel.activeSelf) {
				UpdateResourcesList();
			}

			UpdateButtonRequiredResourceItems();
		} else {
			if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && GameManager.tileM.mapState != TileManager.MapState.Generating && !playerTyping) {
				if (loadUniversePanel.activeSelf) {
					SetSelectedUniverseElement(null);
				}
				if (loadPlanetPanel.activeSelf) {
					SetSelectedPlanetElement(null);
				}
				if (loadColonyPanel.activeSelf) {
					SetSelectedColonyElement(null);
				}
				if (loadSavePanel.activeSelf) {
					SetSelectedSaveElement(null);
				}
				SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);
			}
			UpdateMainMenuBackground();
		}

		playerTyping = IsPlayerTyping();
	}

	private bool IsPlayerTyping() {
		if (universeNameInputField.isFocused) {
			return true;
		}
		if (planetRegenerationCodeInputField.isFocused) {
			return true;
		}
		if (planetNameInputField.isFocused) {
			return true;
		}
		if (planetSeedInputField.isFocused) {
			return true;
		}
		if (mapRegenerationCodeInputField.isFocused) {
			return true;
		}
		if (colonyNameInputField.isFocused) {
			return true;
		}
		if (mapSeedInputField.isFocused) {
			return true;
		}

		if (GameManager.debugM.debugInput.isFocused) {
			return true;
		}
		if (clothesSearchInputField.isFocused) {
			return true;
		}
		if (resourcesSearchInputField.isFocused) {
			return true;
		}
		return false;
	}

	private void UpdateButtonRequiredResourceItems() {
		foreach (GameObject buttonRequiredResourceItem in buttonRequiredResourceItems) {
			if (buttonRequiredResourceItem.activeSelf) {
				ResourceManager.Resource resource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), buttonRequiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text.Replace(" ", string.Empty)));
				buttonRequiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + resource.GetAvailableAmount();
				if (int.Parse(buttonRequiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text.Split(' ')[1]) > resource.GetAvailableAmount()) {
					buttonRequiredResourceItem.GetComponent<Image>().color = GetColour(Colours.LightRed);
				} else {
					buttonRequiredResourceItem.GetComponent<Image>().color = GetColour(Colours.LightGreen);
				}
			}
		}
	}

	public void SetSelectedContainer(ResourceManager.Container container) {

		selectedContainer = null;

		if (selectedMTO != null) {
			SetSelectedManufacturingTileObject(null);
		}

		selectedContainer = container;
		SetSelectedContainerInfo();
	}

	void ToggleMainMenuButtons(GameObject coverPanel) {
		if (mainMenuButtonsPanel.activeSelf && coverPanel.activeSelf) {
			mainMenuButtonsPanel.SetActive(false);
			snowshipLogo.SetActive(false);
		} else if (!mainMenuButtonsPanel.activeSelf && !coverPanel.activeSelf) {
			mainMenuButtonsPanel.SetActive(true);
			snowshipLogo.SetActive(true);
			GetMainMenuContinueFile();
		}
		darkBackground.SetActive(!snowshipLogo.activeSelf);
	}

	//void SetNewGameMainMenuActive(bool active) {
	//	mapSelectionPanel.SetActive(active);
	//	ToggleMainMenuButtons(mapSelectionPanel);
	//	if (mapSelectionPanel.activeSelf && !createdPlanet) {
	//		CreateNewGamePlanet();
	//	}
	//}

	void ExitToDesktop() {
		Application.Quit();
	}

	//public void SetSelectedPlanetTileInfo() {
	//	if (selectedPlanetTile != null) {
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileTemperature-Panel/TemperatureValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedPlanetTile.averageTemperature) + "°C";
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTilePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedPlanetTile.averagePrecipitation * 100) + "%";
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>().text = Mathf.RoundToInt((selectedPlanetTile.tile.height - selectedPlanetTile.terrainTypeHeights[TileManager.TileTypes.GrassWater]) * 5000f) + "m";
	//		mapSelectionPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(selectedPlanetTile.position.x) + ", " + Mathf.FloorToInt(selectedPlanetTile.position.y) + ")";
	//	} else {
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileTemperature-Panel/TemperatureValue-Text").GetComponent<Text>().text = "";
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTilePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>().text = "";
	//		mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>().text = "";
	//		mapSelectionPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>().text = string.Empty;
	//	}
	//}

	//public void ParseMapRegenerationCode(string mapRegenerationCode) {
	//	List<string> splitMRC = mapRegenerationCode.Split('~').ToList();
	//	int planetSeed = int.Parse(splitMRC[0]);
	//	if (planetSeed.ToString().Length > planetSeedInputField.characterLimit) {
	//		MonoBehaviour.print("planetSeed too long: " + planetSeed);
	//		return;
	//	}
	//	MonoBehaviour.print("planetSeed: " + planetSeed);

	//	int planetSize = int.Parse(splitMRC[1]);
	//	int planetTileSize = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetSize);
	//	if (!planetTileSizes.Contains(planetTileSize)) {
	//		MonoBehaviour.print("planetTileSize/planetSize not valid: " + planetTileSize + "/" + planetSize);
	//		return;
	//	}
	//	MonoBehaviour.print("planetTileSize: " + planetTileSize);
	//	MonoBehaviour.print("planetSize: " + planetSize);


	//	int planetTemperatureRange = int.Parse(splitMRC[2]);
	//	planetTemperatureRange = Mathf.FloorToInt(planetTemperatureRange / 10f);
	//	if (planetTemperatureRange < temperatureRangeSlider.minValue || planetTemperatureRange > temperatureRangeSlider.maxValue) {
	//		MonoBehaviour.print("planetTemperatureRange out of range: " + planetTemperatureRange);
	//		return;
	//	}
	//	MonoBehaviour.print("planetTemperatureRange: " + planetTemperatureRange);

	//	float planetDistance = float.Parse(splitMRC[3]);
	//	planetDistance = (10 * planetDistance) - 6;
	//	if (planetDistance < planetDistanceSlider.minValue || planetDistance > planetDistanceSlider.maxValue) {
	//		MonoBehaviour.print("planetDistance out of range: " + planetDistance);
	//		return;
	//	}
	//	MonoBehaviour.print("planetDistance: " + planetDistance);

	//	int planetPrimaryWindDirection = int.Parse(splitMRC[4]);
	//	if (planetPrimaryWindDirection < windDirectionSlider.minValue || planetPrimaryWindDirection > windDirectionSlider.maxValue) {
	//		MonoBehaviour.print("planetPrimaryWindDirection out of range: " + planetPrimaryWindDirection);
	//		return;
	//	}
	//	MonoBehaviour.print("planetPrimaryWindDirection: " + planetPrimaryWindDirection);

	//	Vector2 planetTilePosition = new Vector2(int.Parse(splitMRC[5]), int.Parse(splitMRC[6]));
	//	if (planetTilePosition.x < 0 || planetTilePosition.y < 0 || planetTilePosition.x >= planetSize || planetTilePosition.y >= planetSize) {
	//		MonoBehaviour.print("planetTilePosition out of range: " + planetTilePosition);
	//		return;
	//	}
	//	MonoBehaviour.print("planetTilePosition: " + planetTilePosition);

	//	int mapSize = int.Parse(splitMRC[7]);
	//	mapSize = Mathf.FloorToInt(mapSize / 50f);
	//	if (mapSize < mapSizeSlider.minValue || mapSize > mapSizeSlider.maxValue) {
	//		MonoBehaviour.print("mapSize out of range: " + mapSize);
	//		return;
	//	}
	//	MonoBehaviour.print("mapSize: " + mapSize);

	//	int mapSeed = int.Parse(splitMRC[8]);
	//	if (mapSeed.ToString().Length > mapSeedInputField.characterLimit) {
	//		MonoBehaviour.print("mapSeed too long: " + mapSeed);
	//		return;
	//	}
	//	MonoBehaviour.print("mapSeed: " + mapSeed);

	//	planetSeedInputField.text = planetSeed.ToString();
	//	planetSizeSlider.value = planetTileSizes.IndexOf(planetTileSize);
	//	planetDistanceSlider.value = planetDistance;
	//	temperatureRangeSlider.value = planetTemperatureRange;
	//	windDirectionSlider.value = planetPrimaryWindDirection;
	//	GeneratePlanet();

	//	SetSelectedPlanetTile(planetTiles.Find(planetTile => planetTile.position == planetTilePosition));
	//	mapSeedInputField.text = mapSeed.ToString();
	//	mapSizeSlider.value = mapSize;
	//}

	//public void PlayButton() {
	//	colonyName = mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/ColonyName-Panel/InputField").GetComponent<InputField>().text;
	//	if (string.IsNullOrEmpty(colonyName) || new Regex(@"\W+", RegexOptions.IgnorePatternWhitespace).Replace(colonyName, string.Empty).Length <= 0) {
	//		colonyName = "Colony";
	//	}

	//	string mapSeedString = mapSeedInputField.text;
	//	int mapSeed = SeedParser(mapSeedString, mapSeedInputField);

	//	MainMenuToGameTransition(false);

	//	TileManager.MapData mapData = new TileManager.MapData(
	//		planet.mapData,
	//		mapSeed,
	//		mapSize,
	//		true,
	//		selectedPlanetTile.equatorOffset,
	//		false,
	//		0,
	//		0,
	//		0,
	//		false,
	//		selectedPlanetTile.averageTemperature,
	//		selectedPlanetTile.averagePrecipitation,
	//		selectedPlanetTile.terrainTypeHeights,
	//		selectedPlanetTile.surroundingPlanetTileHeightDirections,
	//		selectedPlanetTile.river,
	//		selectedPlanetTile.surroundingPlanetTileRivers,
	//		false,
	//		planet.mapData.primaryWindDirection,
	//		selectedPlanetTile.position
	//	);
	//	GameManager.tileM.Initialize(mapData);

	//	pauseMenu.transform.Find("MapRegenerationCode-InputField").GetComponent<InputField>().text = GameManager.tileM.map.mapData.mapRegenerationCode;
	//}

	//public void MainMenuToGameTransition(bool enableGameUIImmediately) {
	//	mainMenu.SetActive(false);
	//	if (enableGameUIImmediately) {
	//		SetLoadingScreenActive(false);
	//		SetGameUIActive(true);
	//	} else {
	//		SetLoadingScreenActive(true);
	//		SetGameUIActive(false);
	//	}
	//}

	//public void GameToMainMenuTransition() {
	//	SetGameUIActive(false);
	//	SetLoadingScreenActive(false);
	//	mainMenu.SetActive(true);
	//}

	//public void UpdateMapSizeText() {
	//	mapSize = Mathf.RoundToInt(mapSizeSlider.value * 50);
	//	mapSizeText.text = mapSize.ToString();
	//}

	public void SetMainMenuActive(bool active) {
		mainMenu.SetActive(active);
	}

	void SetMainMenuBackground() {

		Vector2 screenResolution = new Vector2(screenWidth, screenHeight);
		float targetNewSize = Mathf.Max(screenResolution.x, screenResolution.y);

		List<Sprite> backgroundImages = Resources.LoadAll<Sprite>(@"UI/Backgrounds/SingleMap").ToList();
		mainMenuBackground.GetComponent<Image>().sprite = backgroundImages[UnityEngine.Random.Range(0, backgroundImages.Count)];

		Vector2 menuBackgroundSize = new Vector2(mainMenuBackground.GetComponent<Image>().sprite.texture.width, mainMenuBackground.GetComponent<Image>().sprite.texture.height);
		float menuBackgroundTargetSize = Mathf.Max(menuBackgroundSize.x, menuBackgroundSize.y);
		float menuBackgroundRatio = menuBackgroundTargetSize / targetNewSize;
		Vector2 newMenuBackgroundSize = menuBackgroundSize / menuBackgroundRatio;
		mainMenuBackground.GetComponent<RectTransform>().sizeDelta = newMenuBackgroundSize * 1.5f;
		originalBackgroundPosition = mainMenuBackground.GetComponent<RectTransform>().position;

		Vector2 logoSize = new Vector2(snowshipLogo.GetComponent<Image>().sprite.texture.width, snowshipLogo.GetComponent<Image>().sprite.texture.height);
		float logoTargetSize = Mathf.Max(logoSize.x, logoSize.y);
		float logoRatio = logoTargetSize / targetNewSize;
		Vector2 newLogoSize = logoSize / logoRatio;
		snowshipLogo.GetComponent<RectTransform>().sizeDelta = newLogoSize * 1.05f;
	}

	private Vector3 originalBackgroundPosition;
	private float movementMultiplier = 25f;
	void UpdateMainMenuBackground() {
		mainMenuBackground.GetComponent<RectTransform>().position = originalBackgroundPosition + new Vector3((-Input.mousePosition.x) / (screenWidth / movementMultiplier), (-Input.mousePosition.y) / (screenHeight / movementMultiplier)) + (new Vector3(screenWidth, screenHeight) / movementMultiplier / 2);
	}

	public void UpdateLoadingStateText(string primaryText, string secondaryText) {
		if (loadingStateText.gameObject.activeSelf) {
			loadingStateText.text = primaryText.ToUpper();
		}
		if (subLoadingStateText.gameObject.activeSelf) {
			subLoadingStateText.text = secondaryText.ToUpper();
		}
	}

	public void SetLoadingScreenActive(bool active) {
		loadingStateText.transform.parent.gameObject.SetActive(active);
	}

	public void SetGameUIActive(bool state) {
		gameUI.SetActive(state);
	}

	/*	Menu Structure:
	 *		Build Menu Button -> Build Menu Panel
	 *			Build Menu Group Button -> Group Panel
	 *				Build Menu Subgroup Button -> Subgroup Panel
	 *					Build Menu Prefab Button
	 *		Command Menu Button -> Command Menu Panel
	 *			Command Menu Subgroup Button -> Panel
	 *				Command Menu Prefab Button
	*/

	public void CreateBottomLeftMenus() {
		Dictionary<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> menus = CreateBuildMenuButtons();

		foreach (KeyValuePair<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> menuKVP in menus) {
			GameObject menuKVPPanel = menuKVP.Key.transform.Find("Panel").gameObject;
			menuKVP.Key.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> otherMenuKVP in menus) {
					if (menuKVP.Key != otherMenuKVP.Key) {
						otherMenuKVP.Key.transform.Find("Panel").gameObject.SetActive(false);
						foreach (KeyValuePair<GameObject, Dictionary<GameObject, List<GameObject>>> groupKVP in otherMenuKVP.Value) {
							groupKVP.Key.SetActive(false);
							foreach (KeyValuePair<GameObject, List<GameObject>> subgroupKVP in groupKVP.Value) {
								subgroupKVP.Key.SetActive(false);
							}
						}
					}
				}
			});
			menuKVP.Key.GetComponent<Button>().onClick.AddListener(delegate { menuKVPPanel.SetActive(!menuKVPPanel.activeSelf); });
		}

		foreach (KeyValuePair<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> menuKVP in menus) {

			menuKVP.Key.transform.Find("Panel").gameObject.SetActive(false);
			foreach (KeyValuePair<GameObject, Dictionary<GameObject, List<GameObject>>> groupKVP in menuKVP.Value) {
				groupKVP.Key.SetActive(false);
				foreach (KeyValuePair<GameObject, List<GameObject>> subgroupKVP in groupKVP.Value) {
					subgroupKVP.Key.SetActive(false);
				}
			}
		}
	}

	private List<GameObject> buttonRequiredResourceItems = new List<GameObject>();

	public Dictionary<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> CreateBuildMenuButtons() {

		Dictionary<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>> menus = new Dictionary<GameObject, Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>>();

		GameObject buildMenuButton = gameUI.transform.Find("BuildMenu-Button").gameObject;
		GameObject buildMenuPanel = buildMenuButton.transform.Find("Panel").gameObject;

		Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>> groupPanels = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();

		foreach (ResourceManager.TileObjectPrefabGroup group in GameManager.resourceM.tileObjectPrefabGroups) {
			if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.None) {
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Command) {
				menus.Add(gameUI.transform.Find("CommandMenu-Button").gameObject, CreateAdditionalMenuButtons(gameUI.transform.Find("CommandMenu-Button").gameObject, group));
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Farm) {
				menus.Add(gameUI.transform.Find("FarmMenu-Button").gameObject, CreateAdditionalMenuButtons(gameUI.transform.Find("FarmMenu-Button").gameObject, group));
				continue;
			}

			GameObject groupButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), buildMenuPanel.transform, false);
			groupButton.transform.Find("Text").GetComponent<Text>().text = group.name;
			GameObject groupPanel = groupButton.transform.Find("Panel").gameObject;
			groupPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(100, 21);

			Dictionary<GameObject, List<GameObject>> subgroupPanels = new Dictionary<GameObject, List<GameObject>>();
			foreach (ResourceManager.TileObjectPrefabSubGroup subgroup in group.tileObjectPrefabSubGroups) {
				GameObject subgroupButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), groupPanel.transform, false);
				subgroupButton.transform.Find("Text").GetComponent<Text>().text = subgroup.name;
				GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

				List<GameObject> prefabButtons = new List<GameObject>();
				foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
					GameObject prefabButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), subgroupPanel.transform, false);
					prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
					if (prefab.baseSprite != null) {
						prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
					}
					prefabButton.GetComponent<Button>().onClick.AddListener(delegate { GameManager.jobM.SetSelectedPrefab(prefab); });
					GameObject requiredResourcesPanel = prefabButton.transform.Find("RequiredResources-Panel").gameObject;
					prefabButton.GetComponent<HoverToggleScript>().Initialize(requiredResourcesPanel);
					foreach (ResourceManager.ResourceAmount requiredResource in prefab.resourcesToBuild) {
						GameObject requiredResourceItem = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesPanel.transform, false);
						requiredResourceItem.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = requiredResource.resource.image;
						requiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text = requiredResource.resource.name;
						requiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text = "Need " + requiredResource.amount;
						requiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + requiredResource.resource.GetAvailableAmount();
						buttonRequiredResourceItems.Add(requiredResourceItem);
					}
					prefabButtons.Add(prefabButton);
				}
				subgroupPanels.Add(subgroupPanel, prefabButtons);

				subgroupButton.GetComponent<Button>().onClick.AddListener(delegate {
					foreach (KeyValuePair<GameObject, List<GameObject>> subgroupKVP in subgroupPanels) {
						if (subgroupKVP.Key != subgroupPanel) {
							subgroupKVP.Key.SetActive(false);
						}
					}
				});
				subgroupButton.GetComponent<Button>().onClick.AddListener(delegate { subgroupPanel.SetActive(!subgroupPanel.activeSelf); });
			}
			groupPanels.Add(groupPanel, subgroupPanels);

			groupButton.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject, Dictionary<GameObject, List<GameObject>>> groupKVP in groupPanels) {
					if (groupKVP.Key != groupPanel) {
						groupKVP.Key.SetActive(false);
					}
					foreach (KeyValuePair<GameObject, List<GameObject>> subgroupKVP in groupKVP.Value) {
						subgroupKVP.Key.SetActive(false);
					}
				}
			});
			groupButton.GetComponent<Button>().onClick.AddListener(delegate { groupPanel.SetActive(!groupPanel.activeSelf); });
		}

		menus.Add(buildMenuButton, groupPanels);

		return menus;
	}

	public Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>> CreateAdditionalMenuButtons(GameObject parentButton, ResourceManager.TileObjectPrefabGroup group) {

		Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>> groupPanels = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();

		GameObject parentMenuButton = parentButton;
		GameObject parentMenuPanel = parentMenuButton.transform.Find("Panel").gameObject;

		Dictionary<GameObject, List<GameObject>> subgroupPanels = new Dictionary<GameObject, List<GameObject>>();
		foreach (ResourceManager.TileObjectPrefabSubGroup subgroup in group.tileObjectPrefabSubGroups) {
			GameObject subgroupButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), parentMenuPanel.transform, false);
			subgroupButton.transform.Find("Text").GetComponent<Text>().text = subgroup.name;
			GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

			List<GameObject> prefabButtons = new List<GameObject>();
			foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
				GameObject prefabButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), subgroupPanel.transform, false);
				prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
				if (prefab.baseSprite != null) {
					prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
				}
				prefabButton.GetComponent<Button>().onClick.AddListener(delegate { GameManager.jobM.SetSelectedPrefab(prefab); });
				GameObject requiredResourcesPanel = prefabButton.transform.Find("RequiredResources-Panel").gameObject;
				prefabButton.GetComponent<HoverToggleScript>().Initialize(requiredResourcesPanel);
				foreach (ResourceManager.ResourceAmount requiredResource in prefab.resourcesToBuild) {
					GameObject requiredResourceItem = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesPanel.transform, false);
					requiredResourceItem.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = requiredResource.resource.image;
					requiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text = requiredResource.resource.name;
					requiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text = "Need " + requiredResource.amount;
					requiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + requiredResource.resource.GetAvailableAmount();
					buttonRequiredResourceItems.Add(requiredResourceItem);
				}
				prefabButtons.Add(prefabButton);
			}
			subgroupPanels.Add(subgroupPanel, prefabButtons);

			subgroupButton.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject, List<GameObject>> subgroupKVP in subgroupPanels) {
					if (subgroupKVP.Key != subgroupPanel) {
						subgroupKVP.Key.SetActive(false);
					}
				}
			});
			subgroupButton.GetComponent<Button>().onClick.AddListener(delegate { subgroupPanel.SetActive(!subgroupPanel.activeSelf); });
		}
		groupPanels.Add(parentMenuPanel, subgroupPanels);

		return groupPanels;
	}

	public void InitializeTileInformation() {
		tileResourceElements.Add(MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ResourceData-Panel"), tileInformation.transform, false));

		plantObjectElements.Add(MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"), tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"), false));
		plantObjectElements.Add(MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"), tileInformation.transform, false));
	}

	private List<GameObject> tileResourceElements = new List<GameObject>();
	private List<GameObject> plantObjectElements = new List<GameObject>();
	private Dictionary<int, List<GameObject>> tileObjectElements = new Dictionary<int, List<GameObject>>();

	public void UpdateTileInformation() {
		if (mouseOverTile != null) {
			foreach (KeyValuePair<int, List<GameObject>> tileObjectElementKVP in tileObjectElements) {
				foreach (GameObject tileObjectDataElement in tileObjectElementKVP.Value) {
					tileObjectDataElement.SetActive(false);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage").GetComponent<Image>().sprite = mouseOverTile.obj.GetComponent<SpriteRenderer>().sprite;

			ResourceManager.Resource mouseOverTileResource = mouseOverTile.GetResource();
			if (mouseOverTileResource != null) {
				foreach (GameObject tileResourceElement in tileResourceElements) {
					tileResourceElement.SetActive(true);
				}

				tileResourceElements[0].transform.Find("TileInfo-ResourceData-Value").GetComponent<Text>().text = mouseOverTileResource.name;
				tileResourceElements[0].transform.Find("TileInfo-ResourceData-Image").GetComponent<Image>().sprite = mouseOverTileResource.image;
				tileResourceElements[0].GetComponent<Image>().color = GetColour(Colours.LightGrey200);
			} else {
				foreach (GameObject tileResourceElement in tileResourceElements) {
					tileResourceElement.SetActive(false);
				}
			}

			if (mouseOverTile.plant != null) {
				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(true);
				}
				plantObjectElements[0].GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "Plant";
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = mouseOverTile.plant.name;
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Image").GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;

				if (mouseOverTile.plant.group.maxIntegrity > 0) {
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = mouseOverTile.plant.group.maxIntegrity;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().value = mouseOverTile.plant.integrity;
					plantObjectElements[1].transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), mouseOverTile.plant.integrity / mouseOverTile.plant.group.maxIntegrity);
				} else {
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = 1;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().value = 1;
					plantObjectElements[1].transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightGrey200);
				}
			} else {
				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(false);
				}
			}
			if (mouseOverTile.GetAllObjectInstances().Count > 0) {
				foreach (ResourceManager.TileObjectInstance tileObject in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
					if (!tileObjectElements.ContainsKey(tileObject.prefab.layer)) {
						tileObjectElements.Add(tileObject.prefab.layer, new List<GameObject>() {
							MonoBehaviour.Instantiate(GameManager.resourceM.tileImage, tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"), false),
							MonoBehaviour.Instantiate(GameManager.resourceM.objectDataPanel, tileInformation.transform, false)
						});
					}

					GameObject tileLayerSpriteObject = tileObjectElements[tileObject.prefab.layer][0];
					tileLayerSpriteObject.GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;
					tileLayerSpriteObject.SetActive(true);

					GameObject tileObjectDataObject = tileObjectElements[tileObject.prefab.layer][1];
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "L" + tileObject.prefab.layer;
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = tileObject.prefab.name;
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Image").GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;

					if (tileObject.prefab.maxIntegrity > 0) {
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = tileObject.prefab.maxIntegrity;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().value = tileObject.integrity;
						tileObjectDataObject.transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), tileObject.integrity / tileObject.prefab.maxIntegrity);
					} else {
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = 1;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().value = 1;
						tileObjectDataObject.transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightGrey200);
					}

					tileObjectDataObject.SetActive(true);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "(" + Mathf.FloorToInt(mouseOverTile.obj.transform.position.x) + ", " + Mathf.FloorToInt(mouseOverTile.obj.transform.position.y) + ")";
			string tileTypeString = mouseOverTile.tileType.name;
			if (TileManager.liquidWaterEquivalentTileTypes.Contains(mouseOverTile.tileType.type)) {
				tileTypeString = "Water";
			} else if (TileManager.waterEquivalentTileTypes.Contains(mouseOverTile.tileType.type)) {
				tileTypeString = "Ice";
			}
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Type").GetComponent<Text>().text = tileTypeString;
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = mouseOverTile.biome.name;
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Temperature").GetComponent<Text>().text = Mathf.RoundToInt(mouseOverTile.temperature) + "°C";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Precipitation").GetComponent<Text>().text = Mathf.RoundToInt(mouseOverTile.GetPrecipitation() * 100f) + "%";

			if (!tileInformation.activeSelf) {
				tileInformation.SetActive(true);
			}
		} else {
			tileInformation.SetActive(false);
		}
	}

	public class SkillElement {
		public ColonistManager.Colonist colonist;
		public ColonistManager.SkillInstance skill;
		public GameObject obj;

		public SkillElement(ColonistManager.Colonist colonist, ColonistManager.SkillInstance skill, Transform parent) {
			this.colonist = colonist;
			this.skill = skill;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SkillInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = skill.prefab.name;
			// ADD SKILL IMAGE

			Update();
		}

		public void Update() {
			obj.transform.Find("Level").GetComponent<Text>().text = "Level " + skill.level + " (+" + Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f) + "%)";
			obj.transform.Find("Experience-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f);

			float highestLevel = 0;
			float highestSkill = 0;
			ColonistManager.Colonist highestSkillColonist = colonist;
			foreach (ColonistManager.Colonist otherColonist in GameManager.colonistM.colonists) {
				ColonistManager.SkillInstance foundSkill = otherColonist.skills.Find(findSkill => findSkill.prefab == skill.prefab);
				float otherColonistSkill = foundSkill.level + (foundSkill.currentExperience / foundSkill.nextLevelExperience);
				if (otherColonistSkill > highestSkill) {
					highestSkill = otherColonistSkill;
					highestSkillColonist = otherColonist;
				}
				if (foundSkill.level > highestLevel) {
					highestLevel = foundSkill.level;
				}
			}

			obj.transform.Find("Level-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.level / (highestLevel > 0 ? highestLevel : 1)) * 100f);

			float skillValue = skill.level + (skill.currentExperience / skill.nextLevelExperience);
			if (highestSkillColonist == colonist || Mathf.Approximately(highestSkill, skillValue)) {
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkYellow);
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = GetColour(Colours.LightYellow);
			} else {
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkGreen);
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = GetColour(Colours.LightGreen);
			}
		}
	}

	public class InventoryElement {
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public InventoryElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
			this.resourceAmount = resourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;
			obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	public class TradeResourceElement {
		public ResourceManager.TradeResourceAmount tradeResourceAmount;

		public GameObject obj;

		private InputField tradeAmountInputField;

		public TradeResourceElement(ResourceManager.TradeResourceAmount tradeResourceAmount, Transform parent) {
			this.tradeResourceAmount = tradeResourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TradeResourceElement-Panel"), parent, false);

			tradeAmountInputField = obj.transform.Find("TradeAmount-InputField").GetComponent<InputField>();
			tradeAmountInputField.text = tradeResourceAmount.GetTradeAmount().ToString();
			ValidateTradeAmountInputField();
			tradeAmountInputField.onEndEdit.AddListener(delegate { ValidateTradeAmountInputField(); });

			obj.transform.Find("CaravanResource-Image").GetComponent<Image>().sprite = tradeResourceAmount.resource.image;
			obj.transform.Find("CaravanResourceName-Text").GetComponent<Text>().text = tradeResourceAmount.resource.name;
			obj.transform.Find("CaravanResourceValue-Text").GetComponent<Text>().text = tradeResourceAmount.caravanResourcePrice.ToString();

			obj.transform.Find("BuyIncreaseOne-Button").GetComponent<Button>().onClick.AddListener(delegate { ChangeTradeAmount(1); });
			obj.transform.Find("BuyIncreaseAll-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeAmount(tradeResourceAmount.caravanAmount); });

			obj.transform.Find("SellIncreaseOne-Button").GetComponent<Button>().onClick.AddListener(delegate { ChangeTradeAmount(-1); });
			obj.transform.Find("SellIncreaseAll-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeAmount(-tradeResourceAmount.resource.GetAvailableAmount()); });

			obj.transform.Find("ColonyResource-Image").GetComponent<Image>().sprite = tradeResourceAmount.resource.image;
			obj.transform.Find("ColonyResourceName-Text").GetComponent<Text>().text = tradeResourceAmount.resource.name;
			obj.transform.Find("ColonyResourceValue-Text").GetComponent<Text>().text = tradeResourceAmount.resource.price.ToString();
		}

		private void ChangeTradeAmount(int amount) {
			SetTradeAmountInputField(tradeResourceAmount.GetTradeAmount() + amount);
		}

		private void SetTradeAmount(int tradeAmount) {
			SetTradeAmountInputField(tradeAmount);
		}

		private void SetTradeAmountInputField(int tradeAmount) {
			tradeAmountInputField.text = tradeAmount.ToString();
			ValidateTradeAmountInputField();
		}

		private void ValidateTradeAmountInputField() {
			Text text = tradeAmountInputField.transform.Find("Text").GetComponent<Text>();
			text.color = GetColour(Colours.DarkGrey50);
			int tradeAmount = 0;
			if (int.TryParse(tradeAmountInputField.text, out tradeAmount)) {
				if (tradeAmount == 0) {
					tradeAmountInputField.text = string.Empty;
				} else if (tradeAmount > tradeResourceAmount.caravanAmount) {
					tradeAmount = tradeResourceAmount.caravanAmount;
					tradeAmountInputField.text = tradeAmount.ToString();
				} else if (tradeAmount < -tradeResourceAmount.resource.GetAvailableAmount()) {
					tradeAmount = -tradeResourceAmount.resource.GetAvailableAmount();
					tradeAmountInputField.text = tradeAmount.ToString();
				}
			} else {
				tradeAmountInputField.text = string.Empty;
			}
			tradeResourceAmount.SetTradeAmount(tradeAmount);
			if (tradeAmount == 0) {
				tradeAmountInputField.text = string.Empty;
			}
			if (!string.IsNullOrEmpty(tradeAmountInputField.text)) {
				if (tradeAmount > 0) {
					text.color = GetColour(Colours.LightGreen);
				} else if (tradeAmount < 0) {
					text.color = GetColour(Colours.LightRed);
				} else {
					text.color = GetColour(Colours.DarkGrey50);
				}
			}
		}

		public bool Update() {
			tradeResourceAmount.Update();

			int tradeAmount = tradeResourceAmount.GetTradeAmount();

			if (tradeResourceAmount.caravanAmount == 0 && tradeResourceAmount.resource.GetAvailableAmount() == 0) {
				MonoBehaviour.Destroy(obj);
				return true; // Removed
			}

			obj.transform.Find("CaravanResourceAmount-Text").GetComponent<Text>().text = (tradeResourceAmount.caravanAmount - tradeAmount).ToString();
			obj.transform.Find("ColonyResourceAmount-Text").GetComponent<Text>().text = (tradeResourceAmount.resource.GetAvailableAmount() + tradeAmount).ToString();

			return false; // Not Removed
		}
	}

	public class ReservedResourcesColonistElement {
		public ResourceManager.ReservedResources reservedResources;
		public List<ReservedResourceElement> reservedResourceElements = new List<ReservedResourceElement>();
		public GameObject obj;

		public ReservedResourcesColonistElement(HumanManager.Human human, ResourceManager.ReservedResources reservedResources, Transform parent) {
			this.reservedResources = reservedResources;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourcesColonistInfoElement-Panel"), parent, false);

			obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = human.name;
			obj.transform.Find("ColonistReservedCount-Text").GetComponent<Text>().text = reservedResources.resources.Count.ToString();
			obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = human.moveSprites[0];

			foreach (ResourceManager.ResourceAmount ra in reservedResources.resources) {
				reservedResourceElements.Add(new ReservedResourceElement(ra, parent));
			}
		}
	}

	public class ReservedResourceElement {
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public ReservedResourceElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
			this.resourceAmount = resourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourceInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;

			obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	public class NeedElement {
		public ColonistManager.NeedInstance needInstance;
		public GameObject obj;

		public NeedElement(ColonistManager.NeedInstance needInstance, Transform parent) {
			this.needInstance = needInstance;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/NeedElement-Panel"), parent, false);

			obj.transform.Find("NeedName-Text").GetComponent<Text>().text = needInstance.prefab.name;

			Update();
		}

		public void Update() {
			obj.transform.Find("NeedValue-Text").GetComponent<Text>().text = needInstance.GetRoundedValue() + "%";

			obj.transform.Find("Need-Slider").GetComponent<Slider>().value = needInstance.GetValue();
			obj.transform.Find("Need-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkGreen), GetColour(Colours.DarkRed), (needInstance.GetValue() / needInstance.prefab.clampValue));
			obj.transform.Find("Need-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightGreen), GetColour(Colours.LightRed), (needInstance.GetValue() / needInstance.prefab.clampValue));
		}
	}

	public class HappinessModifierElement {

		public ColonistManager.HappinessModifierInstance happinessModifierInstance;
		public GameObject obj;

		public HappinessModifierElement(ColonistManager.HappinessModifierInstance happinessModifierInstance, Transform parent) {
			this.happinessModifierInstance = happinessModifierInstance;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/HappinessModifierElement-Panel"), parent, false);

			obj.transform.Find("HappinessModifierName-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.name;
			if (happinessModifierInstance.prefab.effectAmount > 0) {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGreen);
			} else if (happinessModifierInstance.prefab.effectAmount < 0) {
				obj.GetComponent<Image>().color = GetColour(Colours.LightRed);
			} else {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
			}

			Update();
		}

		public bool Update() {
			if (!happinessModifierInstance.colonist.happinessModifiers.Contains(happinessModifierInstance)) {
				MonoBehaviour.Destroy(obj);
				return false;
			}
			if (happinessModifierInstance.prefab.infinite) {
				obj.transform.Find("HappinessModifierTime-Text").GetComponent<Text>().text = "Until Not";
			} else {
				obj.transform.Find("HappinessModifierTime-Text").GetComponent<Text>().text = Mathf.RoundToInt(happinessModifierInstance.timer) + "s (" + Mathf.RoundToInt(happinessModifierInstance.prefab.effectLengthSeconds) + "s)";
			}
			if (happinessModifierInstance.prefab.effectAmount > 0) {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = "+" + happinessModifierInstance.prefab.effectAmount + "%";
			} else if (happinessModifierInstance.prefab.effectAmount < 0) {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.effectAmount + "%";
			} else {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.effectAmount + "%";
			}
			return true;
		}
	}

	public void SetRightListPanelSize() {
		RectTransform rightListPanel = gameUI.transform.Find("RightList-Panel").GetComponent<RectTransform>();
		Vector2 rightListSize = rightListPanel.offsetMin;
		int verticalOffset = 5; // Nothing active default
		if (selectedColonistInformationPanel.activeSelf) {
			verticalOffset = 360;
		} else if (selectedTraderMenu.activeSelf) {
			verticalOffset = 160;
		}
		rightListPanel.offsetMin = new Vector2(rightListSize.x, verticalOffset);
	}

	private List<NeedElement> selectedColonistNeedElements = new List<NeedElement>();
	private List<HappinessModifierElement> selectedColonistHappinessModifierElements = new List<HappinessModifierElement>();

	private List<SkillElement> selectedColonistSkillElements = new List<SkillElement>();

	private List<InventoryElement> selectedColonistInventoryElements = new List<InventoryElement>();
	private List<ReservedResourcesColonistElement> selectedColonistReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

	public void SetSelectedColonistInformation(bool sameColonistSelected) {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;

			selectedColonistInformationPanel.SetActive(true);

			selectedColonistInformationPanel.transform.Find("ColonistName-Text").GetComponent<Text>().text = GameManager.humanM.selectedHuman.name + " (" + GameManager.humanM.selectedHuman.gender.ToString()[0] + ")";
			selectedColonistInformationPanel.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = GameManager.humanM.selectedHuman.moveSprites[0];

			selectedColonistInformationPanel.transform.Find("AffiliationName-Text").GetComponent<Text>().text = "Colonist of " + GameManager.colonyM.colony.name;

			RemakeSelectedColonistNeeds();
			RemakeSelectedColonistHappinessModifiers();
			RemakeSelectedColonistSkills();
			RemakeSelectedColonistInventory(selectedColonist);
			RemakeSelectedColonistClothing(selectedColonist, sameColonistSelected);
		} else {
			selectedColonistInformationPanel.SetActive(false);

			if (selectedColonistSkillElements.Count > 0) {
				foreach (SkillElement skillElement in selectedColonistSkillElements) {
					MonoBehaviour.Destroy(skillElement.obj);
				}
				selectedColonistSkillElements.Clear();
			}
			if (selectedColonistNeedElements.Count > 0) {
				foreach (NeedElement needElement in selectedColonistNeedElements) {
					MonoBehaviour.Destroy(needElement.obj);
				}
				selectedColonistNeedElements.Clear();
			}
			if (selectedColonistReservedResourcesColonistElements.Count > 0) {
				foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
					foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
						MonoBehaviour.Destroy(reservedResourceElement.obj);
					}
					reservedResourcesColonistElement.reservedResourceElements.Clear();
					MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
				}
				selectedColonistReservedResourcesColonistElements.Clear();
			}
			if (selectedColonistInventoryElements.Count > 0) {
				foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
					MonoBehaviour.Destroy(inventoryElement.obj);
				}
				selectedColonistInventoryElements.Clear();
			}
		}
	}

	public void SetSelectedColonistTab(GameObject tabButtonClicked) {
		bool needsSkillsClicked = tabButtonClicked == selectedColonistNeedsSkillsTabButton;
		bool inventoryClicked = tabButtonClicked == selectedColonistInventoryTabButton;
		bool clothingClicked = tabButtonClicked == selectedColonistClothingTabButton;

		selectedColonistNeedsSkillsPanel.SetActive(needsSkillsClicked);
		selectedColonistNeedsSkillsTabButtonLinkPanel.SetActive(needsSkillsClicked);
		selectedColonistHappinessModifiersPanel.SetActive(false);

		selectedColonistInventoryPanel.SetActive(inventoryClicked);
		selectedColonistInventoryTabButtonLinkPanel.SetActive(inventoryClicked);

		selectedColonistClothingPanel.SetActive(clothingClicked);
		selectedColonistClothingTabButtonLinkPanel.SetActive(clothingClicked);
		SetSelectedColonistClothingSelectionPanelActive(false);
	}

	public void RemakeSelectedColonistNeeds() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;
			foreach (NeedElement needElement in selectedColonistNeedElements) {
				MonoBehaviour.Destroy(needElement.obj);
			}
			selectedColonistNeedElements.Clear();
			List<ColonistManager.NeedInstance> sortedNeeds = selectedColonist.needs.OrderByDescending(need => need.GetValue()).ToList();
			foreach (ColonistManager.NeedInstance need in sortedNeeds) {
				selectedColonistNeedElements.Add(new NeedElement(need, selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/Needs-ScrollPanel/NeedsList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistHappinessModifiers() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;
			foreach (HappinessModifierElement happinessModifierElement in selectedColonistHappinessModifierElements) {
				MonoBehaviour.Destroy(happinessModifierElement.obj);
			}
			selectedColonistHappinessModifierElements.Clear();
			foreach (ColonistManager.HappinessModifierInstance hmi in selectedColonist.happinessModifiers) {
				selectedColonistHappinessModifierElements.Add(new HappinessModifierElement(hmi, selectedColonistHappinessModifiersPanel.transform.Find("HappinessModifier-ScrollPanel/HappinessModifierList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistSkills() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;
			foreach (SkillElement skillElement in selectedColonistSkillElements) {
				MonoBehaviour.Destroy(skillElement.obj);
			}
			selectedColonistSkillElements.Clear();
			List<ColonistManager.SkillInstance> sortedSkills = selectedColonist.skills.OrderByDescending(skill => skill.CalculateTotalSkillLevel()).ToList();
			foreach (ColonistManager.SkillInstance skill in sortedSkills) {
				selectedColonistSkillElements.Add(new SkillElement(selectedColonist, skill, selectedColonistNeedsSkillsPanel.transform.Find("Skills-Panel/Skills-ScrollPanel/SkillsList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistInventory(ColonistManager.Colonist selectedColonist) {
		foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
			foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
				MonoBehaviour.Destroy(reservedResourceElement.obj);
			}
			reservedResourcesColonistElement.reservedResourceElements.Clear();
			MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
		}
		selectedColonistReservedResourcesColonistElements.Clear();
		foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
			MonoBehaviour.Destroy(inventoryElement.obj);
		}
		selectedColonistInventoryElements.Clear();
		foreach (ResourceManager.ReservedResources rr in GameManager.humanM.selectedHuman.inventory.reservedResources) {
			selectedColonistReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.human, rr, selectedColonistInventoryPanel.transform.Find("Inventory-ScrollPanel/InventoryList-Panel")));
		}
		foreach (ResourceManager.ResourceAmount ra in GameManager.humanM.selectedHuman.inventory.resources) {
			selectedColonistInventoryElements.Add(new InventoryElement(ra, selectedColonistInventoryPanel.transform.Find("Inventory-ScrollPanel/InventoryList-Panel")));
		}

		Button emptyInventoryButton = selectedColonistInventoryPanel.transform.Find("EmptyInventory-Button").GetComponent<Button>();
		emptyInventoryButton.onClick.RemoveAllListeners();
		emptyInventoryButton.onClick.AddListener(delegate { selectedColonist.EmptyInventory(selectedColonist.FindValidContainersToEmptyInventory()); });
	}

	public void RemakeSelectedColonistClothing(ColonistManager.Colonist selectedColonist, bool selectionPanelKeepState) {
		selectedColonistClothingPanel.transform.Find("ColonistBody-Image").GetComponent<Image>().sprite = selectedColonist.moveSprites[0];

		foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in selectedColonist.clothes) {
			Sprite clothingSprite = GameManager.resourceM.clearSquareSprite;
			string clothingName = "None";
			if (selectedColonist.clothes[appearanceToClothingKVP.Key] != null) {
				clothingSprite = selectedColonist.clothes[appearanceToClothingKVP.Key].moveSprites[0];
				clothingName = selectedColonist.clothes[appearanceToClothingKVP.Key].name;
			}
			selectedColonistClothingPanel.transform.Find("ColonistBody-Image/Colonist" + appearanceToClothingKVP.Key + "-Image").GetComponent<Image>().sprite = clothingSprite;

			Button clothingTypeButton = selectedColonistClothingPanel.transform.Find("ClothingButtons-List/" + appearanceToClothingKVP.Key + "-Button").GetComponent<Button>();
			clothingTypeButton.interactable = GameManager.resourceM.GetClothesByAppearance(appearanceToClothingKVP.Key).Count > 0 || selectedColonist.clothes[appearanceToClothingKVP.Key] != null;
			clothingTypeButton.onClick.RemoveAllListeners();
			clothingTypeButton.onClick.AddListener(delegate { SetSelectedColonistClothingSelectionPanel(true, appearanceToClothingKVP.Key, selectedColonist); });

			clothingTypeButton.transform.Find("Name").GetComponent<Text>().text = clothingName;
			clothingTypeButton.transform.Find("Image").GetComponent<Image>().sprite = clothingSprite;
		}

		if (!selectionPanelKeepState) {
			SetSelectedColonistClothingSelectionPanelActive(false);
		}
	}

	private List<ClothingElement> availableClothingElements = new List<ClothingElement>();
	private List<ClothingElement> takenClothingElements = new List<ClothingElement>();

	public void SetSelectedColonistClothingSelectionPanelActive(bool active) {
		selectedColonistClothingSelectionPanel.SetActive(active);

		Button disrobeButton = selectedColonistClothingSelectionPanel.transform.Find("Disrobe-Button").GetComponent<Button>();
		disrobeButton.onClick.RemoveAllListeners();

		foreach (ClothingElement clothingElement in availableClothingElements) {
			MonoBehaviour.Destroy(clothingElement.obj);
		}
		availableClothingElements.Clear();
		foreach (ClothingElement clothingElement in takenClothingElements) {
			MonoBehaviour.Destroy(clothingElement.obj);
		}
		takenClothingElements.Clear();
	}

	public void SetSelectedColonistClothingSelectionPanel(bool active, HumanManager.Human.Appearance clothingType, ColonistManager.Colonist selectedColonist) {
		SetSelectedColonistClothingSelectionPanelActive(active);

		Button disrobeButton = selectedColonistClothingSelectionPanel.transform.Find("Disrobe-Button").GetComponent<Button>();

		if (selectedColonistClothingSelectionPanel.activeSelf) {
			selectedColonistClothingSelectionPanel.transform.Find("SelectClothes-Text").GetComponent<Text>().text = "Select " + clothingType;

			disrobeButton.interactable = selectedColonist.clothes[clothingType] != null;
			disrobeButton.onClick.AddListener(delegate {
				selectedColonist.ChangeClothing(clothingType, null, selectedColonist.clothes[clothingType].type);
				SetSelectedColonistClothingSelectionPanelActive(false);
				SetSelectedColonistInformation(true);
			});

			List<ResourceManager.Clothing> clothes = GameManager.resourceM.GetClothesByAppearance(clothingType);
			foreach (ResourceManager.Clothing clothing in clothes.Where(c => c.GetWorldTotalAmount() > 0)) {
				if (clothing.GetAvailableAmount() > 0) {
					availableClothingElements.Add(new ClothingElement(clothing, GameManager.humanM.selectedHuman, availableClothingTitleAndList.Value.transform));
				} else if (selectedColonist.clothes[clothingType] == null || clothing.name != selectedColonist.clothes[clothingType].name) {
					takenClothingElements.Add(new ClothingElement(clothing, GameManager.humanM.selectedHuman, takenClothingTitleAndList.Value.transform));
				}
			}

			availableClothingTitleAndList.Key.SetActive(availableClothingElements.Count > 0);
			availableClothingTitleAndList.Value.SetActive(availableClothingElements.Count > 0);
			takenClothingTitleAndList.Key.SetActive(takenClothingElements.Count > 0);
			takenClothingTitleAndList.Value.SetActive(takenClothingElements.Count > 0);
		}
	}

	public class ClothingElement {
		public ResourceManager.Clothing clothing;
		public HumanManager.Human human;
		public GameObject obj;

		public ClothingElement(ResourceManager.Clothing clothing, HumanManager.Human human, Transform parent) {
			this.clothing = clothing;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ClothingElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = clothing.name;
			obj.transform.Find("Image").GetComponent<Image>().sprite = clothing.moveSprites[0];
			obj.transform.Find("InsulationWaterResistance").GetComponent<Text>().text = "❄ " + clothing.prefab.insulation + " / ☂ " + clothing.prefab.waterResistance;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				human.ChangeClothing(clothing.prefab.appearance, clothing, clothing.type);
				GameManager.uiM.SetSelectedColonistClothingSelectionPanelActive(false);
				GameManager.uiM.SetSelectedColonistInformation(true);
			});
		}
	}

	private static readonly Dictionary<int, int> happinessModifierButtonSizeMap = new Dictionary<int, int>() {
		{ 1, 45 }, { 2, 60 }, { 3, 65 }
	};
	private static readonly Dictionary<int, int> happinessModifierValueHorizontalPositionMap = new Dictionary<int, int>() {
		{ 1, -50 }, { 2, -65 }, { 3, -70 }
	};

	private List<HappinessModifierElement> removeHME = new List<HappinessModifierElement>();

	public void UpdateSelectedColonistInformation() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.health * 100);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedColonist.health);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.health);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.health * 100) + "%";

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHappiness-Panel/ColonistHappiness-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.effectiveHappiness);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHappiness-Panel/ColonistHappiness-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedColonist.effectiveHappiness / 100f);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHappiness-Panel/ColonistHappiness-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.effectiveHappiness / 100f);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHappiness-Panel/ColonistHappinessValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveHappiness) + "%";

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/ColonistInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/ColonistInventory-Slider").GetComponent<Slider>().maxValue = selectedColonist.inventory.maxAmount;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/ColonistInventory-Slider").GetComponent<Slider>().value = selectedColonist.inventory.CountResources();
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/ColonistInventoryValue-Text").GetComponent<Text>().text = selectedColonist.inventory.CountResources() + "/ " + selectedColonist.inventory.maxAmount;

			selectedColonistInformationPanel.transform.Find("ColonistCurrentAction-Text").GetComponent<Text>().text = GameManager.jobM.GetJobDescription(selectedColonist.job);
			if (selectedColonist.storedJob != null) {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = GameManager.jobM.GetJobDescription(selectedColonist.storedJob);
			} else {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = string.Empty;
			}

			selectedColonistNeedsSkillsPanel.transform.Find("Skills-Panel/ProfessionLabel-Text").GetComponent<Text>().text = selectedColonist.profession.name;

			int happinessModifiersSum = Mathf.RoundToInt(selectedColonist.happinessModifiersSum);

			int happinessLength = Mathf.Abs(happinessModifiersSum).ToString().Length;
			Text happinessModifierAmountText = selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessModifiers-Button/HappinessModifiersAmount-Text").GetComponent<Text>();
			if (happinessModifiersSum > 0) {
				happinessModifierAmountText.text = "+" + happinessModifiersSum + "%";
				happinessModifierAmountText.color = GetColour(Colours.LightGreen);
			} else if (happinessModifiersSum < 0) {
				happinessModifierAmountText.text = happinessModifiersSum + "%";
				happinessModifierAmountText.color = GetColour(Colours.LightRed);
			} else {
				happinessModifierAmountText.text = happinessModifiersSum + "%";
				happinessModifierAmountText.color = GetColour(Colours.DarkGrey50);
			}
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessModifiers-Button").GetComponent<RectTransform>().sizeDelta = new Vector2(happinessModifierButtonSizeMap[happinessLength], 20);
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<RectTransform>().offsetMax = new Vector2(happinessModifierValueHorizontalPositionMap[happinessLength], 0);
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveHappiness) + "%";
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<Text>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.effectiveHappiness / 100f);

			foreach (SkillElement skillElement in selectedColonistSkillElements) {
				skillElement.Update();
			}
			foreach (NeedElement needElement in selectedColonistNeedElements) {
				needElement.Update();
			}
			foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
				inventoryElement.Update();
			}
			foreach (HappinessModifierElement happinessModifierElement in selectedColonistHappinessModifierElements) {
				bool keep = happinessModifierElement.Update();
				if (!keep) {
					removeHME.Add(happinessModifierElement);
				}
			}
			if (removeHME.Count > 0) {
				foreach (HappinessModifierElement happinessModifierElement in removeHME) {
					MonoBehaviour.Destroy(happinessModifierElement.obj);
					selectedColonistHappinessModifierElements.Remove(happinessModifierElement);
				}
				removeHME.Clear();
			}
		}
	}

	public class ColonistElement {

		public ColonistManager.Colonist colonist;
		public GameObject obj;

		public ColonistElement(ColonistManager.Colonist colonist, Transform transform) {
			this.colonist = colonist;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), transform, false);

			obj.GetComponent<RectTransform>().sizeDelta = new Vector2(170, obj.GetComponent<RectTransform>().sizeDelta.y);

			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });

			Update();
		}

		public void Update() {
			obj.GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), colonist.health);
			obj.GetComponent<Outline>().effectColor = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), colonist.health);
		}

		public void DestroyObject() {
			MonoBehaviour.Destroy(obj);
		}
	}

	List<ColonistElement> colonistElements = new List<ColonistElement>();

	public void RemoveColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.DestroyObject();
		}
		colonistElements.Clear();
	}

	public void SetColonistElements() {
		RemoveColonistElements();
		if (colonistsPanel.activeSelf) {
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				colonistElements.Add(new ColonistElement(colonist, colonistsPanel.transform.Find("ColonistList-Panel")));
			}
		}
	}

	public void UpdateColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.Update();
		}
	}

	public class CaravanElement {

		public CaravanManager.Caravan caravan;
		public GameObject obj;

		private Text affiliatedColonyNameText;
		private Text wealthLevelText;
		private Text resourceRichnessText;
		private Text cityClassText;
		private Text biomeTemperatureText;

		public CaravanElement(CaravanManager.Caravan caravan, Transform transform) {
			this.caravan = caravan;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CaravanElement-Panel"), transform, false);

			obj.GetComponent<RectTransform>().sizeDelta = new Vector2(170, obj.GetComponent<RectTransform>().sizeDelta.y);

			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(caravan.traders[0]); });

			affiliatedColonyNameText = obj.transform.Find("AffiliatedColonyName-Text").GetComponent<Text>();
			wealthLevelText = obj.transform.Find("AffiliatedColonyStats-Panel/WealthLevel-Text").GetComponent<Text>();
			resourceRichnessText = obj.transform.Find("AffiliatedColonyStats-Panel/ResourceRichness-Text").GetComponent<Text>();
			cityClassText = obj.transform.Find("AffiliatedColonyStats-Panel/CityClass-Text").GetComponent<Text>();
			biomeTemperatureText = obj.transform.Find("AffiliatedColonyStats-Panel/BiomeTemperatureClass-Text").GetComponent<Text>();

			Update();
		}

		public void Update() {
			obj.GetComponent<Image>().color = caravan.confirmedResourcesToTrade.Count > 0 ? GetColour(Colours.LightYellow) : GetColour(Colours.LightPurple);
			obj.GetComponent<Outline>().effectColor = caravan.confirmedResourcesToTrade.Count > 0 ? GetColour(Colours.DarkYellow) : GetColour(Colours.DarkPurple);

			Color textColour = caravan.confirmedResourcesToTrade.Count > 0 ? GetColour(Colours.DarkGrey50) : GetColour(Colours.LightGrey220);
			affiliatedColonyNameText.color = textColour;
			wealthLevelText.color = textColour;
			resourceRichnessText.color = textColour;
			cityClassText.color = textColour;
			biomeTemperatureText.color = textColour;

			obj.GetComponent<Button>().enabled = caravan.traders.Count > 0;
		}

		public void DestroyObject() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private List<CaravanElement> caravanElements = new List<CaravanElement>();

	public void RemoveCaravanElements() {
		foreach (CaravanElement caravanElement in caravanElements) {
			caravanElement.DestroyObject();
		}
		caravanElements.Clear();
	}

	public void SetCaravanElements() {
		if (GameManager.caravanM.caravans.Count > 0) {
			RemoveCaravanElements();
			caravansPanel.SetActive(true);
			if (caravansPanel.activeSelf) {
				foreach (CaravanManager.Caravan caravan in GameManager.caravanM.caravans) {
					caravanElements.Add(new CaravanElement(caravan, caravansPanel.transform.Find("CaravanList-Panel")));
				}
			}
		} else {
			caravansPanel.SetActive(false);
			RemoveCaravanElements();
		}
	}

	public void UpdateCaravanElements() {
		foreach (CaravanElement caravanElement in caravanElements) {
			caravanElement.Update();
		}
	}

	public class JobElement {
		public JobManager.Job job;
		public ColonistManager.Colonist colonist;
		public GameObject obj;
		public GameObject colonistObj;

		public JobElement(JobManager.Job job, ColonistManager.Colonist colonist, Transform parent) {
			this.job = job;
			this.colonist = colonist;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/JobInfoElement-Panel"), parent, false);
			Text jobInfoNameText = obj.transform.Find("JobInfo/Name").GetComponent<Text>();

			obj.transform.Find("JobInfo/Image").GetComponent<Image>().sprite = job.prefab.baseSprite;
			if (job.prefab.jobType == JobManager.JobTypesEnum.Mine ||
				job.prefab.jobType == JobManager.JobTypesEnum.Dig) {
				jobInfoNameText.text = job.tile.tileType.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.PlantPlant) {
				jobInfoNameText.text = job.plant.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.ChopPlant) {
				jobInfoNameText.text = job.tile.plant.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.PlantFarm) {
				jobInfoNameText.text = job.prefab.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.HarvestFarm) {
				jobInfoNameText.text = job.tile.farm.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
				jobInfoNameText.text = job.createResource.name;
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.Remove) {
				jobInfoNameText.text = job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.name;
			} else {
				jobInfoNameText.text = job.prefab.name;
			}
			obj.transform.Find("JobInfo/Type").GetComponent<Text>().text = SplitByCapitals(job.prefab.jobType.ToString());
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				GameManager.cameraM.SetCameraPosition(job.tile.obj.transform.position);
			});

			if (job.priority > 0) {
				obj.transform.Find("JobInfo").GetComponent<Image>().color = GetColour(Colours.LightYellow);
				obj.GetComponent<Outline>().effectColor = GetColour(Colours.DarkYellow);
			} else if (job.priority < 0) {
				obj.transform.Find("JobInfo").GetComponent<Image>().color = GetColour(Colours.LightRed);
				obj.GetComponent<Outline>().effectColor = GetColour(Colours.DarkRed);
			} else {
				obj.transform.Find("JobInfo").GetComponent<Image>().color = GetColour(Colours.LightGrey220);
				obj.GetComponent<Outline>().effectColor = GetColour(Colours.LightGrey180);
			}

			if (colonist != null) {
				obj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<RectTransform>().sizeDelta.x, 77);

				colonistObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), obj.transform, false);
				colonistObj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
				colonistObj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
				colonistObj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });
				colonistObj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<RectTransform>().sizeDelta.x, colonistObj.GetComponent<RectTransform>().sizeDelta.y);
				colonistObj.GetComponent<Outline>().enabled = false;

				if (colonist.job.started) {
					obj.GetComponent<Image>().color = GetColour(Colours.LightGreen);
					obj.GetComponent<Outline>().effectColor = GetColour(Colours.DarkGreen);
					colonistObj.GetComponent<Image>().color = GetColour(Colours.LightGreen);

					obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().minValue = 0;
					obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
					obj.transform.Find("JobInfo/JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightGreen);
				} else {
					obj.GetComponent<Image>().color = GetColour(Colours.LightOrange);
					obj.GetComponent<Outline>().effectColor = GetColour(Colours.DarkOrange);
					colonistObj.GetComponent<Image>().color = GetColour(Colours.LightOrange);

					obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().minValue = 0;
					obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().maxValue = colonist.startPathLength;
					obj.transform.Find("JobInfo/JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightOrange);
				}
			}

			job.jobUIElement = this;

			Update();
		}

		public void Update() {
			if (colonist != null && !colonist.job.started && colonist.startPathLength > 0 && colonist.path.Count > 0) {
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().value = (1 - (colonist.path.Count / (float)colonist.startPathLength)) * colonist.startPathLength;//Mathf.Lerp((1 - (colonist.path.Count / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - ((colonist.path.Count - 1) / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - Vector2.Distance(colonist.obj.transform.position, colonist.path[0].obj.transform.position)) + 0.5f);
			} else {
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().value = job.colonistBuildTime - job.jobProgress;
			}
		}

		public void DestroyObjects() {
			job.jobUIElement = null;
			MonoBehaviour.Destroy(obj);
			MonoBehaviour.Destroy(colonistObj);
		}

		public void Remove() {
			GameManager.uiM.jobElements.Remove(this);
			job.jobUIElement = null;
			DestroyObjects();
		}
	}

	public List<JobElement> jobElements = new List<JobElement>();

	public void RemoveJobElements() {
		foreach (JobElement jobElement in jobElements) {
			jobElement.job.jobUIElement = null;
			jobElement.DestroyObjects();
		}
		jobElements.Clear();
	}

	public void SetJobElements() {
		if (GameManager.jobM.jobs.Count > 0 || GameManager.colonistM.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
			RemoveJobElements();
			List<ColonistManager.Colonist> orderedColonists = GameManager.colonistM.colonists.Where(colonist => colonist.job != null).ToList();
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => colonist.job.started).OrderBy(colonist => colonist.job.jobProgress)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
			}
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => !colonist.job.started).OrderBy(colonist => colonist.path.Count)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
			}
			foreach (JobManager.Job job in GameManager.jobM.jobs.Where(j => j.started).OrderBy(j => (j.jobProgress / j.colonistBuildTime))) {
				jobElements.Add(new JobElement(job, null, jobListPanel.transform));
			}
			foreach (JobManager.Job job in GameManager.jobM.jobs.Where(j => !j.started).OrderByDescending(j => j.priority)) {
				jobElements.Add(new JobElement(job, null, jobListPanel.transform));
			}
			jobsPanel.SetActive(true);
		} else {
			jobsPanel.SetActive(false);
			RemoveJobElements();
		}
	}

	public void UpdateJobElements() {
		foreach (JobElement jobElement in jobElements) {
			jobElement.Update();
		}
	}

	public void SetListPanelCollapsed(GameObject listPanel, Text arrowText) {
		listPanel.SetActive(!listPanel.activeSelf);
		if (listPanel.activeSelf) {
			arrowText.text = ">";
		} else {
			arrowText.text = "<";
		}
	}

	public void UpdateDateTimeInformation(int minute, int hour, int day, int month, int year, bool isDay) {
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Time-Text").GetComponent<Text>().text = GameManager.timeM.Get12HourTime() + ":" + (minute < 10 ? ("0" + minute) : minute.ToString()) + (hour < 12 || hour > 23 ? "AM" : "PM") + " (" + (isDay ? "D" : "N") + ")";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = GameManager.timeM.GetTimeModifier() > 0 ? new string('>', GameManager.timeM.GetTimeModifier()) : "-";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Date-Text").GetComponent<Text>().text = "D" + day + " M" + month + " Y" + year;
	}

	public void SelectionSizeCanvasSetActive(bool active) {
		selectionSizeCanvas.SetActive(active);
	}

	public void UpdateSelectionSizePanel(float xSize, float ySize, int selectionAreaCount, ResourceManager.TileObjectPrefab prefab) {
		int ixSize = Mathf.Abs(Mathf.FloorToInt(xSize));
		int iySize = Mathf.Abs(Mathf.FloorToInt(ySize));

		selectionSizePanel.transform.Find("Dimensions-Text").GetComponent<Text>().text = ixSize + " × " + iySize;
		selectionSizePanel.transform.Find("TotalSize-Text").GetComponent<Text>().text = (Mathf.RoundToInt(ixSize * iySize)) + " (" + selectionAreaCount + ")";
		selectionSizePanel.transform.Find("SelectedPrefabName-Text").GetComponent<Text>().text = prefab.name;
		selectionSizePanel.transform.Find("SelectedPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite;

		selectionSizeCanvas.transform.localScale = Vector2.one * 0.005f * 0.5f * GameManager.cameraM.cameraComponent.orthographicSize;
		selectionSizeCanvas.transform.position = new Vector2(
			mousePosition.x + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.x / 2f * selectionSizeCanvas.transform.localScale.x),
			mousePosition.y + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.y / 2f * selectionSizeCanvas.transform.localScale.y)
		);
	}

	public void InitializeSelectedContainerIndicator() {
		selectedContainerIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
		SpriteRenderer sCISR = selectedContainerIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = GameManager.resourceM.selectionCornersSprite;
		sCISR.name = "SelectedContainerIndicator";
		sCISR.sortingOrder = 20; // Selected Container Indicator Sprite
		sCISR.color = new Color(1f, 1f, 1f, 0.75f);
		selectedContainerIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
		selectedContainerIndicator.SetActive(false);
	}

	List<ReservedResourcesColonistElement> containerReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();
	List<InventoryElement> containerInventoryElements = new List<InventoryElement>();
	public void SetSelectedContainerInfo() {
		if (selectedContainer != null) {
			selectedContainerIndicator.SetActive(true);
			selectedContainerIndicator.transform.position = selectedContainer.obj.transform.position;

			selectedContainerInventoryPanel.SetActive(true);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					MonoBehaviour.Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				MonoBehaviour.Destroy(inventoryElement.obj);
			}
			containerInventoryElements.Clear();

			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventoryName-Text").GetComponent<Text>().text = selectedContainer.prefab.name;

			int numResources = selectedContainer.inventory.CountResources();
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().maxValue = selectedContainer.prefab.maxInventoryAmount;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().value = numResources;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventorySizeValue-Text").GetComponent<Text>().text = numResources + "/ " + selectedContainer.prefab.maxInventoryAmount;

			foreach (ResourceManager.ReservedResources rr in selectedContainer.inventory.reservedResources) {
				containerReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.human, rr, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel")));
			}
			foreach (ResourceManager.ResourceAmount ra in selectedContainer.inventory.resources) {
				containerInventoryElements.Add(new InventoryElement(ra, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel")));
			}
			selectedContainerInventoryPanel.transform.Find("SelectedContainerSprite-Image").GetComponent<Image>().sprite = selectedContainer.obj.GetComponent<SpriteRenderer>().sprite;
		} else {
			selectedContainerIndicator.SetActive(false);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					MonoBehaviour.Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				MonoBehaviour.Destroy(inventoryElement.obj);
			}
			containerInventoryElements.Clear();
			selectedContainerInventoryPanel.SetActive(false);
		}
	}

	public void UpdateSelectedContainerInfo() {
		foreach (InventoryElement inventoryElement in containerInventoryElements) {
			inventoryElement.Update();
		}
	}

	public void DisableAdminPanels(GameObject parentObj) {
		if (professionsList.activeSelf && parentObj != professionsList) {
			DisableProfessionsList();
		}
		if (objectPrefabsList.activeSelf && parentObj != objectPrefabsList) {
			DisableObjectPrefabsList();
		}
		if (clothesMenuPanel.activeSelf && parentObj != clothesMenuPanel) {
			DisableClothesList();
		}
		if (resourcesMenuPanel.activeSelf && parentObj != resourcesMenuPanel) {
			DisableResourcesList();
		}
	}

	public class ProfessionElement {
		public ColonistManager.Profession profession;
		public GameObject obj;
		public GameObject colonistsInProfessionListObj;
		public List<GameObject> colonistsInProfessionElements = new List<GameObject>();
		public GameObject editColonistsInProfessionListObj;
		public List<GameObject> editColonistsInProfessionElements = new List<GameObject>();
		public bool lastRemoveState = false;

		public ProfessionElement(ColonistManager.Profession profession, Transform parent, UIManager uiM) {
			this.profession = profession;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ProfessionInfoElement-Panel"), parent, false);

			colonistsInProfessionListObj = obj.transform.Find("ColonistsInProfessionList-Panel").gameObject;
			obj.transform.Find("ColonistsInProfession-Button").GetComponent<Button>().onClick.AddListener(delegate {
				foreach (ProfessionElement professionElement in uiM.professionElements) {
					if (professionElement != this) {
						professionElement.colonistsInProfessionListObj.SetActive(false);
					}
					foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
						MonoBehaviour.Destroy(obj);
					}
				}
				colonistsInProfessionListObj.SetActive(!colonistsInProfessionListObj.activeSelf);
				if (colonistsInProfessionListObj.activeSelf) {
					uiM.SetColonistsInProfessionList(this);
				}
			});

			editColonistsInProfessionListObj = obj.transform.Find("EditColonistsInProfessionList-Panel").gameObject;

			obj.transform.Find("ColonistsInProfession-Button/ColonistsInProfessionAmount-Text").GetComponent<Text>().text = profession.colonistsInProfession.Count.ToString();

			obj.transform.Find("ProfessionName-Text").GetComponent<Text>().text = profession.name;

			obj.transform.Find("AddColonists-Button").GetComponent<Button>().onClick.AddListener(delegate {
				foreach (ProfessionElement professionElement in uiM.professionElements) {
					if (professionElement != this) {
						professionElement.editColonistsInProfessionListObj.SetActive(false);
					}
					foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
						MonoBehaviour.Destroy(obj);
					}
				}
				editColonistsInProfessionListObj.SetActive(!editColonistsInProfessionListObj.activeSelf);
				if (lastRemoveState) {
					editColonistsInProfessionListObj.SetActive(true);
				}
				if (editColonistsInProfessionListObj.activeSelf) {
					uiM.SetEditColonistsInProfessionList(this, false);
				}
				lastRemoveState = false;
			});
			if (profession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
				obj.transform.Find("RemoveColonists-Button").GetComponent<Button>().onClick.AddListener(delegate {
					foreach (ProfessionElement professionElement in uiM.professionElements) {
						if (professionElement != this) {
							professionElement.editColonistsInProfessionListObj.SetActive(false);
						}
						foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
							MonoBehaviour.Destroy(obj);
						}
					}
					editColonistsInProfessionListObj.SetActive(!editColonistsInProfessionListObj.activeSelf);
					if (!lastRemoveState) {
						editColonistsInProfessionListObj.SetActive(true);
					}
					if (editColonistsInProfessionListObj.activeSelf) {
						uiM.SetEditColonistsInProfessionList(this, true);
					}
					lastRemoveState = true;
				});
			} else {
				obj.transform.Find("RemoveColonists-Button").gameObject.SetActive(false);
			}
			colonistsInProfessionListObj.SetActive(false);
			editColonistsInProfessionListObj.SetActive(false);
		}

		public void Update(int colonistCount) {
			obj.transform.Find("ColonistsInProfession-Button/ColonistsInProfessionAmount-Text").GetComponent<Text>().text = profession.colonistsInProfession.Count.ToString();

			bool colonistsInProfessionGreaterThanZero = profession.colonistsInProfession.Count > 0;
			obj.transform.Find("ColonistsInProfession-Button").GetComponent<Button>().interactable = colonistsInProfessionGreaterThanZero;
			obj.transform.Find("AddColonists-Button").GetComponent<Button>().interactable = colonistCount != profession.colonistsInProfession.Count;
			obj.transform.Find("RemoveColonists-Button").GetComponent<Button>().interactable = colonistsInProfessionGreaterThanZero;
		}
	}

	public void SetColonistsInProfessionList(ProfessionElement professionElement) {
		foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
			MonoBehaviour.Destroy(obj);
		}
		professionElement.colonistsInProfessionElements.Clear();
		foreach (ColonistManager.Colonist colonist in professionElement.profession.colonistsInProfession) {
			GameObject obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), professionElement.colonistsInProfessionListObj.transform, false);
			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				GameManager.humanM.SetSelectedHuman(colonist);
			});
			professionElement.colonistsInProfessionElements.Add(obj);
		}
	}



	private void UpdateProfessionLevelInfo(GameObject obj, ColonistManager.Colonist colonist, ColonistManager.Profession currentProfession, ColonistManager.Profession nextProfession) {
		obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = currentProfession.name;
		if (colonist.profession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
			double currentSkillLevel = currentProfession.CalculateSkillLevelFromPrimarySkill(colonist, true, 2);
			obj.transform.Find("ColonistProfessionLevel-Text").GetComponent<Text>().text = currentSkillLevel.ToString();
		} else {
			obj.transform.Find("ColonistProfessionLevel-Text").GetComponent<Text>().text = "0";
		}

		obj.transform.Find("ColonistNextProfession-Text").GetComponent<Text>().text = nextProfession.name;
		if (nextProfession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
			double nextSkillLevel = nextProfession.CalculateSkillLevelFromPrimarySkill(colonist, true, 2);
			obj.transform.Find("ColonistNextProfessionLevel-Text").GetComponent<Text>().text = nextSkillLevel.ToString();
		} else {
			obj.transform.Find("ColonistNextProfessionLevel-Text").GetComponent<Text>().text = "0";
		}
	}

	public void SetEditColonistsInProfessionList(ProfessionElement professionElement, bool remove) {
		foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
			MonoBehaviour.Destroy(obj);
		}
		professionElement.editColonistsInProfessionElements.Clear();
		if (remove) { // User clicked red minus button
			List<ColonistManager.Colonist> validColonists = GameManager.colonistM.colonists.Where(c => c.profession == professionElement.profession).ToList();
			validColonists = validColonists.OrderBy(colonist => colonist.profession.CalculateSkillLevelFromPrimarySkill(colonist, false, 0)).ToList();
			foreach (ColonistManager.Colonist colonist in validColonists) {
				GameObject obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"), professionElement.editColonistsInProfessionListObj.transform, false);
				obj.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					if (colonist.profession == professionElement.profession) {
						colonist.ChangeProfession(colonist.oldProfession);
						obj.GetComponent<Image>().color = GetColour(Colours.LightRed);
					} else {
						colonist.ChangeProfession(professionElement.profession);
						obj.GetComponent<Image>().color = GetColour(Colours.LightBlue);
					}
					UpdateProfessionLevelInfo(obj, colonist, colonist.profession, colonist.oldProfession);
				});
				obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];
				obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;

				UpdateProfessionLevelInfo(obj, colonist, colonist.profession, colonist.oldProfession);

				obj.GetComponent<Image>().color = GetColour(Colours.LightBlue);
				professionElement.editColonistsInProfessionElements.Add(obj);
			}
		} else { // User clicked green plus button
			List<ColonistManager.Colonist> validColonists = GameManager.colonistM.colonists.Where(c => c.profession != professionElement.profession).ToList();
			if (professionElement.profession.type == ColonistManager.ProfessionTypeEnum.Nothing) {
				validColonists = validColonists.OrderBy(colonist => professionElement.profession.CalculateSkillLevelFromPrimarySkill(colonist, false, 0)).ToList();
			} else {
				validColonists = validColonists.OrderByDescending(colonist => professionElement.profession.CalculateSkillLevelFromPrimarySkill(colonist, false, 0)).ToList();
			}
			foreach (ColonistManager.Colonist colonist in validColonists) {
				GameObject obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"), professionElement.editColonistsInProfessionListObj.transform, false);
				obj.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					if (colonist.profession == professionElement.profession) {
						colonist.ChangeProfession(colonist.oldProfession);
						obj.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
					} else {
						colonist.ChangeProfession(professionElement.profession);
						obj.GetComponent<Image>().color = GetColour(Colours.LightBlue);
					}
					UpdateProfessionLevelInfo(obj, colonist, colonist.profession, colonist.oldProfession);
				});
				obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];
				obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;

				UpdateProfessionLevelInfo(obj, colonist, colonist.profession, professionElement.profession);

				professionElement.editColonistsInProfessionElements.Add(obj);
			}
		}
	}

	private List<ProfessionElement> professionElements = new List<ProfessionElement>();
	public void CreateProfessionsList() {
		foreach (ProfessionElement professionElement in professionElements) {
			MonoBehaviour.Destroy(professionElement.obj);
		}
		professionElements.Clear();
		foreach (ColonistManager.Profession profession in GameManager.colonistM.professions) {
			professionElements.Add(new ProfessionElement(profession, professionsList.transform, this));
		}
		SetProfessionsList();
	}

	public void SetProfessionsList() {
		DisableAdminPanels(professionsList);
		professionsList.SetActive(!professionsList.activeSelf);
		foreach (ProfessionElement professionElement in professionElements) {
			foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
				MonoBehaviour.Destroy(obj);
			}
			foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
				MonoBehaviour.Destroy(obj);
			}
			professionElement.colonistsInProfessionListObj.SetActive(false);
			professionElement.editColonistsInProfessionListObj.SetActive(false);
			professionElement.obj.SetActive(professionsList.activeSelf);
		}
	}

	public void DisableProfessionsList() {
		professionsList.SetActive(false);
		foreach (ProfessionElement professionElement in professionElements) {
			foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
				MonoBehaviour.Destroy(obj);
			}
			foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
				MonoBehaviour.Destroy(obj);
			}
			professionElement.colonistsInProfessionListObj.SetActive(false);
			professionElement.editColonistsInProfessionListObj.SetActive(false);
			professionElement.obj.SetActive(false);
		}
	}

	public void UpdateProfessionsList() {
		foreach (ProfessionElement professionElement in professionElements) {
			professionElement.Update(GameManager.colonistM.colonists.Count);
		}
	}

	public void SetSelectedTraderMenu() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader) {
			CaravanManager.Trader selectedTrader = (CaravanManager.Trader)GameManager.humanM.selectedHuman;

			selectedTraderMenu.SetActive(true);

			selectedTraderMenu.transform.Find("TraderBaseSprite-Image").GetComponent<Image>().sprite = selectedTrader.moveSprites[0];

			selectedTraderMenu.transform.Find("TraderName-Text").GetComponent<Text>().text = selectedTrader.name;

			selectedTraderMenu.transform.Find("TraderAffiliationName-Text").GetComponent<Text>().text = "Affiliation (WIP)";
			selectedTraderMenu.transform.Find("TraderCurrentAction-Text").GetComponent<Text>().text = "Current Action (WIP)";
			selectedTraderMenu.transform.Find("TraderStoredAction-Text").GetComponent<Text>().text = "Stored Action (WIP)";

			selectedTraderMenu.transform.Find("TradeWithCaravan-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeMenu(); });
		} else {
			selectedTraderMenu.SetActive(false);
		}
	}

	public void UpdateSelectedTraderMenu() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader) {
			CaravanManager.Trader selectedTrader = (CaravanManager.Trader)GameManager.humanM.selectedHuman;

			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedTrader.health * 100);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedTrader.health);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedTrader.health);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedTrader.health * 100) + "%";

			selectedTraderMenu.transform.Find("TraderInventorySlider-Panel/TraderInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedTraderMenu.transform.Find("TraderInventorySlider-Panel/TraderInventory-Slider").GetComponent<Slider>().maxValue = selectedTrader.inventory.maxAmount;
			selectedTraderMenu.transform.Find("TraderInventorySlider-Panel/TraderInventory-Slider").GetComponent<Slider>().value = selectedTrader.inventory.CountResources();
			selectedTraderMenu.transform.Find("TraderInventorySlider-Panel/TraderInventoryValue-Text").GetComponent<Text>().text = selectedTrader.inventory.CountResources() + "/ " + selectedTrader.inventory.maxAmount;
		}
	}

	private List<TradeResourceElement> tradeResourceElements = new List<TradeResourceElement>();

	public void SetTradeMenuActive(bool active) {
		tradeMenu.SetActive(active);

		foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
			MonoBehaviour.Destroy(tradeResourceElement.obj);
		}
		tradeResourceElements.Clear();
	}

	public void SetTradeMenu() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader) {
			CaravanManager.Trader selectedTrader = (CaravanManager.Trader)GameManager.humanM.selectedHuman;
			CaravanManager.Caravan caravan = selectedTrader.caravan;

			SetTradeMenuActive(true);

			tradeMenu.transform.Find("AffiliationCaravanName-Text").GetComponent<Text>().text = "Trade Caravan of {affiliation}";
			tradeMenu.transform.Find("AffiliationDescription-Text").GetComponent<Text>().text =
				"{affiliation-colony} is a {wealth-level}, {resource-richness} {city-class}." +
				"\nIt is in a {biome-temperature-class} climate.";

			RemakeTradeResourceElements(caravan);
		} else {
			SetTradeMenuActive(false);
		}
	}

	private void RemakeTradeResourceElements(CaravanManager.Caravan caravan) {
		foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
			MonoBehaviour.Destroy(tradeResourceElement.obj);
		}
		tradeResourceElements.Clear();
		foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in caravan.GetTradeResourceAmounts()) {
			tradeResourceElements.Add(new TradeResourceElement(tradeResourceAmount, tradeMenu.transform.Find("TradeResources-Panel/TradeResources-ScrollPanel/TradeResourcesList-Panel")));
		}
	}

	public void UpdateTradeMenu() {
		if (tradeMenu.activeSelf) {
			ResourceManager.Resource.Price caravanGainedPrice = new ResourceManager.Resource.Price();
			ResourceManager.Resource.Price colonyGainedPrice = new ResourceManager.Resource.Price();

			int caravanTradeAmount = 0;
			int colonyTradeAmount = 0;

			List<TradeResourceElement> removedTradeResourceElements = new List<TradeResourceElement>();
			foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
				bool removed = tradeResourceElement.Update();
				if (removed) {
					removedTradeResourceElements.Add(tradeResourceElement);
					continue;
				}

				ResourceManager.TradeResourceAmount tradeResourceAmount = tradeResourceElement.tradeResourceAmount;
				int tradeAmount = tradeResourceAmount.GetTradeAmount();

				if (tradeAmount != 0) {
					caravanGainedPrice.ChangePrice(tradeResourceAmount.caravanResourcePrice, tradeAmount);
					colonyGainedPrice.ChangePrice(tradeResourceAmount.resource.price, -tradeAmount);
				}

				caravanTradeAmount -= tradeAmount;
				colonyTradeAmount += tradeAmount;
			}

			tradeMenu.transform.Find("CaravanTradeValueGained-Text").GetComponent<Text>().text = caravanGainedPrice.ToString() + " (" + caravanTradeAmount + ")";
			tradeMenu.transform.Find("ColonyTradeValueGained-Text").GetComponent<Text>().text = "(" + colonyTradeAmount + ") " + colonyGainedPrice.ToString();

			foreach (TradeResourceElement tradeResourceElement in removedTradeResourceElements) {
				tradeResourceElements.Remove(tradeResourceElement);
			}
			removedTradeResourceElements.Clear();
		}
	}

	public void ConfirmTrade() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader) {
			CaravanManager.Trader selectedTrader = (CaravanManager.Trader)GameManager.humanM.selectedHuman;
			CaravanManager.Caravan caravan = selectedTrader.caravan;

			SetTradeMenuActive(false);
			caravan.ConfirmTrade();
		}
	}

	public class ObjectPrefabElement {
		public ResourceManager.TileObjectPrefab prefab;
		public GameObject obj;
		public GameObject objectInstancesList;
		public List<ObjectInstanceElement> instanceElements = new List<ObjectInstanceElement>();

		public ObjectPrefabElement(ResourceManager.TileObjectPrefab prefab, Transform parent) {
			this.prefab = prefab;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectPrefab-Button"), parent, false);

			obj.transform.Find("ObjectPrefabSprite-Panel/ObjectPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite;
			obj.transform.Find("ObjectPrefabName-Text").GetComponent<Text>().text = prefab.name;

			objectInstancesList = obj.transform.Find("ObjectInstancesList-ScrollPanel").gameObject;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				objectInstancesList.SetActive(!objectInstancesList.activeSelf);
				if (objectInstancesList.activeSelf) {
					objectInstancesList.transform.SetParent(GameManager.uiM.canvas.transform);
					foreach (ObjectPrefabElement objectPrefabElement in GameManager.uiM.objectPrefabElements) {
						if (objectPrefabElement != this) {
							objectPrefabElement.objectInstancesList.SetActive(false);
						}
					}
				} else {
					objectInstancesList.transform.SetParent(obj.transform);
				}
			});

			AddObjectInstancesList(true);

			Update();
		}

		public void AddObjectInstancesList(bool newList) {
			RemoveObjectInstances();
			bool objectInstancesListState = objectInstancesList.activeSelf;
			if (newList) {
				objectInstancesListState = false;
			}
			objectInstancesList.SetActive(true);
			foreach (ResourceManager.TileObjectInstance instance in GameManager.resourceM.GetTileObjectInstanceList(prefab)) {
				instanceElements.Add(new ObjectInstanceElement(instance, objectInstancesList.transform.Find("ObjectInstancesList-Panel")));
			}
			objectInstancesList.SetActive(objectInstancesListState);
			Update();
		}

		public void Remove() {
			RemoveObjectInstances();
			MonoBehaviour.Destroy(obj);
		}

		public void RemoveObjectInstances() {
			foreach (ObjectInstanceElement instance in instanceElements) {
				MonoBehaviour.Destroy(instance.obj);
			}
			instanceElements.Clear();
		}

		public void Update() {
			obj.transform.Find("ObjectPrefabAmount-Text").GetComponent<Text>().text = instanceElements.Count.ToString();
		}
	}

	public class ObjectInstanceElement {
		public ResourceManager.TileObjectInstance instance;
		public GameObject obj;

		public ObjectInstanceElement(ResourceManager.TileObjectInstance instance, Transform parent) {
			this.instance = instance;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectInstance-Button"), parent, false);

			obj.transform.Find("ObjectInstanceSprite-Panel/ObjectInstanceSprite-Image").GetComponent<Image>().sprite = instance.obj.GetComponent<SpriteRenderer>().sprite;
			obj.transform.Find("ObjectInstanceName-Text").GetComponent<Text>().text = instance.prefab.name;
			obj.transform.Find("TilePosition-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(instance.tile.obj.transform.position.x) + ", " + Mathf.FloorToInt(instance.tile.obj.transform.position.y) + ")"; ;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				GameManager.cameraM.SetCameraPosition(instance.obj.transform.position);
			});

			ResourceManager.Container container = GameManager.resourceM.containers.Find(findContainer => findContainer == instance);
			if (container != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.selectedContainer = container;
					GameManager.uiM.SetSelectedContainerInfo();
				});
			}
			ResourceManager.ManufacturingTileObject mto = GameManager.resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO == instance);
			if (mto != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedManufacturingTileObject(mto);
				});
			}
		}
	}

	public List<ObjectPrefabElement> objectPrefabElements = new List<ObjectPrefabElement>();

	public void ToggleObjectPrefabsList(bool changeActiveState) {
		if (changeActiveState) {
			DisableAdminPanels(objectPrefabsList);
			objectPrefabsList.SetActive(!objectPrefabsList.activeSelf);
		}
		foreach (ObjectPrefabElement objectPrefabElement in objectPrefabElements) {
			objectPrefabElement.objectInstancesList.transform.SetParent(objectPrefabElement.obj.transform);
			objectPrefabElement.objectInstancesList.SetActive(false);
		}
		if (objectPrefabElements.Count <= 0) {
			objectPrefabsList.SetActive(false);
		}
	}

	public void DisableObjectPrefabsList() {
		objectPrefabsList.SetActive(!objectPrefabsList.activeSelf);
		ToggleObjectPrefabsList(false);
	}

	public enum ChangeTypesEnum { Add, Update, Remove };

	public void ChangeObjectPrefabElements(ChangeTypesEnum changeType, ResourceManager.TileObjectPrefab prefab) {
		if (prefab.tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.None || prefab.tileObjectPrefabSubGroup.tileObjectPrefabGroup.type == ResourceManager.TileObjectPrefabGroupsEnum.Command) {
			return;
		}
		switch (changeType) {
			case ChangeTypesEnum.Add:
				AddObjectPrefabElement(prefab);
				break;
			case ChangeTypesEnum.Update:
				UpdateObjectPrefabElement(prefab);
				break;
			case ChangeTypesEnum.Remove:
				RemoveObjectPrefabElement(prefab);
				break;
		}
	}

	private void AddObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement == null) {
			objectPrefabElements.Add(new ObjectPrefabElement(prefab, objectPrefabsList.transform.Find("ObjectPrefabsList-Panel")));
		} else {
			UpdateObjectPrefabElement(prefab);
		}
	}

	private void UpdateObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement != null) {
			objectPrefabElement.AddObjectInstancesList(false);
		}
	}

	private void RemoveObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement != null) {
			objectPrefabElement.Remove();
			objectPrefabElements.Remove(objectPrefabElement);
		}
		if (objectPrefabElements.Count <= 0) {
			objectPrefabsList.SetActive(true);
			ToggleObjectPrefabsList(false);
		}
	}

	public class ResourceElement {
		public ResourceManager.Resource resource;
		public GameObject obj;

		public InputField desiredAmountInput;
		public Text desiredAmountText;

		public ResourceElement(ResourceManager.Resource resource, Transform parent) {
			this.resource = resource;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceListResourceElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resource.name;

			obj.transform.Find("Image").GetComponent<Image>().sprite = resource.image;

			desiredAmountInput = obj.transform.Find("DesiredAmount-Input").GetComponent<InputField>();
			desiredAmountText = desiredAmountInput.transform.Find("Text").GetComponent<Text>();
			desiredAmountInput.onEndEdit.AddListener(delegate {
				int newDesiredAmount = 0;
				if (int.TryParse(desiredAmountInput.text, out newDesiredAmount)) {
					if (newDesiredAmount >= 0) {
						resource.ChangeDesiredAmount(newDesiredAmount);
						if (newDesiredAmount == 0) {
							desiredAmountInput.text = String.Empty;
						}
					}
				} else {
					resource.ChangeDesiredAmount(0);
				}
			});

			Update();
		}

		private int availableAmountPrev = 0;
		public void Update() {

			obj.SetActive(resource.GetWorldTotalAmount() > 0);

			if (resource.GetWorldTotalAmount() != resource.GetAvailableAmount()) {
				obj.transform.Find("Amount").GetComponent<Text>().text = resource.GetAvailableAmount() + " / " + resource.GetWorldTotalAmount();
			} else {
				obj.transform.Find("Amount").GetComponent<Text>().text = resource.GetWorldTotalAmount().ToString();
			}
			if (resource.desiredAmount > 0) {
				if (resource.desiredAmount > resource.GetAvailableAmount()) {
					desiredAmountText.color = GetColour(Colours.LightRed);
				} else if (resource.desiredAmount <= resource.GetAvailableAmount()) {
					desiredAmountText.color = GetColour(Colours.LightGreen);
				}
			}

			availableAmountPrev = resource.GetAvailableAmount();

			if (availableAmountPrev != resource.GetAvailableAmount()) {
				if (resource.GetAvailableAmount() > 0) {
					obj.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = GetColour(Colours.LightGrey220);
				} else {
					obj.GetComponent<Image>().color = GetColour(Colours.Grey150);
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = GetColour(Colours.Grey150);
				}
			}
		}
	}

	public List<ResourceElement> clothingElements = new List<ResourceElement>();

	public void CreateClothesList() {
		foreach (ResourceElement clothingElement in clothingElements) {
			MonoBehaviour.Destroy(clothingElement.obj);
		}
		clothingElements.Clear();
		Transform clothesListParent = clothesList.transform.Find("ClothesList-Panel");
		foreach (ResourceManager.Clothing clothing in GameManager.resourceM.resources.Where(r => r.resourceClasses.Contains(ResourceManager.ResourceClass.Clothing)).Select(r => (ResourceManager.Clothing)r)) {
			ResourceElement newClothingElement = new ResourceElement(clothing, clothesListParent);
			newClothingElement.resource.resourceListElement = newClothingElement;
			clothingElements.Add(newClothingElement);
		}
	}

	public void SetClothesList() {
		DisableAdminPanels(clothesMenuPanel);
		clothesMenuPanel.SetActive(!clothesMenuPanel.activeSelf);
		if (clothingElements.Count <= 0) {
			clothesMenuPanel.SetActive(false);
		}
	}

	public void FilterClothesList(string searchString) {
		foreach (ResourceElement clothingElement in clothingElements) {
			clothingElement.obj.SetActive(string.IsNullOrEmpty(searchString) || clothingElement.resource.name.ToLower().Contains(searchString.ToLower()));
		}
	}

	public void DisableClothesList() {
		clothesMenuPanel.SetActive(false);
	}

	public void UpdateClothesList() {
		foreach (ResourceElement clothingElement in clothingElements) {
			clothingElement.Update();
		}
		int index = 0;
		foreach (ResourceElement clothingElement in clothingElements.OrderByDescending(ce => ce.resource.GetWorldTotalAmount()).ThenBy(ce => ce.resource.name)) {
			clothingElement.obj.transform.SetSiblingIndex(index);
			index += 1;
		}
	}

	public List<ResourceElement> resourceElements = new List<ResourceElement>();

	public void CreateResourcesList() {
		foreach (ResourceElement resourceElement in resourceElements) {
			MonoBehaviour.Destroy(resourceElement.obj);
		}
		resourceElements.Clear();
		Transform resourcesListParent = resourcesList.transform.Find("ResourcesList-Panel");
		foreach (ResourceManager.Resource resource in GameManager.resourceM.resources.Where(r => !r.resourceClasses.Contains(ResourceManager.ResourceClass.Clothing))) {
			ResourceElement newResourceElement = new ResourceElement(resource, resourcesListParent);
			newResourceElement.resource.resourceListElement = newResourceElement;
			resourceElements.Add(newResourceElement);
		}
	}

	public void SetResourcesList() {
		DisableAdminPanels(resourcesMenuPanel);
		resourcesMenuPanel.SetActive(!resourcesMenuPanel.activeSelf);
		if (resourceElements.Count <= 0) {
			resourcesMenuPanel.SetActive(false);
		}
	}

	public void FilterResourcesList(string searchString) {
		foreach (ResourceElement resourceElement in resourceElements) {
			resourceElement.obj.SetActive(string.IsNullOrEmpty(searchString) || resourceElement.resource.name.ToLower().Contains(searchString.ToLower()));
		}
	}

	public void DisableResourcesList() {
		resourcesMenuPanel.SetActive(false);
	}

	public void UpdateResourcesList() {
		foreach (ResourceElement resourceElement in resourceElements) {
			resourceElement.Update();
		}
		int index = 0;
		foreach (ResourceElement resourceElement in resourceElements.OrderByDescending(re => re.resource.GetWorldTotalAmount()).ThenBy(re => re.resource.name)) {
			resourceElement.obj.transform.SetSiblingIndex(index);
			index += 1;
		}
	}

	public void InitializeSelectedManufacturingTileObjectIndicator() {
		selectedMTOIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
		SpriteRenderer sCISR = selectedMTOIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = GameManager.resourceM.selectionCornersSprite;
		sCISR.name = "SelectedMTOIndicator";
		sCISR.sortingOrder = 20; // Selected MTO Indicator Sprite
		sCISR.color = new Color(1f, 1f, 1f, 0.75f);
		selectedMTOIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
		selectedMTOIndicator.SetActive(false);
	}

	public class MTOPanel {

		public GameObject obj;
		private bool fuel;

		private Dictionary<GameObject, ResourceManager.Resource> selectResourceListElements = new Dictionary<GameObject, ResourceManager.Resource>();
		private Dictionary<GameObject, ResourceManager.Resource> selectFuelResourceListElements = new Dictionary<GameObject, ResourceManager.Resource>();

		private GameObject selectResourcePanel = null;
		private GameObject selectResourceList = null;

		private GameObject selectFuelResourcePanel = null;
		private GameObject selectFuelResourceList = null;

		private GameObject activeValueButton = null; // Independent of fuel/no-fuel panel
		private GameObject activeValueText = null; // Independent of fuel/no-fuel panel

		public MTOPanel(GameObject obj, bool fuel) {
			this.obj = obj;
			this.fuel = fuel;

			Initialize();

			obj.SetActive(false);
		}

		private void Initialize() {
			selectResourcePanel = obj.transform.Find("SelectResource-Panel").gameObject;
			selectResourceList = selectResourcePanel.transform.Find("SelectResource-ScrollPanel/SelectResourceList-Panel").gameObject;

			obj.transform.Find("SelectResource-Button").GetComponent<Button>().onClick.AddListener(delegate {
				selectResourcePanel.SetActive(!selectResourcePanel.activeSelf);
				if (fuel) {
					selectFuelResourcePanel.SetActive(false);
				}
			});

			selectResourcePanel.SetActive(false);

			if (fuel) {
				selectFuelResourcePanel = obj.transform.Find("SelectFuelResource-Panel").gameObject;
				selectFuelResourceList = selectFuelResourcePanel.transform.Find("SelectFuelResource-ScrollPanel/SelectFuelResourceList-Panel").gameObject;

				foreach (ResourceManager.Resource fuelResource in ResourceManager.GetResourcesInClass(ResourceManager.ResourceClass.Fuel)) {
					GameObject selectFuelResourceButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SelectFuelResource-Panel"), selectFuelResourceList.transform, false);
					selectFuelResourceButton.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = fuelResource.image;
					selectFuelResourceButton.transform.Find("ResourceName-Text").GetComponent<Text>().text = fuelResource.name;
					selectFuelResourceButton.transform.Find("EnergyValue-Text").GetComponent<Text>().text = fuelResource.fuelEnergy.ToString();

					selectFuelResourceListElements.Add(selectFuelResourceButton, fuelResource);
				}

				obj.transform.Find("SelectFuelResource-Button").GetComponent<Button>().onClick.AddListener(delegate {
					selectFuelResourcePanel.SetActive(!selectFuelResourcePanel.activeSelf);
					selectResourcePanel.SetActive(false);
				});

				selectFuelResourcePanel.SetActive(false);
			} else {
				selectFuelResourcePanel = null;
				selectFuelResourceList = null;
				selectFuelResourceListElements.Clear();
			}
		}

		public void Select(ResourceManager.ManufacturingTileObject selectedMTO, GameObject selectedMTOIndicator) {
			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectResourceListElementKVP in selectResourceListElements) {
				MonoBehaviour.Destroy(selectResourceListElementKVP.Key);
			}
			selectResourceListElements.Clear();

			obj.SetActive(true);

			selectedMTOIndicator.SetActive(true);
			selectedMTOIndicator.transform.position = selectedMTO.obj.transform.position;

			obj.transform.Find("SelectedManufacturingTileObjectName-Text").GetComponent<Text>().text = selectedMTO.prefab.name;

			obj.transform.Find("SelectedManufacturingTileObjectSprite-Panel/SelectedManufacturingTileObjectSprite-Image").GetComponent<Image>().sprite = selectedMTO.obj.GetComponent<SpriteRenderer>().sprite;

			foreach (ResourceManager.Resource manufacturableResource in ResourceManager.GetResourcesInClass(ResourceManager.ResourceClass.Manufacturable)) {
				if (manufacturableResource.requiredMTOs.Contains(selectedMTO.prefab) || (manufacturableResource.requiredMTOs.Count <= 0 && manufacturableResource.requiredMTOSubGroups.Contains(selectedMTO.prefab.tileObjectPrefabSubGroup))) {
					GameObject selectResourceButton = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SelectManufacturedResource-Panel"), selectResourceList.transform, false);
					selectResourceButton.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = manufacturableResource.image;
					selectResourceButton.transform.Find("ResourceName-Text").GetComponent<Text>().text = manufacturableResource.name;
					//selectResourceButton.transform.Find("ResourceManufactureTileObjectSubGroupName-Text").GetComponent<Text>().text = manufacturableResource.manufacturingTileObjectSubGroup.name;
					selectResourceButton.transform.Find("RequiredEnergy-Text").GetComponent<Text>().text = manufacturableResource.requiredEnergy.ToString();

					selectResourceListElements.Add(selectResourceButton, manufacturableResource);
				}
			}

			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectResourceButtonKVP in selectResourceListElements) {
				selectResourceButtonKVP.Key.GetComponent<Button>().onClick.AddListener(delegate {
					SetSelectedMTOCreateResource(selectResourceButtonKVP.Value, selectedMTO);
				});
			}

			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
				selectFuelResourceButtonKVP.Key.GetComponent<Button>().onClick.AddListener(delegate {
					SetSelectedMTOFuelResource(selectFuelResourceButtonKVP.Value, selectedMTO);
				});
			}

			activeValueButton = obj.transform.Find("ActiveValueToggle-Button").gameObject;
			activeValueText = activeValueButton.transform.Find("ActiveValue-Text").gameObject;

			activeValueButton.GetComponent<Button>().onClick.AddListener(delegate {
				selectedMTO.active = !selectedMTO.active;
			});

			SetSelectedMTOCreateResource(selectedMTO.createResource, selectedMTO);
			if (selectedMTO.createResource != null) {
				selectedMTO.createResource.UpdateDesiredAmountText();
			}

			if (fuel) {
				SetSelectedMTOFuelResource(selectedMTO.fuelResource, selectedMTO);
			}
		}

		private void SetSelectedMTOCreateResource(ResourceManager.Resource newCreateResource, ResourceManager.ManufacturingTileObject selectedMTO) {
			selectedMTO.createResource = newCreateResource;

			selectResourcePanel.SetActive(false);
			SetSelectedMTOResourceRequiredResources(selectedMTO);
			if (selectedMTO.createResource != null) {
				selectedMTO.createResource.UpdateDesiredAmountText();
				obj.transform.Find("SelectResource-Button/SelectedResourceImage-Image").GetComponent<Image>().sprite = selectedMTO.createResource.image;
				obj.transform.Find("CreateResourceAmount-Text").GetComponent<Text>().text = "Creates " + selectedMTO.createResource.amountCreated + " " + selectedMTO.createResource.name;
			} else {
				obj.transform.Find("SelectResource-Button/SelectedResourceImage-Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(@"UI/NoSelectedResourceImage");
				obj.transform.Find("CreateResourceAmount-Text").GetComponent<Text>().text = "No Resource Selected";
			}
		}

		private Dictionary<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResources = new Dictionary<GameObject, ResourceManager.ResourceAmount>(); // Independent of fuel/no-fuel panel

		private void SetSelectedMTOResourceRequiredResources(ResourceManager.ManufacturingTileObject selectedMTO) {
			foreach (KeyValuePair<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResourceKVP in selectedMTOResourceRequiredResources) {
				MonoBehaviour.Destroy(selectedMTOResourceRequiredResourceKVP.Key);
			}
			selectedMTOResourceRequiredResources.Clear();

			if (selectedMTO.createResource != null) {
				Transform requiredResourcesList = obj.transform.Find("RequiredResources-Panel/RequiredResources-ScrollPanel/RequiredResourcesList-Panel");
				foreach (ResourceManager.ResourceAmount requiredResource in selectedMTO.createResource.requiredResources) {
					GameObject requiredResourcePanel = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesList, false);

					requiredResourcePanel.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = requiredResource.resource.image;
					requiredResourcePanel.transform.Find("ResourceName-Text").GetComponent<Text>().text = requiredResource.resource.name;
					requiredResourcePanel.transform.Find("RequiredAmount-Text").GetComponent<Text>().text = "Need " + requiredResource.amount.ToString();

					selectedMTOResourceRequiredResources.Add(requiredResourcePanel, requiredResource);
				}

				InputField desiredAmountInput = obj.transform.Find("ResourceTargetAmount-Panel/TargetAmount-Input").GetComponent<InputField>();
				desiredAmountInput.onEndEdit.AddListener(delegate {
					int newDesiredAmount = 0;
					if (int.TryParse(desiredAmountInput.text, out newDesiredAmount)) {
						if (newDesiredAmount >= 0) {
							selectedMTO.createResource.ChangeDesiredAmount(newDesiredAmount);
							if (newDesiredAmount == 0) {
								desiredAmountInput.text = String.Empty;
							}
						}
					} else {
						selectedMTO.createResource.ChangeDesiredAmount(0);
					}
				});
			}
		}

		private void SetSelectedMTOFuelResource(ResourceManager.Resource newFuelResource, ResourceManager.ManufacturingTileObject selectedMTO) {
			selectedMTO.fuelResource = newFuelResource;

			selectFuelResourcePanel.SetActive(false);
			if (selectedMTO.fuelResource != null) {
				obj.transform.Find("SelectFuelResource-Button/SelectedFuelResourceImage-Image").GetComponent<Image>().sprite = selectedMTO.fuelResource.image;
			} else {
				obj.transform.Find("SelectFuelResource-Button/SelectedFuelResourceImage-Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(@"UI/NoSelectedResourceImage");
			}
		}

		public void Update(ResourceManager.ManufacturingTileObject selectedMTO) {
			if (selectedMTO.createResource != null) {
				obj.transform.Find("SelectResource-Button/SelectedResourceName-Text").GetComponent<Text>().text = selectedMTO.createResource.name;

				foreach (KeyValuePair<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResourceKVP in selectedMTOResourceRequiredResources) {
					selectedMTOResourceRequiredResourceKVP.Key.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + selectedMTOResourceRequiredResourceKVP.Value.resource.GetAvailableAmount();
					if (selectedMTOResourceRequiredResourceKVP.Value.resource.GetAvailableAmount() < selectedMTOResourceRequiredResourceKVP.Value.amount) {
						selectedMTOResourceRequiredResourceKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightRed);
					} else {
						selectedMTOResourceRequiredResourceKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightGreen);
					}
				}

				obj.transform.Find("ResourceTargetAmount-Panel/CurrentAmountValue-Text").GetComponent<Text>().text = selectedMTO.createResource.GetAvailableAmount().ToString();

			} else {
				obj.transform.Find("SelectResource-Button/SelectedResourceName-Text").GetComponent<Text>().text = "Select Resource";

				obj.transform.Find("ResourceTargetAmount-Panel/CurrentAmountValue-Text").GetComponent<Text>().text = String.Empty;
				obj.transform.Find("ResourceTargetAmount-Panel/TargetAmount-Input").GetComponent<InputField>().text = String.Empty;
			}
			if (fuel) {
				if (selectedMTO.fuelResource != null) {
					obj.transform.Find("SelectFuelResource-Button/SelectedFuelResourceName-Text").GetComponent<Text>().text = selectedMTO.fuelResource.name;
				} else {
					obj.transform.Find("SelectFuelResource-Button/SelectedFuelResourceName-Text").GetComponent<Text>().text = "Select Fuel";
				}
				if (selectFuelResourcePanel.activeSelf) {
					foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
						selectFuelResourceButtonKVP.Key.transform.Find("AmountAvailableValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.GetAvailableAmount().ToString();
					}
					if (selectedMTO.createResource != null) {
						foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
							GameObject selectFuelResourceButton = selectFuelResourceButtonKVP.Key;
							ResourceManager.Resource fuelResource = selectFuelResourceButtonKVP.Value;
							float energyRatio = (float)Math.Round((selectedMTO.createResource.requiredEnergy) / ((float)fuelResource.fuelEnergy), 2);
							selectFuelResourceButtonKVP.Key.transform.Find("EnergyValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.fuelEnergy + " (" + energyRatio + " : 1)";
							if (Mathf.CeilToInt(energyRatio) > fuelResource.GetAvailableAmount()) {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightRed);
							} else {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightGreen);
							}
						}
					} else {
						foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
							selectFuelResourceButtonKVP.Key.transform.Find("EnergyValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.fuelEnergy.ToString();
							if (selectFuelResourceButtonKVP.Value.GetAvailableAmount() <= 0) {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightRed);
							} else {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = GetColour(Colours.LightGreen);
							}
						}
					}
				}
			}

			if (selectedMTO.active) {
				activeValueText.GetComponent<Text>().text = "Active";
				if (selectedMTO.canActivate) {
					activeValueButton.GetComponent<Image>().color = GetColour(Colours.LightGreen);
				} else {
					activeValueButton.GetComponent<Image>().color = GetColour(Colours.LightOrange);
				}
			} else {
				activeValueText.GetComponent<Text>().text = "Inactive";
				if (selectedMTO.canActivate) {
					activeValueButton.GetComponent<Image>().color = GetColour(Colours.LightOrange);
				} else {
					activeValueButton.GetComponent<Image>().color = GetColour(Colours.LightRed);
				}
			}

			obj.transform.Find("SelectResource-Button").GetComponent<Image>().color = (selectedMTO.createResource != null ? (selectedMTO.hasEnoughRequiredResources ? GetColour(Colours.LightGreen) : GetColour(Colours.LightRed)) : GetColour(Colours.LightGrey220));
			if (fuel) {
				obj.transform.Find("SelectFuelResource-Button").GetComponent<Image>().color = (selectedMTO.fuelResource != null ? (selectedMTO.hasEnoughFuel ? GetColour(Colours.LightGreen) : GetColour(Colours.LightRed)) : GetColour(Colours.LightGrey220));
			}
		}

		public void Deselect(GameObject selectedMTOIndicator) {
			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectResourceButtonKVP in selectResourceListElements) {
				selectResourceButtonKVP.Key.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			foreach (KeyValuePair<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResourceKVP in selectedMTOResourceRequiredResources) {
				MonoBehaviour.Destroy(selectedMTOResourceRequiredResourceKVP.Key);
			}
			selectedMTOResourceRequiredResources.Clear();
			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
				selectFuelResourceButtonKVP.Key.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			activeValueButton.GetComponent<Button>().onClick.RemoveAllListeners();
			obj.transform.Find("ResourceTargetAmount-Panel/TargetAmount-Input").GetComponent<InputField>().onEndEdit.RemoveAllListeners();
			selectedMTOIndicator.SetActive(false);
			obj.SetActive(false);
		}
	}

	public ResourceManager.ManufacturingTileObject selectedMTO;
	public void SetSelectedManufacturingTileObject(ResourceManager.ManufacturingTileObject newSelectedMTO) {

		selectedMTO = null;

		if (selectedContainer != null) {
			SetSelectedContainer(null);
		}

		selectedMTO = newSelectedMTO;

		if (selectedMTOPanel != null) {
			selectedMTOPanel.Deselect(selectedMTOIndicator);
		}

		if (selectedMTO != null) {
			if (ResourceManager.manufacturingTileObjectsFuel.Contains(selectedMTO.prefab.type)) {
				selectedMTOPanel = selectedMTOFuelPanel;
			} else {
				selectedMTOPanel = selectedMTONoFuelPanel;
			}
			selectedMTOPanel.Select(selectedMTO, selectedMTOIndicator);
		}
	}

	public void SetPauseMenuActive(bool active) {
		//SetLoadMenuActive(false, false);
		SetSettingsMenuActive(false);

		pauseMenu.SetActive(active);

		GameManager.timeM.SetPaused(pauseMenu.activeSelf);
	}

	public void TogglePauseMenuButtons(bool state) {
		pauseMenuButtons.SetActive(state);
		pauseLabel.SetActive(pauseMenuButtons.activeSelf);
	}

	//public void SetLoadMenuActive(bool active, bool fromMainMenu) {
	//	loadGamePanel.SetActive(active);
	//}

	public void GetMainMenuContinueFile() {

	}

	//private string saveFileName;
	//public void ToggleSaveMenu() {
	//	pauseSavePanel.SetActive(!pauseSavePanel.activeSelf);
	//	TogglePauseMenuButtons(!pauseSavePanel.activeSelf);
	//	if (pauseSavePanel.activeSelf) {
	//		saveFileName = persistenceM.GenerateSaveFileName();
	//		pauseSavePanel.transform.Find("SaveFileName-Text").GetComponent<Text>().text = saveFileName;
	//		pauseSavePanel.transform.Find("PauseSavePanelSave-Button").GetComponent<Button>().onClick.RemoveAllListeners();
	//		pauseSavePanel.transform.Find("PauseSavePanelSave-Button").GetComponent<Button>().onClick.AddListener(delegate {
	//			persistenceM.SaveGame(saveFileName);
	//			ToggleSaveMenu();
	//		});
	//	} else {
	//		saveFileName = string.Empty;
	//	}
	//}

	//public List<LoadFile> loadFiles = new List<LoadFile>();

	//public class LoadFile {

	//	public string fileName;
	//	public GameObject loadFilePanel;

	//	public LoadFile(string fileName, Transform loadFilePanelParent, bool fromMainMenu, UIManager uiM) {
	//		this.fileName = fileName;

	//		string colonyName = fileName.Split('-')[2];

	//		string rawSaveDT = fileName.Split('-')[3];
	//		List<string> splitRawSaveDT = new Regex(@"[a-zA-Z]").Split(rawSaveDT).ToList();
	//		string year = splitRawSaveDT[0];
	//		string month = (splitRawSaveDT[1].Length == 1 ? "0" : "") + splitRawSaveDT[1];
	//		string day = (splitRawSaveDT[2].Length == 1 ? "0" : "") + splitRawSaveDT[2];
	//		string saveDate = year + "-" + month + "-" + day;
	//		string hour = (splitRawSaveDT[3].Length == 1 ? "0" : "") + splitRawSaveDT[3];
	//		string minute = (splitRawSaveDT[4].Length == 1 ? "0" : "") + splitRawSaveDT[4];
	//		string second = (splitRawSaveDT[5].Length == 1 ? "0" : "") + splitRawSaveDT[5];
	//		string saveTime = hour + ":" + minute + ":" + second;

	//		loadFilePanel = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/LoadFile-Panel"), loadFilePanelParent, false);
	//		loadFilePanel.transform.Find("ColonyName-Text").GetComponent<Text>().text = colonyName;
	//		loadFilePanel.transform.Find("SaveDate-Text").GetComponent<Text>().text = saveDate;
	//		loadFilePanel.transform.Find("SaveTime-Text").GetComponent<Text>().text = saveTime;

	//		bool compatible = false;
	//		int gameVersion = -1;
	//		int saveVersion = -1;
	//		if (fileName.Split('-').Length == 6) {
	//			gameVersion = int.Parse(fileName.Split('-')[4]);
	//			saveVersion = int.Parse(fileName.Split('-')[5].Split('.')[0]);
	//			if (saveVersion == PersistenceManager.saveVersion) {
	//				compatible = true;
	//			}
	//		}

	//		loadFilePanel.transform.Find("GameVersionValue-Text").GetComponent<Text>().text = uiM.persistenceM.GetGameVersionString(gameVersion);
	//		loadFilePanel.transform.Find("SaveFileVersionValue-Text").GetComponent<Text>().text = (saveVersion == -1 ? "Unsupported" : saveVersion.ToString());

	//		string imageFile = "file://" + fileName.Split('.')[0] + ".png";
	//		WWW www = new WWW(imageFile);
	//		if (string.IsNullOrEmpty(www.error)) {
	//			uiM.StartCoroutine(LoadSaveFileImage(www));
	//		}

	//		loadFilePanel.GetComponent<Button>().interactable = compatible;
	//		if (compatible) {
	//			loadFilePanel.GetComponent<Button>().onClick.AddListener(delegate { uiM.SetSelectedLoadFile(this, fromMainMenu); });
	//		}
	//	}

	//	IEnumerator LoadSaveFileImage(WWW www) {
	//		while (!www.isDone) {
	//			yield return null;
	//		}
	//		if (string.IsNullOrEmpty(www.error)) {
	//			Texture2D texture = new Texture2D(35, 63, TextureFormat.RGB24, false);
	//			www.LoadImageIntoTexture(texture);
	//			if (loadFilePanel != null && loadFilePanel.transform.Find("SavePreview-Image") != null) {
	//				loadFilePanel.transform.Find("SavePreview-Image").GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(new Vector2(0, 0), new Vector2(texture.width, texture.height)), new Vector2(0, 0));
	//			}
	//		}
	//	}
	//}

	//private LoadFile selectedLoadFile;

	//public void SetSelectedLoadFile(LoadFile newSelectedLoadFile, bool fromMainMenu) {
	//	if (selectedLoadFile != null) {
	//		selectedLoadFile.loadFilePanel.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
	//	}
	//	selectedLoadFile = newSelectedLoadFile;
	//	if (selectedLoadFile != null) {
	//		selectedLoadFile.loadFilePanel.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
	//		loadGamePanel.transform.Find("LoadGamePanelLoad-Button").GetComponent<Button>().onClick.RemoveAllListeners();
	//		loadGamePanel.transform.Find("LoadGamePanelLoad-Button").GetComponent<Button>().onClick.AddListener(delegate {
	//			persistenceM.LoadGame(selectedLoadFile.fileName, fromMainMenu);
	//		});
	//	}
	//}

	//private LoadFile continueLoadFile = null;
	//public void GetMainMenuContinueFile() {
	//	if (continueLoadFile != null) {
	//		MonoBehaviour.Destroy(continueLoadFile.loadFilePanel);
	//		continueLoadFile = null;
	//	}
	//	List<string> saveFiles = new List<string>();
	//	try {
	//		saveFiles = Directory.GetFiles(persistenceM.GenerateSavePath("")).ToList().OrderBy(fileName => SaveFileDateTimeSum(fileName)).Reverse().ToList();
	//	} catch (DirectoryNotFoundException) {
	//		return;
	//	}
	//	if (saveFiles.Count > 0) {
	//		continueLoadFile = new LoadFile(saveFiles[0], mainMenu.transform.Find("MainMenuButtons-Panel/Continue-Button/LoadFilePanelParent-Panel"), true, this);
	//		continueLoadFile.loadFilePanel.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
	//		MonoBehaviour.Destroy(continueLoadFile.loadFilePanel.GetComponent<Button>());
	//	} else {
	//		MonoBehaviour.Destroy(continueLoadFile.loadFilePanel);
	//	}
	//}

	//public void ToggleMainMenuContinue() {
	//	if (continueLoadFile != null) {
	//		persistenceM.LoadGame(continueLoadFile.fileName, true);
	//	}
	//}

	//public void SetLoadMenuActive(bool active, bool fromMainMenu) {
	//	if (active) {
	//		if (!loadGamePanel.activeSelf) {
	//			ToggleLoadMenu(fromMainMenu);
	//		}
	//	} else {
	//		if (loadGamePanel.activeSelf) {
	//			ToggleLoadMenu(fromMainMenu);
	//		}
	//	}
	//}

	//public void ToggleLoadMenu(bool fromMainMenu) {
	//	loadGamePanel.SetActive(!loadGamePanel.activeSelf);
	//	ToggleMainMenuButtons(loadGamePanel);
	//	TogglePauseMenuButtons(!loadGamePanel.activeSelf);
	//	foreach (LoadFile loadFile in loadFiles) {
	//		MonoBehaviour.Destroy(loadFile.loadFilePanel);
	//	}
	//	loadFiles.Clear();
	//	if (loadGamePanel.activeSelf) {
	//		List<string> saveFiles;
	//		try {
	//			saveFiles = Directory.GetFiles(persistenceM.GenerateSavePath("")).ToList().OrderBy(fileName => SaveFileDateTimeSum(fileName)).Reverse().ToList();
	//		} catch (DirectoryNotFoundException) {
	//			return;
	//		}
	//		foreach (string fileName in saveFiles) {
	//			if (fileName.Split('.')[1] == "snowship") {
	//				LoadFile loadFile = new LoadFile(fileName, loadGamePanel.transform.Find("LoadFilesList-ScrollPanel/LoadFilesList-Panel"), fromMainMenu, this);
	//				loadFiles.Add(loadFile);
	//			}
	//		}
	//	} else {
	//		SetSelectedLoadFile(null, false);
	//	}
	//}

	//public string SaveFileDateTimeSum(string fileName) {
	//	List<string> splitRawSaveDT = new Regex(@"[a-zA-Z]").Split(fileName.Split('-')[3]).ToList();
	//	string year = splitRawSaveDT[0];
	//	string month = (splitRawSaveDT[1].Length == 1 ? "0" : "") + splitRawSaveDT[1];
	//	string day = (splitRawSaveDT[2].Length == 1 ? "0" : "") + splitRawSaveDT[2];
	//	string hour = (splitRawSaveDT[3].Length == 1 ? "0" : "") + splitRawSaveDT[3];
	//	string minute = (splitRawSaveDT[4].Length == 1 ? "0" : "") + splitRawSaveDT[4];
	//	string second = (splitRawSaveDT[5].Length == 1 ? "0" : "") + splitRawSaveDT[5];
	//	string saveDT = year + month + day + hour + minute + second;
	//	return saveDT;
	//}

	public enum UIScaleMode {
		ConstantPixelSize,
		ScaleWithScreenSize
	};

	public void SetSettingsMenuActive(bool active) {
		settingsPanel.SetActive(active);

		ToggleMainMenuButtons(settingsPanel);
		TogglePauseMenuButtons(!settingsPanel.activeSelf);
		if (settingsPanel.activeSelf) {
			GameObject resolutionSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/ResolutionSettings-Panel").gameObject;
			Slider resolutionSlider = resolutionSettingsPanel.transform.Find("Resolution-Slider").GetComponent<Slider>();
			resolutionSlider.minValue = 0;
			resolutionSlider.maxValue = Screen.resolutions.Length - 1;
			resolutionSlider.onValueChanged.AddListener(delegate {
				Resolution r = Screen.resolutions[Mathf.RoundToInt(resolutionSlider.value)];
				GameManager.persistenceM.settingsState.resolution = r;
				GameManager.persistenceM.settingsState.resolutionWidth = r.width;
				GameManager.persistenceM.settingsState.resolutionHeight = r.height;
				GameManager.persistenceM.settingsState.refreshRate = r.refreshRate;
				resolutionSettingsPanel.transform.Find("ResolutionValue-Text").GetComponent<Text>().text = GameManager.persistenceM.settingsState.resolutionWidth + " × " + GameManager.persistenceM.settingsState.resolutionHeight + " @ " + r.refreshRate + "hz";
			});
			resolutionSlider.value = Screen.resolutions.ToList().IndexOf(GameManager.persistenceM.settingsState.resolution);

			GameObject fullscreenSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/FullscreenSettings-Panel").gameObject;
			Toggle fullscreenToggle = fullscreenSettingsPanel.transform.Find("Fullscreen-Toggle").GetComponent<Toggle>();
			fullscreenToggle.onValueChanged.AddListener(delegate { GameManager.persistenceM.settingsState.fullscreen = fullscreenToggle.isOn; });
			fullscreenToggle.isOn = GameManager.persistenceM.settingsState.fullscreen;

			GameObject UIScaleModeSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/UIScaleModeSettings-Panel").gameObject;
			Toggle UIScaleModeToggle = UIScaleModeSettingsPanel.transform.Find("UIScaleMode-Toggle").GetComponent<Toggle>();
			UIScaleModeToggle.onValueChanged.AddListener(delegate {
				if (UIScaleModeToggle.isOn) {
					GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				} else {
					GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
				}
			});
			if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
				UIScaleModeToggle.isOn = true;
			} else if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {
				UIScaleModeToggle.isOn = false;
			}
		}
	}

	public Resolution GetResolutionByDimensions(int width, int height, int refreshRate) {
		List<Resolution> resolutions = Screen.resolutions.ToList();
		return resolutions.Find(resolution => resolution.width == width && resolution.height == height && resolution.refreshRate == refreshRate);
	}

	public void ExitToMenu() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public bool IsPointerOverUI() {
		return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
	}
}