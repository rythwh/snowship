using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

public class UIManager : MonoBehaviour {

	public string SplitByCapitals(string combinedString) {
		var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
				 RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString, " ");
	}

	public Color HexToColor(string hexString) {
		int r = int.Parse("" + hexString[2] + hexString[3], System.Globalization.NumberStyles.HexNumber);
		int g = int.Parse("" + hexString[4] + hexString[5], System.Globalization.NumberStyles.HexNumber);
		int b = int.Parse("" + hexString[6] + hexString[7], System.Globalization.NumberStyles.HexNumber);
		return new Color(r, g, b, 255f) / 255f;
	}

	public string RemoveNonAlphanumericChars(string removeFromString) {
		return new Regex("[^a-zA-Z0-9 -]").Replace(removeFromString, string.Empty);
	}

	public enum Colours { DarkRed, DarkGreen, LightRed, LightGreen, LightGrey220, LightGrey200, Grey150, DarkGrey50, LightBlue, LightOrange, White, DarkYellow, LightYellow };

	private Dictionary<Colours, Color> colourMap = new Dictionary<Colours, Color>() {
		{Colours.DarkRed,new Color(192f, 57f, 43f, 255f) / 255f },
		{Colours.DarkGreen,new Color(39f, 174f, 96f, 255f) / 255f },
		{Colours.LightRed,new Color(231f, 76f, 60f, 255f) / 255f },
		{Colours.LightGreen,new Color(46f, 204f, 113f, 255f) / 255f },
		{Colours.LightGrey220,new Color(220f, 220f, 220f, 255f) / 255f },
		{Colours.LightGrey200,new Color(200f, 200f, 200f, 255f) / 255f },
		{Colours.Grey150,new Color(150f, 150f, 150f, 255f) / 255f },
		{Colours.DarkGrey50,new Color(50f, 50f, 50f, 255f) / 255f },
		{Colours.LightBlue,new Color(52f, 152f, 219f, 255f) / 255f },
		{Colours.LightOrange,new Color(230f, 126f, 34f, 255f) / 255f },
		{Colours.White,new Color(255f, 255f, 255f, 255f) / 255f },
		{Colours.DarkYellow,new Color(216f, 176f, 15f, 255f) / 255f },
		{Colours.LightYellow,new Color(241f, 196f, 15f, 255f) / 255f }
	};

	public Color GetColour(Colours colourKey) {
		return colourMap[colourKey];
	}

	public int screenWidth = 0;
	public int screenHeight = 0;

	private TileManager tileM;
	private JobManager jobM;
	private ResourceManager resourceM;
	private CameraManager cameraM;
	private ColonistManager colonistM;
	private TimeManager timeM;
	private PersistenceManager persistenceM;

	public GameObject canvas;

	public GameObject mainMenu;
	private GameObject mainMenuBackground;
	private GameObject mainMenuButtonsPanel;
	private GameObject snowshipLogo;
	private GameObject darkBackground;

	private GameObject loadGamePanel;

	private GameObject settingsPanel;

	private GameObject mapSelectionPanel;
	private GameObject mapPanelExitButton;

	public GameObject planetPreviewPanel;

	private InputField planetSeedInputField;

	public int planetTileSize = 0;
	private Slider planetSizeSlider;
	private Text planetSizeText;

	public float planetDistance = 0;
	private Slider planetDistanceSlider;
	private Text planetDistanceText;

	public int temperatureRange = 0;
	private Slider temperatureRangeSlider;
	private Text temperatureRangeText;

	public int windDirection = 0;
	private Slider windDirectionSlider;
	private Text windDirectionText;

	public string colonyName;

	public int mapSize = 0;
	private Slider mapSizeSlider;
	private Text mapSizeText;
	private InputField mapSeedInputField;

	private GameObject playButton;

	private GameObject loadMapCodeButton;

	private Text loadingStateText;
	private Text subLoadingStateText;

	private GameObject gameUI;

	private Vector2 mousePosition;
	public TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	private GameObject colonistListToggleButton;
	private GameObject colonistList;
	private GameObject jobListToggleButton;
	private GameObject jobList;

	private GameObject selectedColonistInformationPanel;
	private GameObject selectedColonistInventoryPanel;
	private GameObject selectedColonistHappinessModifiersButton;
	private GameObject selectedColonistHappinessModifiersPanel;

	private GameObject dateTimeInformationPanel;

	private GameObject selectionSizeCanvas;
	private GameObject selectionSizePanel;

	private GameObject selectedContainerIndicator;
	private GameObject selectedContainerInventoryPanel;

	private GameObject professionMenuButton;
	private GameObject professionsList;

	private GameObject objectsMenuButton;
	private GameObject objectPrefabsList;

	private GameObject resourcesMenuButton;
	private GameObject resourcesList;

	/*
	private GameObject cancelButton;

	private GameObject priorityIncreaseButton;
	private GameObject priorityDecreaseButton;
	*/

	private GameObject selectedMTOIndicator;
	private GameObject mtoNoFuelPanelObj;
	private GameObject mtoFuelPanelObj;
	private MTOPanel selectedMTOFuelPanel;
	private MTOPanel selectedMTONoFuelPanel;
	public MTOPanel selectedMTOPanel;

	public GameObject pauseMenu;
	private GameObject pauseMenuButtons;
	private GameObject pauseLabel;

	private GameObject pauseSavePanel;

	public SettingsState settingsState;

	void Awake() {

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		tileM = GetComponent<TileManager>();
		jobM = GetComponent<JobManager>();
		resourceM = GetComponent<ResourceManager>();
		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		timeM = GetComponent<TimeManager>();
		persistenceM = GetComponent<PersistenceManager>();

		canvas = GameObject.Find("Canvas");

		mainMenu = canvas.transform.Find("MainMenu").gameObject;

		string gameVersionString = "Snowship " + persistenceM.GetGameVersionString(PersistenceManager.gameVersion);
		string disclaimerText = "<size=14>" + gameVersionString + "</size>\nSnowship by Ryan White - flizzehh.itch.io/snowship\nThis game is a work in progress and subject to major changes.";
		mainMenu.transform.Find("Disclaimer-Text").GetComponent<Text>().text = disclaimerText;

		mainMenuBackground = mainMenu.transform.Find("MainMenuBackground-Image").gameObject;
		snowshipLogo = mainMenu.transform.Find("SnowshipLogo-Image").gameObject;
		SetMainMenuBackground();

		darkBackground = mainMenu.transform.Find("DarkBackground-Image").gameObject;

		mainMenuButtonsPanel = mainMenu.transform.Find("MainMenuButtons-Panel").gameObject;

		loadGamePanel = canvas.transform.Find("LoadGame-Panel").gameObject;
		loadGamePanel.transform.Find("LoadGamePanelClose-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleLoadMenu(false); });

		settingsPanel = canvas.transform.Find("Settings-Panel").gameObject;
		settingsPanel.transform.Find("SettingsCancel-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleSettingsMenu(); });
		settingsPanel.transform.Find("SettingsApply-Button").GetComponent<Button>().onClick.AddListener(delegate { ApplySettings(false); });
		settingsPanel.transform.Find("SettingsAccept-Button").GetComponent<Button>().onClick.AddListener(delegate { ApplySettings(true); });

		mapSelectionPanel = mainMenu.transform.Find("Map-Panel").gameObject;
		mapPanelExitButton = mapSelectionPanel.transform.Find("Exit-Button").gameObject;
		mapPanelExitButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleNewGameMM(); });

		mainMenuButtonsPanel.transform.Find("New-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleNewGameMM(); });

		mainMenuButtonsPanel.transform.Find("Continue-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleMainMenuContinue(); });
		mainMenuButtonsPanel.transform.Find("Continue-Button").GetComponent<HoverToggleScript>().Initialize(mainMenu.transform.Find("MainMenuButtons-Panel/Continue-Button/LoadFilePanelParent-Panel").gameObject);

		mainMenuButtonsPanel.transform.Find("Load-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleLoadMenu(true); });
		mainMenuButtonsPanel.transform.Find("Settings-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleSettingsMenu(); });
		mainMenuButtonsPanel.transform.Find("Exit-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });

		planetPreviewPanel = mapSelectionPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").gameObject;

		planetSeedInputField = mapSelectionPanel.transform.Find("PlanetSettings-Panel/PlanetSeed-Panel/InputField").GetComponent<InputField>();

		planetSizeSlider = mapSelectionPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSize-Slider").GetComponent<Slider>();
		planetSizeText = mapSelectionPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSizeValue-Text").GetComponent<Text>();
		planetSizeSlider.maxValue = planetTileSizes.Count - 1;
		planetSizeSlider.minValue = 0;

		planetDistanceSlider = mapSelectionPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistance-Slider").GetComponent<Slider>();
		planetDistanceText = mapSelectionPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistanceValue-Text").GetComponent<Text>();
		planetDistanceSlider.maxValue = 7;
		planetDistanceSlider.minValue = 1;

		temperatureRangeSlider = mapSelectionPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRange-Slider").GetComponent<Slider>();
		temperatureRangeText = mapSelectionPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRangeValue-Text").GetComponent<Text>();
		temperatureRangeSlider.maxValue = 10;
		temperatureRangeSlider.minValue = 0;

		windDirectionSlider = mapSelectionPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirection-Slider").GetComponent<Slider>();
		windDirectionText = mapSelectionPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirectionValue-Text").GetComponent<Text>();
		windDirectionSlider.maxValue = 7;
		windDirectionSlider.minValue = 0;

		planetSizeSlider.onValueChanged.AddListener(delegate { UpdatePlanetInfo(); });
		planetSizeSlider.value = Mathf.FloorToInt((planetTileSizes.Count - 1) / 2f);

		planetDistanceSlider.onValueChanged.AddListener(delegate { UpdatePlanetInfo(); });
		planetDistanceSlider.value = 4;

		temperatureRangeSlider.onValueChanged.AddListener(delegate { UpdatePlanetInfo(); });
		temperatureRangeSlider.value = 5;

		windDirectionSlider.onValueChanged.AddListener(delegate { UpdatePlanetInfo(); });
		windDirectionSlider.value = UnityEngine.Random.Range(0, 8); // 0 -> 7 (8 is exclusive)

		mapSelectionPanel.transform.Find("PlanetSettings-Panel/ReloadPlanet-Button").GetComponent<Button>().onClick.AddListener(delegate { GeneratePlanet(); });

		mapSizeText = mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/MapSize-Panel/MapSizeValue-Text").GetComponent<Text>();
		mapSizeSlider = mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/MapSize-Panel/MapSize-Slider").GetComponent<Slider>();
		mapSizeSlider.onValueChanged.AddListener(delegate { UpdateMapSizeText(); });
		mapSizeSlider.value = 2;

		mapSeedInputField = mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/MapSeed-Panel/MapSeed-InputField").GetComponent<InputField>();

		playButton = mapSelectionPanel.transform.Find("Play-Button").gameObject;
		playButton.GetComponent<Button>().onClick.AddListener(delegate { PlayButton(); });

		loadMapCodeButton = mapSelectionPanel.transform.Find("MapRegenerationCode-Panel/LoadMapCode-Button").gameObject;
		loadMapCodeButton.GetComponent<Button>().onClick.AddListener(delegate { ParseMapRegenerationCode(mapSelectionPanel.transform.Find("MapRegenerationCode-Panel/InputField").GetComponent<InputField>().text); });

		loadingStateText = canvas.transform.Find("LoadingScreen/LoadingState-Text").GetComponent<Text>();
		subLoadingStateText = canvas.transform.Find("LoadingScreen/SubLoadingState-Text").GetComponent<Text>();
		ToggleLoadingScreen(false);

		gameUI = canvas.transform.Find("Game-BackgroundPanel").gameObject;

		gameUI.transform.Find("Disclaimer-Text").GetComponent<Text>().text = gameVersionString;

		tileInformation = gameUI.transform.Find("TileInformation-Panel").gameObject;

		colonistListToggleButton = gameUI.transform.Find("ColonistListToggle-Button").gameObject;
		colonistList = gameUI.transform.Find("RightList-Panel/ColonistList-ScrollPanel").gameObject;
		colonistListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { colonistList.SetActive(!colonistList.activeSelf); });
		jobListToggleButton = gameUI.transform.Find("JobListToggle-Button").gameObject;
		jobList = gameUI.transform.Find("RightList-Panel/JobList-ScrollPanel").gameObject;
		jobListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { jobList.SetActive(!jobList.activeSelf); });

		selectedColonistInformationPanel = gameUI.transform.Find("SelectedColonistInfo-Panel").gameObject;

		selectedColonistInventoryPanel = selectedColonistInformationPanel.transform.Find("ColonistInventory-Panel").gameObject;

		selectedColonistHappinessModifiersPanel = selectedColonistInformationPanel.transform.Find("HappinessModifier-Panel").gameObject;
		selectedColonistHappinessModifiersButton = selectedColonistInformationPanel.transform.Find("Needs-Panel/HappinessModifiers-Button").gameObject;
		selectedColonistHappinessModifiersButton.GetComponent<Button>().onClick.AddListener(delegate { selectedColonistHappinessModifiersPanel.SetActive(!selectedColonistHappinessModifiersPanel.activeSelf); });
		selectedColonistHappinessModifiersPanel.SetActive(false);

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

		resourcesMenuButton = gameUI.transform.Find("AdminMenu-Panel/ResourcesMenu-Button").gameObject;
		resourcesList = resourcesMenuButton.transform.Find("ResourcesList-ScrollPanel").gameObject;
		resourcesMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetResourcesList(); });
		resourcesList.SetActive(false);

		/*
		cancelButton = gameUI.transform.Find("Cancel-Button").gameObject;
		cancelButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Cancel)); });

		priorityIncreaseButton = gameUI.transform.Find("PriorityIncrease-Button").gameObject;
		priorityIncreaseButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.IncreasePriority)); });

		priorityDecreaseButton = gameUI.transform.Find("PriorityDecrease-Button").gameObject;
		priorityDecreaseButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.DecreasePriority)); });
		*/

		mtoNoFuelPanelObj = gameUI.transform.Find("SelectedManufacturingTileObjectNoFuel-Panel").gameObject;
		mtoFuelPanelObj = gameUI.transform.Find("SelectedManufacturingTileObjectFuel-Panel").gameObject;

		pauseMenu = canvas.transform.Find("PauseMenu-BackgroundPanel").gameObject;
		pauseMenuButtons = pauseMenu.transform.Find("ButtonsList-Panel").gameObject;
		pauseLabel = pauseMenu.transform.Find("PausedLabel-Text").gameObject;

		pauseMenuButtons.transform.Find("PauseContinue-Button").GetComponent<Button>().onClick.AddListener(delegate { TogglePauseMenu(); });

		pauseSavePanel = pauseMenu.transform.Find("PauseSave-Panel").gameObject;
		pauseMenuButtons.transform.Find("PauseSave-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleSaveMenu(); });
		pauseSavePanel.transform.Find("PauseSavePanelClose-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleSaveMenu(); });
		ToggleSaveMenu();

		pauseMenuButtons.transform.Find("PauseLoad-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleLoadMenu(false); });

		pauseMenuButtons.transform.Find("PauseSettings-Button").GetComponent<Button>().onClick.AddListener(delegate { ToggleSettingsMenu(); });

		pauseMenuButtons.transform.Find("PauseExitToMainMenu-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToMenu(); });

		pauseMenuButtons.transform.Find("PauseExitToDesktop-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });

		ToggleLoadMenu(false);
		ToggleSettingsMenu();

		TogglePauseMenu();

		ToggleNewGameMM();

		InitializeTileInformation();
		InitializeSelectedContainerIndicator();
		InitializeSelectedManufacturingTileObjectIndicator();

		PreviewMainMenuContinueFile();
	}

	public void InitializeGameUI() {
		SetSelectedColonistInformation();
		SetSelectedContainerInfo();
		SetJobElements();
		InitializeProfessionsList();
		InitializeResourcesList();

		selectedMTOFuelPanel = new MTOPanel(mtoFuelPanelObj, true, resourceM);
		selectedMTONoFuelPanel = new MTOPanel(mtoNoFuelPanelObj, false, resourceM);

		ToggleLoadingScreen(false);
	}

	public ResourceManager.Container selectedContainer;
	void Update() {
		if (tileM.generated) {
			mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile newMouseOverTile = tileM.map.GetTileFromPosition(mousePosition);
			if (newMouseOverTile != mouseOverTile) {
				mouseOverTile = newMouseOverTile;
				if (!pauseMenu.activeSelf) {
					UpdateTileInformation();
				}
			}
			if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				if (jobM.firstTile != null) {
					jobM.StopSelection();
				} else {
					if (jobM.GetSelectedPrefab() != null) {
						jobM.SetSelectedPrefab(null);
					} else if (!Input.GetMouseButtonDown(1)) {
						TogglePauseMenu();
					}
				}
			}
			if (colonistM.selectedColonist != null) {
				UpdateSelectedColonistInformation();
			}
			if (jobElements.Count > 0) {
				UpdateJobElements();
			}
			if (colonistElements.Count > 0) {
				UpdateColonistElements();
			}
			if (selectedContainer != null) {
				if (Input.GetMouseButtonDown(1)) {
					SetSelectedContainer(null);
				}
				UpdateSelectedContainerInfo();
			}
			if (selectedMTO != null) {
				selectedMTOPanel.Update(selectedMTO, this);
				if (Input.GetMouseButtonDown(1)) {
					SetSelectedManufacturingTileObject(null);
				}
			}
			if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == newMouseOverTile || findContainer.parentObject.additionalTiles.Contains(newMouseOverTile));
				if (container != null) {
					SetSelectedManufacturingTileObject(null);
					SetSelectedContainer(container);
				}
				ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(mtoi => mtoi.parentObject.tile == newMouseOverTile || mtoi.parentObject.additionalTiles.Contains(newMouseOverTile));
				if (mto != null) {
					SetSelectedContainer(null);
					SetSelectedManufacturingTileObject(mto);
				}
			}
			if (professionsList.activeSelf) {
				UpdateProfessionsList();
			}
			if (resourcesList.activeSelf) {
				UpdateResourcesList();
			}
			UpdateButtonRequiredResourceItems();
		} else {
			playButton.GetComponent<Button>().interactable = (selectedPlanetTile != null);
			if (Input.GetMouseButtonDown(1) && selectedPlanetTile != null && !tileM.generating) {
				SetSelectedPlanetTile(null);
			}
			UpdateMainMenuBackground();
		}
	}

	private void UpdateButtonRequiredResourceItems() {
		foreach (GameObject buttonRequiredResourceItem in buttonRequiredResourceItems) {
			if (buttonRequiredResourceItem.activeSelf) {
				ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), buttonRequiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text.Replace(" ", string.Empty)));
				buttonRequiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + resource.worldTotalAmount;
				if (int.Parse(buttonRequiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text.Split(' ')[1]) > resource.worldTotalAmount) {
					buttonRequiredResourceItem.GetComponent<Image>().color = colourMap[Colours.LightRed];
				} else {
					buttonRequiredResourceItem.GetComponent<Image>().color = colourMap[Colours.LightGreen];
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
			PreviewMainMenuContinueFile();
		}
		darkBackground.SetActive(!snowshipLogo.activeSelf);
	}

	void ToggleNewGameMM() {
		mapSelectionPanel.SetActive(!mapSelectionPanel.activeSelf);
		ToggleMainMenuButtons(mapSelectionPanel);
		if (mapSelectionPanel.activeSelf && !createdPlanet) {
			CreateNewGamePlanet();
		}
	}

	void ExitToDesktop() {
		Application.Quit();
	}

	private bool createdPlanet = false;
	void CreateNewGamePlanet() {
		createdPlanet = true;

		GeneratePlanet();
		SetSelectedPlanetTileInfo();
	}


	public PlanetTile selectedPlanetTile;

	public void SetSelectedPlanetTile(PlanetTile selectedPlanetTile) {
		if (selectedPlanetTile == null || !tileM.GetWaterEquivalentTileTypes().Contains(selectedPlanetTile.tile.tileType.type)) {
			this.selectedPlanetTile = selectedPlanetTile;
			SetSelectedPlanetTileInfo();
		}
	}

	public List<PlanetTile> planetTiles = new List<PlanetTile>();

	public class PlanetTile {

		private TileManager tileM;
		private UIManager uiM;

		void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public TileManager.Tile tile;
		public GameObject obj;

		public Image image;

		public Vector2 position;

		public TileManager.MapData data;
		private int planetSize;
		private float planetTemperature;

		public float equatorOffset;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileManager.TileTypes, float> terrainTypeHeights;
		public List<int> surroundingPlanetTileHeightDirections = new List<int>();

		public PlanetTile(TileManager.Tile tile, Transform parent, Vector2 position, int planetSize, float planetTemperature) {

			GetScriptReferences();

			this.tile = tile;

			this.position = position;

			this.planetSize = planetSize;
			this.planetTemperature = planetTemperature;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/PlanetTile"), parent, false);
			obj.name = "Planet Tile: " + position;
			image = obj.GetComponent<Image>();
			image.sprite = tile.obj.GetComponent<SpriteRenderer>().sprite;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				uiM.SetSelectedPlanetTile(this);
			});

			SetMapData();

			Destroy(tile.obj);
		}

		public void SetMapData() {
			equatorOffset = ((position.y - (planetSize / 2f)) * 2) / planetSize;
			averageTemperature = tile.temperature + planetTemperature;
			averagePrecipitation = tile.GetPrecipitation();

			if (!tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) {
				foreach (TileManager.Tile nTile in tile.horizontalSurroundingTiles) {
					if (nTile != null) {
						if (tileM.GetWaterEquivalentTileTypes().Contains(nTile.tileType.type)) {
							surroundingPlanetTileHeightDirections.Add(-2);
						} else if (tileM.GetStoneEquivalentTileTypes().Contains(nTile.tileType.type)) {
							surroundingPlanetTileHeightDirections.Add(5);
						} else {
							surroundingPlanetTileHeightDirections.Add(0);
						}
					}
				}
			} else {
				obj.GetComponent<Button>().interactable = false;
			}

			float waterThreshold = 0.40f;
			float stoneThreshold = 0.75f;
			waterThreshold = waterThreshold * tile.GetPrecipitation() * (1 - tile.height);
			stoneThreshold = stoneThreshold * (1 - (tile.height - (1 - stoneThreshold)));

			terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>() {
				{ TileManager.TileTypes.GrassWater,waterThreshold},{ TileManager.TileTypes.Stone,stoneThreshold }
			};
		}
	}

	void SetSelectedPlanetTileInfo() {
		if (selectedPlanetTile != null) {
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileTemperature-Panel/TemperatureValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedPlanetTile.averageTemperature) + "°C";
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTilePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedPlanetTile.averagePrecipitation * 100) + "%";
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>().text = Mathf.RoundToInt((selectedPlanetTile.tile.height - selectedPlanetTile.terrainTypeHeights[TileManager.TileTypes.GrassWater]) * 10000f) + "m";
			mapSelectionPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(selectedPlanetTile.position.x) + "," + Mathf.FloorToInt(selectedPlanetTile.position.y) + ")";
		} else {
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileTemperature-Panel/TemperatureValue-Text").GetComponent<Text>().text = "";
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTilePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>().text = "";
			mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>().text = "";
			mapSelectionPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>().text = string.Empty;
		}
	}

	public int CalculatePlanetTemperature(float distance) {

		float starMass = 1; // 1 (lower = colder)
		float albedo = 29; // 29 (higher = colder)
		float greenhouse = 0.4f; // 1 (lower = colder)

		float sigma = 5.6703f * Mathf.Pow(10, -5);
		float L = 3.846f * Mathf.Pow(10, 33) * Mathf.Pow(starMass, 3);
		float D = distance * 1.496f * Mathf.Pow(10, 13);
		float A = albedo / 100f;
		float T = greenhouse * 0.5841f;
		float X = Mathf.Sqrt((1 - A) * L / (16 * Mathf.PI * sigma));
		float T_eff = Mathf.Sqrt(X) * (1 / Mathf.Sqrt(D));
		float T_eq = (Mathf.Pow(T_eff, 4)) * (1 + (3 * T / 4));
		float T_sur = T_eq / 0.9f;
		float T_kel = Mathf.Round(Mathf.Sqrt(Mathf.Sqrt(T_sur)));
		int celsius = Mathf.RoundToInt(T_kel - 273);

		return celsius;
	}

	public TileManager.Map planet;
	private List<int> planetTileSizes = new List<int>() { 20, 15, 12, 10, 8, 6, 5 }; // Some divisors of 600

	public static class StaticPlanetMapDataValues {
		public static bool actualMap = false;
		public static float equatorOffset = -1;
		public static bool planetTemperature = true;
		public static float averageTemperature = -1;
		public static float averagePrecipitation = -1;
		public static readonly Dictionary<TileManager.TileTypes, float> terrainTypeHeights =
			new Dictionary<TileManager.TileTypes, float> {
				{ TileManager.TileTypes.GrassWater, 0.40f },
				{ TileManager.TileTypes.Stone, 0.75f }
			};
		public static List<int> surroundingPlanetTileHeightDirections = null;
		public static bool preventEdgeTouching = true;
		public static Vector2 planetTilePosition = Vector2.zero;
	}

	public void GeneratePlanet() {
		SetSelectedPlanetTile(null);

		foreach (PlanetTile tile in planetTiles) {
			Destroy(tile.obj);
		}
		planetTiles.Clear();

		int planetSeed = SeedParser(planetSeedInputField.text, planetSeedInputField);

		int planetSize = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetTileSize);

		int planetTemperature = CalculatePlanetTemperature(planetDistance);

		planetPreviewPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(planetTileSize, planetTileSize);
		planetPreviewPanel.GetComponent<GridLayoutGroup>().constraintCount = planetSize;

		TileManager.MapData planetData = new TileManager.MapData(
			null,
			planetSeed,
			planetSize,
			StaticPlanetMapDataValues.actualMap,
			StaticPlanetMapDataValues.equatorOffset,
			StaticPlanetMapDataValues.planetTemperature,
			temperatureRange,
			planetDistance,
			planetTemperature,
			StaticPlanetMapDataValues.averageTemperature,
			StaticPlanetMapDataValues.averagePrecipitation,
			StaticPlanetMapDataValues.terrainTypeHeights,
			StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
			StaticPlanetMapDataValues.preventEdgeTouching,
			windDirection,
			StaticPlanetMapDataValues.planetTilePosition
		);
		planet = new TileManager.Map(planetData, false);
		foreach (TileManager.Tile tile in planet.tiles) {
			planetTiles.Add(new PlanetTile(tile, planetPreviewPanel.transform, tile.position, planetSize, planetTemperature));
		}
	}

	private Dictionary<int, string> windCardinalDirectionMap = new Dictionary<int, string>() {
		{0,"N" },
		{1,"E" },
		{2,"S" },
		{3,"W" },
		{4,"NE" },
		{5,"SE" },
		{6,"SW" },
		{7,"NW" },
	};
	private Dictionary<int, int> windCircularDirectionMap = new Dictionary<int, int>() {
		{0,0 },
		{1,4 },
		{2,1 },
		{3,5 },
		{4,2 },
		{5,6 },
		{6,3 },
		{7,7 },
	};

	public void UpdatePlanetInfo() {
		planetTileSize = planetTileSizes[Mathf.RoundToInt(planetSizeSlider.value)];
		planetSizeText.text = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetTileSize).ToString();

		planetDistance = (float)Math.Round(0.1f * (planetDistanceSlider.value + 6), 1);
		planetDistanceText.text = planetDistance + " AU";

		temperatureRange = Mathf.RoundToInt(temperatureRangeSlider.value * 10);
		temperatureRangeText.text = temperatureRange + "°C";

		windDirection = windCircularDirectionMap[Mathf.RoundToInt(windDirectionSlider.value)];
		windDirectionText.text = windCardinalDirectionMap[windDirection];
	}

	public int SeedParser(string seedString, InputField inputObject) {
		if (string.IsNullOrEmpty(seedString)) {
			seedString = UnityEngine.Random.Range(0, int.MaxValue).ToString();
			inputObject.text = seedString;
		}
		int mapSeed = 0;
		if (!int.TryParse(seedString, out mapSeed)) {
			int seedCharacterIndex = 1;
			foreach (char seedCharacter in seedString) {
				mapSeed += seedCharacter * seedCharacterIndex;
				seedCharacterIndex += 1;
			}
		}
		return mapSeed;
	}

	public void ParseMapRegenerationCode(string mapRegenerationCode) {
		List<string> splitMRC = mapRegenerationCode.Split('~').ToList();
		int planetSeed = int.Parse(splitMRC[0]);
		if (planetSeed.ToString().Length > planetSeedInputField.characterLimit) {
			print("planetSeed too long: " + planetSeed);
			return;
		}
		print("planetSeed: " + planetSeed);

		int planetSize = int.Parse(splitMRC[1]);
		int planetTileSize = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetSize);
		if (!planetTileSizes.Contains(planetTileSize)) {
			print("planetTileSize/planetSize not valid: " + planetTileSize + "/" + planetSize);
			return;
		}
		print("planetTileSize: " + planetTileSize);
		print("planetSize: " + planetSize);


		int planetTemperatureRange = int.Parse(splitMRC[2]);
		planetTemperatureRange = Mathf.FloorToInt(planetTemperatureRange / 10f);
		if (planetTemperatureRange < temperatureRangeSlider.minValue || planetTemperatureRange > temperatureRangeSlider.maxValue) {
			print("planetTemperatureRange out of range: " + planetTemperatureRange);
			return;
		}
		print("planetTemperatureRange: " + planetTemperatureRange);

		float planetDistance = float.Parse(splitMRC[3]);
		planetDistance = (10 * planetDistance) - 6;
		if (planetDistance < planetDistanceSlider.minValue || planetDistance > planetDistanceSlider.maxValue) {
			print("planetDistance out of range: " + planetDistance);
			return;
		}
		print("planetDistance: " + planetDistance);

		int planetPrimaryWindDirection = int.Parse(splitMRC[4]);
		if (planetPrimaryWindDirection < windDirectionSlider.minValue || planetPrimaryWindDirection > windDirectionSlider.maxValue) {
			print("planetPrimaryWindDirection out of range: " + planetPrimaryWindDirection);
			return;
		}
		print("planetPrimaryWindDirection: " + planetPrimaryWindDirection);

		Vector2 planetTilePosition = new Vector2(int.Parse(splitMRC[5]), int.Parse(splitMRC[6]));
		if (planetTilePosition.x < 0 || planetTilePosition.y < 0 || planetTilePosition.x >= planetSize || planetTilePosition.y >= planetSize) {
			print("planetTilePosition out of range: " + planetTilePosition);
			return;
		}
		print("planetTilePosition: " + planetTilePosition);

		int mapSize = int.Parse(splitMRC[7]);
		mapSize = Mathf.FloorToInt(mapSize / 50f);
		if (mapSize < mapSizeSlider.minValue || mapSize > mapSizeSlider.maxValue) {
			print("mapSize out of range: " + mapSize);
			return;
		}
		print("mapSize: " + mapSize);

		int mapSeed = int.Parse(splitMRC[8]);
		if (mapSeed.ToString().Length > mapSeedInputField.characterLimit) {
			print("mapSeed too long: " + mapSeed);
			return;
		}
		print("mapSeed: " + mapSeed);

		planetSeedInputField.text = planetSeed.ToString();
		planetSizeSlider.value = planetTileSizes.IndexOf(planetTileSize);
		planetDistanceSlider.value = planetDistance;
		temperatureRangeSlider.value = planetTemperatureRange;
		windDirectionSlider.value = planetPrimaryWindDirection;
		GeneratePlanet();

		SetSelectedPlanetTile(planetTiles.Find(planetTile => planetTile.position == planetTilePosition));
		mapSeedInputField.text = mapSeed.ToString();
		mapSizeSlider.value = mapSize;
	}

	public void PlayButton() {
		colonyName = mapSelectionPanel.transform.Find("SelectedPlanetTileSettings-Panel/ColonyName-Panel/InputField").GetComponent<InputField>().text;
		if (string.IsNullOrEmpty(colonyName) || new Regex(@"\W+", RegexOptions.IgnorePatternWhitespace).Replace(colonyName, string.Empty).Length <= 0) {
			colonyName = "Colony";
		}

		string mapSeedString = mapSeedInputField.text;
		int mapSeed = SeedParser(mapSeedString, mapSeedInputField);

		MainMenuToGameTransition(false);

		TileManager.MapData mapData = new TileManager.MapData(
			planet.mapData,
			mapSeed,
			mapSize,
			true,
			selectedPlanetTile.equatorOffset,
			false,
			0,
			0,
			0,
			selectedPlanetTile.averageTemperature,
			selectedPlanetTile.averagePrecipitation,
			selectedPlanetTile.terrainTypeHeights,
			selectedPlanetTile.surroundingPlanetTileHeightDirections,
			false,
			planet.mapData.primaryWindDirection,
			selectedPlanetTile.position
		);
		tileM.Initialize(mapData);

		pauseMenu.transform.Find("MapRegenerationCode-InputField").GetComponent<InputField>().text = tileM.map.mapData.mapRegenerationCode;
	}

	public void MainMenuToGameTransition(bool enableGameUIImmediately) {
		mainMenu.SetActive(false);
		if (enableGameUIImmediately) {
			ToggleLoadingScreen(false);
			ToggleGameUI(true);
		} else {
			ToggleLoadingScreen(true);
			ToggleGameUI(false);
		}
	}

	public void GameToMainMenuTransition() {
		ToggleGameUI(false);
		ToggleLoadingScreen(false);
		mainMenu.SetActive(true);
	}

	public void UpdateMapSizeText() {
		mapSize = Mathf.RoundToInt(mapSizeSlider.value * 50);
		mapSizeText.text = mapSize.ToString();
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

	public void ToggleLoadingScreen(bool state) {
		loadingStateText.transform.parent.gameObject.SetActive(state);
	}

	public void ToggleGameUI(bool state) {
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

	public void CreateMenus() {
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

		foreach (ResourceManager.TileObjectPrefabGroup group in resourceM.tileObjectPrefabGroups) {
			if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.None) {
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Command) {
				menus.Add(gameUI.transform.Find("CommandMenu-Button").gameObject, CreateAdditionalMenuButtons(gameUI.transform.Find("CommandMenu-Button").gameObject, group));
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Farm) {
				menus.Add(gameUI.transform.Find("FarmMenu-Button").gameObject, CreateAdditionalMenuButtons(gameUI.transform.Find("FarmMenu-Button").gameObject, group));
				continue;
			}

			GameObject groupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), buildMenuPanel.transform, false);
			groupButton.transform.Find("Text").GetComponent<Text>().text = group.name;
			GameObject groupPanel = groupButton.transform.Find("Panel").gameObject;
			groupPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(100, 21);

			Dictionary<GameObject, List<GameObject>> subgroupPanels = new Dictionary<GameObject, List<GameObject>>();
			foreach (ResourceManager.TileObjectPrefabSubGroup subgroup in group.tileObjectPrefabSubGroups) {
				GameObject subgroupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), groupPanel.transform, false);
				subgroupButton.transform.Find("Text").GetComponent<Text>().text = subgroup.name;
				GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

				List<GameObject> prefabButtons = new List<GameObject>();
				foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
					GameObject prefabButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), subgroupPanel.transform, false);
					prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
					if (prefab.baseSprite != null) {
						prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
					}
					prefabButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(prefab); });
					GameObject requiredResourcesPanel = prefabButton.transform.Find("RequiredResources-Panel").gameObject;
					prefabButton.GetComponent<HoverToggleScript>().Initialize(requiredResourcesPanel);
					foreach (ResourceManager.ResourceAmount requiredResource in prefab.resourcesToBuild) {
						GameObject requiredResourceItem = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesPanel.transform, false);
						requiredResourceItem.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = requiredResource.resource.image;
						requiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text = requiredResource.resource.name;
						requiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text = "Need " + requiredResource.amount;
						requiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + requiredResource.resource.worldTotalAmount;
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
			GameObject subgroupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"), parentMenuPanel.transform, false);
			subgroupButton.transform.Find("Text").GetComponent<Text>().text = subgroup.name;
			GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

			List<GameObject> prefabButtons = new List<GameObject>();
			foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
				GameObject prefabButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), subgroupPanel.transform, false);
				prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
				if (prefab.baseSprite != null) {
					prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
				}
				prefabButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(prefab); });
				GameObject requiredResourcesPanel = prefabButton.transform.Find("RequiredResources-Panel").gameObject;
				prefabButton.GetComponent<HoverToggleScript>().Initialize(requiredResourcesPanel);
				foreach (ResourceManager.ResourceAmount requiredResource in prefab.resourcesToBuild) {
					GameObject requiredResourceItem = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesPanel.transform, false);
					requiredResourceItem.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = requiredResource.resource.image;
					requiredResourceItem.transform.Find("ResourceName-Text").GetComponent<Text>().text = requiredResource.resource.name;
					requiredResourceItem.transform.Find("RequiredAmount-Text").GetComponent<Text>().text = "Need " + requiredResource.amount;
					requiredResourceItem.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + requiredResource.resource.worldTotalAmount;
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
		plantObjectElements.Add(Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"), tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"), false));
		plantObjectElements.Add(Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"), tileInformation.transform, false));
	}

	private Dictionary<int, List<GameObject>> tileObjectElements = new Dictionary<int, List<GameObject>>();
	private List<GameObject> plantObjectElements = new List<GameObject>();
	public void UpdateTileInformation() {
		if (mouseOverTile != null) {
			foreach (KeyValuePair<int, List<GameObject>> tileObjectElementKVP in tileObjectElements) {
				foreach (GameObject tileObjectDataElement in tileObjectElementKVP.Value) {
					tileObjectDataElement.SetActive(false);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage").GetComponent<Image>().sprite = mouseOverTile.obj.GetComponent<SpriteRenderer>().sprite;

			if (mouseOverTile.plant != null) {
				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(true);
				}
				plantObjectElements[0].GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "Plant";
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = mouseOverTile.plant.name;

				if (mouseOverTile.plant.group.maxIntegrity > 0) {
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = mouseOverTile.plant.group.maxIntegrity;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().value = mouseOverTile.plant.integrity;
					plantObjectElements[1].transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(colourMap[Colours.LightRed], colourMap[Colours.LightGreen], mouseOverTile.plant.integrity / mouseOverTile.plant.group.maxIntegrity);
				} else {
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = 1;
					plantObjectElements[1].transform.Find("Integrity-Slider").GetComponent<Slider>().value = 1;
					plantObjectElements[1].transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = colourMap[Colours.LightGrey220];
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
							Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"), tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"), false),
							Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"), tileInformation.transform, false)
						});
					}

					GameObject tileLayerSpriteObject = tileObjectElements[tileObject.prefab.layer][0];
					tileLayerSpriteObject.GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;
					tileLayerSpriteObject.SetActive(true);

					GameObject tileObjectDataObject = tileObjectElements[tileObject.prefab.layer][1];
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "L" + tileObject.prefab.layer;
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = tileObject.prefab.name;

					if (tileObject.prefab.maxIntegrity > 0) {
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = tileObject.prefab.maxIntegrity;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().value = tileObject.integrity;
						tileObjectDataObject.transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(colourMap[Colours.LightRed], colourMap[Colours.LightGreen], tileObject.integrity / tileObject.prefab.maxIntegrity);
					} else {
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().minValue = 0;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().maxValue = 1;
						tileObjectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>().value = 1;
						tileObjectDataObject.transform.Find("Integrity-Slider/Fill Area/Fill").GetComponent<Image>().color = colourMap[Colours.LightGrey220];
					}

					tileObjectDataObject.SetActive(true);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "(" + Mathf.FloorToInt(mouseOverTile.obj.transform.position.x) + ", " + Mathf.FloorToInt(mouseOverTile.obj.transform.position.y) + ")";
			if (tileM.GetStoneEquivalentTileTypes().Contains(mouseOverTile.tileType.type)) {
				tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Tile Type";
				tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = mouseOverTile.tileType.name;
			} else if (tileM.GetWaterEquivalentTileTypes().Contains(mouseOverTile.tileType.type)) {
				if (tileM.GetWaterEquivalentTileTypes().Contains(mouseOverTile.tileType.type) && !tileM.GetLiquidWaterEquivalentTileTypes().Contains(mouseOverTile.tileType.type)) {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Tile Type";
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = "Ice";
				} else {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Tile Type";
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = "Water";
				}
			} else if (tileM.GetResourceTileTypes().Contains(mouseOverTile.tileType.type) || (tileM.GetWaterToGroundResourceMap().ContainsKey(mouseOverTile.tileType.type) && tileM.GetResourceTileTypes().Contains(tileM.GetWaterToGroundResourceMap()[mouseOverTile.tileType.type]))) {
				if (tileM.GetResourceTileTypes().Contains(mouseOverTile.tileType.type)) {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Tile Type";
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = mouseOverTile.tileType.name;
				} else {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Tile Type";
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = tileM.GetTileTypeByEnum(tileM.GetWaterToGroundResourceMap()[mouseOverTile.tileType.type]).name;
				}
			} else {
				tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-BiomeLabel").GetComponent<Text>().text = "Biome";
				tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = mouseOverTile.biome.name;
			}
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
		private UIManager uiM;

		public ColonistManager.Colonist colonist;
		public ColonistManager.SkillInstance skill;
		public GameObject obj;

		public SkillElement(ColonistManager.Colonist colonist, ColonistManager.SkillInstance skill, Transform parent, UIManager uiM) {
			this.uiM = uiM;

			this.colonist = colonist;
			this.skill = skill;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SkillInfoElement-Panel"), parent, false);

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
			foreach (ColonistManager.Colonist otherColonist in uiM.colonistM.colonists) {
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
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = uiM.colourMap[Colours.DarkYellow];
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = uiM.colourMap[Colours.LightYellow];
			} else {
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = uiM.colourMap[Colours.DarkGreen];
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
			}
		}
	}

	public class InventoryElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public InventoryElement(ColonistManager.Colonist colonist, ResourceManager.ResourceAmount resourceAmount, Transform parent, UIManager uiM) {
			this.colonist = colonist;
			this.resourceAmount = resourceAmount;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;
			obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	public class ReservedResourcesColonistElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ReservedResources reservedResources;
		public List<ReservedResourceElement> reservedResourceElements = new List<ReservedResourceElement>();
		public GameObject obj;

		public ReservedResourcesColonistElement(ColonistManager.Colonist colonist, ResourceManager.ReservedResources reservedResources, Transform parent) {
			this.colonist = colonist;
			this.reservedResources = reservedResources;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourcesColonistInfoElement-Panel"), parent, false);

			obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;
			obj.transform.Find("ColonistReservedCount-Text").GetComponent<Text>().text = reservedResources.resources.Count.ToString();
			obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];

			foreach (ResourceManager.ResourceAmount ra in reservedResources.resources) {
				reservedResourceElements.Add(new ReservedResourceElement(colonist, ra, parent));
			}
		}
	}

	public class ReservedResourceElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public ReservedResourceElement(ColonistManager.Colonist colonist, ResourceManager.ResourceAmount resourceAmount, Transform parent) {
			this.colonist = colonist;
			this.resourceAmount = resourceAmount;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourceInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;

			obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	public class NeedElement {
		private UIManager uiM;

		public ColonistManager.NeedInstance needInstance;
		public GameObject obj;

		public NeedElement(ColonistManager.NeedInstance needInstance, Transform parent, UIManager uiM) {
			this.uiM = uiM;

			this.needInstance = needInstance;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/NeedElement-Panel"), parent, false);

			obj.transform.Find("NeedName-Text").GetComponent<Text>().text = needInstance.prefab.name;

			Update();
		}

		public void Update() {
			obj.transform.Find("NeedValue-Text").GetComponent<Text>().text = (Mathf.RoundToInt((needInstance.value / needInstance.prefab.clampValue) * 100)) + "%";

			obj.transform.Find("Need-Slider").GetComponent<Slider>().value = needInstance.value;
			obj.transform.Find("Need-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(uiM.colourMap[Colours.DarkGreen], uiM.colourMap[Colours.DarkRed], (needInstance.value / needInstance.prefab.clampValue));
			obj.transform.Find("Need-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(uiM.colourMap[Colours.LightGreen], uiM.colourMap[Colours.LightRed], (needInstance.value / needInstance.prefab.clampValue));
		}
	}

	public class HappinessModifierElement {

		public ColonistManager.HappinessModifierInstance happinessModifierInstance;
		public GameObject obj;

		public HappinessModifierElement(ColonistManager.HappinessModifierInstance happinessModifierInstance, Transform parent, UIManager uiM) {
			this.happinessModifierInstance = happinessModifierInstance;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/HappinessModifierElement-Panel"), parent, false);

			obj.transform.Find("HappinessModifierName-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.name;
			if (happinessModifierInstance.prefab.effectAmount > 0) {
				obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
			} else if (happinessModifierInstance.prefab.effectAmount < 0) {
				obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightRed];
			} else {
				obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightGrey220];
			}

			Update();
		}

		public bool Update() {
			if (!happinessModifierInstance.colonist.happinessModifiers.Contains(happinessModifierInstance)) {
				Destroy(obj);
				return false;
			}
			if (happinessModifierInstance.prefab.infinite) {
				obj.transform.Find("HappinessModifierTime-Text").GetComponent<Text>().text = "Until Not";
			} else {
				obj.transform.Find("HappinessModifierTime-Text").GetComponent<Text>().text = Mathf.RoundToInt(happinessModifierInstance.timer) + "s (" + Mathf.RoundToInt(happinessModifierInstance.prefab.effectLengthSeconds) + "s)";
			}
			if (happinessModifierInstance.prefab.effectAmount > 0) {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = "+" + happinessModifierInstance.prefab.effectAmount;
			} else if (happinessModifierInstance.prefab.effectAmount < 0) {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.effectAmount.ToString();
			} else {
				obj.transform.Find("HappinessModifierAmount-Text").GetComponent<Text>().text = happinessModifierInstance.prefab.effectAmount.ToString();
			}
			return true;
		}
	}

	List<SkillElement> skillElements = new List<SkillElement>();

	List<NeedElement> needElements = new List<NeedElement>();
	List<HappinessModifierElement> happinessModifierElements = new List<HappinessModifierElement>();

	List<InventoryElement> inventoryElements = new List<InventoryElement>();
	List<ReservedResourcesColonistElement> reservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

	/* Called from ColonistManager.SetSelectedColonistFromInput(), ColonistManager.SetSelectedColonist(), ColonistManager.DeselectSelectedColonist(), TileManager.Initialize() */
	public void SetSelectedColonistInformation() {
		if (colonistM.selectedColonist != null) {
			selectedColonistInformationPanel.SetActive(true);

			RectTransform rightListPanel = gameUI.transform.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x, 310);

			selectedColonistInformationPanel.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonistM.selectedColonist.name + " (" + colonistM.selectedColonist.gender.ToString()[0] + ")";
			selectedColonistInformationPanel.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = colonistM.selectedColonist.moveSprites[0];

			selectedColonistInformationPanel.transform.Find("AffiliationName-Text").GetComponent<Text>().text = "Colonist of " + colonyName;

			foreach (SkillElement skillElement in skillElements) {
				Destroy(skillElement.obj);
			}
			skillElements.Clear();
			foreach (NeedElement needElement in needElements) {
				Destroy(needElement.obj);
			}
			needElements.Clear();
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in reservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			reservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in inventoryElements) {
				Destroy(inventoryElement.obj);
			}
			inventoryElements.Clear();
			foreach (HappinessModifierElement happinessModifierElement in happinessModifierElements) {
				Destroy(happinessModifierElement.obj);
			}
			happinessModifierElements.Clear();

			foreach (ColonistManager.SkillInstance skill in colonistM.selectedColonist.skills) {
				skillElements.Add(new SkillElement(colonistM.selectedColonist, skill, selectedColonistInformationPanel.transform.Find("SkillsList-Panel"), this));
			}
			foreach (ColonistManager.NeedInstance need in colonistM.selectedColonist.needs) {
				needElements.Add(new NeedElement(need, selectedColonistInformationPanel.transform.Find("Needs-Panel/Needs-ScrollPanel/NeedsList-Panel"), this));
			}
			foreach (ResourceManager.ReservedResources rr in colonistM.selectedColonist.inventory.reservedResources) {
				reservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.colonist, rr, selectedColonistInformationPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryList-Panel")));
			}
			foreach (ResourceManager.ResourceAmount ra in colonistM.selectedColonist.inventory.resources) {
				inventoryElements.Add(new InventoryElement(colonistM.selectedColonist, ra, selectedColonistInformationPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryList-Panel"), this));
			}
			foreach (ColonistManager.HappinessModifierInstance hmi in colonistM.selectedColonist.happinessModifiers) {
				happinessModifierElements.Add(new HappinessModifierElement(hmi, selectedColonistInformationPanel.transform.Find("HappinessModifier-Panel/HappinessModifier-ScrollPanel/HappinessModifierList-Panel"), this));
			}
		} else {
			selectedColonistInformationPanel.SetActive(false);

			RectTransform rightListPanel = gameUI.transform.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x, 5);

			if (skillElements.Count > 0) {
				foreach (SkillElement skillElement in skillElements) {
					Destroy(skillElement.obj);
				}
				skillElements.Clear();
			}
			if (needElements.Count > 0) {
				foreach (NeedElement needElement in needElements) {
					Destroy(needElement.obj);
				}
				needElements.Clear();
			}
			if (reservedResourcesColonistElements.Count > 0) {
				foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in reservedResourcesColonistElements) {
					foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
						Destroy(reservedResourceElement.obj);
					}
					reservedResourcesColonistElement.reservedResourceElements.Clear();
					Destroy(reservedResourcesColonistElement.obj);
				}
				reservedResourcesColonistElements.Clear();
			}
			if (inventoryElements.Count > 0) {
				foreach (InventoryElement inventoryElement in inventoryElements) {
					Destroy(inventoryElement.obj);
				}
				inventoryElements.Clear();
			}
		}
	}

	private Dictionary<int, int> happinessModifierButtonSizeMap = new Dictionary<int, int>() {
		{1,45 },{2,60 },{3,65 }
	};
	private Dictionary<int, int> happinessModifierValueHorizontalPositionMap = new Dictionary<int, int>() {
		{1,-50 },{2,-65 },{3,-70 }
	};

	private List<HappinessModifierElement> removeHME = new List<HappinessModifierElement>();
	public void UpdateSelectedColonistInformation() {
		if (colonistM.selectedColonist != null) {

			selectedColonistInformationPanel.transform.Find("ColonistHealth-Panel/ColonistHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(colonistM.selectedColonist.health * 100);
			selectedColonistInformationPanel.transform.Find("ColonistHealth-Panel/ColonistHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(colourMap[Colours.DarkRed], colourMap[Colours.DarkGreen], colonistM.selectedColonist.health);
			selectedColonistInformationPanel.transform.Find("ColonistHealth-Panel/ColonistHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(colourMap[Colours.LightRed], colourMap[Colours.LightGreen], colonistM.selectedColonist.health);
			selectedColonistInformationPanel.transform.Find("ColonistHealth-Panel/ColonistHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(colonistM.selectedColonist.health * 100) + "%";

			selectedColonistInventoryPanel.transform.Find("ColonistInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedColonistInventoryPanel.transform.Find("ColonistInventory-Slider").GetComponent<Slider>().maxValue = colonistM.selectedColonist.inventory.maxAmount;
			selectedColonistInventoryPanel.transform.Find("ColonistInventory-Slider").GetComponent<Slider>().value = colonistM.selectedColonist.inventory.CountResources();
			selectedColonistInventoryPanel.transform.Find("ColonistInventoryValue-Text").GetComponent<Text>().text = colonistM.selectedColonist.inventory.CountResources() + "/ " + colonistM.selectedColonist.inventory.maxAmount;

			selectedColonistInformationPanel.transform.Find("ColonistCurrentAction-Text").GetComponent<Text>().text = jobM.GetJobDescription(colonistM.selectedColonist.job);
			if (colonistM.selectedColonist.storedJob != null) {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = jobM.GetJobDescription(colonistM.selectedColonist.storedJob);
			} else {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = string.Empty;
			}

			selectedColonistInformationPanel.transform.Find("SkillsList-Panel/SkillsListTitle-Panel/Profession-Text").GetComponent<Text>().text = colonistM.selectedColonist.profession.name;

			int happinessModifiersSum = Mathf.RoundToInt(colonistM.selectedColonist.happinessModifiersSum);

			int happinessLength = Mathf.Abs(happinessModifiersSum).ToString().Length;
			Text happinessModifierAmountText = selectedColonistHappinessModifiersButton.transform.Find("HappinessModifiersAmount-Text").GetComponent<Text>();
			if (happinessModifiersSum > 0) {
				happinessModifierAmountText.text = "+" + happinessModifiersSum + "%";
				happinessModifierAmountText.color = colourMap[Colours.LightGreen];
			} else if (happinessModifiersSum < 0) {
				happinessModifierAmountText.text = happinessModifiersSum + "%";
				happinessModifierAmountText.color = colourMap[Colours.LightRed];
			} else {
				happinessModifierAmountText.text = happinessModifiersSum + "%";
				happinessModifierAmountText.color = colourMap[Colours.LightGrey200];
			}
			selectedColonistHappinessModifiersButton.GetComponent<RectTransform>().sizeDelta = new Vector2(happinessModifierButtonSizeMap[happinessLength], 20);
			selectedColonistInformationPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<RectTransform>().offsetMax = new Vector2(happinessModifierValueHorizontalPositionMap[happinessLength], 0);
			selectedColonistInformationPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(colonistM.selectedColonist.effectiveHappiness) + "%";
			selectedColonistInformationPanel.transform.Find("Needs-Panel/HappinessValue-Text").GetComponent<Text>().color = Color.Lerp(colourMap[Colours.LightRed], colourMap[Colours.LightGreen], colonistM.selectedColonist.effectiveHappiness / 100f);

			foreach (SkillElement skillElement in skillElements) {
				skillElement.Update();
			}
			foreach (NeedElement needElement in needElements) {
				needElement.Update();
			}
			foreach (InventoryElement inventoryElement in inventoryElements) {
				inventoryElement.Update();
			}
			foreach (HappinessModifierElement happinessModifierElement in happinessModifierElements) {
				bool keep = happinessModifierElement.Update();
				if (!keep) {
					removeHME.Add(happinessModifierElement);
				}
			}
			if (removeHME.Count > 0) {
				foreach (HappinessModifierElement happinessModifierElement in removeHME) {
					Destroy(happinessModifierElement.obj);
					happinessModifierElements.Remove(happinessModifierElement);
				}
				removeHME.Clear();
			}
		}
	}

	public class ColonistElement {

		public ColonistManager.Colonist colonist;
		public GameObject obj;

		public ColonistElement(ColonistManager.Colonist colonist, Transform transform, UIManager uiM) {
			this.colonist = colonist;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), transform, false);

			obj.GetComponent<RectTransform>().sizeDelta = new Vector2(170, obj.GetComponent<RectTransform>().sizeDelta.y);

			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate { colonist.colonistM.SetSelectedColonist(colonist); });

			Update(uiM);
		}

		public void Update(UIManager uiM) {
			obj.GetComponent<Image>().color = Color.Lerp(uiM.colourMap[Colours.LightRed], uiM.colourMap[Colours.LightGreen], colonist.health);
		}

		public void DestroyObject() {
			Destroy(obj);
		}
	}

	List<ColonistElement> colonistElements = new List<ColonistElement>();

	public void RemoveColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.DestroyObject();
		}
		colonistElements.Clear();
	}

	/* Called from ColonistManager.AddColonists() */
	public void SetColonistElements() {
		RemoveColonistElements();
		if (colonistList.activeSelf) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonistElements.Add(new ColonistElement(colonist, colonistList.transform.Find("ColonistList-Panel"), this));
			}
		}
	}

	public void UpdateColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.Update(this);
		}
	}

	public class JobElement {

		public JobManager.Job job;
		public ColonistManager.Colonist colonist;
		public GameObject obj;
		public GameObject colonistObj;

		public JobElement(JobManager.Job job, ColonistManager.Colonist colonist, Transform parent, UIManager uiM, CameraManager cameraM) {
			this.job = job;
			this.colonist = colonist;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/JobInfoElement-Panel"), parent, false);
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
			obj.transform.Find("JobInfo/Type").GetComponent<Text>().text = uiM.SplitByCapitals(job.prefab.jobType.ToString());
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				cameraM.SetCameraPosition(job.tile.obj.transform.position);
				cameraM.SetCameraZoom(5);
			});

			if (job.priority > 0) {
				obj.transform.Find("JobInfo").GetComponent<Image>().color = uiM.colourMap[Colours.LightYellow];
			}

			if (colonist != null) {
				obj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<RectTransform>().sizeDelta.x, 93);
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().minValue = 0;
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
				if (colonist.job.started) {
					obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
				} else {
					obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightYellow];
				}

				colonistObj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), obj.transform, false);

				colonistObj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
				colonistObj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
				colonistObj.GetComponent<Button>().onClick.AddListener(delegate { colonist.colonistM.SetSelectedColonist(colonist); });
				colonistObj.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
				colonistObj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<RectTransform>().sizeDelta.x, colonistObj.GetComponent<RectTransform>().sizeDelta.y);
				colonistObj.GetComponent<Outline>().enabled = false;
			}

			job.jobUIElement = this;

			Update();
		}

		public void Update() {
			obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().value = job.colonistBuildTime - job.jobProgress;
		}

		public void DestroyObjects() {
			job.jobUIElement = null;
			Destroy(obj);
			Destroy(colonistObj);
		}

		public void Remove(UIManager uiM) {
			uiM.jobElements.Remove(this);
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

	/* Called from Colonist.FinishJob(), Colonist.CreateJob(), Colonist.AddExistingJob(), JobManager.GiveJobsToColonists(), TileManager.Initialize() */
	public void SetJobElements() {
		if (jobM.jobs.Count > 0 || colonistM.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
			jobList.SetActive(true);
			RemoveJobElements();
			List<ColonistManager.Colonist> orderedColonists = colonistM.colonists.Where(colonist => colonist.job != null).OrderBy(colonist => colonist.job.jobProgress).ToList();
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => colonist.job.started)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobList.transform.Find("JobList-Panel"), this, cameraM));
			}
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => !colonist.job.started)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobList.transform.Find("JobList-Panel"), this, cameraM));
			}
			foreach (JobManager.Job job in jobM.jobs.Where(j => j.started).OrderBy(j => (j.jobProgress / j.colonistBuildTime))) {
				jobElements.Add(new JobElement(job, null, jobList.transform.Find("JobList-Panel"), this, cameraM));
			}
			foreach (JobManager.Job job in jobM.jobs.Where(j => !j.started).OrderByDescending(j => j.priority)) {
				jobElements.Add(new JobElement(job, null, jobList.transform.Find("JobList-Panel"), this, cameraM));
			}
		} else {
			jobList.SetActive(false);
			if (jobElements.Count > 0) {
				RemoveJobElements();
			}
		}
	}

	public void UpdateJobElements() {
		foreach (JobElement jobElement in jobElements) {
			jobElement.Update();
		}
	}

	public void UpdateDateTimeInformation(int minute, int hour, int day, int month, int year, bool isDay) {
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Time-Text").GetComponent<Text>().text = timeM.Get12HourTime() + ":" + (minute < 10 ? ("0" + minute) : minute.ToString()) + (hour < 12 || hour > 23 ? "AM" : "PM") + " (" + (isDay ? "D" : "N") + ")";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = (timeM.GetTimeModifier() > 0 ? new string('>', timeM.GetTimeModifier()) : "-");
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

		selectionSizeCanvas.transform.localScale = Vector2.one * 0.005f * 0.5f * cameraM.cameraComponent.orthographicSize;
		selectionSizeCanvas.transform.position = new Vector2(
			mousePosition.x + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.x / 2f * selectionSizeCanvas.transform.localScale.x),
			mousePosition.y + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.y / 2f * selectionSizeCanvas.transform.localScale.y)
		);
	}

	public void InitializeSelectedContainerIndicator() {
		selectedContainerIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"), Vector2.zero, Quaternion.identity);
		SpriteRenderer sCISR = selectedContainerIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = Resources.Load<Sprite>(@"UI/selectionCorners");
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
			selectedContainerIndicator.transform.position = selectedContainer.parentObject.obj.transform.position;

			selectedContainerInventoryPanel.SetActive(true);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				Destroy(inventoryElement.obj);
			}
			containerInventoryElements.Clear();

			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventoryName-Text").GetComponent<Text>().text = selectedContainer.parentObject.prefab.name;

			int numResources = selectedContainer.inventory.CountResources();
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().maxValue = selectedContainer.maxAmount;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().value = numResources;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventorySizeValue-Text").GetComponent<Text>().text = numResources + "/ " + selectedContainer.maxAmount;

			foreach (ResourceManager.ReservedResources rr in selectedContainer.inventory.reservedResources) {
				containerReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.colonist, rr, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel")));
			}
			foreach (ResourceManager.ResourceAmount ra in selectedContainer.inventory.resources) {
				containerInventoryElements.Add(new InventoryElement(colonistM.selectedColonist, ra, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel"), this));
			}
			selectedContainerInventoryPanel.transform.Find("SelectedContainerSprite-Image").GetComponent<Image>().sprite = selectedContainer.parentObject.obj.GetComponent<SpriteRenderer>().sprite;
		} else {
			selectedContainerIndicator.SetActive(false);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				Destroy(inventoryElement.obj);
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
			//SetProfessionsList();
			DisableProfessionsList();
		}
		if (objectPrefabsList.activeSelf && parentObj != objectPrefabsList) {
			//ToggleObjectPrefabsList();
			DisableObjectPrefabsList();
		}
		if (resourcesList.activeSelf && parentObj != resourcesList) {
			//SetResourcesList();
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

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ProfessionInfoElement-Panel"), parent, false);

			colonistsInProfessionListObj = obj.transform.Find("ColonistsInProfessionList-Panel").gameObject;
			obj.transform.Find("ColonistsInProfession-Button").GetComponent<Button>().onClick.AddListener(delegate {
				foreach (ProfessionElement professionElement in uiM.professionElements) {
					if (professionElement != this) {
						professionElement.colonistsInProfessionListObj.SetActive(false);
					}
					foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
						Destroy(obj);
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
						Destroy(obj);
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
							Destroy(obj);
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

		public void Update() {
			obj.transform.Find("ColonistsInProfession-Button/ColonistsInProfessionAmount-Text").GetComponent<Text>().text = profession.colonistsInProfession.Count.ToString();
		}
	}

	public void SetColonistsInProfessionList(ProfessionElement professionElement) {
		foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
			Destroy(obj);
		}
		professionElement.colonistsInProfessionElements.Clear();
		foreach (ColonistManager.Colonist colonist in professionElement.profession.colonistsInProfession) {
			GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), professionElement.colonistsInProfessionListObj.transform, false);
			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				colonistM.SetSelectedColonist(colonist);
			});
			professionElement.colonistsInProfessionElements.Add(obj);
		}
	}

	double CalculateProfessionSkillLevel(ColonistManager.Profession profession, ColonistManager.Colonist colonist, bool round, int decimalPlaces) {
		if (profession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
			ColonistManager.SkillInstance skillInstance = colonist.skills.Find(skill => skill.prefab == profession.primarySkill);
			double skillLevel = skillInstance.level + (skillInstance.currentExperience / skillInstance.nextLevelExperience);
			if (round) {
				return Math.Round(skillLevel, decimalPlaces);
			}
			return skillLevel;
		}
		return 0;
	}

	void UpdateProfessionLevelInfo(GameObject obj, ColonistManager.Colonist colonist, ColonistManager.Profession currentProfession, ColonistManager.Profession nextProfession) {
		obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = currentProfession.name;
		if (colonist.profession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
			/*ColonistManager.SkillInstance currentSkillInstance = colonist.skills.Find(skill => skill.prefab == colonist.profession.primarySkill);
			double currentSkillLevel = Math.Round(currentSkillInstance.level + (currentSkillInstance.currentExperience / currentSkillInstance.nextLevelExperience), 2);*/
			double currentSkillLevel = CalculateProfessionSkillLevel(currentProfession, colonist, true, 2);
			obj.transform.Find("ColonistProfessionLevel-Text").GetComponent<Text>().text = currentSkillLevel.ToString();
		} else {
			obj.transform.Find("ColonistProfessionLevel-Text").GetComponent<Text>().text = "0";
		}

		obj.transform.Find("ColonistNextProfession-Text").GetComponent<Text>().text = nextProfession.name;
		if (nextProfession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
			/*ColonistManager.SkillInstance nextSkillInstance = colonist.skills.Find(skill => skill.prefab == nextProfession.primarySkill);
			double nextSkillLevel = Math.Round(nextSkillInstance.level + (nextSkillInstance.currentExperience / nextSkillInstance.nextLevelExperience), 2);*/
			double nextSkillLevel = CalculateProfessionSkillLevel(nextProfession, colonist, true, 2);
			obj.transform.Find("ColonistNextProfessionLevel-Text").GetComponent<Text>().text = nextSkillLevel.ToString();
		} else {
			obj.transform.Find("ColonistNextProfessionLevel-Text").GetComponent<Text>().text = "0";
		}
	}

	public void SetEditColonistsInProfessionList(ProfessionElement professionElement, bool remove) {
		foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
			Destroy(obj);
		}
		professionElement.editColonistsInProfessionElements.Clear();
		if (remove) { // User clicked red minus button
			List<ColonistManager.Colonist> validColonists = colonistM.colonists.Where(c => c.profession == professionElement.profession).ToList();
			validColonists = validColonists.OrderBy(c => CalculateProfessionSkillLevel(c.profession, c, false, 0)).ToList();
			foreach (ColonistManager.Colonist colonist in validColonists) {
				GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"), professionElement.editColonistsInProfessionListObj.transform, false);
				obj.GetComponent<Image>().color = colourMap[Colours.LightGrey200];
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					if (colonist.profession == professionElement.profession) {
						colonist.ChangeProfession(colonist.oldProfession);
						obj.GetComponent<Image>().color = colourMap[Colours.LightRed];
					} else {
						colonist.ChangeProfession(professionElement.profession);
						obj.GetComponent<Image>().color = colourMap[Colours.LightBlue];
					}
					UpdateProfessionLevelInfo(obj, colonist, colonist.profession, colonist.oldProfession);
				});
				obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];
				obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;

				UpdateProfessionLevelInfo(obj, colonist, colonist.profession, colonist.oldProfession);

				obj.GetComponent<Image>().color = colourMap[Colours.LightBlue];
				professionElement.editColonistsInProfessionElements.Add(obj);
			}
		} else { // User clicked green plus button
			List<ColonistManager.Colonist> validColonists = colonistM.colonists.Where(c => c.profession != professionElement.profession).ToList();
			if (professionElement.profession.type == ColonistManager.ProfessionTypeEnum.Nothing) {
				validColonists = validColonists.OrderBy(c => CalculateProfessionSkillLevel(professionElement.profession, c, false, 0)).ToList();
			} else {
				validColonists = validColonists.OrderByDescending(c => CalculateProfessionSkillLevel(professionElement.profession, c, false, 0)).ToList();
			}
			foreach (ColonistManager.Colonist colonist in validColonists) {
				GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"), professionElement.editColonistsInProfessionListObj.transform, false);
				obj.GetComponent<Image>().color = colourMap[Colours.LightGrey200];
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					if (colonist.profession == professionElement.profession) {
						colonist.ChangeProfession(colonist.oldProfession);
						obj.GetComponent<Image>().color = colourMap[Colours.LightGrey200];
					} else {
						colonist.ChangeProfession(professionElement.profession);
						obj.GetComponent<Image>().color = colourMap[Colours.LightBlue];
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
	public void InitializeProfessionsList() {
		foreach (ProfessionElement professionElement in professionElements) {
			Destroy(professionElement.obj);
		}
		professionElements.Clear();
		foreach (ColonistManager.Profession profession in colonistM.professions) {
			professionElements.Add(new ProfessionElement(profession, professionsList.transform, this));
		}
		SetProfessionsList();
	}

	public void SetProfessionsList() {
		DisableAdminPanels(professionsList);
		professionsList.SetActive(!professionsList.activeSelf);
		foreach (ProfessionElement professionElement in professionElements) {
			foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
				Destroy(obj);
			}
			foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
				Destroy(obj);
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
				Destroy(obj);
			}
			foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
				Destroy(obj);
			}
			professionElement.colonistsInProfessionListObj.SetActive(false);
			professionElement.editColonistsInProfessionListObj.SetActive(false);
			professionElement.obj.SetActive(false);
		}
	}

	public void UpdateProfessionsList() {
		foreach (ProfessionElement professionElement in professionElements) {
			professionElement.Update();
		}
	}

	public class ObjectPrefabElement {
		public ResourceManager.TileObjectPrefab prefab;
		public GameObject obj;
		public GameObject objectInstancesList;
		public List<ObjectInstanceElement> instanceElements = new List<ObjectInstanceElement>();

		public ObjectPrefabElement(ResourceManager.TileObjectPrefab prefab, Transform parent, CameraManager cameraM, ResourceManager resourceM, UIManager uiM) {
			this.prefab = prefab;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectPrefab-Button"), parent, false);

			obj.transform.Find("ObjectPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite;
			obj.transform.Find("ObjectPrefabName-Text").GetComponent<Text>().text = prefab.name;

			objectInstancesList = obj.transform.Find("ObjectInstancesList-ScrollPanel").gameObject;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				objectInstancesList.SetActive(!objectInstancesList.activeSelf);
				if (objectInstancesList.activeSelf) {
					objectInstancesList.transform.SetParent(uiM.canvas.transform);
					foreach (ObjectPrefabElement objectPrefabElement in uiM.objectPrefabElements) {
						if (objectPrefabElement != this) {
							objectPrefabElement.objectInstancesList.SetActive(false);
						}
					}
				} else {
					objectInstancesList.transform.SetParent(obj.transform);
				}
			});

			AddObjectInstancesList(true, cameraM, resourceM, uiM);

			Update();
		}

		public void AddObjectInstancesList(bool newList, CameraManager cameraM, ResourceManager resourceM, UIManager uiM) {
			RemoveObjectInstances();
			bool objectInstancesListState = objectInstancesList.activeSelf;
			if (newList) {
				objectInstancesListState = false;
			}
			objectInstancesList.SetActive(true);
			foreach (ResourceManager.TileObjectInstance instance in resourceM.GetTileObjectInstanceList(prefab)) {
				instanceElements.Add(new ObjectInstanceElement(instance, objectInstancesList.transform.Find("ObjectInstancesList-Panel"), cameraM, resourceM, uiM));
			}
			objectInstancesList.SetActive(objectInstancesListState);
			Update();
		}

		public void Remove() {
			RemoveObjectInstances();
			Destroy(obj);
		}

		public void RemoveObjectInstances() {
			foreach (ObjectInstanceElement instance in instanceElements) {
				Destroy(instance.obj);
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

		public ObjectInstanceElement(ResourceManager.TileObjectInstance instance, Transform parent, CameraManager cameraM, ResourceManager resourceM, UIManager uiM) {
			this.instance = instance;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectInstance-Button"), parent, false);

			obj.transform.Find("ObjectInstanceSprite-Image").GetComponent<Image>().sprite = instance.obj.GetComponent<SpriteRenderer>().sprite;
			obj.transform.Find("ObjectInstanceName-Text").GetComponent<Text>().text = instance.prefab.name;
			obj.transform.Find("TilePosition-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(instance.tile.obj.transform.position.x) + ", " + Mathf.FloorToInt(instance.tile.obj.transform.position.y) + ")"; ;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				cameraM.SetCameraPosition(instance.obj.transform.position);
				cameraM.SetCameraZoom(5);
			});

			ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject == instance);
			if (container != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					uiM.selectedContainer = container;
					uiM.SetSelectedContainerInfo();
				});
			}
			ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject == instance);
			if (mto != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					uiM.SetSelectedManufacturingTileObject(mto);
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
			objectPrefabElements.Add(new ObjectPrefabElement(prefab, objectPrefabsList.transform.Find("ObjectPrefabsList-Panel"), cameraM, resourceM, this));
		} else {
			UpdateObjectPrefabElement(prefab);
		}
	}

	private void UpdateObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement != null) {
			objectPrefabElement.AddObjectInstancesList(false, cameraM, resourceM, this);
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

	public class ResourceInstanceElement {
		private UIManager uiM;

		public ResourceManager.Resource resource;
		public GameObject obj;

		public InputField desiredAmountInput;
		public Text desiredAmountText;

		public ResourceInstanceElement(ResourceManager.Resource resource, Transform parent, UIManager uiM) {
			this.uiM = uiM;

			this.resource = resource;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceListResourceElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resource.name;

			obj.transform.Find("Image").GetComponent<Image>().sprite = resource.image;

			desiredAmountInput = obj.transform.Find("DesiredAmount-Input").GetComponent<InputField>();
			/*
			if (resource.desiredAmount > 0) {
				desiredAmountInput.text = resource.desiredAmount.ToString();
			}
			*/
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

		private int worldTotalAmountPrev = 0;
		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resource.worldTotalAmount.ToString();
			if (resource.desiredAmount > 0) {
				if (resource.desiredAmount > resource.worldTotalAmount) {
					desiredAmountText.color = uiM.colourMap[Colours.LightRed];
				} else if (resource.desiredAmount <= resource.worldTotalAmount) {
					desiredAmountText.color = uiM.colourMap[Colours.LightGreen];
				}
			}

			worldTotalAmountPrev = resource.worldTotalAmount;

			if (worldTotalAmountPrev != resource.worldTotalAmount) {
				if (resource.worldTotalAmount > 0) {
					obj.GetComponent<Image>().color = uiM.colourMap[Colours.LightGrey220];
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = uiM.colourMap[Colours.LightGrey220];
				} else {
					obj.GetComponent<Image>().color = uiM.colourMap[Colours.Grey150];
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = uiM.colourMap[Colours.Grey150];
				}
			}
		}
	}

	public void InitializeResourcesList() {
		foreach (ResourceInstanceElement rie in resourceInstanceElements) {
			Destroy(rie.obj);
		}
		resourceInstanceElements.Clear();
		Transform resourcesListParent = resourcesList.transform.Find("ResourcesList-Panel");
		foreach (ResourceManager.Resource resource in resourceM.resources) {
			ResourceInstanceElement newRIE = new ResourceInstanceElement(resource, resourcesListParent, this);
			newRIE.resource.resourceListElement = newRIE;
			resourceInstanceElements.Add(newRIE);
		}
	}

	public List<ResourceInstanceElement> resourceInstanceElements = new List<ResourceInstanceElement>();

	public void SetResourcesList() {
		DisableAdminPanels(resourcesList);
		resourcesList.SetActive(!resourcesList.activeSelf);
		if (resourceInstanceElements.Count <= 0) {
			resourcesList.SetActive(false);
		}/* else {
			foreach (ResourceInstanceElement rie in resourceInstanceElements) {
				rie.desiredAmountInput.text = rie.resource.desiredAmount.ToString();
				if (rie.resource.desiredAmount > 0) {
					print(rie.resource.name);
				}
			}
		}*/
	}

	public void DisableResourcesList() {
		resourcesList.SetActive(false);
	}

	public void UpdateResourcesList() {
		foreach (ResourceInstanceElement resourceInstanceElement in resourceInstanceElements) {
			resourceInstanceElement.Update();
		}
		int index = 0;
		foreach (ResourceInstanceElement resourceInstanceElement in resourceInstanceElements.OrderByDescending(element => element.resource.worldTotalAmount).ThenBy(element => element.resource.name)) {
			resourceInstanceElement.obj.transform.SetSiblingIndex(index);
			index += 1;
		}
	}

	public void InitializeSelectedManufacturingTileObjectIndicator() {
		selectedMTOIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"), Vector2.zero, Quaternion.identity);
		SpriteRenderer sCISR = selectedMTOIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		sCISR.name = "SelectedMTOIndicator";
		sCISR.sortingOrder = 20; // Selected MTO Indicator Sprite
		sCISR.color = new Color(1f, 1f, 1f, 0.75f);
		selectedMTOIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
		selectedMTOIndicator.SetActive(false);
	}

	public class MTOPanel {

		private ResourceManager resourceM;

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

		public MTOPanel(GameObject obj, bool fuel, ResourceManager resourceM) {
			this.resourceM = resourceM;

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

				foreach (ResourceManager.ResourcesEnum fuelEnum in resourceM.GetFuelResources()) {
					ResourceManager.Resource fuelResource = resourceM.GetResourceByEnum(fuelEnum);

					GameObject selectFuelResourceButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SelectFuelResource-Panel"), selectFuelResourceList.transform, false);
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
				Destroy(selectResourceListElementKVP.Key);
			}
			selectResourceListElements.Clear();

			obj.SetActive(true);

			selectedMTOIndicator.SetActive(true);
			selectedMTOIndicator.transform.position = selectedMTO.parentObject.obj.transform.position;

			obj.transform.Find("SelectedManufacturingTileObjectName-Text").GetComponent<Text>().text = selectedMTO.parentObject.prefab.name;

			obj.transform.Find("SelectedManufacturingTileObjectSprite-Image").GetComponent<Image>().sprite = selectedMTO.parentObject.obj.GetComponent<SpriteRenderer>().sprite;

			foreach (ResourceManager.ResourcesEnum resourceEnum in resourceM.GetManufacturableResources()) {
				ResourceManager.Resource resource = resourceM.GetResourceByEnum(resourceEnum);

				if (resource.requiredMTOs.Contains(selectedMTO.parentObject.prefab) || (resource.requiredMTOs.Count <= 0 && resource.requiredMTOSubGroups.Contains(selectedMTO.parentObject.prefab.tileObjectPrefabSubGroup))) {
					GameObject selectResourceButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SelectManufacturedResource-Panel"), selectResourceList.transform, false);
					selectResourceButton.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resource.image;
					selectResourceButton.transform.Find("ResourceName-Text").GetComponent<Text>().text = resource.name;
					//selectResourceButton.transform.Find("ResourceManufactureTileObjectSubGroupName-Text").GetComponent<Text>().text = resource.manufacturingTileObjectSubGroup.name;
					selectResourceButton.transform.Find("RequiredEnergy-Text").GetComponent<Text>().text = resource.requiredEnergy.ToString();

					selectResourceListElements.Add(selectResourceButton, resource);
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
				Destroy(selectedMTOResourceRequiredResourceKVP.Key);
			}
			selectedMTOResourceRequiredResources.Clear();

			if (selectedMTO.createResource != null) {
				Transform requiredResourcesList = obj.transform.Find("RequiredResources-Panel/RequiredResources-ScrollPanel/RequiredResourcesList-Panel");
				foreach (ResourceManager.ResourceAmount requiredResource in selectedMTO.createResource.requiredResources) {
					GameObject requiredResourcePanel = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), requiredResourcesList, false);

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

		public void Update(ResourceManager.ManufacturingTileObject selectedMTO, UIManager uiM) {
			if (selectedMTO.createResource != null) {
				obj.transform.Find("SelectResource-Button/SelectedResourceName-Text").GetComponent<Text>().text = selectedMTO.createResource.name;

				foreach (KeyValuePair<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResourceKVP in selectedMTOResourceRequiredResources) {
					selectedMTOResourceRequiredResourceKVP.Key.transform.Find("AvailableAmount-Text").GetComponent<Text>().text = "Have " + selectedMTOResourceRequiredResourceKVP.Value.resource.worldTotalAmount;
					if (selectedMTOResourceRequiredResourceKVP.Value.resource.worldTotalAmount < selectedMTOResourceRequiredResourceKVP.Value.amount) {
						selectedMTOResourceRequiredResourceKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightRed];
					} else {
						selectedMTOResourceRequiredResourceKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
					}
				}

				obj.transform.Find("ResourceTargetAmount-Panel/CurrentAmountValue-Text").GetComponent<Text>().text = selectedMTO.createResource.worldTotalAmount.ToString();

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
						selectFuelResourceButtonKVP.Key.transform.Find("AmountAvailableValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.worldTotalAmount.ToString();
					}
					if (selectedMTO.createResource != null) {
						foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
							GameObject selectFuelResourceButton = selectFuelResourceButtonKVP.Key;
							ResourceManager.Resource fuelResource = selectFuelResourceButtonKVP.Value;
							float energyRatio = (float)Math.Round((selectedMTO.createResource.requiredEnergy) / ((float)fuelResource.fuelEnergy), 2);
							selectFuelResourceButtonKVP.Key.transform.Find("EnergyValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.fuelEnergy + " (" + energyRatio + " : 1)";
							if (Mathf.CeilToInt(energyRatio) > fuelResource.worldTotalAmount) {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightRed];
							} else {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
							}
						}
					} else {
						foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectFuelResourceButtonKVP in selectFuelResourceListElements) {
							selectFuelResourceButtonKVP.Key.transform.Find("EnergyValue-Text").GetComponent<Text>().text = selectFuelResourceButtonKVP.Value.fuelEnergy.ToString();
							if (selectFuelResourceButtonKVP.Value.worldTotalAmount <= 0) {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightRed];
							} else {
								selectFuelResourceButtonKVP.Key.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
							}
						}
					}
				}
			}

			if (selectedMTO.active) {
				activeValueText.GetComponent<Text>().text = "Active";
				if (selectedMTO.canActivate) {
					activeValueButton.GetComponent<Image>().color = uiM.colourMap[Colours.LightGreen];
				} else {
					activeValueButton.GetComponent<Image>().color = uiM.colourMap[Colours.LightOrange];
				}
			} else {
				activeValueText.GetComponent<Text>().text = "Inactive";
				if (selectedMTO.canActivate) {
					activeValueButton.GetComponent<Image>().color = uiM.colourMap[Colours.LightOrange];
				} else {
					activeValueButton.GetComponent<Image>().color = uiM.colourMap[Colours.LightRed];
				}
			}

			obj.transform.Find("SelectResource-Button").GetComponent<Image>().color = (selectedMTO.createResource != null ? (selectedMTO.hasEnoughRequiredResources ? uiM.colourMap[Colours.LightGreen] : uiM.colourMap[Colours.LightRed]) : (uiM.colourMap[Colours.LightGrey220]));
			if (fuel) {
				obj.transform.Find("SelectFuelResource-Button").GetComponent<Image>().color = (selectedMTO.fuelResource != null ? (selectedMTO.hasEnoughFuel ? uiM.colourMap[Colours.LightGreen] : uiM.colourMap[Colours.LightRed]) : (uiM.colourMap[Colours.LightGrey220]));
			}
		}

		public void Deselect(GameObject selectedMTOIndicator) {
			foreach (KeyValuePair<GameObject, ResourceManager.Resource> selectResourceButtonKVP in selectResourceListElements) {
				selectResourceButtonKVP.Key.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			foreach (KeyValuePair<GameObject, ResourceManager.ResourceAmount> selectedMTOResourceRequiredResourceKVP in selectedMTOResourceRequiredResources) {
				Destroy(selectedMTOResourceRequiredResourceKVP.Key);
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
			if (resourceM.GetManufacturingTileObjectsFuel().Contains(selectedMTO.parentObject.prefab.type)) {
				selectedMTOPanel = selectedMTOFuelPanel;
			} else {
				selectedMTOPanel = selectedMTONoFuelPanel;
			}
			selectedMTOPanel.Select(selectedMTO, selectedMTOIndicator);
		}
	}

	public void TogglePauseMenu() {
		if (pauseSavePanel.activeSelf) {
			ToggleSaveMenu();
		}
		if (loadGamePanel.activeSelf) {
			ToggleLoadMenu(false);
		}
		if (settingsPanel.activeSelf) {
			ToggleSettingsMenu();
		}
		pauseMenu.SetActive(!pauseMenu.activeSelf);
		timeM.SetPaused(pauseMenu.activeSelf);
	}

	public void TogglePauseMenuButtons(bool state) {
		pauseMenuButtons.SetActive(state);
		pauseLabel.SetActive(pauseMenuButtons.activeSelf);
	}

	private string saveFileName;
	public void ToggleSaveMenu() {
		pauseSavePanel.SetActive(!pauseSavePanel.activeSelf);
		TogglePauseMenuButtons(!pauseSavePanel.activeSelf);
		if (pauseSavePanel.activeSelf) {
			saveFileName = persistenceM.GenerateSaveFileName();
			pauseSavePanel.transform.Find("SaveFileName-Text").GetComponent<Text>().text = saveFileName;
			pauseSavePanel.transform.Find("PauseSavePanelSave-Button").GetComponent<Button>().onClick.RemoveAllListeners();
			pauseSavePanel.transform.Find("PauseSavePanelSave-Button").GetComponent<Button>().onClick.AddListener(delegate {
				persistenceM.SaveGame(saveFileName);
				ToggleSaveMenu();
			});
		} else {
			saveFileName = string.Empty;
		}
	}

	public List<LoadFile> loadFiles = new List<LoadFile>();

	public class LoadFile {

		public string fileName;
		public GameObject loadFilePanel;

		public LoadFile(string fileName, Transform loadFilePanelParent, bool fromMainMenu, UIManager uiM) {
			this.fileName = fileName;

			string colonyName = fileName.Split('-')[2];

			string rawSaveDT = fileName.Split('-')[3];
			List<string> splitRawSaveDT = new Regex(@"[a-zA-Z]").Split(rawSaveDT).ToList();
			string year = splitRawSaveDT[0];
			string month = (splitRawSaveDT[1].Length == 1 ? "0" : "") + splitRawSaveDT[1];
			string day = (splitRawSaveDT[2].Length == 1 ? "0" : "") + splitRawSaveDT[2];
			string saveDate = year + "-" + month + "-" + day;
			string hour = (splitRawSaveDT[3].Length == 1 ? "0" : "") + splitRawSaveDT[3];
			string minute = (splitRawSaveDT[4].Length == 1 ? "0" : "") + splitRawSaveDT[4];
			string second = (splitRawSaveDT[5].Length == 1 ? "0" : "") + splitRawSaveDT[5];
			string saveTime = hour + ":" + minute + ":" + second;

			loadFilePanel = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/LoadFile-Panel"), loadFilePanelParent, false);
			loadFilePanel.transform.Find("ColonyName-Text").GetComponent<Text>().text = colonyName;
			loadFilePanel.transform.Find("SaveDate-Text").GetComponent<Text>().text = saveDate;
			loadFilePanel.transform.Find("SaveTime-Text").GetComponent<Text>().text = saveTime;

			bool compatible = false;
			int gameVersion = -1;
			int saveVersion = -1;
			if (fileName.Split('-').Length == 6) {
				gameVersion = int.Parse(fileName.Split('-')[4]);
				saveVersion = int.Parse(fileName.Split('-')[5].Split('.')[0]);
				if (saveVersion == PersistenceManager.saveVersion) {
					compatible = true;
				}
			}

			loadFilePanel.transform.Find("GameVersionValue-Text").GetComponent<Text>().text = uiM.persistenceM.GetGameVersionString(gameVersion);
			loadFilePanel.transform.Find("SaveFileVersionValue-Text").GetComponent<Text>().text = (saveVersion == -1 ? "Unsupported" : saveVersion.ToString());

			string imageFile = "file://" + fileName.Split('.')[0] + ".png";
			WWW www = new WWW(imageFile);
			if (string.IsNullOrEmpty(www.error)) {
				uiM.StartCoroutine(LoadSaveFileImage(www));
			}

			loadFilePanel.GetComponent<Button>().interactable = compatible;
			if (compatible) {
				loadFilePanel.GetComponent<Button>().onClick.AddListener(delegate { uiM.SetSelectedLoadFile(this, fromMainMenu); });
			}
		}

		IEnumerator LoadSaveFileImage(WWW www) {
			while (!www.isDone) {
				yield return null;
			}
			if (string.IsNullOrEmpty(www.error)) {
				Texture2D texture = new Texture2D(35, 63, TextureFormat.RGB24, false);
				www.LoadImageIntoTexture(texture);
				if (loadFilePanel != null && loadFilePanel.transform.Find("SavePreview-Image") != null) {
					loadFilePanel.transform.Find("SavePreview-Image").GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(new Vector2(0, 0), new Vector2(texture.width, texture.height)), new Vector2(0, 0));
				}
			}
		}
	}

	private LoadFile selectedLoadFile;

	public void SetSelectedLoadFile(LoadFile newSelectedLoadFile, bool fromMainMenu) {
		if (selectedLoadFile != null) {
			selectedLoadFile.loadFilePanel.GetComponent<Image>().color = colourMap[Colours.LightGrey220];
		}
		selectedLoadFile = newSelectedLoadFile;
		if (selectedLoadFile != null) {
			selectedLoadFile.loadFilePanel.GetComponent<Image>().color = colourMap[Colours.LightGrey200];
			loadGamePanel.transform.Find("LoadGamePanelLoad-Button").GetComponent<Button>().onClick.RemoveAllListeners();
			loadGamePanel.transform.Find("LoadGamePanelLoad-Button").GetComponent<Button>().onClick.AddListener(delegate {
				persistenceM.LoadGame(selectedLoadFile.fileName, fromMainMenu);
			});
		}
	}

	private LoadFile continueLoadFile = null;
	public void PreviewMainMenuContinueFile() {
		if (continueLoadFile != null) {
			Destroy(continueLoadFile.loadFilePanel);
			continueLoadFile = null;
		}
		List<string> saveFiles = new List<string>();
		try {
			saveFiles = Directory.GetFiles(persistenceM.GenerateSavePath("")).ToList().OrderBy(fileName => SaveFileDateTimeSum(fileName)).Reverse().ToList();
		} catch (DirectoryNotFoundException) {
			return;
		}
		if (saveFiles.Count > 0) {
			continueLoadFile = new LoadFile(saveFiles[0], mainMenu.transform.Find("MainMenuButtons-Panel/Continue-Button/LoadFilePanelParent-Panel"), true, this);
			continueLoadFile.loadFilePanel.GetComponent<Image>().color = colourMap[Colours.LightGrey200];
			Destroy(continueLoadFile.loadFilePanel.GetComponent<Button>());
		} else {
			Destroy(continueLoadFile.loadFilePanel);
		}
	}

	public void ToggleMainMenuContinue() {
		if (continueLoadFile != null) {
			persistenceM.LoadGame(continueLoadFile.fileName, true);
		}
	}

	public void SetLoadMenuActive(bool active, bool fromMainMenu) {
		if (active) {
			if (!loadGamePanel.activeSelf) {
				ToggleLoadMenu(fromMainMenu);
			}
		} else {
			if (loadGamePanel.activeSelf) {
				ToggleLoadMenu(fromMainMenu);
			}
		}
	}

	public void ToggleLoadMenu(bool fromMainMenu) {
		loadGamePanel.SetActive(!loadGamePanel.activeSelf);
		ToggleMainMenuButtons(loadGamePanel);
		TogglePauseMenuButtons(!loadGamePanel.activeSelf);
		foreach (LoadFile loadFile in loadFiles) {
			Destroy(loadFile.loadFilePanel);
		}
		loadFiles.Clear();
		if (loadGamePanel.activeSelf) {
			List<string> saveFiles;
			try {
				saveFiles = Directory.GetFiles(persistenceM.GenerateSavePath("")).ToList().OrderBy(fileName => SaveFileDateTimeSum(fileName)).Reverse().ToList();
			} catch (DirectoryNotFoundException) {
				return;
			}
			foreach (string fileName in saveFiles) {
				if (fileName.Split('.')[1] == "snowship") {
					LoadFile loadFile = new LoadFile(fileName, loadGamePanel.transform.Find("LoadFilesList-ScrollPanel/LoadFilesList-Panel"), fromMainMenu, this);
					loadFiles.Add(loadFile);
				}
			}
		} else {
			SetSelectedLoadFile(null, false);
		}
	}

	public string SaveFileDateTimeSum(string fileName) {
		List<string> splitRawSaveDT = new Regex(@"[a-zA-Z]").Split(fileName.Split('-')[3]).ToList();
		string year = splitRawSaveDT[0];
		string month = (splitRawSaveDT[1].Length == 1 ? "0" : "") + splitRawSaveDT[1];
		string day = (splitRawSaveDT[2].Length == 1 ? "0" : "") + splitRawSaveDT[2];
		string hour = (splitRawSaveDT[3].Length == 1 ? "0" : "") + splitRawSaveDT[3];
		string minute = (splitRawSaveDT[4].Length == 1 ? "0" : "") + splitRawSaveDT[4];
		string second = (splitRawSaveDT[5].Length == 1 ? "0" : "") + splitRawSaveDT[5];
		string saveDT = year + month + day + hour + minute + second;
		return saveDT;
	}

	public enum UIScaleMode {
		ConstantPixelSize,
		ScaleWithScreenSize
	};

	public class SettingsState {
		private UIManager uiM;

		public Resolution resolution;
		public int resolutionWidth;
		public int resolutionHeight;
		public int refreshRate;
		public bool fullscreen;
		public CanvasScaler.ScaleMode scaleMode;

		public SettingsState(UIManager uiM) {
			this.uiM = uiM;

			resolution = Screen.currentResolution;
			resolutionWidth = Screen.currentResolution.width;
			resolutionHeight = Screen.currentResolution.height;
			refreshRate = Screen.currentResolution.refreshRate;
			fullscreen = true;
			scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		}

		public void SetSettings() {
			Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen);

			uiM.canvas.GetComponent<CanvasScaler>().uiScaleMode = scaleMode;
		}
	}

	public void ToggleSettingsMenu() {
		settingsPanel.SetActive(!settingsPanel.activeSelf);
		ToggleMainMenuButtons(settingsPanel);
		TogglePauseMenuButtons(!settingsPanel.activeSelf);
		if (settingsPanel.activeSelf) {
			GameObject resolutionSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/ResolutionSettings-Panel").gameObject;
			Slider resolutionSlider = resolutionSettingsPanel.transform.Find("Resolution-Slider").GetComponent<Slider>();
			resolutionSlider.minValue = 0;
			resolutionSlider.maxValue = Screen.resolutions.Length - 1;
			resolutionSlider.onValueChanged.AddListener(delegate {
				Resolution r = Screen.resolutions[Mathf.RoundToInt(resolutionSlider.value)];
				settingsState.resolution = r;
				settingsState.resolutionWidth = r.width;
				settingsState.resolutionHeight = r.height;
				settingsState.refreshRate = r.refreshRate;
				resolutionSettingsPanel.transform.Find("ResolutionValue-Text").GetComponent<Text>().text = settingsState.resolutionWidth + " × " + settingsState.resolutionHeight + " @ " + r.refreshRate + "hz";
			});
			resolutionSlider.value = Screen.resolutions.ToList().IndexOf(settingsState.resolution);

			GameObject fullscreenSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/FullscreenSettings-Panel").gameObject;
			Toggle fullscreenToggle = fullscreenSettingsPanel.transform.Find("Fullscreen-Toggle").GetComponent<Toggle>();
			fullscreenToggle.onValueChanged.AddListener(delegate { settingsState.fullscreen = fullscreenToggle.isOn; });
			fullscreenToggle.isOn = settingsState.fullscreen;

			GameObject UIScaleModeSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/UIScaleModeSettings-Panel").gameObject;
			Toggle UIScaleModeToggle = UIScaleModeSettingsPanel.transform.Find("UIScaleMode-Toggle").GetComponent<Toggle>();
			UIScaleModeToggle.onValueChanged.AddListener(delegate {
				if (UIScaleModeToggle.isOn) {
					settingsState.scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				} else {
					settingsState.scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
				}
			});
			if (settingsState.scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
				UIScaleModeToggle.isOn = true;
			} else if (settingsState.scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {
				UIScaleModeToggle.isOn = false;
			}
		}
	}

	public void ApplySettings(bool closeAfterApplying) {
		if (closeAfterApplying) {
			ToggleSettingsMenu();
		}
		settingsState.SetSettings();
		persistenceM.SaveSettings();
	}

	public void ExitToMenu() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}