using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PersistenceManager : BaseManager {

	private GameManager startCoroutineReference;

	public void SetStartCoroutineReference(GameManager startCoroutineReference) {
		this.startCoroutineReference = startCoroutineReference;
	}

	public static readonly KeyValuePair<int, string> gameVersion = new KeyValuePair<int, string>(3, "2018.2");
	public static readonly KeyValuePair<int, string> saveVersion = new KeyValuePair<int, string>(3, "2018.2");

	public SettingsState settingsState;

	public void CreateSettingsState() {
		settingsState = new SettingsState();
		if (!LoadSettings()) {
			SaveSettings();
		}
		settingsState.ApplySettings();
	}

	// Settings Saving

	public class SettingsState {
		private string filePath;

		public Resolution resolution;
		public int resolutionWidth;
		public int resolutionHeight;
		public int refreshRate;
		public bool fullscreen;
		public CanvasScaler.ScaleMode scaleMode;

		public SettingsState() {
			resolution = Screen.currentResolution;
			resolutionWidth = Screen.currentResolution.width;
			resolutionHeight = Screen.currentResolution.height;
			refreshRate = Screen.currentResolution.refreshRate;
			fullscreen = true;
			scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		}

		public enum Setting {
			ResolutionWidth,
			ResolutionHeight,
			RefreshRate,
			Fullscreen,
			ScaleMode
		}

		private static readonly Dictionary<Setting, Func<SettingsState, string>> settingToStringFunctions = new Dictionary<Setting, Func<SettingsState, string>>() {
			{ Setting.ResolutionWidth, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.resolutionWidth.ToString(); }) },
			{ Setting.ResolutionHeight, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.resolutionHeight.ToString(); }) },
			{ Setting.RefreshRate, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.refreshRate.ToString(); }) },
			{ Setting.Fullscreen, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.fullscreen.ToString(); }) },
			{ Setting.ScaleMode, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.scaleMode.ToString(); }) }
		};

		private static readonly Dictionary<Setting, Action<SettingsState, string>> stringToSettingFunctions = new Dictionary<Setting, Action<SettingsState, string>>() {
			{ Setting.ResolutionWidth, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.resolutionWidth = int.Parse(value); }) },
			{ Setting.ResolutionHeight, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.resolutionHeight = int.Parse(value); }) },
			{ Setting.RefreshRate, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.refreshRate = int.Parse(value); }) },
			{ Setting.Fullscreen, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.fullscreen = bool.Parse(value); }) },
			{ Setting.ScaleMode, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.scaleMode = (CanvasScaler.ScaleMode)Enum.Parse(typeof(CanvasScaler.ScaleMode), value); }) }
		};

		public void ApplySettings() {
			Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen, refreshRate);

			GameManager.uiM.canvas.GetComponent<CanvasScaler>().uiScaleMode = scaleMode;
		}

		public void LoadSetting(Setting setting, string value) {
			stringToSettingFunctions[setting](this, value);
		}

		public void SaveSetting(Setting setting, StreamWriter file) {
			file.WriteLine("<" + setting + ">" + settingToStringFunctions[setting](this));
		}
	}

	public string GenerateSettingsDirectoryPath() {
		return Application.persistentDataPath + "/Settings/";
	}

	public string GenerateSettingsFilePath() {
		return GenerateSettingsDirectoryPath() + "settings.snowship";
	}

	public void SaveSettings() {
		Directory.CreateDirectory(GenerateSettingsDirectoryPath());

		string settingsFilePath = GenerateSettingsFilePath();
		FileStream createFile = File.Create(settingsFilePath);
		createFile.Close();

		File.WriteAllText(settingsFilePath, string.Empty);

		StreamWriter file = new StreamWriter(settingsFilePath);

		foreach (SettingsState.Setting setting in Enum.GetValues(typeof(SettingsState.Setting))) {
			settingsState.SaveSetting(setting, file);
		}

		file.Close();
	}

	public bool LoadSettings() {
		StreamReader file = null;
		try {
			file = new StreamReader(GenerateSettingsFilePath());
			if (file == null) {
				return false;
			}
		} catch (Exception) {
			return false;
		}

		foreach (string settingsFileLine in file.ReadToEnd().Split('\n')) {
			if (!string.IsNullOrEmpty(settingsFileLine)) {
				string key = settingsFileLine.Split('>')[0].Replace("<", string.Empty);
				SettingsState.Setting setting = (SettingsState.Setting)Enum.Parse(typeof(SettingsState.Setting), key);

				string value = settingsFileLine.Split('>')[1];

				settingsState.LoadSetting(setting, value);
			}
		}

		file.Close();

		return true;
	}

	public void ApplySettings() {
		settingsState.ApplySettings();
		SaveSettings();
	}

	// Game Saving

	public class UniverseState {
		public static readonly string saveVersion = "2018.2";
		public static readonly string gameVersion = "2018.2";
	}

	public static string GenerateDateTimeString() {
		DateTime now = DateTime.Now;
		string dateTime = string.Format(
			"{0}{1}{2}{3}{4}{5}{6}",
			now.Year.ToString().PadLeft(4, '0'),
			now.Month.ToString().PadLeft(2, '0'),
			now.Day.ToString().PadLeft(2, '0'),
			now.Hour.ToString().PadLeft(2, '0'),
			now.Minute.ToString().PadLeft(2, '0'),
			now.Second.ToString().PadLeft(2, '0'),
			now.Millisecond.ToString().PadLeft(4, '0')
		);
		return dateTime;
	}

	public static string GenerateSaveDateTimeString() {
		DateTime now = DateTime.Now;
		return string.Format(
			"{0}:{1}:{2} {3}/{4}/{5}",
			now.Hour.ToString().PadLeft(2, '0'),
			now.Minute.ToString().PadLeft(2, '0'),
			now.Second.ToString().PadLeft(2, '0'),
			now.Day.ToString().PadLeft(2, '0'),
			now.Month.ToString().PadLeft(2, '0'),
			now.Year.ToString().PadLeft(4, '0')
		);
	}

	IEnumerator CreateScreenshot(string fileName) {
		GameObject canvas = GameObject.Find("Canvas");
		canvas.SetActive(false);
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(fileName + ".png");
		canvas.SetActive(true);
	}

	public WWW CreateWWWForFile(string path) {
		WWW www = new WWW(path);
		if (string.IsNullOrEmpty(www.error)) {
			return www;
		}
		return null;
	}

	private Sprite LoadSaveImage(WWW www) {
		Sprite saveImage = null;
		while (!www.isDone) {
			continue;
		}
		if (string.IsNullOrEmpty(www.error)) {
			Texture2D texture = new Texture2D(280, 158, TextureFormat.RGB24, false);
			www.LoadImageIntoTexture(texture);
			saveImage = Sprite.Create(texture, new Rect(new Vector2(0, 0), new Vector2(texture.width, texture.height)), new Vector2(0, 0));
		}
		return saveImage;
	}

	public static string CreateKeyValueString(object key, object value, int level) {
		return (new string('\t', level)) + "<" + key.ToString() + ">" + value.ToString();
	}

	public static string FormatVector2ToString(Vector2 vector2) {
		return vector2.x + "," + vector2.y;
	}

	public static StreamWriter CreateFileAtDirectory(string directory, string fileName) {
		string filePath = directory + "/" + fileName;
		FileStream fileStream = File.Create(filePath);
		fileStream.Close();

		return new StreamWriter(filePath);
	}

	public static List<KeyValuePair<string, object>> GetKeyValuePairsFromFile(string path) {
		List<string> lines = File.ReadAllLines(path).ToList();
		List<KeyValuePair<string, object>> properties = new List<KeyValuePair<string, object>>();
		while (true) {
			if (lines.Count <= 0) {
				break;
			}
			string line = lines[0];
			lines.RemoveAt(0);

			string key = line.Split('>')[0].Replace("<", string.Empty).Replace("\t", string.Empty);
			string value = line.Split('>')[1].Replace("\n", string.Empty).Replace("\r", string.Empty);

			key = Regex.Replace(key, @"\n|\r", string.Empty);
			value = Regex.Replace(value, @"\n|\r", string.Empty);

			if (string.IsNullOrEmpty(value)) {
				properties.Add(new KeyValuePair<string, object>(key, GetSubPropertiesFromProperty(lines, 1)));
			} else {
				properties.Add(new KeyValuePair<string, object>(key, value));
			}
		}
		return properties;
	}

	public static List<KeyValuePair<string, object>> GetSubPropertiesFromProperty(List<string> lines, int level) {
		List<KeyValuePair<string, object>> properties = new List<KeyValuePair<string, object>>();
		while (true) {
			if (lines.Count <= 0) {
				break;
			}

			string line = lines[0];

			string key = line.Split('>')[0].Replace("<", string.Empty);
			if (key.Length - key.Replace("\t", string.Empty).Length < level) {
				break;
			} else {
				lines.RemoveAt(0);
			}
			key = key.Replace("\t", string.Empty);
			string value = line.Split('>')[1].Replace("\n", string.Empty).Replace("\r", string.Empty);

			key = Regex.Replace(key, @"\n|\r", string.Empty);
			value = Regex.Replace(value, @"\n|\r", string.Empty);

			if (string.IsNullOrEmpty(value)) {
				properties.Add(new KeyValuePair<string, object>(key, GetSubPropertiesFromProperty(lines, level + 1)));
			} else {
				properties.Add(new KeyValuePair<string, object>(key, value));
			}
		}
		return properties;
	}

	public static string GenerateUniversesPath() {
		return Application.persistentDataPath + "/Universes";
	}

	public void CreateUniverse(UniverseManager.Universe universe) {
		if (universe == null) {
			Debug.LogError("Universe to be saved is null.");
			return;
		}

		string universesDirectoryPath = GenerateUniversesPath();
		string dateTimeString = GenerateDateTimeString();
		string universeDirectoryPath = universesDirectoryPath + "/Universe-" + dateTimeString;
		Directory.CreateDirectory(universeDirectoryPath);
		universe.SetDirectory(universeDirectoryPath);

		StreamWriter configurationFile = CreateFileAtDirectory(universeDirectoryPath, "configuration.snowship");
		SaveConfiguration(configurationFile);
		configurationFile.Close();

		StreamWriter universeFile = CreateFileAtDirectory(universeDirectoryPath, "universe.snowship");
		SaveUniverse(universeFile, universe);
		universeFile.Close();

		string planetsDirectoryPath = universeDirectoryPath + "/Planets";
		Directory.CreateDirectory(planetsDirectoryPath);
	}

	public void CreatePlanet(PlanetManager.Planet planet) {
		if (planet == null) {
			Debug.LogError("Planet to be saved is null.");
			return;
		}

		if (GameManager.universeM.universe == null) {
			Debug.LogError("Universe to save the planet to is null.");
			return;
		}

		if (string.IsNullOrEmpty(GameManager.universeM.universe.directory)) {
			Debug.LogError("Universe directory is null or empty.");
			return;
		}

		string planetsDirectoryPath = GameManager.universeM.universe.directory + "/Planets";
		string dateTimeString = GenerateDateTimeString();
		string planetDirectoryPath = planetsDirectoryPath + "/Planet-" + dateTimeString;
		Directory.CreateDirectory(planetDirectoryPath);
		planet.SetDirectory(planetDirectoryPath);

		StreamWriter planetFile = CreateFileAtDirectory(planetDirectoryPath, "planet.snowship");
		SavePlanet(planetFile, planet);
		planetFile.Close();

		string citiesDirectoryPath = planetDirectoryPath + "/Cities";
		Directory.CreateDirectory(citiesDirectoryPath);

		string coloniesDirectoryPath = planetDirectoryPath + "/Colonies";
		Directory.CreateDirectory(coloniesDirectoryPath);
	}

	public void CreateCity() {

	}

	public void CreateColony(ColonyManager.Colony colony) {
		if (colony == null) {
			Debug.LogError("Colony to be saved is null.");
			return;
		}

		if (GameManager.planetM.planet == null) {
			Debug.LogError("Planet to save the colony to is null.");
			return;
		}

		if (string.IsNullOrEmpty(GameManager.planetM.planet.directory)) {
			Debug.LogError("Planet directory is null or empty.");
			return;
		}

		string coloniesDirectoryPath = GameManager.planetM.planet.directory + "/Colonies";
		string dateTimeString = GenerateDateTimeString();
		string colonyDirectoryPath = coloniesDirectoryPath + "/Colony-" + dateTimeString;
		Directory.CreateDirectory(colonyDirectoryPath);
		colony.SetDirectory(colonyDirectoryPath);

		StreamWriter colonyFile = CreateFileAtDirectory(colonyDirectoryPath, "colony.snowship");
		SaveColony(colonyFile, colony);
		colonyFile.Close();

		string mapDirectoryPath = colonyDirectoryPath + "/Map";
		Directory.CreateDirectory(mapDirectoryPath);

		StreamWriter tilesFile = CreateFileAtDirectory(mapDirectoryPath, "tiles.snowship");
		SaveOriginalTiles(tilesFile);
		tilesFile.Close();

		StreamWriter riversFile = CreateFileAtDirectory(mapDirectoryPath, "rivers.snowship");
		SaveRivers(riversFile);
		riversFile.Close();

		string savesDirectoryPath = colonyDirectoryPath + "/Saves";
		Directory.CreateDirectory(savesDirectoryPath);
	}

	public void CreateSave(ColonyManager.Colony colony) {
		string savesDirectoryPath = colony.directory + "/Saves";
		string dateTimeString = GenerateDateTimeString();
		string saveDirectoryPath = savesDirectoryPath + "/Save-" + dateTimeString;

		try {
			Directory.CreateDirectory(saveDirectoryPath);

			StreamWriter cameraFile = CreateFileAtDirectory(saveDirectoryPath, "camera.snowship");
			SaveCamera(cameraFile);
			cameraFile.Close();

			StreamWriter caravansFile = CreateFileAtDirectory(saveDirectoryPath, "caravans.snowship");
			SaveCaravans(caravansFile);
			caravansFile.Close();

			StreamWriter colonistsFile = CreateFileAtDirectory(saveDirectoryPath, "colonists.snowship");
			SaveColonists(colonistsFile);
			colonistsFile.Close();

			StreamWriter jobsFile = CreateFileAtDirectory(saveDirectoryPath, "jobs.snowship");
			SaveJobs(jobsFile);
			jobsFile.Close();

			StreamWriter objectsFile = CreateFileAtDirectory(saveDirectoryPath, "objects.snowship");
			SaveObjects(objectsFile);
			objectsFile.Close();

			StreamWriter resourcesFile = CreateFileAtDirectory(saveDirectoryPath, "resources.snowship");
			SaveResources(resourcesFile);
			resourcesFile.Close();

			StreamWriter tilesFile = CreateFileAtDirectory(saveDirectoryPath, "tiles.snowship");
			SaveModifiedTiles(tilesFile, LoadOriginalTiles(colony.directory + "/Map/tiles.snowship"));
			tilesFile.Close();

			StreamWriter timeFile = CreateFileAtDirectory(saveDirectoryPath, "time.snowship");
			SaveTime(timeFile);
			timeFile.Close();

			string lastSaveDateTime = GenerateSaveDateTimeString();
			GameManager.universeM.universe.lastSaveDateTime = lastSaveDateTime;
			GameManager.planetM.planet.lastSaveDateTime = lastSaveDateTime;
			colony.lastSaveDateTime = lastSaveDateTime;

			StreamWriter saveFile = CreateFileAtDirectory(saveDirectoryPath, "save.snowship");
			SaveSave(saveFile, lastSaveDateTime);
			saveFile.Close();

			startCoroutineReference.StartCoroutine(CreateScreenshot(saveDirectoryPath + "/screenshot-" + dateTimeString));
		} catch (Exception e) {
			Directory.Delete(saveDirectoryPath);
			Debug.LogError(e.ToString());
		}
	}

	public enum ConfigurationProperty {
		GameVersion, SaveVersion
	}

	public void SaveConfiguration(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(ConfigurationProperty.GameVersion, gameVersion.Value, 0));
		file.WriteLine(CreateKeyValueString(ConfigurationProperty.SaveVersion, saveVersion.Value, 0));
	}

	public Dictionary<ConfigurationProperty, string> LoadConfiguration(string path) {
		Dictionary<ConfigurationProperty, string> properties = new Dictionary<ConfigurationProperty, string>();

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			ConfigurationProperty key = (ConfigurationProperty)Enum.Parse(typeof(ConfigurationProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case ConfigurationProperty.GameVersion:
					properties.Add(key, (string)value);
					break;
				case ConfigurationProperty.SaveVersion:
					properties.Add(key, (string)value);
					break;
				default:
					Debug.LogError("Unknown configuration property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return properties;
	}

	public void ApplyLoadedConfiguration(PersistenceUniverse persistenceUniverse) {

	}

	public enum UniverseProperty {
		Name, LastSaveDateTime
	}

	public void SaveUniverse(StreamWriter file, UniverseManager.Universe universe) {
		file.WriteLine(CreateKeyValueString(UniverseProperty.LastSaveDateTime, universe.lastSaveDateTime, 0));

		file.WriteLine(CreateKeyValueString(UniverseProperty.Name, universe.name, 0));
	}

	public class PersistenceUniverse {
		public string path;

		public Dictionary<ConfigurationProperty, string> configurationProperties;
		public Dictionary<UniverseProperty, string> universeProperties;

		public PersistenceUniverse(string path) {
			this.path = path;

			configurationProperties = GameManager.persistenceM.LoadConfiguration(path + "/configuration.snowship");
			universeProperties = GameManager.persistenceM.LoadUniverse(path + "/universe.snowship");
		}
	}

	public List<PersistenceUniverse> GetPersistenceUniverses() {
		List<PersistenceUniverse> persistenceUniverses = new List<PersistenceUniverse>();
		string universesPath = GenerateUniversesPath();
		if (Directory.Exists(universesPath)) {
			foreach (string universeDirectoryPath in Directory.GetDirectories(universesPath)) {
				persistenceUniverses.Add(new PersistenceUniverse(universeDirectoryPath));
			}
		}
		persistenceUniverses = persistenceUniverses.OrderByDescending(pu => pu.path).ToList();
		return persistenceUniverses;
	}

	public Dictionary<UniverseProperty, string> LoadUniverse(string path) {
		Dictionary<UniverseProperty, string> properties = new Dictionary<UniverseProperty, string>();

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			UniverseProperty key = (UniverseProperty)Enum.Parse(typeof(UniverseProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case UniverseProperty.LastSaveDateTime:
					properties.Add(key, (string)value);
					break;
				case UniverseProperty.Name:
					properties.Add(key, (string)value);
					break;
				default:
					Debug.LogError("Unknown universe property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return properties;
	}

	public void ApplyLoadedUniverse(PersistenceUniverse persistenceUniverse) {
		UniverseManager.Universe universe = new UniverseManager.Universe(persistenceUniverse.universeProperties[UniverseProperty.Name]) {
			directory = persistenceUniverse.path,
			lastSaveDateTime = persistenceUniverse.universeProperties[UniverseProperty.LastSaveDateTime]
		};
		GameManager.universeM.SetUniverse(universe);
	}

	public enum PlanetProperty {
		LastSaveDateTime, Name, Seed, Size, SunDistance, TempRange, RandomOffsets, WindDirection
	}

	public void SavePlanet(StreamWriter file, PlanetManager.Planet planet) {
		file.WriteLine(CreateKeyValueString(PlanetProperty.LastSaveDateTime, planet.lastSaveDateTime, 0));

		file.WriteLine(CreateKeyValueString(PlanetProperty.Name, planet.name, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.Seed, planet.mapData.mapSeed, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.Size, planet.mapData.mapSize, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.SunDistance, planet.mapData.planetDistance, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.TempRange, planet.mapData.temperatureRange, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.RandomOffsets, planet.mapData.randomOffsets, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.WindDirection, planet.mapData.primaryWindDirection, 0));
	}

	public class PersistencePlanet {
		public string path;

		public string lastSaveDateTime;

		public string name;
		public int seed;
		public int size;
		public float sunDistance;
		public int temperatureRange;
		public bool randomOffsets;
		public int windDirection;

		public PersistencePlanet(string path) {
			this.path = path;
		}
	}

	public List<PersistencePlanet> GetPersistencePlanets() {
		List<PersistencePlanet> persistencePlanets = new List<PersistencePlanet>();
		string planetsPath = GameManager.universeM.universe.directory + "/Planets";
		if (Directory.Exists(planetsPath)) {
			foreach (string planetDirectoryPath in Directory.GetDirectories(planetsPath)) {
				persistencePlanets.Add(LoadPlanet(planetDirectoryPath + "/planet.snowship"));
			}
		}
		persistencePlanets = persistencePlanets.OrderByDescending(pp => pp.path).ToList();
		return persistencePlanets;
	}

	public PersistencePlanet LoadPlanet(string path) {

		PersistencePlanet persistencePlanet = new PersistencePlanet(path);

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			PlanetProperty key = (PlanetProperty)Enum.Parse(typeof(PlanetProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case PlanetProperty.LastSaveDateTime:
					persistencePlanet.lastSaveDateTime = (string)value;
					break;
				case PlanetProperty.Name:
					persistencePlanet.name = (string)value;
					break;
				case PlanetProperty.Seed:
					persistencePlanet.seed = int.Parse((string)value);
					break;
				case PlanetProperty.Size:
					persistencePlanet.size = int.Parse((string)value);
					break;
				case PlanetProperty.SunDistance:
					persistencePlanet.sunDistance = float.Parse((string)value);
					break;
				case PlanetProperty.TempRange:
					persistencePlanet.temperatureRange = int.Parse((string)value);
					break;
				case PlanetProperty.RandomOffsets:
					persistencePlanet.randomOffsets = bool.Parse((string)value);
					break;
				case PlanetProperty.WindDirection:
					persistencePlanet.windDirection = int.Parse((string)value);
					break;
				default:
					Debug.LogError("Unknown planet property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistencePlanet;
	}

	public PlanetManager.Planet ApplyLoadedPlanet(PersistencePlanet persistencePlanet) {
		PlanetManager.Planet planet = GameManager.planetM.CreatePlanet(
			persistencePlanet.name,
			persistencePlanet.seed,
			persistencePlanet.size,
			persistencePlanet.sunDistance,
			persistencePlanet.temperatureRange,
			persistencePlanet.randomOffsets,
			persistencePlanet.windDirection
		);
		planet.SetLastSaveDateTime(persistencePlanet.lastSaveDateTime);
		planet.SetDirectory(Directory.GetParent(persistencePlanet.path).FullName);
		return planet;
	}

	public enum ColonyProperty {
		LastSaveDateTime, Name, PlanetPosition, Seed, Size, AverageTemperature, AveragePrecipitation, TerrainTypeHeights, SurroundingPlanetTileHeights, OnRiver, SurroundingPlanetTileRivers
	}

	public void SaveColony(StreamWriter file, ColonyManager.Colony colony) {
		file.WriteLine(CreateKeyValueString(ColonyProperty.LastSaveDateTime, colony.lastSaveDateTime, 0));

		file.WriteLine(CreateKeyValueString(ColonyProperty.Name, colony.name, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.PlanetPosition, FormatVector2ToString(colony.map.mapData.planetTilePosition), 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.Seed, colony.map.mapData.mapSeed, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.Size, colony.map.mapData.mapSize, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.AverageTemperature, colony.map.mapData.averageTemperature, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.AveragePrecipitation, colony.map.mapData.averagePrecipitation, 0));

		file.WriteLine(CreateKeyValueString(ColonyProperty.TerrainTypeHeights, string.Empty, 0));
		foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeight in colony.map.mapData.terrainTypeHeights) {
			file.WriteLine(CreateKeyValueString(terrainTypeHeight.Key, terrainTypeHeight.Value, 1));
		}

		file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileHeights, string.Join(",", colony.map.mapData.surroundingPlanetTileHeightDirections.Select(i => i.ToString()).ToArray()), 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.OnRiver, colony.map.mapData.isRiver, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileRivers, string.Join(",", colony.map.mapData.surroundingPlanetTileRivers.Select(i => i.ToString()).ToArray()), 0));
	}

	public class PersistenceColony {
		public string path;

		public Sprite lastSaveImage;

		public string lastSaveDateTime;

		public string name;
		public Vector2 planetPosition;
		public int seed;
		public int size;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>();
		public List<int> surroundingPlanetTileHeights = new List<int>();
		public bool onRiver;
		public List<int> surroundingPlanetTileRivers = new List<int>();

		public PersistenceColony(string path) {
			this.path = path;
		}

		public void SetLastSaveImage(Sprite lastSaveImage) {
			this.lastSaveImage = lastSaveImage;
		}
	}

	public List<PersistenceColony> GetPersistenceColonies() {
		List<PersistenceColony> persistenceColonies = new List<PersistenceColony>();
		string coloniesPath = GameManager.planetM.planet.directory + "/Colonies";
		if (Directory.Exists(coloniesPath)) {
			foreach (string colonyDirectoryPath in Directory.GetDirectories(coloniesPath)) {
				PersistenceColony persistenceColony = LoadColony(colonyDirectoryPath + "/colony.snowship");

				string savesDirectoryPath = colonyDirectoryPath + "/Saves";
				List<string> saveDirectories = Directory.GetDirectories(savesDirectoryPath).OrderByDescending(sd => sd).ToList();
				if (saveDirectories.Count > 0) {
					string screenshotPath = Directory.GetFiles(saveDirectories[0]).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
					if (screenshotPath != null) {
						WWW www = CreateWWWForFile(screenshotPath);
						if (www != null) {
							Sprite saveImage = LoadSaveImage(www);
							if (saveImage != null) {
								persistenceColony.lastSaveImage = saveImage;
							}
						}
					}
				}
				persistenceColonies.Add(persistenceColony);
			}
		}
		persistenceColonies = persistenceColonies.OrderByDescending(pc => pc.path).ToList();
		return persistenceColonies;
	}

	public PersistenceColony LoadColony(string path) {

		PersistenceColony persistenceColony = new PersistenceColony(path);

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			ColonyProperty key = (ColonyProperty)Enum.Parse(typeof(ColonyProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case ColonyProperty.LastSaveDateTime:
					persistenceColony.lastSaveDateTime = (string)value;
					break;
				case ColonyProperty.Name:
					persistenceColony.name = (string)value;
					break;
				case ColonyProperty.PlanetPosition:
					persistenceColony.planetPosition = new Vector2(float.Parse(((string)value).Split(',')[0]), float.Parse(((string)value).Split(',')[1]));
					break;
				case ColonyProperty.Seed:
					persistenceColony.seed = int.Parse((string)value);
					break;
				case ColonyProperty.Size:
					persistenceColony.size = int.Parse((string)value);
					break;
				case ColonyProperty.AverageTemperature:
					persistenceColony.averageTemperature = float.Parse((string)value);
					break;
				case ColonyProperty.AveragePrecipitation:
					persistenceColony.averagePrecipitation = float.Parse((string)value);
					break;
				case ColonyProperty.TerrainTypeHeights:
					foreach (KeyValuePair<string, object> terrainTypeHeightProperty in (List<KeyValuePair<string, object>>)value) {
						TileManager.TileTypes terrainTypeHeightPropertyKey = (TileManager.TileTypes)Enum.Parse(typeof(TileManager.TileTypes), terrainTypeHeightProperty.Key);
						persistenceColony.terrainTypeHeights.Add(terrainTypeHeightPropertyKey, float.Parse((string)terrainTypeHeightProperty.Value));
					}
					break;
				case ColonyProperty.SurroundingPlanetTileHeights:
					foreach (string height in ((string)value).Split(',')) {
						persistenceColony.surroundingPlanetTileHeights.Add(int.Parse(height));
					}
					break;
				case ColonyProperty.OnRiver:
					persistenceColony.onRiver = bool.Parse((string)value);
					break;
				case ColonyProperty.SurroundingPlanetTileRivers:
					foreach (string riverIndex in ((string)value).Split(',')) {
						persistenceColony.surroundingPlanetTileRivers.Add(int.Parse(riverIndex));
					}
					break;
				default:
					Debug.LogError("Unknown colony property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceColony;
	}

	public ColonyManager.Colony ApplyLoadedColony(PersistenceColony persistenceColony) {
		ColonyManager.Colony colony = GameManager.colonyM.CreateColony(
			persistenceColony.name,
			persistenceColony.planetPosition,
			persistenceColony.seed,
			persistenceColony.size,
			persistenceColony.averageTemperature,
			persistenceColony.averagePrecipitation,
			persistenceColony.terrainTypeHeights,
			persistenceColony.surroundingPlanetTileHeights,
			persistenceColony.onRiver,
			persistenceColony.surroundingPlanetTileRivers
		);
		colony.SetLastSaveDateTime(persistenceColony.lastSaveDateTime);
		colony.SetDirectory(Directory.GetParent(persistenceColony.path).FullName);

		GameManager.colonyM.SetColony(colony);

		return colony;
	}

	public enum SaveProperty {
		SaveDateTime
	}

	public void SaveSave(StreamWriter file, string saveDateTime) {
		file.WriteLine(CreateKeyValueString(SaveProperty.SaveDateTime, saveDateTime, 0));
	}

	public class PersistenceSave {

		public string path;

		public Sprite image;

		public string saveDateTime;

		public bool loadable;

		public PersistenceSave(string path) {
			this.path = path;

			loadable = true;
		}
	}

	public List<PersistenceSave> GetPersistenceSaves() {
		List<PersistenceSave> persistenceSaves = new List<PersistenceSave>();
		string savesPath = GameManager.colonyM.colony.directory + "/Saves";
		if (Directory.Exists(savesPath)) {
			foreach (string saveDirectoryPath in Directory.GetDirectories(savesPath)) {
				PersistenceSave persistenceSave = LoadSave(saveDirectoryPath + "/save.snowship");

				string screenshotPath = Directory.GetFiles(saveDirectoryPath).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
				if (screenshotPath != null) {
					WWW www = CreateWWWForFile(screenshotPath);
					if (www != null) {
						Sprite saveImage = LoadSaveImage(www);
						if (saveImage != null) {
							persistenceSave.image = saveImage;
						}
					}
				}
				persistenceSaves.Add(persistenceSave);
			}
		}
		persistenceSaves = persistenceSaves.OrderByDescending(ps => ps.path).ToList();
		return persistenceSaves;
	}

	public PersistenceSave LoadSave(string path) {
		PersistenceSave persistenceSave = new PersistenceSave(path);

		List<KeyValuePair<string, object>> properties = null;
		try {
			properties = GetKeyValuePairsFromFile(path);
		} catch (Exception e) {
			Debug.LogError(e.ToString());
			persistenceSave.loadable = false;
			return persistenceSave;
		}
		foreach (KeyValuePair<string, object> property in properties) {
			SaveProperty key = (SaveProperty)Enum.Parse(typeof(SaveProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case SaveProperty.SaveDateTime:
					persistenceSave.saveDateTime = (string)value;
					break;
				default:
					Debug.LogError("Unknown save property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceSave;
	}

	public IEnumerator ApplyLoadedSave(PersistenceSave persistenceSave) {
		if (persistenceSave != null) {
			GameManager.tileM.mapState = TileManager.MapState.Generating;

			GameManager.uiM.SetMainMenuActive(false);
			GameManager.uiM.SetLoadingScreenActive(true);
			GameManager.uiM.SetGameUIActive(false);

			GameManager.colonyM.LoadColony(GameManager.colonyM.colony, false);
			//while (!GameManager.colonyM.colony.map.created) {
			//	yield return null;
			//}

			string saveDirectoryPath = Directory.GetParent(persistenceSave.path).FullName;

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Camera"); yield return null;
			LoadCamera(saveDirectoryPath + "/camera.snowship");
			GameManager.uiM.UpdateLoadingStateText("Persistence", "Time"); yield return null;
			LoadTime(saveDirectoryPath + "/time.snowship");
			GameManager.uiM.UpdateLoadingStateText("Persistence", "Resources"); yield return null;
			LoadResources(saveDirectoryPath + "/resources.snowship");

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Generating Original Map"); yield return null;

			GameManager.colonyM.colony.map = new TileManager.Map() { mapData = GameManager.colonyM.colony.mapData };
			TileManager.Map map = GameManager.colonyM.colony.map;

			List<PersistenceTile> persistenceTiles = LoadOriginalTiles(GameManager.colonyM.colony.directory + "/Map/tiles.snowship");

			if (persistenceTiles.Count != Mathf.Pow(map.mapData.mapSize, 2)) {

			}

			for (int y = 0; y < map.mapData.mapSize; y++) {
				List<TileManager.Tile> innerTiles = new List<TileManager.Tile>();
				for (int x = 0; x < map.mapData.mapSize; x++) {
					PersistenceTile persistenceTile = persistenceTiles[y * map.mapData.mapSize + x];

					TileManager.Tile tile = new TileManager.Tile(map, new Vector2(x, y), persistenceTile.tileHeight);
					map.tiles.Add(tile);
					innerTiles.Add(tile);

					tile.SetTileType(persistenceTile.tileType, false, false, false, false);
					if (persistenceTile.plantGroup != null) {
						tile.SetPlant(false, new ResourceManager.Plant(persistenceTile.plantGroup, tile, false, persistenceTile.plantSmall, map.smallPlants, persistenceTile.plantHarvestResource != null, persistenceTile.plantHarvestResource));
					}
				}
				map.sortedTiles.Add(innerTiles);
			}

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Applying Changes to Map"); yield return null;
			LoadModifiedTiles(saveDirectoryPath + "/tiles.snowship");

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Setting Backend Data"); yield return null;
			map.SetSurroundingTiles();
			map.SetMapEdgeTiles();
			map.SetSortedMapEdgeTiles();
			map.SetTileRegions(false);

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Determining Drainage Basins"); yield return null;
			map.DetermineDrainageBasins();

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Setting Biomes"); yield return null;
			map.CalculateTemperature();
			map.CalculatePrecipitation();
			map.SetBiomes(false);

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Setting Backend Data"); yield return null;
			map.CreateRegionBlocks();
			map.SetRoofs();

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Calculating Lighting"); yield return null;
			map.DetermineShadowDirectionsAtHour();
			map.DetermineShadowTiles(map.tiles, false);
			map.SetTileBrightness(GameManager.timeM.tileBrightnessTime);
			map.DetermineVisibleRegionBlocks();

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Validating"); yield return null;
			map.Bitmasking(map.tiles);

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Setting Consistent Appearance"); yield return null;
			for (int i = 0; i < map.tiles.Count; i++) {
				TileManager.Tile tile = map.tiles[i];
				PersistenceTile persistenceTile = persistenceTiles[i];

				if (tile.plant != null) {
					Sprite plantSprite = null;
					if (tile.plant.small) {
						plantSprite = persistenceTile.plantGroup.smallSprites.Find(s => s.name == persistenceTile.plantSpriteName);
					} else {
						plantSprite = persistenceTile.plantGroup.fullSprites.Find(s => s.name == persistenceTile.plantSpriteName);
					}
					if (plantSprite != null) {
						tile.plant.obj.GetComponent<SpriteRenderer>().sprite = plantSprite;
					}
				}

				Sprite tileSprite = null;
				tileSprite = tile.tileType.baseSprites.Find(s => s.name == persistenceTile.tileSpriteName);
				if (tileSprite != null) {
					tile.sr.sprite = tileSprite;
					continue;
				}

				tileSprite = tile.tileType.bitmaskSprites.Find(s => s.name == persistenceTile.tileSpriteName);
				if (tileSprite != null) {
					tile.sr.sprite = tileSprite;
					continue;
				}

				tileSprite = tile.tileType.riverSprites.Find(s => s.name == persistenceTile.tileSpriteName);
				if (tileSprite != null) {
					tile.sr.sprite = tileSprite;
					continue;
				}
			}

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Objects"); yield return null;
			LoadObjects(saveDirectoryPath + "/objects.snowship");

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Caravans"); yield return null;
			LoadCaravans(saveDirectoryPath + "/caravans.snowship");

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Jobs"); yield return null;
			LoadJobs(saveDirectoryPath + "/jobs.snowship");

			GameManager.uiM.UpdateLoadingStateText("Persistence", "Colonists"); yield return null;
			LoadColonists(saveDirectoryPath + "/colonists.snowship");

			GameManager.tileM.mapState = TileManager.MapState.Generated;
			GameManager.uiM.SetGameUIActive(true);
			GameManager.uiM.SetLoadingScreenActive(false);
		} else {
			Debug.LogError("Unable to load a save without a save being selected.");
		}
	}

	public enum TileProperty {
		Tile, Height, TileType, Sprite, Plant, Roof, Dug
	}

	public enum PlantProperty {
		Type, Sprite, Small, GrowthProgress, HarvestResource, Integrity
	}

	public class PersistenceTile {
		public float tileHeight;
		public TileManager.TileType tileType;
		public string tileSpriteName;

		public ResourceManager.PlantGroup plantGroup;
		public string plantSpriteName;
		public bool plantSmall;
		public float plantGrowthProgress;
		public ResourceManager.Resource plantHarvestResource;
		public float plantIntegrity;

		public PersistenceTile(float tileHeight, TileManager.TileType tileType, string tileSpriteName, ResourceManager.PlantGroup plantGroup, string plantSpriteName, bool plantSmall, float plantGrowthProgress, ResourceManager.Resource plantHarvestResource, float plantIntegrity) {
			this.tileHeight = tileHeight;
			this.tileType = tileType;
			this.tileSpriteName = tileSpriteName;

			this.plantGroup = plantGroup;
			this.plantSpriteName = plantSpriteName;
			this.plantSmall = plantSmall;
			this.plantGrowthProgress = plantGrowthProgress;
			this.plantHarvestResource = plantHarvestResource;
			this.plantIntegrity = plantIntegrity;
		}
	}

	public void SaveOriginalTiles(StreamWriter file) {
		foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
			file.WriteLine(CreateKeyValueString(TileProperty.Tile, string.Empty, 0));

			file.WriteLine(CreateKeyValueString(TileProperty.Height, tile.height, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.TileType, tile.tileType.type, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Sprite, tile.sr.sprite.name, 1));

			if (tile.plant != null) {
				file.WriteLine(CreateKeyValueString(TileProperty.Plant, string.Empty, 1));

				file.WriteLine(CreateKeyValueString(PlantProperty.Type, tile.plant.group.type, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.Small, tile.plant.small, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.GrowthProgress, tile.plant.growthProgress, 2));
				if (tile.plant.harvestResource != null) {
					file.WriteLine(CreateKeyValueString(PlantProperty.HarvestResource, tile.plant.harvestResource.type, 2));
				}
				file.WriteLine(CreateKeyValueString(PlantProperty.Integrity, tile.plant.integrity, 2));
			}
		}
	}

	public List<PersistenceTile> LoadOriginalTiles(string path) {
		List<PersistenceTile> originalTiles = new List<PersistenceTile>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			TileProperty key = (TileProperty)Enum.Parse(typeof(TileProperty), property.Key);
			switch (key) {
				case TileProperty.Tile:
					float tileHeight = -1;
					TileManager.TileType tileType = null;
					string tileSpriteName = null;

					ResourceManager.PlantGroup plantGroup = null;
					string plantSpriteName = null;
					bool plantSmall = false;
					float plantGrowthProgress = -1;
					ResourceManager.Resource plantHarvestResource = null;
					float plantIntegrity = -1;

					foreach (KeyValuePair<string, object> tileProperty in (List<KeyValuePair<string, object>>)property.Value) {
						TileProperty tilePropertyKey = (TileProperty)Enum.Parse(typeof(TileProperty), tileProperty.Key);
						switch (tilePropertyKey) {
							case TileProperty.Height:
								tileHeight = float.Parse((string)tileProperty.Value);
								break;
							case TileProperty.TileType:
								tileType = GameManager.tileM.GetTileTypeByEnum((TileManager.TileTypes)Enum.Parse(typeof(TileManager.TileTypes), (string)tileProperty.Value));
								break;
							case TileProperty.Sprite:
								tileSpriteName = (string)tileProperty.Value;
								break;
							case TileProperty.Plant:
								foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)tileProperty.Value) {
									PlantProperty plantPropertyKey = (PlantProperty)Enum.Parse(typeof(PlantProperty), plantProperty.Key);
									switch (plantPropertyKey) {
										case PlantProperty.Type:
											plantGroup = GameManager.resourceM.GetPlantGroupByEnum((ResourceManager.PlantGroupsEnum)Enum.Parse(typeof(ResourceManager.PlantGroupsEnum), (string)plantProperty.Value));
											break;
										case PlantProperty.Sprite:
											plantSpriteName = (string)plantProperty.Value;
											break;
										case PlantProperty.Small:
											plantSmall = bool.Parse((string)plantProperty.Value);
											break;
										case PlantProperty.GrowthProgress:
											plantGrowthProgress = float.Parse((string)plantProperty.Value);
											break;
										case PlantProperty.HarvestResource:
											plantHarvestResource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), (string)plantProperty.Value));
											break;
										case PlantProperty.Integrity:
											plantIntegrity = float.Parse((string)plantProperty.Value);
											break;
										default:
											Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown tile property: " + tileProperty.Key + " " + tileProperty.Value);
								break;
						}
					}

					originalTiles.Add(new PersistenceTile(tileHeight, tileType, tileSpriteName, plantGroup, plantSpriteName, plantSmall, plantGrowthProgress, plantHarvestResource, plantIntegrity));
					break;
				default:
					Debug.LogError("Unknown tile property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return originalTiles;
	}

	public enum RiverProperty {
		River, LargeRiver, StartTilePosition, CentreTilePosition, EndTilePosition, ExpandRadius, IgnoreStone, TilePositions
	}

	public void SaveRivers(StreamWriter file) {
		foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.rivers) {
			WriteRiverLines(file, river, 0, RiverProperty.River);
		}
		foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.largeRivers) {
			WriteRiverLines(file, river, 0, RiverProperty.LargeRiver);
		}
	}

	private void WriteRiverLines(StreamWriter file, TileManager.Map.River river, int startLevel, RiverProperty riverType) {
		file.WriteLine(CreateKeyValueString(riverType, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(RiverProperty.StartTilePosition, FormatVector2ToString(river.startTile.obj.transform.position), startLevel + 1));
		if (river.centreTile != null) {
			file.WriteLine(CreateKeyValueString(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.obj.transform.position), startLevel + 1));
		}
		file.WriteLine(CreateKeyValueString(RiverProperty.EndTilePosition, FormatVector2ToString(river.endTile.obj.transform.position), startLevel + 1));

		file.WriteLine(CreateKeyValueString(RiverProperty.ExpandRadius, river.expandRadius, startLevel + 1));
		file.WriteLine(CreateKeyValueString(RiverProperty.IgnoreStone, river.ignoreStone, startLevel + 1));
		file.WriteLine(CreateKeyValueString(RiverProperty.TilePositions, string.Join(";", river.tiles.Select(t => FormatVector2ToString(t.obj.transform.position)).ToArray()), startLevel + 1));
	}

	public void SaveModifiedTiles(StreamWriter file, List<PersistenceTile> originalTiles) {
		for (int i = 0; i < GameManager.colonyM.colony.map.tiles.Count; i++) {
			TileManager.Tile tile = GameManager.colonyM.colony.map.tiles[i];
			PersistenceTile originalTile = originalTiles[i];

			Dictionary<TileProperty, string> tileDifferences = new Dictionary<TileProperty, string>();
			Dictionary<PlantProperty, string> plantDifferences = new Dictionary<PlantProperty, string>();

			if (tile.height != originalTile.tileHeight) {
				tileDifferences.Add(TileProperty.Height, tile.height.ToString());
			}
			if (tile.tileType != originalTile.tileType) {
				tileDifferences.Add(TileProperty.TileType, tile.tileType.ToString());
			}
			if (tile.sr.sprite.name != originalTile.tileSpriteName) {
				tileDifferences.Add(TileProperty.Sprite, tile.sr.sprite.name);
			}

			if (originalTile.plantGroup == null) {
				if (tile.plant != null) { // No original plant, plant was added
					tileDifferences.Add(TileProperty.Plant, string.Empty);

					plantDifferences.Add(PlantProperty.Type, tile.plant.group.type.ToString());
					plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
					plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
					plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
					if (tile.plant.harvestResource != null) {
						plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
					}
					plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
				}
			} else {
				if (tile.plant == null) { // Original plant, plant was removed
					tileDifferences.Add(TileProperty.Plant, string.Empty);

					plantDifferences.Add(PlantProperty.Type, "None");
				} else { // Plant has remained, properties potentially changed
					tileDifferences.Add(TileProperty.Plant, string.Empty);

					if (tile.plant.group.type != originalTile.plantGroup.type) {
						plantDifferences.Add(PlantProperty.Type, tile.plant.group.type.ToString());
					}
					if (tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name != originalTile.plantSpriteName) {
						plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
					}
					if (tile.plant.small != originalTile.plantSmall) {
						plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
					}
					if (tile.plant.growthProgress != originalTile.plantGrowthProgress) {
						plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
					}
					if (tile.plant.harvestResource != originalTile.plantHarvestResource) {
						if (tile.plant.harvestResource == null) {
							plantDifferences.Add(PlantProperty.HarvestResource, "None");
						} else {
							plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
						}
					}
					if (tile.plant.integrity != originalTile.plantIntegrity) {
						plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
					}
				}
			}
		}
	}

	public void LoadModifiedTiles(string path) {
		Debug.LogWarning("Load Modified Tiles");
	}

	public enum CameraProperty {
		Position, Zoom
	}

	public void SaveCamera(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(CameraProperty.Position, FormatVector2ToString(GameManager.cameraM.GetCameraPosition()), 0));
		file.WriteLine(CreateKeyValueString(CameraProperty.Zoom, GameManager.cameraM.GetCameraZoom(), 0));
	}

	public void LoadCamera(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			CameraProperty key = (CameraProperty)Enum.Parse(typeof(CameraProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case CameraProperty.Position:
					GameManager.cameraM.SetCameraPosition(new Vector2(float.Parse(((string)value).Split(',')[0]), float.Parse(((string)value).Split(',')[1])));
					break;
				case CameraProperty.Zoom:
					GameManager.cameraM.SetCameraZoom(float.Parse((string)value));
					break;
				default:
					Debug.LogError("Unknown camera property: " + property.Key + " " + property.Value);
					break;
			}
		}
	}

	public enum ResourceAmountProperty {
		ResourceAmount, Type, Amount
	}

	public enum ReservedResourceAmountsProperty {
		ReservedResourceAmounts, HumanName
	}

	public enum LifeProperty {
		Life, Health, Gender, Position, PreviousPosition, PathEndPosition
	}

	private void WriteLifeLines(StreamWriter file, LifeManager.Life life, int startLevel) {
		file.WriteLine(CreateKeyValueString(LifeProperty.Health, life.health, startLevel));
		file.WriteLine(CreateKeyValueString(LifeProperty.Gender, life.gender, startLevel));
		file.WriteLine(CreateKeyValueString(LifeProperty.Position, FormatVector2ToString(life.obj.transform.position), startLevel));
		file.WriteLine(CreateKeyValueString(LifeProperty.PreviousPosition, FormatVector2ToString(life.previousPosition), startLevel));
		if (life.path.Count > 0) {
			file.WriteLine(CreateKeyValueString(LifeProperty.PathEndPosition, FormatVector2ToString(life.path[life.path.Count - 1].obj.transform.position), startLevel));
		}
	}

	public enum HumanProperty {
		Human, Name, SkinIndex, HairIndex, Clothes, Inventory
	}

	private void WriteHumanLines(StreamWriter file, HumanManager.Human human, int startLevel) {
		file.WriteLine(CreateKeyValueString(HumanProperty.Name, human.name, startLevel));
		file.WriteLine(CreateKeyValueString(HumanProperty.SkinIndex, human.bodyIndices[HumanManager.Human.Appearance.Skin], startLevel));
		file.WriteLine(CreateKeyValueString(HumanProperty.HairIndex, human.bodyIndices[HumanManager.Human.Appearance.Hair], startLevel));

		if (human.clothes.Any(kvp => kvp.Value != null)) {
			file.WriteLine(CreateKeyValueString(HumanProperty.Clothes, string.Empty, startLevel));
			foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothing in human.clothes) {
				if (appearanceToClothing.Value != null) {
					file.WriteLine(CreateKeyValueString(appearanceToClothing.Key, appearanceToClothing.Value.prefab.appearance + ":" + appearanceToClothing.Value.colour, startLevel + 1));
				}
			}
		}

		WriteInventoryLines(file, human.inventory, startLevel);
	}

	public enum TraderProperty {
		Trader
	}

	public enum CaravanProperty {
		Caravan, Type, Location, Traders, Inventory, ResourcesToTrade, ConfirmedResourcesToTrade
	}

	public enum TradeResourceAmountProperty {
		TradeResourceAmount, Type, CaravanAmount, TradeAmount, Price
	}

	public enum PriceProperty {
		Gold, Silver, Bronze
	}

	public enum ConfirmedTradeResourceAmountProperty {
		ConfirmedTradeResourceAmount, Type, TradeAmount, AmountRemaining
	}

	public void SaveCaravans(StreamWriter file) {
		Debug.LogWarning("Save Caravans");
	}

	public void LoadCaravans(string path) {
		Debug.LogWarning("Load Caravans");
	}

	public enum ColonistProperty {
		Colonist, Profession, OldProfession, PlayerMoved, Job, StoredJob, Skills, Traits, Needs, BaseHappiness, EffectiveHappiness, HappinessModifiers
	}

	public enum SkillProperty {
		Skill, Type, Level, NextLevelExperience, CurrentExperience
	}

	public enum TraitProperty {
		Trait, Type
	}

	public enum NeedProperty {
		Need, Type, Value
	}

	public enum HappinessModifierProperty {
		HappinessModifier, Type, TimeRemaining
	}

	public void SaveColonists(StreamWriter file) {
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			file.WriteLine(CreateKeyValueString(ColonistProperty.Colonist, string.Empty, 0));

			WriteLifeLines(file, colonist, 1);

			WriteHumanLines(file, colonist, 1);

			file.WriteLine(CreateKeyValueString(ColonistProperty.Profession, colonist.profession.name, 1));
			file.WriteLine(CreateKeyValueString(ColonistProperty.OldProfession, colonist.oldProfession.name, 1));

			file.WriteLine(CreateKeyValueString(ColonistProperty.PlayerMoved, colonist.playerMoved, 1));

			if (colonist.job != null) {
				WriteJobLines(file, colonist.job, JobProperty.Job, 1);
			}
			if (colonist.storedJob != null) {
				WriteJobLines(file, colonist.storedJob, JobProperty.StoredJob, 1);
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.Skills, string.Empty, 1));
			foreach (ColonistManager.SkillInstance skill in colonist.skills) {
				file.WriteLine(CreateKeyValueString(SkillProperty.Skill, string.Empty, 2));

				file.WriteLine(CreateKeyValueString(SkillProperty.Type, skill.prefab.type, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.Level, skill.level, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.NextLevelExperience, skill.nextLevelExperience, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.CurrentExperience, skill.currentExperience, 3));
			}

			if (colonist.traits.Count > 0) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.Traits, string.Empty, 1));
				foreach (ColonistManager.TraitInstance trait in colonist.traits) {
					file.WriteLine(CreateKeyValueString(TraitProperty.Trait, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(TraitProperty.Type, trait.prefab.type, 3));
				}
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.Needs, string.Empty, 1));
			foreach (ColonistManager.NeedInstance need in colonist.needs) {
				file.WriteLine(CreateKeyValueString(NeedProperty.Need, string.Empty, 2));

				file.WriteLine(CreateKeyValueString(NeedProperty.Type, need.prefab.type, 3));
				file.WriteLine(CreateKeyValueString(NeedProperty.Value, need.GetValue(), 3));
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.BaseHappiness, colonist.baseHappiness, 1));
			file.WriteLine(CreateKeyValueString(ColonistProperty.EffectiveHappiness, colonist.effectiveHappiness, 1));

			if (colonist.happinessModifiers.Count > 0) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.HappinessModifiers, string.Empty, 1));
				foreach (ColonistManager.HappinessModifierInstance happinessModifier in colonist.happinessModifiers) {
					file.WriteLine(CreateKeyValueString(HappinessModifierProperty.HappinessModifier, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(HappinessModifierProperty.Type, happinessModifier.prefab.type, 3));
					file.WriteLine(CreateKeyValueString(HappinessModifierProperty.TimeRemaining, happinessModifier.timer, 3));
				}
			}
		}
	}

	public void LoadColonists(string path) {
		Debug.LogWarning("Load Colonists");
	}

	public enum JobProperty {
		Job, StoredJob, Type, Position, RotationIndex, Priority, Started, Progress, ColonistBuildTime, ResourcesToBuild, ColonistResources, ContainerPickups, Plant, CreateResource, ActiveTileObject
	}

	public enum ContainerPickupProperty {
		ContainerPickup, Position
	}

	public void SaveJobs(StreamWriter file) {
		foreach (JobManager.Job job in GameManager.jobM.jobs) {
			WriteJobLines(file, job, JobProperty.Job, 0);
		}
	}

	public void LoadJobs(string path) {
		Debug.LogWarning("Load Jobs");
	}

	private void WriteJobLines(StreamWriter file, JobManager.Job job, JobProperty jobType, int startLevel) {
		file.WriteLine(CreateKeyValueString(JobProperty.Job, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(JobProperty.Type, job.prefab.type, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Position, FormatVector2ToString(job.tile.obj.transform.position), startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.RotationIndex, job.rotationIndex, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Priority, job.priority, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Started, job.started, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Progress, job.jobProgress, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.ColonistBuildTime, job.colonistBuildTime, startLevel + 1));

		if (job.resourcesToBuild != null) {
			file.WriteLine(CreateKeyValueString(JobProperty.ResourcesToBuild, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}

		if (job.colonistResources != null) {
			file.WriteLine(CreateKeyValueString(JobProperty.ColonistResources, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in job.colonistResources) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}

		if (job.containerPickups != null) {
			file.WriteLine(CreateKeyValueString(JobProperty.ContainerPickups, string.Empty, startLevel + 1));
			foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
				file.WriteLine(CreateKeyValueString(ContainerPickupProperty.ContainerPickup, string.Empty, startLevel + 2));

				file.WriteLine(CreateKeyValueString(ContainerPickupProperty.Position, FormatVector2ToString(containerPickup.container.obj.transform.position), startLevel + 3));

				foreach (ResourceManager.ResourceAmount resourceAmount in containerPickup.resourcesToPickup) {
					WriteResourceAmountLines(file, resourceAmount, startLevel + 3);
				}
			}
		}
	}

	public enum TileObjectProperty {
		Object, Type, Position, RotationIndex, Integrity, Active
	}

	public enum ContainerProperty {
		Inventory
	}

	public enum ManufacturingTileObjectProperty {
		CreateResource, FuelResource
	}

	public enum FarmProperty {
		SeedType, GrowTimer, MaxGrowthTime
	}

	public void SaveObjects(StreamWriter file) {
		foreach (List<ResourceManager.TileObjectInstance> instances in GameManager.resourceM.tileObjectInstances.Values) {
			foreach (ResourceManager.TileObjectInstance instance in instances) {
				file.WriteLine(CreateKeyValueString(TileObjectProperty.Object, string.Empty, 0));

				file.WriteLine(CreateKeyValueString(TileObjectProperty.Type, instance.prefab.type, 1));
				file.WriteLine(CreateKeyValueString(TileObjectProperty.Position, FormatVector2ToString(instance.obj.transform.position), 1));
				file.WriteLine(CreateKeyValueString(TileObjectProperty.RotationIndex, instance.rotationIndex, 1));
				file.WriteLine(CreateKeyValueString(TileObjectProperty.Integrity, instance.integrity, 1));
				file.WriteLine(CreateKeyValueString(TileObjectProperty.Active, instance.active, 1));

				if (instance is ResourceManager.Container) {
					ResourceManager.Container container = (ResourceManager.Container)instance;
					WriteInventoryLines(file, container.inventory, 1);
				} else if (instance is ResourceManager.ManufacturingTileObject) {
					ResourceManager.ManufacturingTileObject manufacturingTileObject = (ResourceManager.ManufacturingTileObject)instance;
					if (manufacturingTileObject.createResource != null) {
						file.WriteLine(CreateKeyValueString(ManufacturingTileObjectProperty.CreateResource, manufacturingTileObject.createResource.type, 1));
					}
					if (manufacturingTileObject.fuelResource != null) {
						file.WriteLine(CreateKeyValueString(ManufacturingTileObjectProperty.FuelResource, manufacturingTileObject.fuelResource.type, 1));
					}
				} else if (instance is ResourceManager.Farm) {
					ResourceManager.Farm farm = (ResourceManager.Farm)instance;
					file.WriteLine(CreateKeyValueString(FarmProperty.SeedType, farm.seedType, 1));
					file.WriteLine(CreateKeyValueString(FarmProperty.GrowTimer, farm.growTimer, 1));
					file.WriteLine(CreateKeyValueString(FarmProperty.MaxGrowthTime, farm.maxGrowthTime, 1));
				}
			}
		}
	}

	public void LoadObjects(string path) {
		Debug.LogWarning("Load Objects");
	}

	private void WriteInventoryLines(StreamWriter file, ResourceManager.Inventory inventory, int startLevel) {
		file.WriteLine(CreateKeyValueString(ContainerProperty.Inventory, string.Empty, startLevel));
		foreach (ResourceManager.ResourceAmount resourceAmount in inventory.resources) {
			WriteResourceAmountLines(file, resourceAmount, startLevel + 1);
		}
		foreach (ResourceManager.ReservedResources reservedResources in inventory.reservedResources) {
			file.WriteLine(CreateKeyValueString(ReservedResourceAmountsProperty.ReservedResourceAmounts, string.Empty, startLevel + 1));
			file.WriteLine(CreateKeyValueString(ReservedResourceAmountsProperty.HumanName, reservedResources.human.name, startLevel + 2));
			foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}
	}

	private void WriteResourceAmountLines(StreamWriter file, ResourceManager.ResourceAmount resourceAmount, int startLevel) {
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.ResourceAmount, string.Empty, startLevel));
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.Type, resourceAmount.resource.type, startLevel + 1));
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.Amount, resourceAmount.amount, startLevel + 1));
	}

	public enum ResourceProperty {
		Resource, Type, DesiredAmount
	}

	public void SaveResources(StreamWriter file) {
		foreach (ResourceManager.Resource resource in GameManager.resourceM.resources) {
			file.WriteLine(CreateKeyValueString(ResourceProperty.Resource, string.Empty, 0));

			file.WriteLine(CreateKeyValueString(ResourceProperty.Type, resource.type, 1));
			file.WriteLine(CreateKeyValueString(ResourceProperty.DesiredAmount, resource.desiredAmount, 1));
		}
	}

	public void LoadResources(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			ResourceProperty key = (ResourceProperty)Enum.Parse(typeof(ResourceProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case ResourceProperty.Resource:

					ResourceManager.Resource resource = null;
					int desiredAmount = 0;

					foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)property.Value) {
						ResourceProperty resourcePropertyKey = (ResourceProperty)Enum.Parse(typeof(ResourceProperty), resourceProperty.Key);
						switch (resourcePropertyKey) {
							case ResourceProperty.Type:
								resource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), (string)resourceProperty.Value));
								break;
							case ResourceProperty.DesiredAmount:
								desiredAmount = int.Parse((string)resourceProperty.Value);
								break;
							default:
								Debug.LogError("Unknown resource property: " + resourceProperty.Key + " " + resourceProperty.Value);
								break;
						}
					}

					resource.ChangeDesiredAmount(desiredAmount);

					break;
				default:
					Debug.LogError("Unknown resource property: " + property.Key + " " + property.Value);
					break;
			}
		}
	}

	public enum TimeProperty {
		Minute, Hour, Day, Month, Year
	}

	public void SaveTime(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(TimeProperty.Minute, GameManager.timeM.GetMinute(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Hour, GameManager.timeM.GetHour(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Day, GameManager.timeM.GetDay(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Month, GameManager.timeM.GetMonth(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Year, GameManager.timeM.GetYear(), 0));
	}

	public void LoadTime(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			TimeProperty key = (TimeProperty)Enum.Parse(typeof(TimeProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case TimeProperty.Minute:
					GameManager.timeM.SetMinute(int.Parse((string)value));
					break;
				case TimeProperty.Hour:
					GameManager.timeM.SetHour(int.Parse((string)value));
					break;
				case TimeProperty.Day:
					GameManager.timeM.SetDay(int.Parse((string)value));
					break;
				case TimeProperty.Month:
					GameManager.timeM.SetMonth(int.Parse((string)value));
					break;
				case TimeProperty.Year:
					GameManager.timeM.SetYear(int.Parse((string)value));
					break;
				default:
					Debug.LogError("Unknown time property: " + property.Key + " " + property.Value);
					break;
			}
		}
	}

	public void ContinueFromMostRecentSave() {
		Debug.LogWarning("Continue From Most Recent Save");
	}

	//// OLD SAVE GAME METHODS

	//public void SaveGame(string fileName) {
	//	string universesPath = GenerateUniversesPath();
	//	string dateTimeString = GenerateDateTimeString();

	//	string universePath = universesPath + "/Universe-" + dateTimeString;
	//	Directory.CreateDirectory(universePath);

	//	string planetsPath = universePath + "/Planets";
	//	Directory.CreateDirectory(planetsPath);

	//	string planetPath = planetsPath + "/PlanetName" + dateTimeString;
	//	Directory.CreateDirectory(planetPath);

	//	string coloniesPath = planetPath + "/Colonies";
	//	Directory.CreateDirectory(coloniesPath);

	//	string savesPath = universePath + "/Saves";
	//	Directory.CreateDirectory(savesPath);

	//	StreamWriter file = new StreamWriter(fileName);

	//	string versionData = "Version";
	//	versionData += "/SaveVersion," + saveVersion;
	//	versionData += "/GameVersion," + gameVersion;
	//	file.WriteLine(versionData);

	//	string saveFileFormatData = "Format";
	//	saveFileFormatData += "/Time,1";
	//	saveFileFormatData += "/Camera,1";
	//	//saveFileFormatData += "/PlanetTiles," + uiM.planetTiles.Count;
	//	saveFileFormatData += "/PlanetTiles,1";
	//	saveFileFormatData += "/MapTiles," + tileM.map.tiles.Count;
	//	saveFileFormatData += "/LargeRivers," + tileM.map.largeRivers.Count;
	//	saveFileFormatData += "/Rivers," + tileM.map.rivers.Count;
	//	saveFileFormatData += "/Resources,1";
	//	saveFileFormatData += "/ObjectInstances," + resourceM.tileObjectInstances.Values.Sum(objList => objList.Count);
	//	saveFileFormatData += "/MTOs," + resourceM.manufacturingTileObjectInstances.Count;
	//	saveFileFormatData += "/Farms," + resourceM.farms.Count;
	//	saveFileFormatData += "/Container," + resourceM.containers.Count;
	//	saveFileFormatData += "/Colonists," + colonistM.colonists.Count;
	//	saveFileFormatData += "/Clothes," + resourceM.clothingInstances.Count;
	//	saveFileFormatData += "/Jobs," + jobM.jobs.Count;
	//	file.WriteLine(saveFileFormatData);

	//	// Save the time data
	//	string timeData = "Time";
	//	timeData += "/Time," + timeM.tileBrightnessTime;
	//	timeData += "/Date," + timeM.GetDateString();
	//	file.WriteLine(timeData);

	//	// Save the camera data
	//	string cameraData = "Camera";
	//	cameraData += "/Position," + cameraM.cameraGO.transform.position.x + "," + cameraM.cameraGO.transform.position.y;
	//	cameraData += "/Zoom," + cameraM.cameraComponent.orthographicSize;
	//	file.WriteLine(cameraData);

	//	// Save the planet data
	//	string planetData = "PlanetTiles";
	//	planetData += "/PlanetSeed," + uiM.planet.mapData.mapSeed;
	//	planetData += "/PlanetSize," + uiM.planet.mapData.mapSize;
	//	planetData += "/PlanetDistance," + uiM.planetDistance;
	//	planetData += "/PlanetTempRange," + uiM.temperatureRange;
	//	planetData += "/RandomOffsets," + uiM.randomOffsets;
	//	planetData += "/PlanetWindDirection," + uiM.planet.mapData.primaryWindDirection;
	//	file.WriteLine(planetData);
	//	/*
	//	foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
	//		file.WriteLine(GetPlanetTileDataString(planetTile));
	//	}
	//	*/

	//			// Save the tile data
	//			string tileMapData = "Tiles";
	//	tileMapData += "/ColonyName," + uiM.colonyName;
	//	tileMapData += "/MapSeed," + tileM.map.mapData.mapSeed;
	//	tileMapData += "/MapSize," + tileM.map.mapData.mapSize;
	//	tileMapData += "/EquatorOffset," + tileM.map.mapData.equatorOffset;
	//	tileMapData += "/AverageTemperature," + tileM.map.mapData.averageTemperature;
	//	tileMapData += "/AveragePrecipitation," + tileM.map.mapData.averagePrecipitation;
	//	tileMapData += "/WindDirection," + tileM.map.mapData.primaryWindDirection;
	//	tileMapData += "/TerrainTypeHeights";
	//	foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightsKVP in tileM.map.mapData.terrainTypeHeights) {
	//		tileMapData += "," + terrainTypeHeightsKVP.Key + ":" + terrainTypeHeightsKVP.Value;
	//	}
	//	tileMapData += "/SurroundingPlanetTileHeightDirections";
	//	foreach (int surroundingPlanetTileHeightDirection in tileM.map.mapData.surroundingPlanetTileHeightDirections) {
	//		tileMapData += "," + surroundingPlanetTileHeightDirection;
	//	}
	//	tileMapData += "/River," + tileM.map.mapData.river;
	//	tileMapData += "/SurroundingPlanetTileRivers";
	//	foreach (int surroundingPlanetTileRiver in tileM.map.mapData.surroundingPlanetTileRivers) {
	//		tileMapData += "," + surroundingPlanetTileRiver;
	//	}
	//	tileMapData += "/PlanetTilePosition," + tileM.map.mapData.planetTilePosition.x + "," + tileM.map.mapData.planetTilePosition.y;
	//	file.WriteLine(tileMapData);

	//	foreach (TileManager.Tile tile in tileM.map.tiles) {
	//		file.WriteLine(GetTileDataString(tile));
	//	}

	//	// Save the large river data
	//	foreach (TileManager.Map.River largeRiver in tileM.map.largeRivers) {
	//		file.WriteLine(GetRiverDataString(largeRiver));
	//	}

	//	// Save the river data
	//	foreach (TileManager.Map.River river in tileM.map.rivers) {
	//		file.WriteLine(GetRiverDataString(river));
	//	}

	//	// Save the resource data
	//	string resourceData = "Resources";
	//	foreach (ResourceManager.Resource resource in resourceM.resources) {
	//		resourceData += "/" + resource.type + "," + resource.desiredAmount;
	//	}
	//	file.WriteLine(resourceData);

	//	// Save the object data
	//	foreach (KeyValuePair<ResourceManager.TileObjectPrefab, List<ResourceManager.TileObjectInstance>> objectInstanceKVP in resourceM.tileObjectInstances) {
	//		foreach (ResourceManager.TileObjectInstance objectInstance in objectInstanceKVP.Value) {
	//			file.WriteLine(GetObjectInstanceDataString(objectInstance));
	//		}
	//	}

	//	// Save the manufacturing tile object data
	//	foreach (ResourceManager.ManufacturingTileObject mto in resourceM.manufacturingTileObjectInstances) {
	//		file.WriteLine(GetManufacturingTileObjectDataString(mto));
	//	}

	//	// Save the farm object data
	//	foreach (ResourceManager.Farm farm in resourceM.farms) {
	//		file.WriteLine(GetFarmDataString(farm));
	//	}

	//	// Save the container data
	//	foreach (ResourceManager.Container container in resourceM.containers) {
	//		file.WriteLine(GetContainerDataString(container));
	//	}

	//	// Save the colonist data
	//	foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
	//		file.WriteLine(GetColonistDataString(colonist));
	//	}

	//	// Save the clothing data
	//	foreach (ResourceManager.ClothingInstance clothingInstance in resourceM.clothingInstances) {
	//		file.WriteLine(GetClothingDataString(clothingInstance));
	//	}

	//	// Save the job data
	//	foreach (JobManager.Job job in jobM.jobs) {
	//		file.WriteLine(GetJobDataString(job, false));
	//	}

	//	file.Close();

	//	StartCoroutine(CreateScreenshot(fileName));
	//}

	//public string GetPlanetTileDataString(UIManager.PlanetTile planetTile) {
	//	string planetTileData = string.Empty;
	//	planetTileData += planetTile.equatorOffset;
	//	planetTileData += "/" + planetTile.averageTemperature;
	//	planetTileData += "/" + planetTile.averagePrecipitation;
	//	planetTileData += "/";
	//	int terrainTypeHeightIndex = 0;
	//	foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightKVP in planetTile.terrainTypeHeights) {
	//		planetTileData += terrainTypeHeightKVP.Key + ":" + terrainTypeHeightKVP.Value + (terrainTypeHeightIndex + 1 == planetTile.terrainTypeHeights.Count ? "" : ",");
	//		terrainTypeHeightIndex += 1;
	//	}
	//	planetTileData += "/";
	//	int surroundingPlanetTileHeightDirectionIndex = 0;
	//	foreach (int surroundingPlanetTileHeightDirection in planetTile.surroundingPlanetTileHeightDirections) {
	//		planetTileData += surroundingPlanetTileHeightDirection + (surroundingPlanetTileHeightDirectionIndex + 1 == planetTile.surroundingPlanetTileHeightDirections.Count ? "" : ",");
	//		surroundingPlanetTileHeightDirectionIndex += 1;
	//	}
	//	planetTileData += "/" + planetTile.river;
	//	planetTileData += "/";
	//	int surroundingPlanetTileRiversIndex = 0;
	//	foreach (int surroundingPlanetTileRiver in planetTile.surroundingPlanetTileRivers) {
	//		planetTileData += surroundingPlanetTileRiver + (surroundingPlanetTileRiversIndex + 1 == planetTile.surroundingPlanetTileRivers.Count ? "" : ",");
	//		surroundingPlanetTileRiversIndex += 1;
	//	}
	//	return planetTileData;
	//}

	///*
	//	"xPos,yPos/height/temperature/precipitation/dugPreviously"

	//	Example: "35,45/0.25/23/0.4/false"
	//*/
	//public string GetTileDataString(TileManager.Tile tile) {
	//	string tileData = string.Empty;
	//	tileData += tile.tileType.type + "," + tile.sr.sprite.name;
	//	if (tile.plant != null) {
	//		tileData += "/" + tile.plant.group.type + "," + tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name + "," + tile.plant.small + "," + tile.plant.growthProgress + "," + (tile.plant.harvestResource != null ? tile.plant.harvestResource.type.ToString() : "None");
	//	} else {
	//		tileData += "/None";
	//	}
	//	tileData += "/";
	//	return tileData;
	//}

	///*
	//	"River/StartTilePos,x,y/EndTilePos,x,y/RiverTile,x,y/RiverTile,x,y/..."
	//*/
	//public string GetRiverDataString(TileManager.Map.River river) {
	//	string riverData = "River";
	//	riverData += "/StartTilePos," + river.startTile.obj.transform.position.x + "," + river.startTile.obj.transform.position.y;
	//	riverData += "/CentreTilePos," + (river.centreTile != null ? river.startTile.obj.transform.position.x + "," + river.startTile.obj.transform.position.y : "None");
	//	riverData += "/EndTilePos," + river.endTile.obj.transform.position.x + "," + river.endTile.obj.transform.position.y;
	//	riverData += "/ExpandRadius," + river.expandRadius;
	//	riverData += "/IgnoreStone," + river.ignoreStone;
	//	foreach (TileManager.Tile riverTile in river.tiles) {
	//		riverData += "/" + riverTile.obj.transform.position.x + "," + riverTile.obj.transform.position.y;
	//	}
	//	return riverData;
	//}

	///*
	//	"ObjectInstance/Position,x,y/PrefabType,prefabType/RotationIndex,rotationIndex"

	//	Example: "ObjectInstance/Position,35.0,45.0/PrefabType,WoodenChest/RotationIndex,0"
	//*/
	//public string GetObjectInstanceDataString(ResourceManager.TileObjectInstance objectInstance) {
	//	string objectInstanceData = "ObjectInstance";
	//	objectInstanceData += "/Position," + objectInstance.tile.obj.transform.position.x + "," + objectInstance.tile.obj.transform.position.y;
	//	objectInstanceData += "/PrefabType," + objectInstance.prefab.type;
	//	objectInstanceData += "/RotationIndex," + objectInstance.rotationIndex;
	//	objectInstanceData += "/Integrity," + objectInstance.integrity;
	//	return objectInstanceData;
	//}

	///*
	//	"MTO/Position,x,y/CreateResource,resourceType/FuelResource,resourceType"

	//	Example: "MTO/Position,35.0,45.0/CreateResource,Brick/FuelResource,Firewood"
	//*/
	//public string GetManufacturingTileObjectDataString(ResourceManager.ManufacturingTileObject mto) {
	//	string mtoData = "MTO";
	//	mtoData += "/Position," + mto.parentObject.tile.obj.transform.position.x + "," + mto.parentObject.tile.obj.transform.position.y;
	//	if (mto.createResource != null) {
	//		mtoData += "/CreateResource," + mto.createResource.type;
	//	} else {
	//		mtoData += "/None";
	//	}
	//	if (mto.fuelResource != null) {
	//		mtoData += "/FuelResource," + mto.fuelResource.type;
	//	} else {
	//		mtoData += "/None";
	//	}
	//	mtoData += "/Active," + mto.active;
	//	mtoData += "/";
	//	return mtoData;
	//}

	///*
	//	"Farm/Position,x,y/SeedType,seedType/GrowTimer,growTimer/MaxGrowthTime,maxGrowthTime"

	//	Example: "Farm/Position,35.0,45.0/SeedType,Potato/GrowTimer,100.51/MaxGrowthTime,1440"
	//*/
	//public string GetFarmDataString(ResourceManager.Farm farm) {
	//	string farmData = "Farm";
	//	farmData += "/Position," + farm.tile.obj.transform.position.x + "," + farm.tile.obj.transform.position.y;
	//	farmData += "/SeedType," + farm.seedType;
	//	farmData += "/GrowTimer," + farm.growTimer;
	//	farmData += "/MaxGrowthTime," + farm.maxGrowthTime;
	//	return farmData;
	//}

	//public string GetContainerDataString(ResourceManager.Container container) {
	//	string containerData = "Container";
	//	containerData += "/Position," + container.parentObject.tile.obj.transform.position.x + "," + container.parentObject.tile.obj.transform.position.y;
	//	containerData += "/InventoryMaxAmount," + container.inventory.maxAmount;
	//	/*
	//	 *	/InventoryResources:Count,InventoryResource:ResourceType:Amount,InventoryResource:ResourceType:Amount,...
	//	 *	
	//	 *	Split(',') -> ["InventoryResources" , "InventoryResource:ResourceType:Amount" , "..."]
	//	 *		Split[0](':') -> ["InventoryResources" , "Count"]
	//	 *		foreach skip 1 (i = 1 -> n):
	//	 *			Split[i](':') -> ["InventoryResource" , "ResourceType" , "Amount"]
	//	 */
	//	containerData += "/InventoryResources:" + container.inventory.resources.Count;
	//	foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources) {
	//		containerData += ",InventoryResource";
	//		containerData += ":" + resourceAmount.resource.type;
	//		containerData += ":" + resourceAmount.amount;
	//	}
	//	/*
	//	 *	/ReservedResources:Count,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...
	//	 * 
	//	 *	Split(',') -> ["ReservedResources:Count" , "ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...", "..."]
	//	 *		Split[0](':') -> ["ReservedResources" , "Count"]
	//	 *		foreach skip 1 (i = 1 -> n):
	//	 *			Split[i](';') -> ["ReservedResourcesColonist:ColonistName" , "ReservedResources:ResourceType:Amount" , "..."]
	//	 *				Split[0](':') -> ["ReservedResourcesColonist" , "ColonistName"]
	//	 *				foreach skip 1 (k = 1 -> n):
	//	 *					Split[k](':') -> ["ReservedResources" , "ResourceType" , "Amount"]
	//	 */
	//	containerData += "/ReservedResources:" + container.inventory.reservedResources.Count;
	//	foreach (ResourceManager.ReservedResources reservedResources in container.inventory.reservedResources) {
	//		containerData += ",ReservedResourcesColonist:" + reservedResources.human.name;
	//		foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
	//			containerData += ";ReservedResource";
	//			containerData += ":" + resourceAmount.resource.type;
	//			containerData += ":" + resourceAmount.amount;
	//		}
	//	}
	//	return containerData;
	//}

	//public string GetColonistDataString(ColonistManager.Colonist colonist) {
	//	string colonistData = "Colonist";
	//	colonistData += "/Position," + colonist.obj.transform.position.x + "," + colonist.obj.transform.position.y;
	//	colonistData += "/Name," + colonist.name;
	//	colonistData += "/Gender," + colonist.gender;
	//	colonistData += "/SkinIndex," + colonist.bodyIndices[ColonistManager.Human.Appearance.Skin];
	//	colonistData += "/HairIndex," + colonist.bodyIndices[ColonistManager.Human.Appearance.Hair];
	//	colonistData += "/Health," + colonist.health;
	//	colonistData += "/PlayerMoved," + colonist.playerMoved;
	//	colonistData += "/Profession," + colonist.profession.type;
	//	colonistData += "/OldProfession," + colonist.oldProfession.type;
	//	colonistData += "/InventoryMaxAmount," + colonist.inventory.maxAmount;
	//	/*
	//	 *	/InventoryResources:Count,InventoryResource:ResourceType:Amount,InventoryResource:ResourceType:Amount,...
	//	 *	
	//	 *	Split(',') -> ["InventoryResources" , "InventoryResource:ResourceType:Amount" , "..."]
	//	 *		Split[0](':') -> ["InventoryResources" , "Count"]
	//	 *		foreach skip 1 (i = 1 -> n):
	//	 *			Split[i](':') -> ["InventoryResource" , "ResourceType" , "Amount"]
	//	 */
	//	colonistData += "/InventoryResources:" + colonist.inventory.resources.Count;
	//	foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources) {
	//		colonistData += ",InventoryResource";
	//		colonistData += ":" + resourceAmount.resource.type;
	//		colonistData += ":" + resourceAmount.amount;
	//	}
	//	/*
	//	 *	/ReservedResources:Count,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...
	//	 * 
	//	 *	Split(',') -> ["ReservedResources:Count" , "ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...", "..."]
	//	 *		Split[0](':') -> ["ReservedResources" , "Count"]
	//	 *		foreach skip 1 (i = 1 -> n):
	//	 *			Split[i](';') -> ["ReservedResourcesColonist:ColonistName" , "ReservedResources:ResourceType:Amount" , "..."]
	//	 *				Split[0](':') -> ["ReservedResourcesColonist" , "ColonistName"]
	//	 *				foreach skip 1 (k = 1 -> n):
	//	 *					Split[k](':') -> ["ReservedResources" , "ResourceType" , "Amount"]
	//	 */
	//	colonistData += "/ReservedResources:" + colonist.inventory.reservedResources.Count;
	//	foreach (ResourceManager.ReservedResources reservedResources in colonist.inventory.reservedResources) {
	//		colonistData += ",ReservedResourcesColonist:" + reservedResources.human;
	//		foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
	//			colonistData += ";ReservedResource";
	//			colonistData += ":" + resourceAmount.resource.type;
	//			colonistData += ":" + resourceAmount.amount;
	//		}
	//	}
	//	if (colonist.job != null) {
	//		colonistData += "/" + GetJobDataString(colonist.job, true).Replace('/', '~');
	//	} else {
	//		colonistData += "/None";
	//	}
	//	if (colonist.storedJob != null) {
	//		colonistData += "/" + GetJobDataString(colonist.storedJob, true).Replace('/', '~');
	//	} else {
	//		colonistData += "/None";
	//	}
	//	colonistData += "/Skills";
	//	foreach (ColonistManager.SkillInstance skill in colonist.skills) {
	//		colonistData += ",Skill";
	//		colonistData += ":" + skill.prefab.type;
	//		colonistData += ":" + skill.level;
	//		colonistData += ":" + skill.nextLevelExperience;
	//		colonistData += ":" + skill.currentExperience;
	//	}
	//	colonistData += "/Traits";
	//	foreach (ColonistManager.TraitInstance trait in colonist.traits) {
	//		colonistData += ",Trait";
	//		colonistData += ":" + trait.prefab.type;
	//	}
	//	colonistData += "/Needs";
	//	foreach (ColonistManager.NeedInstance need in colonist.needs) {
	//		colonistData += ",Need";
	//		colonistData += ":" + need.prefab.type;
	//		colonistData += ":" + need.GetValue();
	//	}
	//	colonistData += "/BaseHappiness," + colonist.baseHappiness;
	//	colonistData += "/EffectiveHappiness," + colonist.effectiveHappiness;
	//	colonistData += "/HappinessModifiers";
	//	foreach (ColonistManager.HappinessModifierInstance happinessModifier in colonist.happinessModifiers) {
	//		colonistData += ",HappinessModifier";
	//		colonistData += ":" + happinessModifier.prefab.type;
	//		colonistData += ":" + happinessModifier.timer;
	//	}
	//	if (colonist.path.Count > 0) {
	//		TileManager.Tile pathEndTile = colonist.path[colonist.path.Count - 1];
	//		colonistData += "/PathEnd," + pathEndTile.obj.transform.position.x + "," + pathEndTile.obj.transform.position.y;
	//	} else {
	//		colonistData += "/None";
	//	}
	//	colonistData += "/";
	//	return colonistData;
	//}

	//public string GetClothingDataString(ResourceManager.ClothingInstance clothingInstance) {
	//	string clothingData = "Clothing";
	//	clothingData += "/Type," + clothingInstance.clothingPrefab.type;
	//	clothingData += "/Name," + clothingInstance.clothingPrefab.name;
	//	clothingData += "/Colour," + clothingInstance.colour;
	//	clothingData += "/Human," + (clothingInstance.human != null ? clothingInstance.human.name : "None");
	//	return clothingData;
	//}

	//public string GetJobDataString(JobManager.Job job, bool onColonist) {
	//	string jobData = "Job";
	//	jobData += "/Position," + job.tile.obj.transform.position.x + "," + job.tile.obj.transform.position.y;
	//	jobData += "/PrefabType," + job.prefab.type;
	//	jobData += "/RotationIndex," + job.rotationIndex;
	//	jobData += "/Started," + job.started;
	//	jobData += "/Progress," + job.jobProgress;
	//	jobData += "/ColonistBuildTime," + job.colonistBuildTime;
	//	// "/ResourceToBuild,ResourceType,Amount"
	//	jobData += "/ResourcesToBuild";
	//	foreach (ResourceManager.ResourceAmount resourceToBuild in job.resourcesToBuild) {
	//		jobData += ",ResourceToBuild";
	//		jobData += ":" + resourceToBuild.resource.type;
	//		jobData += ":" + resourceToBuild.amount;
	//	}
	//	if (onColonist) {
	//		/*
	//		 *	/OnColonist,ColonistResources;ColonistResource:ResourceType:Amount;...,ContainerPickups;ContainerPickup:x`y:ResourceToPickup`ResourceType`Amount:...;...
	//		 * 
	//		 *	Split(',') -> ["OnColonist" , "ColonistResources;..." , "ContainerPickups;..."]
	//		 *		Split[1](';') -> ["ColonistResources" , "ColonistResource:ResourceType:Amount" , "..."]
	//		 *			foreach skip 1 (i = 1 -> n):
	//		 *				Split[i](':') = ["ColonistResource" , "ResourceType" , "Amount"]
	//		 *		Split[2](';') -> ["ContainerPickups" , "ContainerPickup:x`y:ResourceToPickup`ResourceType`Amount:ResourceToPickup
	//		 *			foreach skip 1 (i = 1 -> n):
	//		 *				Split[i](':') = ["ContainerPickup" , "x`y" , "ResourceToPickup`ResourceType`Amount" , "..."]
	//		 *					Split[1]('`') = ["x" , "y"]
	//		 *					foreach skip 2 (k = 2 -> n):
	//		 *						Split[k]('`') = ["ResourceToPickup" , "ResourceType" , "Amount"]
	//		 */
	//		jobData += "/OnColonist";
	//		if (job.colonistResources != null) {
	//			jobData += ",ColonistResources";
	//			foreach (ResourceManager.ResourceAmount colonistResource in job.colonistResources) {
	//				jobData += ";ColonistResource";
	//				jobData += ":" + colonistResource.resource.type;
	//				jobData += ":" + colonistResource.amount;
	//			}
	//		} else {
	//			jobData += ",None";
	//		}
	//		if (job.containerPickups != null) {
	//			jobData += ",ContainerPickups";
	//			foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
	//				jobData += ";ContainerPickup";
	//				jobData += ":" + containerPickup.container.parentObject.tile.obj.transform.position.x + "`" + containerPickup.container.parentObject.tile.obj.transform.position.y;
	//				foreach (ResourceManager.ResourceAmount resourceToPickup in containerPickup.resourcesToPickup) {
	//					jobData += ":ResourceToPickup`" + resourceToPickup.resource.type + "`" + resourceToPickup.amount;
	//				}
	//			}
	//		} else {
	//			jobData += ",None";
	//		}
	//	} else {
	//		jobData += "/None";
	//	}
	//	if (job.plant != null) {
	//		jobData += "/Plant," + job.plant.group.type + "," + (job.plant.harvestResource != null ? job.plant.harvestResource.type.ToString() : "None,");
	//	} else {
	//		jobData += "/None";
	//	}
	//	if (job.createResource != null) {
	//		jobData += "/CreateResource," + job.createResource.type;
	//	} else {
	//		jobData += "/None";
	//	}
	//	if (job.activeTileObject != null) {
	//		jobData += "/ActiveTileObject," + job.activeTileObject.tile.obj.transform.position.x + "," + job.activeTileObject.tile.obj.transform.position.y + "," + job.activeTileObject.prefab.type;
	//	} else {
	//		jobData += "/None";
	//	}
	//	jobData += "/Priority," + job.priority;
	//	jobData += "/";
	//	return jobData;
	//}

	//public void ResetGameState(bool fromMainMenu, bool destroyPlanet) {
	//	tileM.generated = false;

	//	if (!fromMainMenu) {
	//		colonistM.SetSelectedHuman(null);
	//		uiM.SetSelectedContainer(null);
	//		uiM.SetSelectedManufacturingTileObject(null);
	//	}

	//	if (destroyPlanet) {
	//		foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
	//			Destroy(planetTile.obj);
	//		}
	//	}

	//	if (tileM.map != null) {
	//		foreach (TileManager.Tile tile in tileM.map.tiles) {
	//			if (tile.plant != null) {
	//				tileM.map.smallPlants.Remove(tile.plant);
	//				Destroy(tile.plant.obj);
	//				tile.plant = null;
	//			}
	//			Destroy(tile.obj);
	//		}
	//		tileM.map.tiles.Clear();
	//		tileM.map = null;
	//	}

	//	foreach (KeyValuePair<ResourceManager.TileObjectPrefab, List<ResourceManager.TileObjectInstance>> objectInstanceKVP in resourceM.tileObjectInstances) {
	//		foreach (ResourceManager.TileObjectInstance objectInstance in objectInstanceKVP.Value) {
	//			Destroy(objectInstance.obj);
	//		}
	//		objectInstanceKVP.Value.Clear();
	//	}
	//	resourceM.tileObjectInstances.Clear();

	//	resourceM.manufacturingTileObjectInstances.Clear();

	//	resourceM.farms.Clear();

	//	resourceM.containers.Clear();

	//	foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
	//		Destroy(colonist.nameCanvas);
	//		Destroy(colonist.obj);
	//	}
	//	colonistM.colonists.Clear();
	//	uiM.RemoveColonistElements();

	//	foreach (JobManager.Job job in jobM.jobs) {
	//		Destroy(job.jobPreview);
	//	}
	//	jobM.jobs.Clear();
	//	uiM.RemoveJobElements();
	//}

	//public void LoadGame(string fileName, bool fromMainMenu) {
	//	ResetGameState(fromMainMenu,true);

	//	List<string> lines = new StreamReader(fileName).ReadToEnd().Split('\n').ToList();

	//	int sectionIndex = 0;
	//	List<int> sectionLengths = new List<int>();
	//	foreach (string section in lines[1].Split('/').Skip(1)) {
	//		int sectionLength = int.Parse(section.Split(',')[1]);
	//		int additionalLine = (sectionIndex == 3 ? 1 : 0);
	//		sectionLength += additionalLine;
	//		sectionLengths.Add(sectionLength);
	//		sectionIndex += 1;
	//	}

	//	// Planet Data
	//	TileManager.MapData planetData = null;

	//	// Map Data
	//	TileManager.MapData mapData = null;

	//	// Container Data
	//	Dictionary<ResourceManager.Container, string> containerReservedResourcesData = new Dictionary<ResourceManager.Container, string>();

	//	// Colonist Data
	//	Dictionary<ColonistManager.Colonist, string> colonistReservedResourcesData = new Dictionary<ColonistManager.Colonist, string>();

	//	sectionIndex = 0;
	//	int lastSectionEnd = 2;
	//	foreach (int sectionLength in sectionLengths) {
	//		if (sectionLength > 0) {
	//			int sectionStart = lastSectionEnd + 1;
	//			int sectionEnd = sectionStart + sectionLength;
	//			int innerSectionIndex = 0;
	//			for (int lineIndex = sectionStart; lineIndex < sectionEnd; lineIndex++) {
	//				string line = lines[lineIndex - 1];
	//				List<string> lineData = line.Split('/').ToList();
	//				if (sectionIndex == 0) { // Time/Date
	//					timeM.SetTime(float.Parse(lineData[1].Split(',')[1]));
	//					timeM.SetDate(int.Parse(lineData[2].Split(',')[1]), int.Parse(lineData[2].Split(',')[2]), int.Parse(lineData[2].Split(',')[3]));
	//				} else if (sectionIndex == 1) { // Camera
	//					cameraM.SetCameraPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					cameraM.SetCameraZoom(float.Parse(lineData[2].Split(',')[1]));
	//				} else if (sectionIndex == 2) { // Planet
	//					uiM.mainMenu.SetActive(true);
	//					int planetSeed = int.Parse(lineData[1].Split(',')[1]);
	//					int planetSize = int.Parse(lineData[2].Split(',')[1]);
	//					float planetDistance = float.Parse(lineData[3].Split(',')[1]);
	//					float planetTemperature = uiM.CalculatePlanetTemperature(planetDistance);
	//					int temperatureRange = int.Parse(lineData[4].Split(',')[1]);
	//					bool randomOffsets = bool.Parse(lineData[5].Split(',')[1]);
	//					int windDirection = int.Parse(lineData[6].Split(',')[1]);
	//					planetData = new TileManager.MapData(
	//						null,
	//						planetSeed,
	//						planetSize,
	//						UIManager.StaticPlanetMapDataValues.actualMap,
	//						UIManager.StaticPlanetMapDataValues.equatorOffset,
	//						UIManager.StaticPlanetMapDataValues.planetTemperature,
	//						temperatureRange,
	//						planetDistance,
	//						planetTemperature,
	//						randomOffsets,
	//						UIManager.StaticPlanetMapDataValues.averageTemperature,
	//						UIManager.StaticPlanetMapDataValues.averagePrecipitation,
	//						UIManager.StaticPlanetMapDataValues.terrainTypeHeights,
	//						UIManager.StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
	//						UIManager.StaticPlanetMapDataValues.river,
	//						UIManager.StaticPlanetMapDataValues.surroundingPlanetTileRivers,
	//						UIManager.StaticPlanetMapDataValues.preventEdgeTouching,
	//						windDirection,
	//						UIManager.StaticPlanetMapDataValues.planetTilePosition
	//					);
	//					uiM.planet = new TileManager.Map(planetData, false);
	//					foreach (TileManager.Tile tile in uiM.planet.tiles) {
	//						uiM.planetTiles.Add(new UIManager.PlanetTile(tile, uiM.planetPreviewPanel.transform, tile.position, planetData.mapSize, planetData.temperatureOffset));
	//					}
	//					uiM.mainMenu.SetActive(false);
	//				} else if (sectionIndex == 3) { // Tile
	//					if (innerSectionIndex == 0) {
	//						uiM.colonyName = lineData[1].Split(',')[1];
	//						int mapSeed = int.Parse(lineData[2].Split(',')[1]);
	//						int mapSize = int.Parse(lineData[3].Split(',')[1]);
	//						float equatorOffset = float.Parse(lineData[4].Split(',')[1]);
	//						float averageTemperature = float.Parse(lineData[5].Split(',')[1]);
	//						float averagePrecipitation = float.Parse(lineData[6].Split(',')[1]);
	//						int windDirection = int.Parse(lineData[7].Split(',')[1]);
	//						Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>();
	//						foreach (string terrainTypeHeightString in lineData[8].Split(',').Skip(1)) {
	//							terrainTypeHeights.Add((TileManager.TileTypes)Enum.Parse(typeof(TileManager.TileTypes), terrainTypeHeightString.Split(':')[0]), float.Parse(terrainTypeHeightString.Split(':')[1]));
	//						}
	//						List<int> surroundingPlanetTileHeightDirections = new List<int>();
	//						foreach (string surroundingPlanetTileHeightDirectionString in lineData[9].Split(',').Skip(1)) {
	//							surroundingPlanetTileHeightDirections.Add(int.Parse(surroundingPlanetTileHeightDirectionString));
	//						}
	//						bool river = bool.Parse(lineData[10].Split(',')[1]);
	//						List<int> surroundingPlanetTileRivers = new List<int>();
	//						foreach (string surroundingPlanetTileRiverString in lineData[11].Split(',').Skip(1)) {
	//							surroundingPlanetTileRivers.Add(int.Parse(surroundingPlanetTileRiverString));
	//						}
	//						Vector2 planetTilePosition = new Vector2(float.Parse(lineData[12].Split(',')[1]), float.Parse(lineData[12].Split(',')[2]));
	//						mapData = new TileManager.MapData(
	//							uiM.planet.mapData,
	//							mapSeed,
	//							mapSize,
	//							true,
	//							equatorOffset,
	//							false,
	//							0,
	//							0,
	//							0,
	//							false,
	//							averageTemperature,
	//							averagePrecipitation,
	//							terrainTypeHeights,
	//							surroundingPlanetTileHeightDirections,
	//							river,
	//							surroundingPlanetTileRivers,
	//							false,
	//							windDirection,
	//							planetTilePosition
	//						);
	//						tileM.map = new TileManager.Map(mapData, true);
	//						if (fromMainMenu) {
	//							uiM.InitializeGameUI();
	//						}
	//						uiM.pauseMenu.transform.Find("MapRegenerationCode-InputField").GetComponent<InputField>().text = mapData.mapRegenerationCode;
	//					} else {
	//						TileManager.Tile tile = tileM.map.tiles[innerSectionIndex - 1];

	//						TileManager.TileType savedTileType = tileM.GetTileTypeByEnum((TileManager.TileTypes)Enum.Parse(typeof(TileManager.TileTypes), lineData[0].Split(',')[0]));
	//						if (savedTileType != tile.tileType) {
	//							tile.SetTileType(savedTileType, false, false, false, false);
	//							if (TileManager.holeTileTypes.Contains(savedTileType.type)) {
	//								tile.dugPreviously = true;
	//							}
	//						}
	//						string spriteName = lineData[0].Split(',')[1];
	//						Sprite tileSprite = tile.tileType.baseSprites.Find(findTileSprite => findTileSprite.name == spriteName);
	//						if (tileSprite == null) {
	//							tileSprite = tile.tileType.bitmaskSprites.Find(findTileSprite => findTileSprite.name == spriteName);
	//							if (tileSprite == null) {
	//								tileSprite = tile.tileType.riverSprites.Find(findTileSprite => findTileSprite.name == spriteName);
	//							}
	//						}
	//						tile.sr.sprite = tileSprite;

	//						if (lineData[1] == "None") {
	//							if (tile.plant != null) {
	//								tile.SetPlant(true, null);
	//							}
	//						} else {
	//							ResourceManager.PlantGroup savedPlantGroup = resourceM.GetPlantGroupByEnum((ResourceManager.PlantGroupsEnum)Enum.Parse(typeof(ResourceManager.PlantGroupsEnum), lineData[1].Split(',')[0]));
	//							bool savedPlantSmall = bool.Parse(lineData[1].Split(',')[2]);
	//							ResourceManager.Resource harvestResource = null;
	//							if (lineData[1].Split(',')[4] != "None") {
	//								harvestResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[1].Split(',')[4]));
	//							}
	//							ResourceManager.Plant savedPlant = new ResourceManager.Plant(savedPlantGroup, tile, false, savedPlantSmall, tileM.map.smallPlants, false, harvestResource, resourceM) {
	//								growthProgress = float.Parse(lineData[1].Split(',')[3]),

	//							};
	//							tile.SetPlant(false, savedPlant);
	//							Sprite plantSprite = null;
	//							if (harvestResource != null) {
	//								if (savedPlantGroup.harvestResourceSprites.ContainsKey(harvestResource.type)) {
	//									if (savedPlantGroup.harvestResourceSprites[harvestResource.type].ContainsKey(savedPlantSmall)) {
	//										plantSprite = savedPlantGroup.harvestResourceSprites[harvestResource.type][savedPlantSmall].Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
	//									}
	//								}
	//							} else {
	//								if (savedPlantSmall) {
	//									plantSprite = savedPlantGroup.smallPlants.Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
	//								} else {
	//									plantSprite = savedPlantGroup.fullPlants.Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
	//								}
	//							}
	//							if (plantSprite != null) {
	//								tile.plant.obj.GetComponent<SpriteRenderer>().sprite = plantSprite;
	//							}

	//						}
	//						if (fromMainMenu && lineIndex == sectionEnd - 1) {
	//							uiM.MainMenuToGameTransition(true);
	//						}
	//					}
	//				} else if (sectionIndex == 4 || sectionIndex == 5) { // River
	//					if (innerSectionIndex == 0) {
	//						foreach (TileManager.Map.River clearRiver in tileM.map.rivers) {
	//							clearRiver.tiles.Clear();
	//						}
	//						tileM.map.rivers.Clear();
	//						foreach (TileManager.Map.River clearLargeRiver in tileM.map.largeRivers) {
	//							clearLargeRiver.tiles.Clear();
	//						}
	//						tileM.map.largeRivers.Clear();
	//					}
	//					TileManager.Tile startTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					TileManager.Tile centreTile = null;
	//					if (lineData[2].Split(',')[1] != "None") {
	//						centreTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[2].Split(',')[1]), float.Parse(lineData[2].Split(',')[2])));
	//					}
	//					TileManager.Tile endTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[3].Split(',')[1]), float.Parse(lineData[3].Split(',')[2])));
	//					int expandRadius = int.Parse(lineData[4].Split(',')[1]);
	//					bool ignoreStone = bool.Parse(lineData[5].Split(',')[1]);
	//					List<TileManager.Tile> riverTiles = new List<TileManager.Tile>();
	//					foreach (string riverTilePositionString in lineData.Skip(6)) {
	//						riverTiles.Add(tileM.map.GetTileFromPosition(new Vector2(float.Parse(riverTilePositionString.Split(',')[0]), float.Parse(riverTilePositionString.Split(',')[1]))));
	//					}
	//					TileManager.Map.River river = new TileManager.Map.River(startTile, centreTile, endTile, expandRadius, ignoreStone, tileM.map);
	//					if (sectionIndex == 4) {
	//						tileM.map.rivers.Add(river);
	//					} else if (sectionIndex == 5) {
	//						tileM.map.largeRivers.Add(river);
	//					}
	//				} else if (sectionIndex == 6) { // Resources
	//					foreach (string resourceData in lineData.Skip(1)) {
	//						ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceData.Split(',')[0]));
	//						resource.ChangeDesiredAmount(int.Parse(resourceData.Split(',')[1]));
	//					}
	//				} else if (sectionIndex == 7) { // Object
	//					TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					ResourceManager.TileObjectPrefab tileObjectPrefab = resourceM.GetTileObjectPrefabByEnum((ResourceManager.TileObjectPrefabsEnum)Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), lineData[2].Split(',')[1]));
	//					int rotationIndex = int.Parse(lineData[3].Split(',')[1]);
	//					float integrity = int.Parse(lineData[4].Split(',')[1]);
	//					tile.SetTileObject(tileObjectPrefab, rotationIndex);
	//					tile.GetObjectInstanceAtLayer(tileObjectPrefab.layer).integrity = integrity;
	//				} else if (sectionIndex == 8) { // Manufacturing Tile Object
	//					TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == tile);
	//					if (lineData[2] != "None") {
	//						mto.createResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[2].Split(',')[1]));
	//					}
	//					if (lineData[3] != "None") {
	//						mto.fuelResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[3].Split(',')[1]));
	//					}
	//					mto.active = bool.Parse(lineData[4].Split(',')[1]);
	//				} else if (sectionIndex == 9) { // Farm
	//					TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					ResourceManager.Farm farm = resourceM.farms.Find(findFarm => findFarm.tile == tile);
	//					farm.growTimer = float.Parse(lineData[3].Split(',')[1]);
	//					farm.maxGrowthTime = float.Parse(lineData[4].Split(',')[1]);
	//					farm.growProgressSpriteIndex = -1;
	//					farm.Update();
	//				} else if (sectionIndex == 10) { // Container
	//					TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
	//					ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == tile);
	//					ResourceManager.Inventory inventory = new ResourceManager.Inventory(null, container, int.Parse(lineData[2].Split(',')[1]));
	//					foreach (string inventoryResourceString in lineData[3].Split(',').Skip(1)) {
	//						ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), inventoryResourceString.Split(':')[1]));
	//						int amount = int.Parse(inventoryResourceString.Split(':')[2]);
	//						inventory.ChangeResourceAmount(resource, amount);
	//					}
	//					container.inventory = inventory;
	//					containerReservedResourcesData.Add(container, lineData[4]);
	//				} else if (sectionIndex == 11) { // Colonist
	//					Vector2 position = new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2]));
	//					TileManager.Tile tile = tileM.map.GetTileFromPosition(position);

	//					string name = lineData[2].Split(',')[1];

	//					ColonistManager.Life.Gender gender = (ColonistManager.Life.Gender)Enum.Parse(typeof(ColonistManager.Life.Gender), lineData[3].Split(',')[1]);

	//					Dictionary<ColonistManager.Human.Appearance, int> bodyIndices = new Dictionary<ColonistManager.Human.Appearance, int>() {
	//						{ ColonistManager.Human.Appearance.Skin, int.Parse(lineData[4].Split(',')[1]) },
	//						{ ColonistManager.Human.Appearance.Hair, int.Parse(lineData[5].Split(',')[1]) }
	//					};

	//					float health = float.Parse(lineData[6].Split(',')[1]);

	//					bool playerMoved = bool.Parse(lineData[7].Split(',')[1]);

	//					ColonistManager.Profession profession = colonistM.professions.Find(p => p.type.ToString() == lineData[8].Split(',')[1]);
	//					ColonistManager.Profession oldProfession = colonistM.professions.Find(p => p.type.ToString() == lineData[9].Split(',')[1]);

	//					ColonistManager.Colonist colonist = new ColonistManager.Colonist(tile, profession, health) {
	//						bodyIndices = bodyIndices,
	//						gender = gender
	//					};

	//					ResourceManager.Inventory inventory = new ResourceManager.Inventory(colonist, null, int.Parse(lineData[10].Split(',')[1]));
	//					foreach (string inventoryResourceString in lineData[11].Split(',').Skip(1)) {
	//						ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), inventoryResourceString.Split(':')[1]));
	//						int amount = int.Parse(inventoryResourceString.Split(':')[2]);
	//						inventory.ChangeResourceAmount(resource, amount);
	//					}
	//					colonistReservedResourcesData.Add(colonist, lineData[12]);

	//					JobManager.Job job = null;
	//					if (lineData[13] != "None") {
	//						List<string> jobDataSplit = lineData[13].Split('~').ToList();
	//						job = LoadJob(jobDataSplit);
	//						if (job.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
	//							ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == job.tile);
	//							mto.jobBacklog.Add(job);
	//						}
	//					}
	//					JobManager.Job storedJob = null;
	//					if (lineData[14] != "None") {
	//						List<string> jobDataSplit = lineData[14].Split('~').ToList();
	//						storedJob = LoadJob(jobDataSplit);
	//						if (storedJob.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
	//							ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == storedJob.tile);
	//							mto.jobBacklog.Add(storedJob);
	//						}
	//					}

	//					List<ColonistManager.SkillInstance> skills = new List<ColonistManager.SkillInstance>();
	//					foreach (string skillDataString in lineData[15].Split(',').Skip(1)) {
	//						ColonistManager.SkillPrefab skillPrefab = colonistM.GetSkillPrefabFromString(skillDataString.Split(':')[1]);
	//						int level = int.Parse(skillDataString.Split(':')[2]);
	//						float nextLevelExperience = float.Parse(skillDataString.Split(':')[3]);
	//						float currentExperience = float.Parse(skillDataString.Split(':')[4]);
	//						ColonistManager.SkillInstance skill = new ColonistManager.SkillInstance(colonist, skillPrefab, false, level) {
	//							colonist = colonist,
	//							prefab = skillPrefab,
	//							level = level,
	//							nextLevelExperience = nextLevelExperience,
	//							currentExperience = currentExperience
	//						};
	//						skills.Add(skill);
	//					}
	//					foreach (ColonistManager.SkillPrefab skillPrefab in colonistM.skillPrefabs.Where(sP => skills.Find(skill => skill.prefab == sP) == null)) {
	//						ColonistManager.SkillInstance skill = new ColonistManager.SkillInstance(colonist, skillPrefab, true, 0);
	//						skills.Add(skill);
	//					}

	//					List<ColonistManager.TraitInstance> traits = new List<ColonistManager.TraitInstance>();
	//					foreach (string traitDataString in lineData[16].Split(',').Skip(1)) {
	//						ColonistManager.TraitPrefab traitPrefab = colonistM.GetTraitPrefabFromString(traitDataString.Split(':')[1]);
	//						traits.Add(new ColonistManager.TraitInstance(colonist, traitPrefab));
	//					}
	//					foreach (ColonistManager.TraitPrefab traitPrefab in colonistM.traitPrefabs.Where(tP => traits.Find(trait => trait.prefab == tP) == null)) {
	//						ColonistManager.TraitInstance trait = new ColonistManager.TraitInstance(colonist, traitPrefab);
	//						traits.Add(trait);
	//					}

	//					List<ColonistManager.NeedInstance> needs = new List<ColonistManager.NeedInstance>();
	//					foreach (string needDataString in lineData[17].Split(',').Skip(1)) {
	//						ColonistManager.NeedPrefab needPrefab = colonistM.GetNeedPrefabFromString(needDataString.Split(':')[1]);
	//						float value = float.Parse(needDataString.Split(':')[2]);
	//						ColonistManager.NeedInstance needInstance = new ColonistManager.NeedInstance(colonist, needPrefab);
	//						needInstance.SetValue(value);
	//						needs.Add(needInstance);
	//					}
	//					foreach (ColonistManager.NeedPrefab needPrefab in colonistM.needPrefabs.Where(nP => needs.Find(need => need.prefab == nP) == null)) {
	//						ColonistManager.NeedInstance need = new ColonistManager.NeedInstance(colonist, needPrefab);
	//						needs.Add(need);
	//					}
	//					needs.OrderBy(need => need.prefab.priority);
	//					float baseHappiness = float.Parse(lineData[18].Split(',')[1]);
	//					float effectiveHappiness = float.Parse(lineData[19].Split(',')[1]);
	//					List<ColonistManager.HappinessModifierInstance> happinessModifiers = new List<ColonistManager.HappinessModifierInstance>();
	//					foreach (string happinessModifierString in lineData[20].Split(',').Skip(1)) {
	//						ColonistManager.HappinessModifierPrefab happinessModifierPrefab = colonistM.GetHappinessModifierPrefabFromString(happinessModifierString.Split(':')[1]);
	//						float timer = float.Parse(happinessModifierString.Split(':')[2]);
	//						happinessModifiers.Add(new ColonistManager.HappinessModifierInstance(colonist, happinessModifierPrefab) { timer = timer });
	//					}
	//					TileManager.Tile pathEndTile = null;
	//					if (lineData[21] != "None") {
	//						pathEndTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[21].Split(',')[1]), float.Parse(lineData[21].Split(',')[2])));
	//					}
	//					colonist.LoadColonistData(
	//						position,
	//						name,
	//						bodyIndices,
	//						health,
	//						profession,
	//						oldProfession,
	//						inventory,
	//						job,
	//						storedJob,
	//						skills,
	//						traits,
	//						needs,
	//						baseHappiness,
	//						effectiveHappiness,
	//						happinessModifiers,
	//						playerMoved,
	//						pathEndTile
	//					);
	//					colonistM.AddColonist(colonist);
	//					if (lineIndex == sectionEnd - 1) {
	//						foreach (KeyValuePair<ColonistManager.Colonist, string> reservedResourcesStringKVP in colonistReservedResourcesData) {
	//							if (int.Parse(reservedResourcesStringKVP.Value.Split(',')[0].Split(':')[1]) > 0) {
	//								ColonistManager.Colonist reservedResourcesColonist = colonistM.colonists.Find(findColonist => findColonist.name == reservedResourcesStringKVP.Value.Split(';')[0].Split(':')[1]);
	//								List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
	//								foreach (string reservedResourceString in reservedResourcesStringKVP.Value.Split(';').Skip(1)) {
	//									ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), reservedResourceString.Split(':')[1]));
	//									int amount = int.Parse(reservedResourceString.Split(':')[2]);
	//									resourcesToReserve.Add(new ResourceManager.ResourceAmount(resource, amount));
	//								}
	//								reservedResourcesStringKVP.Key.inventory.ReserveResources(resourcesToReserve, reservedResourcesColonist);
	//							}
	//						}
	//						/*
	//						foreach (KeyValuePair<ResourceManager.Container, string> reservedResourcesStringKVP in containerReservedResourcesData) {
	//							if (int.Parse(reservedResourcesStringKVP.Value.Split(',')[0].Split(':')[1]) > 0) {
	//								ColonistManager.Colonist reservedResourcesColonist = colonistM.colonists.Find(findColonist => findColonist.name == reservedResourcesStringKVP.Value.Split(',')[1].Split(';')[0].Split(':')[1]);
	//								List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
	//								foreach (string reservedResourceString in reservedResourcesStringKVP.Value.Split(';').Skip(1)) {
	//									ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), reservedResourceString.Split(':')[1]));
	//									int amount = int.Parse(reservedResourceString.Split(':')[2]);
	//									resourcesToReserve.Add(new ResourceManager.ResourceAmount(resource, amount));
	//								}
	//								reservedResourcesStringKVP.Key.inventory.ReserveResources(resourcesToReserve, reservedResourcesColonist);
	//							}
	//						}
	//						*/
	//					}
	//				} else if (sectionIndex == 12) { // Clothing
	//					ResourceManager.ClothingPrefab clothingPrefab = resourceM.GetClothingPrefabsByAppearance((ColonistManager.Human.Appearance)Enum.Parse(typeof(ColonistManager.Human.Appearance), lineData[1].Split(',')[1])).Find(cp => cp.name == lineData[2].Split(',')[1]);
	//					string colonistName = UIManager.RemoveNonAlphanumericChars(lineData[4].Split(',')[1]);
	//					ColonistManager.Human human = colonistName == "None" ? null : colonistM.colonists.Find(c => c.name == colonistName);
	//					ResourceManager.ClothingInstance clothingInstance = new ResourceManager.ClothingInstance(clothingPrefab, lineData[3].Split(',')[1], human);
	//					resourceM.clothingInstances.Add(clothingInstance);
	//				} else if (sectionIndex == 13) { // Job
	//					JobManager.Job job = LoadJob(lineData);
	//					jobM.AddExistingJob(job);
	//					if (job.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
	//						ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == job.tile);
	//						mto.jobBacklog.Add(job);
	//					}
	//				}
	//				innerSectionIndex += 1;
	//			}
	//			lastSectionEnd = sectionEnd - 1;
	//		}
	//		sectionIndex += 1;
	//	}

	//	tileM.map.SetTileRegions(false);
	//	tileM.map.CreateRegionBlocks();
	//	tileM.map.DetermineShadowTiles(tileM.map.tiles, false);
	//	tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
	//	tileM.map.DetermineVisibleRegionBlocks();
	//	tileM.map.Bitmasking(tileM.map.tiles);
	//	resourceM.Bitmask(tileM.map.tiles);

	//	uiM.SetLoadMenuActive(false, false);
	//	if (!fromMainMenu) {
	//		uiM.TogglePauseMenu();
	//	}
	//	timeM.SetPaused(true);

	//	tileM.generated = true;
	//	tileM.generating = false;
	//}

	//public JobManager.Job LoadJob(List<string> jobDataSplit) {
	//	TileManager.Tile jobTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(jobDataSplit[1].Split(',')[1]), float.Parse(jobDataSplit[1].Split(',')[2])));
	//	ResourceManager.TileObjectPrefab jobPrefab = resourceM.GetTileObjectPrefabByEnum((ResourceManager.TileObjectPrefabsEnum)Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), jobDataSplit[2].Split(',')[1]));
	//	int rotationIndex = int.Parse(jobDataSplit[3].Split(',')[1]);
	//	bool started = bool.Parse(jobDataSplit[4].Split(',')[1]);
	//	float progress = float.Parse(jobDataSplit[5].Split(',')[1]);
	//	float colonistBuildTime = float.Parse(jobDataSplit[6].Split(',')[1]);
	//	List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();
	//	foreach (string resourceToBuildString in jobDataSplit[7].Split(',').Skip(1)) {
	//		ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceToBuildString.Split(':')[1]));
	//		int amount = int.Parse(resourceToBuildString.Split(':')[2]);
	//		resourcesToBuild.Add(new ResourceManager.ResourceAmount(resource, amount));
	//	}
	//	List<ResourceManager.ResourceAmount> colonistResources = new List<ResourceManager.ResourceAmount>();
	//	List<JobManager.ContainerPickup> containerPickups = new List<JobManager.ContainerPickup>();
	//	if (jobDataSplit[8] != "None") {
	//		List<string> onColonistDataSplit = jobDataSplit[8].Split(',').ToList();
	//		if (onColonistDataSplit[1] != "None") {
	//			foreach (string colonistResourceString in onColonistDataSplit[1].Split(';').Skip(1)) {
	//				ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), colonistResourceString.Split(':')[1]));
	//				int amount = int.Parse(colonistResourceString.Split(':')[2]);
	//				colonistResources.Add(new ResourceManager.ResourceAmount(resource, amount));
	//			}
	//		}
	//		if (onColonistDataSplit[2] != "None") {
	//			foreach (string containerPickupString in onColonistDataSplit[2].Split(';').Skip(1)) {
	//				List<string> containerPickupDataSplit = containerPickupString.Split(':').ToList();
	//				Vector2 containerPosition = new Vector2(float.Parse(containerPickupDataSplit[1].Split('`')[0]), float.Parse(containerPickupDataSplit[1].Split('`')[1]));
	//				TileManager.Tile containerTile = tileM.map.GetTileFromPosition(containerPosition);
	//				ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == containerTile);
	//				List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
	//				foreach (string resourceToPickupString in containerPickupDataSplit.Skip(2).ToList()) {
	//					ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceToPickupString.Split('`')[1]));
	//					int amount = int.Parse(resourceToPickupString.Split('`')[2]);
	//					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resource, amount));
	//				}
	//				containerPickups.Add(new JobManager.ContainerPickup(container, resourcesToPickup));
	//			}
	//		}
	//	}

	//	JobManager.Job job = new JobManager.Job(jobTile, jobPrefab, rotationIndex) {
	//		started = started,
	//		jobProgress = progress,
	//		colonistBuildTime = colonistBuildTime,
	//		resourcesToBuild = resourcesToBuild,
	//		colonistResources = (colonistResources.Count > 0 ? colonistResources : null),
	//		containerPickups = (containerPickups.Count > 0 ? containerPickups : null)
	//	};

	//	if (jobDataSplit[9] != "None") {
	//		job.plant = new ResourceManager.Plant(resourceM.GetPlantGroupByEnum((ResourceManager.PlantGroupsEnum)Enum.Parse(typeof(ResourceManager.PlantGroupsEnum),jobDataSplit[9].Split(',')[1])), jobTile, false, true, tileM.map.smallPlants,false,(jobDataSplit[9].Split(',')[2] != "None" ? resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), jobDataSplit[9].Split(',')[2])) : null), resourceM);
	//	}
	//	if (jobDataSplit[10] != "None") {
	//		job.createResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)Enum.Parse(typeof(ResourceManager.ResourcesEnum), jobDataSplit[10].Split(',')[1]));
	//	}
	//	if (jobDataSplit[11] != "None") {
	//		ResourceManager.TileObjectPrefabsEnum activeTileObjectPrefab = (ResourceManager.TileObjectPrefabsEnum)Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), jobDataSplit[11].Split(',')[3]);
	//		TileManager.Tile activeTileObjectTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(jobDataSplit[11].Split(',')[1]), float.Parse(jobDataSplit[11].Split(',')[2])));
	//		ResourceManager.TileObjectInstance activeTileObject = activeTileObjectTile.objectInstances.Values.ToList().Find(oi => oi.tile == activeTileObjectTile && oi.prefab.type == activeTileObjectPrefab);
	//		job.activeTileObject = activeTileObject;
	//	}
	//	job.priority = int.Parse(jobDataSplit[12].Split(',')[1]);
	//	return job;
	//}
}