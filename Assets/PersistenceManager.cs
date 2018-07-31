using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class PersistenceManager : MonoBehaviour {

	public const int gameVersion = 3;
	public const int saveVersion = 2;

	private Dictionary<int, string> gameVersionStrings = new Dictionary<int, string>() {
		{ 1, "Alpha R1" },
		{ 2, "Alpha R1.1" },
		{ 3, "Alpha R2" }
	};
	public string GetGameVersionString(int gameVersion) {
		if (gameVersionStrings.ContainsKey(gameVersion)) {
			return gameVersionStrings[gameVersion];
		} else if (gameVersion == -1) {
			return "Unsupported";
		} else {
			return "Unsupported " + gameVersion;
		}
	}

	private CameraManager cameraM; // Save the camera zoom and position
	private ColonistManager colonistM; // Save all instances of colonists, humans, life, etc.
	private JobManager jobM; // Save all non-taken jobs
	private TileManager tileM; // Save the tile data
	private TimeManager timeM; // Save the date/time
	private UIManager uiM; // Save the planet data
	private ResourceManager resourceM; // Save the object data

	private string settingsFilePath;

	void Awake() {

		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		jobM = GetComponent<JobManager>();
		tileM = GetComponent<TileManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
		resourceM = GetComponent<ResourceManager>();

		settingsFilePath = Application.persistentDataPath + "/Settings/settings.snowship";
		uiM.settingsState = new UIManager.SettingsState(uiM);
		if (!LoadSettings()) {
			SaveSettings();
		}
		uiM.settingsState.SetSettings();
	}

	public void SaveSettings() {
		Directory.CreateDirectory(Application.persistentDataPath + "/Settings/");
		FileStream createFile = File.Create(settingsFilePath);
		createFile.Close();
		File.WriteAllText(settingsFilePath, string.Empty);
		StreamWriter file = new StreamWriter(settingsFilePath);

		file.WriteLine(uiM.settingsState.resolutionWidth + "," + uiM.settingsState.resolutionHeight);
		file.WriteLine(uiM.settingsState.fullscreen);
		file.WriteLine(uiM.settingsState.scaleMode.ToString());

		file.Close();
	}

	public bool LoadSettings() {
		Directory.CreateDirectory(Application.persistentDataPath + "/Settings/");
		FileStream createFile = File.Create(settingsFilePath);
		createFile.Close();
		StreamReader file = new StreamReader(settingsFilePath);
		List<string> lines = file.ReadToEnd().Split('\n').ToList();
		for (int i = 0; i < lines.Count; i++) {
			string line = lines[i];
			if (!string.IsNullOrEmpty(line)) {
				switch (i) {
					case 0:
						uiM.settingsState.resolutionWidth = int.Parse(line.Split(',')[0]);
						uiM.settingsState.resolutionHeight = int.Parse(line.Split(',')[1]);
						break;
					case 1:
						uiM.settingsState.fullscreen = bool.Parse(line);
						break;
					case 2:
						uiM.settingsState.scaleMode = (CanvasScaler.ScaleMode)System.Enum.Parse(typeof(CanvasScaler.ScaleMode), line);
						break;
				}
			}
		}
		file.Close();
		return true;
	}

	/* https://stackoverflow.com/questions/13266496/easily-write-a-whole-class-instance-to-xml-file-and-read-back-in */

	public string GenerateSaveFileName() {
		System.DateTime now = System.DateTime.Now;
		string dateTime = now.Year + "y" + now.Month + "m" + now.Day + "d" + now.Hour + "h" + now.Minute + "m" + now.Second + "s" + now.Millisecond + "m";
		string fileName = "snowship-save-" + uiM.colonyName + "-" + dateTime + "-" + gameVersion + "-" + saveVersion + ".snowship";
		return fileName;
	}

	public string GenerateSavePath(string fileName) {
		return Application.persistentDataPath + "/Saves/" + fileName;
	}

	public void SaveGame(string fileName) {
		fileName = GenerateSavePath(fileName);

		string directory = GenerateSavePath(string.Empty);
		Directory.CreateDirectory(directory);
		StreamWriter file = new StreamWriter(fileName);

		string versionData = "Version";
		versionData += "/SaveVersion," + saveVersion;
		versionData += "/GameVersion," + gameVersion;
		file.WriteLine(versionData);

		string saveFileFormatData = "Format";
		saveFileFormatData += "/Time,1";
		saveFileFormatData += "/Camera,1";
		//saveFileFormatData += "/PlanetTiles," + uiM.planetTiles.Count;
		saveFileFormatData += "/PlanetTiles,1";
		saveFileFormatData += "/MapTiles," + tileM.map.tiles.Count;
		saveFileFormatData += "/Rivers," + tileM.map.rivers.Count;
		saveFileFormatData += "/Resources,1";
		saveFileFormatData += "/ObjectInstances," + resourceM.tileObjectInstances.Values.Sum(objList => objList.Count);
		saveFileFormatData += "/MTOs," + resourceM.manufacturingTileObjectInstances.Count;
		saveFileFormatData += "/Farms," + resourceM.farms.Count;
		saveFileFormatData += "/Container," + resourceM.containers.Count;
		saveFileFormatData += "/Colonists," + colonistM.colonists.Count;
		saveFileFormatData += "/Jobs," + jobM.jobs.Count;
		file.WriteLine(saveFileFormatData);

		// Save the time data
		string timeData = "Time";
		timeData += "/Time," + timeM.tileBrightnessTime;
		timeData += "/Date," + timeM.GetDateString();
		file.WriteLine(timeData);

		// Save the camera data
		string cameraData = "Camera";
		cameraData += "/Position," + cameraM.cameraGO.transform.position.x + "," + cameraM.cameraGO.transform.position.y;
		cameraData += "/Zoom," + cameraM.cameraComponent.orthographicSize;
		file.WriteLine(cameraData);

		// Save the planet data
		string planetData = "PlanetTiles";
		planetData += "/PlanetSeed," + uiM.planet.mapData.mapSeed;
		planetData += "/PlanetSize," + uiM.planet.mapData.mapSize;
		planetData += "/PlanetDistance," + uiM.planetDistance;
		planetData += "/PlanetTempRange," + uiM.temperatureRange;
		planetData += "/PlanetWindDirection," + uiM.planet.mapData.primaryWindDirection;
		file.WriteLine(planetData);
		/*
		foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
			file.WriteLine(GetPlanetTileDataString(planetTile));
		}
		*/

		// Save the tile data
		string tileMapData = "Tiles";
		tileMapData += "/ColonyName," + uiM.colonyName;
		tileMapData += "/MapSeed," + tileM.map.mapData.mapSeed;
		tileMapData += "/MapSize," + tileM.map.mapData.mapSize;
		tileMapData += "/EquatorOffset," + tileM.map.mapData.equatorOffset;
		tileMapData += "/AverageTemperature," + tileM.map.mapData.averageTemperature;
		tileMapData += "/AveragePrecipitation," + tileM.map.mapData.averagePrecipitation;
		tileMapData += "/WindDirection," + tileM.map.mapData.primaryWindDirection;
		tileMapData += "/TerrainTypeHeights";
		foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightsKVP in tileM.map.mapData.terrainTypeHeights) {
			tileMapData += "," + terrainTypeHeightsKVP.Key + ":" + terrainTypeHeightsKVP.Value;
		}
		tileMapData += "/SurroundingPlanetTileHeightDirections";
		foreach (int surroundingPlanetTileHeightDirection in tileM.map.mapData.surroundingPlanetTileHeightDirections) {
			tileMapData += "," + surroundingPlanetTileHeightDirection;
		}
		tileMapData += "/PlanetTilePosition," + tileM.map.mapData.planetTilePosition.x + "," + tileM.map.mapData.planetTilePosition.y;
		file.WriteLine(tileMapData);

		foreach (TileManager.Tile tile in tileM.map.tiles) {
			file.WriteLine(GetTileDataString(tile));
		}

		// Save the river data
		foreach (TileManager.Map.River river in tileM.map.rivers) {
			file.WriteLine(GetRiverDataString(river));
		}

		// Save the resource data
		string resourceData = "Resources";
		foreach (ResourceManager.Resource resource in resourceM.resources) {
			resourceData += "/" + resource.type + "," + resource.desiredAmount;
		}
		file.WriteLine(resourceData);

		// Save the object data
		foreach (KeyValuePair<ResourceManager.TileObjectPrefab, List<ResourceManager.TileObjectInstance>> objectInstanceKVP in resourceM.tileObjectInstances) {
			foreach (ResourceManager.TileObjectInstance objectInstance in objectInstanceKVP.Value) {
				file.WriteLine(GetObjectInstanceDataString(objectInstance));
			}
		}

		// Save the manufacturing tile object data
		foreach (ResourceManager.ManufacturingTileObject mto in resourceM.manufacturingTileObjectInstances) {
			file.WriteLine(GetManufacturingTileObjectDataString(mto));
		}

		// Save the farm object data
		foreach (ResourceManager.Farm farm in resourceM.farms) {
			file.WriteLine(GetFarmDataString(farm));
		}

		// Save the container data
		foreach (ResourceManager.Container container in resourceM.containers) {
			file.WriteLine(GetContainerDataString(container));
		}

		// Save the colonist data
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			file.WriteLine(GetColonistDataString(colonist));
		}

		// Save the job data
		foreach (JobManager.Job job in jobM.jobs) {
			file.WriteLine(GetJobDataString(job, false));
		}

		file.Close();

		StartCoroutine(CreateScreenshot(fileName));
	}

	IEnumerator CreateScreenshot(string fileName) {
		GameObject canvas = GameObject.Find("Canvas");
		canvas.SetActive(false);
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(fileName.Split('.')[0] + ".png");
		canvas.SetActive(true);
	}

	public string GetPlanetTileDataString(UIManager.PlanetTile planetTile) {
		string planetTileData = string.Empty;
		planetTileData += planetTile.equatorOffset;
		planetTileData += "/" + planetTile.averageTemperature;
		planetTileData += "/" + planetTile.averagePrecipitation;
		planetTileData += "/";
		int terrainTypeHeightIndex = 0;
		foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightKVP in planetTile.terrainTypeHeights) {
			planetTileData += terrainTypeHeightKVP.Key + ":" + terrainTypeHeightKVP.Value + (terrainTypeHeightIndex + 1 == planetTile.terrainTypeHeights.Count ? "" : ",");
			terrainTypeHeightIndex += 1;
		}
		planetTileData += "/";
		int surroundingPlanetTileHeightDirectionIndex = 0;
		foreach (int surroundingPlanetTileHeightDirection in planetTile.surroundingPlanetTileHeightDirections) {
			planetTileData += surroundingPlanetTileHeightDirection + (surroundingPlanetTileHeightDirectionIndex + 1 == planetTile.surroundingPlanetTileHeightDirections.Count ? "" : ",");
			surroundingPlanetTileHeightDirectionIndex += 1;
		}
		return planetTileData;
	}

	/*
		"xPos,yPos/height/temperature/precipitation/dugPreviously"

		Example: "35,45/0.25/23/0.4/false"
	*/
	public string GetTileDataString(TileManager.Tile tile) {
		string tileData = string.Empty;
		tileData += tile.tileType.type + "," + tile.sr.sprite.name;
		if (tile.plant != null) {
			tileData += "/" + tile.plant.group.type + "," + tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name + "," + tile.plant.small + "," + tile.plant.growthProgress + "," + (tile.plant.harvestResource != null ? tile.plant.harvestResource.type.ToString() : "None");
		} else {
			tileData += "/None";
		}
		tileData += "/";
		return tileData;
	}

	/*
		"River/StartTilePos,x,y/EndTilePos,x,y/RiverTile,x,y/RiverTile,x,y/..."
	*/
	public string GetRiverDataString(TileManager.Map.River river) {
		string riverData = "River";
		riverData += "/StartTilePos," + river.startTile.obj.transform.position.x + "," + river.startTile.obj.transform.position.y;
		riverData += "/EndTilePos," + river.endTile.obj.transform.position.x + "," + river.endTile.obj.transform.position.y;
		foreach (TileManager.Tile riverTile in river.tiles) {
			riverData += "/" + riverTile.obj.transform.position.x + "," + riverTile.obj.transform.position.y;
		}
		return riverData;
	}

	/*
		"ObjectInstance/Position,x,y/PrefabType,prefabType/RotationIndex,rotationIndex"

		Example: "ObjectInstance/Position,35.0,45.0/PrefabType,WoodenChest/RotationIndex,0"
	*/
	public string GetObjectInstanceDataString(ResourceManager.TileObjectInstance objectInstance) {
		string objectInstanceData = "ObjectInstance";
		objectInstanceData += "/Position," + objectInstance.tile.obj.transform.position.x + "," + objectInstance.tile.obj.transform.position.y;
		objectInstanceData += "/PrefabType," + objectInstance.prefab.type;
		objectInstanceData += "/RotationIndex," + objectInstance.rotationIndex;
		objectInstanceData += "/Integrity," + objectInstance.integrity;
		return objectInstanceData;
	}

	/*
		"MTO/Position,x,y/CreateResource,resourceType/FuelResource,resourceType"

		Example: "MTO/Position,35.0,45.0/CreateResource,Brick/FuelResource,Firewood"
	*/
	public string GetManufacturingTileObjectDataString(ResourceManager.ManufacturingTileObject mto) {
		string mtoData = "MTO";
		mtoData += "/Position," + mto.parentObject.tile.obj.transform.position.x + "," + mto.parentObject.tile.obj.transform.position.y;
		if (mto.createResource != null) {
			mtoData += "/CreateResource," + mto.createResource.type;
		} else {
			mtoData += "/None";
		}
		if (mto.fuelResource != null) {
			mtoData += "/FuelResource," + mto.fuelResource.type;
		} else {
			mtoData += "/None";
		}
		mtoData += "/Active," + mto.active;
		mtoData += "/";
		return mtoData;
	}

	/*
		"Farm/Position,x,y/SeedType,seedType/GrowTimer,growTimer/MaxGrowthTime,maxGrowthTime"

		Example: "Farm/Position,35.0,45.0/SeedType,Potato/GrowTimer,100.51/MaxGrowthTime,1440"
	*/
	public string GetFarmDataString(ResourceManager.Farm farm) {
		string farmData = "Farm";
		farmData += "/Position," + farm.tile.obj.transform.position.x + "," + farm.tile.obj.transform.position.y;
		farmData += "/SeedType," + farm.seedType;
		farmData += "/GrowTimer," + farm.growTimer;
		farmData += "/MaxGrowthTime," + farm.maxGrowthTime;
		return farmData;
	}

	public string GetContainerDataString(ResourceManager.Container container) {
		string containerData = "Container";
		containerData += "/Position," + container.parentObject.tile.obj.transform.position.x + "," + container.parentObject.tile.obj.transform.position.y;
		containerData += "/InventoryMaxAmount," + container.inventory.maxAmount;
		/*
		 *	/InventoryResources:Count,InventoryResource:ResourceType:Amount,InventoryResource:ResourceType:Amount,...
		 *	
		 *	Split(',') -> ["InventoryResources" , "InventoryResource:ResourceType:Amount" , "..."]
		 *		Split[0](':') -> ["InventoryResources" , "Count"]
		 *		foreach skip 1 (i = 1 -> n):
		 *			Split[i](':') -> ["InventoryResource" , "ResourceType" , "Amount"]
		 */
		containerData += "/InventoryResources:" + container.inventory.resources.Count;
		foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources) {
			containerData += ",InventoryResource";
			containerData += ":" + resourceAmount.resource.type;
			containerData += ":" + resourceAmount.amount;
		}
		/*
		 *	/ReservedResources:Count,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...
		 * 
		 *	Split(',') -> ["ReservedResources:Count" , "ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...", "..."]
		 *		Split[0](':') -> ["ReservedResources" , "Count"]
		 *		foreach skip 1 (i = 1 -> n):
		 *			Split[i](';') -> ["ReservedResourcesColonist:ColonistName" , "ReservedResources:ResourceType:Amount" , "..."]
		 *				Split[0](':') -> ["ReservedResourcesColonist" , "ColonistName"]
		 *				foreach skip 1 (k = 1 -> n):
		 *					Split[k](':') -> ["ReservedResources" , "ResourceType" , "Amount"]
		 */
		containerData += "/ReservedResources:" + container.inventory.reservedResources.Count;
		foreach (ResourceManager.ReservedResources reservedResources in container.inventory.reservedResources) {
			containerData += ",ReservedResourcesColonist:" + reservedResources.colonist.name;
			foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
				containerData += ";ReservedResource";
				containerData += ":" + resourceAmount.resource.type;
				containerData += ":" + resourceAmount.amount;
			}
		}
		return containerData;
	}

	public string GetColonistDataString(ColonistManager.Colonist colonist) {
		string colonistData = "Colonist";
		colonistData += "/Position," + colonist.obj.transform.position.x + "," + colonist.obj.transform.position.y;
		colonistData += "/Name," + colonist.name;
		colonistData += "/Gender," + colonist.gender;
		colonistData += "/SkinIndex," + colonist.humanLookIndices[ColonistManager.Human.HumanLook.Skin];
		colonistData += "/HairIndex," + colonist.humanLookIndices[ColonistManager.Human.HumanLook.Hair];
		colonistData += "/ShirtIndex," + colonist.humanLookIndices[ColonistManager.Human.HumanLook.Shirt];
		colonistData += "/PantsIndex," + colonist.humanLookIndices[ColonistManager.Human.HumanLook.Pants];
		colonistData += "/Health," + colonist.health;
		colonistData += "/PlayerMoved," + colonist.playerMoved;
		colonistData += "/Profession," + colonist.profession.type;
		colonistData += "/OldProfession," + colonist.oldProfession.type;
		colonistData += "/InventoryMaxAmount," + colonist.inventory.maxAmount;
		/*
		 *	/InventoryResources:Count,InventoryResource:ResourceType:Amount,InventoryResource:ResourceType:Amount,...
		 *	
		 *	Split(',') -> ["InventoryResources" , "InventoryResource:ResourceType:Amount" , "..."]
		 *		Split[0](':') -> ["InventoryResources" , "Count"]
		 *		foreach skip 1 (i = 1 -> n):
		 *			Split[i](':') -> ["InventoryResource" , "ResourceType" , "Amount"]
		 */
		colonistData += "/InventoryResources:" + colonist.inventory.resources.Count;
		foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources) {
			colonistData += ",InventoryResource";
			colonistData += ":" + resourceAmount.resource.type;
			colonistData += ":" + resourceAmount.amount;
		}
		/*
		 *	/ReservedResources:Count,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...,ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...
		 * 
		 *	Split(',') -> ["ReservedResources:Count" , "ReservedResourcesColonist:ColonistName;ReservedResource:ResourceType:Amount;...", "..."]
		 *		Split[0](':') -> ["ReservedResources" , "Count"]
		 *		foreach skip 1 (i = 1 -> n):
		 *			Split[i](';') -> ["ReservedResourcesColonist:ColonistName" , "ReservedResources:ResourceType:Amount" , "..."]
		 *				Split[0](':') -> ["ReservedResourcesColonist" , "ColonistName"]
		 *				foreach skip 1 (k = 1 -> n):
		 *					Split[k](':') -> ["ReservedResources" , "ResourceType" , "Amount"]
		 */
		colonistData += "/ReservedResources:" + colonist.inventory.reservedResources.Count;
		foreach (ResourceManager.ReservedResources reservedResources in colonist.inventory.reservedResources) {
			colonistData += ",ReservedResourcesColonist:" + reservedResources.colonist;
			foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
				colonistData += ";ReservedResource";
				colonistData += ":" + resourceAmount.resource.type;
				colonistData += ":" + resourceAmount.amount;
			}
		}
		if (colonist.job != null) {
			colonistData += "/" + GetJobDataString(colonist.job, true).Replace('/', '~');
		} else {
			colonistData += "/None";
		}
		if (colonist.storedJob != null) {
			colonistData += "/" + GetJobDataString(colonist.storedJob, true).Replace('/', '~');
		} else {
			colonistData += "/None";
		}
		colonistData += "/Skills";
		foreach (ColonistManager.SkillInstance skill in colonist.skills) {
			colonistData += ",Skill";
			colonistData += ":" + skill.prefab.type;
			colonistData += ":" + skill.level;
			colonistData += ":" + skill.nextLevelExperience;
			colonistData += ":" + skill.currentExperience;
		}
		colonistData += "/Traits";
		foreach (ColonistManager.TraitInstance trait in colonist.traits) {
			colonistData += ",Trait";
			colonistData += ":" + trait.prefab.type;
		}
		colonistData += "/Needs";
		foreach (ColonistManager.NeedInstance need in colonist.needs) {
			colonistData += ",Need";
			colonistData += ":" + need.prefab.type;
			colonistData += ":" + need.value;
		}
		colonistData += "/BaseHappiness," + colonist.baseHappiness;
		colonistData += "/EffectiveHappiness," + colonist.effectiveHappiness;
		colonistData += "/HappinessModifiers";
		foreach (ColonistManager.HappinessModifierInstance happinessModifier in colonist.happinessModifiers) {
			colonistData += ",HappinessModifier";
			colonistData += ":" + happinessModifier.prefab.type;
			colonistData += ":" + happinessModifier.timer;
		}
		if (colonist.path.Count > 0) {
			TileManager.Tile pathEndTile = colonist.path[colonist.path.Count - 1];
			colonistData += "/PathEnd," + pathEndTile.obj.transform.position.x + "," + pathEndTile.obj.transform.position.y;
		} else {
			colonistData += "/None";
		}
		colonistData += "/";
		return colonistData;
	}

	public string GetJobDataString(JobManager.Job job, bool onColonist) {
		string jobData = "Job";
		jobData += "/Position," + job.tile.obj.transform.position.x + "," + job.tile.obj.transform.position.y;
		jobData += "/PrefabType," + job.prefab.type;
		jobData += "/RotationIndex," + job.rotationIndex;
		jobData += "/Started," + job.started;
		jobData += "/Progress," + job.jobProgress;
		jobData += "/ColonistBuildTime," + job.colonistBuildTime;
		// "/ResourceToBuild,ResourceType,Amount"
		jobData += "/ResourcesToBuild";
		foreach (ResourceManager.ResourceAmount resourceToBuild in job.resourcesToBuild) {
			jobData += ",ResourceToBuild";
			jobData += ":" + resourceToBuild.resource.type;
			jobData += ":" + resourceToBuild.amount;
		}
		if (onColonist) {
			/*
			 *	/OnColonist,ColonistResources;ColonistResource:ResourceType:Amount;...,ContainerPickups;ContainerPickup:x`y:ResourceToPickup`ResourceType`Amount:...;...
			 * 
			 *	Split(',') -> ["OnColonist" , "ColonistResources;..." , "ContainerPickups;..."]
			 *		Split[1](';') -> ["ColonistResources" , "ColonistResource:ResourceType:Amount" , "..."]
			 *			foreach skip 1 (i = 1 -> n):
			 *				Split[i](':') = ["ColonistResource" , "ResourceType" , "Amount"]
			 *		Split[2](';') -> ["ContainerPickups" , "ContainerPickup:x`y:ResourceToPickup`ResourceType`Amount:ResourceToPickup
			 *			foreach skip 1 (i = 1 -> n):
			 *				Split[i](':') = ["ContainerPickup" , "x`y" , "ResourceToPickup`ResourceType`Amount" , "..."]
			 *					Split[1]('`') = ["x" , "y"]
			 *					foreach skip 2 (k = 2 -> n):
			 *						Split[k]('`') = ["ResourceToPickup" , "ResourceType" , "Amount"]
			 */
			jobData += "/OnColonist";
			if (job.colonistResources != null) {
				jobData += ",ColonistResources";
				foreach (ResourceManager.ResourceAmount colonistResource in job.colonistResources) {
					jobData += ";ColonistResource";
					jobData += ":" + colonistResource.resource.type;
					jobData += ":" + colonistResource.amount;
				}
			} else {
				jobData += ",None";
			}
			if (job.containerPickups != null) {
				jobData += ",ContainerPickups";
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					jobData += ";ContainerPickup";
					jobData += ":" + containerPickup.container.parentObject.tile.obj.transform.position.x + "`" + containerPickup.container.parentObject.tile.obj.transform.position.y;
					foreach (ResourceManager.ResourceAmount resourceToPickup in containerPickup.resourcesToPickup) {
						jobData += ":ResourceToPickup`" + resourceToPickup.resource.type + "`" + resourceToPickup.amount;
					}
				}
			} else {
				jobData += ",None";
			}
		} else {
			jobData += "/None";
		}
		if (job.plant != null) {
			jobData += "/Plant," + job.plant.group.type + "," + (job.plant.harvestResource != null ? job.plant.harvestResource.type.ToString() : "None,");
		} else {
			jobData += "/None";
		}
		if (job.createResource != null) {
			jobData += "/CreateResource," + job.createResource.type;
		} else {
			jobData += "/None";
		}
		if (job.activeTileObject != null) {
			jobData += "/ActiveTileObject," + job.activeTileObject.tile.obj.transform.position.x + "," + job.activeTileObject.tile.obj.transform.position.y + "," + job.activeTileObject.prefab.type;
		} else {
			jobData += "/None";
		}
		jobData += "/Priority," + job.priority;
		jobData += "/";
		return jobData;
	}

	public void ResetGameState(bool fromMainMenu, bool destroyPlanet) {
		tileM.generated = false;

		if (!fromMainMenu) {
			colonistM.SetSelectedColonist(null);
			uiM.SetSelectedContainer(null);
			uiM.SetSelectedManufacturingTileObject(null);
		}

		if (destroyPlanet) {
			foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
				Destroy(planetTile.obj);
			}
		}

		if (tileM.map != null) {
			foreach (TileManager.Tile tile in tileM.map.tiles) {
				if (tile.plant != null) {
					tileM.map.smallPlants.Remove(tile.plant);
					Destroy(tile.plant.obj);
					tile.plant = null;
				}
				Destroy(tile.obj);
			}
			tileM.map.tiles.Clear();
			tileM.map = null;
		}

		foreach (KeyValuePair<ResourceManager.TileObjectPrefab, List<ResourceManager.TileObjectInstance>> objectInstanceKVP in resourceM.tileObjectInstances) {
			foreach (ResourceManager.TileObjectInstance objectInstance in objectInstanceKVP.Value) {
				Destroy(objectInstance.obj);
			}
			objectInstanceKVP.Value.Clear();
		}
		resourceM.tileObjectInstances.Clear();

		resourceM.manufacturingTileObjectInstances.Clear();

		resourceM.farms.Clear();

		resourceM.containers.Clear();

		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			Destroy(colonist.nameCanvas);
			Destroy(colonist.obj);
		}
		colonistM.colonists.Clear();
		uiM.RemoveColonistElements();

		foreach (JobManager.Job job in jobM.jobs) {
			Destroy(job.jobPreview);
		}
		jobM.jobs.Clear();
		uiM.RemoveJobElements();
	}

	public void LoadGame(string fileName, bool fromMainMenu) {
		ResetGameState(fromMainMenu,true);

		List<string> lines = new StreamReader(fileName).ReadToEnd().Split('\n').ToList();

		int sectionIndex = 0;
		List<int> sectionLengths = new List<int>();
		foreach (string section in lines[1].Split('/').Skip(1)) {
			int sectionLength = int.Parse(section.Split(',')[1]);
			int additionalLine = (sectionIndex == 3 ? 1 : 0);
			sectionLength += additionalLine;
			sectionLengths.Add(sectionLength);
			sectionIndex += 1;
		}

		// Planet Data
		TileManager.MapData planetData = null;

		// Map Data
		TileManager.MapData mapData = null;

		// Container Data
		Dictionary<ResourceManager.Container, string> containerReservedResourcesData = new Dictionary<ResourceManager.Container, string>();

		// Colonist Data
		Dictionary<ColonistManager.Colonist, string> colonistReservedResourcesData = new Dictionary<ColonistManager.Colonist, string>();

		sectionIndex = 0;
		int lastSectionEnd = 2;
		foreach (int sectionLength in sectionLengths) {
			if (sectionLength > 0) {
				int sectionStart = lastSectionEnd + 1;
				int sectionEnd = sectionStart + sectionLength;
				int innerSectionIndex = 0;
				for (int lineIndex = sectionStart; lineIndex < sectionEnd; lineIndex++) {
					string line = lines[lineIndex - 1];
					List<string> lineData = line.Split('/').ToList();
					if (sectionIndex == 0) { // Time/Date
						timeM.SetTime(float.Parse(lineData[1].Split(',')[1]));
						timeM.SetDate(int.Parse(lineData[2].Split(',')[1]), int.Parse(lineData[2].Split(',')[2]), int.Parse(lineData[2].Split(',')[3]));
					} else if (sectionIndex == 1) { // Camera
						cameraM.SetCameraPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						cameraM.SetCameraZoom(float.Parse(lineData[2].Split(',')[1]));
					} else if (sectionIndex == 2) { // Planet
						uiM.mainMenu.SetActive(true);
						int planetSeed = int.Parse(lineData[1].Split(',')[1]);
						int planetSize = int.Parse(lineData[2].Split(',')[1]);
						float planetDistance = float.Parse(lineData[3].Split(',')[1]);
						float planetTemperature = uiM.CalculatePlanetTemperature(planetDistance);
						int temperatureRange = int.Parse(lineData[4].Split(',')[1]);
						int windDirection = int.Parse(lineData[5].Split(',')[1]);
						planetData = new TileManager.MapData(
							null,
							planetSeed,
							planetSize,
							UIManager.StaticPlanetMapDataValues.actualMap,
							UIManager.StaticPlanetMapDataValues.equatorOffset,
							UIManager.StaticPlanetMapDataValues.planetTemperature,
							temperatureRange,
							planetDistance,
							planetTemperature,
							UIManager.StaticPlanetMapDataValues.averageTemperature,
							UIManager.StaticPlanetMapDataValues.averagePrecipitation,
							UIManager.StaticPlanetMapDataValues.terrainTypeHeights,
							UIManager.StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
							UIManager.StaticPlanetMapDataValues.preventEdgeTouching,
							windDirection,
							UIManager.StaticPlanetMapDataValues.planetTilePosition
						);
						uiM.planet = new TileManager.Map(planetData, false);
						foreach (TileManager.Tile tile in uiM.planet.tiles) {
							uiM.planetTiles.Add(new UIManager.PlanetTile(tile, uiM.planetPreviewPanel.transform, tile.position, planetData.mapSize, planetData.temperatureOffset));
						}
						uiM.mainMenu.SetActive(false);
					} else if (sectionIndex == 3) { // Tile
						if (innerSectionIndex == 0) {
							uiM.colonyName = lineData[1].Split(',')[1];
							int mapSeed = int.Parse(lineData[2].Split(',')[1]);
							int mapSize = int.Parse(lineData[3].Split(',')[1]);
							float equatorOffset = float.Parse(lineData[4].Split(',')[1]);
							float averageTemperature = float.Parse(lineData[5].Split(',')[1]);
							float averagePrecipitation = float.Parse(lineData[6].Split(',')[1]);
							int windDirection = int.Parse(lineData[7].Split(',')[1]);
							Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>();
							foreach (string terrainTypeHeightString in lineData[8].Split(',').Skip(1)) {
								terrainTypeHeights.Add((TileManager.TileTypes)System.Enum.Parse(typeof(TileManager.TileTypes), terrainTypeHeightString.Split(':')[0]), float.Parse(terrainTypeHeightString.Split(':')[1]));
							}
							List<int> surroundingPlanetTileHeightDirections = new List<int>();
							foreach (string surroundingPlanetTileHeightDirectionString in lineData[9].Split(',').Skip(1)) {
								surroundingPlanetTileHeightDirections.Add(int.Parse(surroundingPlanetTileHeightDirectionString));
							}
							Vector2 planetTilePosition = new Vector2(float.Parse(lineData[10].Split(',')[1]), float.Parse(lineData[10].Split(',')[2]));
							mapData = new TileManager.MapData(
								uiM.planet.mapData,
								mapSeed,
								mapSize,
								true,
								equatorOffset,
								false,
								0,
								0,
								0,
								averageTemperature,
								averagePrecipitation,
								terrainTypeHeights,
								surroundingPlanetTileHeightDirections,
								false,
								windDirection,
								planetTilePosition
							);
							tileM.map = new TileManager.Map(mapData, true);
							if (fromMainMenu) {
								uiM.InitializeGameUI();
							}
							uiM.pauseMenu.transform.Find("MapRegenerationCode-InputField").GetComponent<InputField>().text = mapData.mapRegenerationCode;
						} else {
							TileManager.Tile tile = tileM.map.tiles[innerSectionIndex - 1];

							TileManager.TileType savedTileType = tileM.GetTileTypeByEnum((TileManager.TileTypes)System.Enum.Parse(typeof(TileManager.TileTypes), lineData[0].Split(',')[0]));
							if (savedTileType != tile.tileType) {
								tile.SetTileType(savedTileType, false, false, false, false);
								if (tileM.GetHoleTileTypes().Contains(savedTileType.type)) {
									tile.dugPreviously = true;
								}
							}
							string spriteName = lineData[0].Split(',')[1];
							Sprite tileSprite = tile.tileType.baseSprites.Find(findTileSprite => findTileSprite.name == spriteName);
							if (tileSprite == null) {
								tileSprite = tile.tileType.bitmaskSprites.Find(findTileSprite => findTileSprite.name == spriteName);
								if (tileSprite == null) {
									tileSprite = tile.tileType.riverSprites.Find(findTileSprite => findTileSprite.name == spriteName);
								}
							}
							tile.sr.sprite = tileSprite;

							if (lineData[1] == "None") {
								if (tile.plant != null) {
									tile.SetPlant(true, null);
								}
							} else {
								ResourceManager.PlantGroup savedPlantGroup = resourceM.GetPlantGroupByEnum((ResourceManager.PlantGroupsEnum)System.Enum.Parse(typeof(ResourceManager.PlantGroupsEnum), lineData[1].Split(',')[0]));
								bool savedPlantSmall = bool.Parse(lineData[1].Split(',')[2]);
								ResourceManager.Resource harvestResource = null;
								if (lineData[1].Split(',')[4] != "None") {
									harvestResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[1].Split(',')[4]));
								}
								ResourceManager.Plant savedPlant = new ResourceManager.Plant(savedPlantGroup, tile, false, savedPlantSmall, tileM.map.smallPlants, false, harvestResource, resourceM) {
									growthProgress = float.Parse(lineData[1].Split(',')[3]),
									
								};
								tile.SetPlant(false, savedPlant);
								Sprite plantSprite = null;
								if (harvestResource != null) {
									if (savedPlantGroup.harvestResourceSprites.ContainsKey(harvestResource.type)) {
										if (savedPlantGroup.harvestResourceSprites[harvestResource.type].ContainsKey(savedPlantSmall)) {
											plantSprite = savedPlantGroup.harvestResourceSprites[harvestResource.type][savedPlantSmall].Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
										}
									}
								} else {
									if (savedPlantSmall) {
										plantSprite = savedPlantGroup.smallPlants.Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
									} else {
										plantSprite = savedPlantGroup.fullPlants.Find(findPlantSprite => findPlantSprite.name == lineData[1].Split(',')[1]);
									}
								}
								if (plantSprite != null) {
									tile.plant.obj.GetComponent<SpriteRenderer>().sprite = plantSprite;
								}
								
							}
							if (fromMainMenu && lineIndex == sectionEnd - 1) {
								uiM.MainMenuToGameTransition(true);
							}
						}
					} else if (sectionIndex == 4) { // River
						if (innerSectionIndex == 0) {
							foreach (TileManager.Map.River river in tileM.map.rivers) {
								river.tiles.Clear();
							}
							tileM.map.rivers.Clear();
						}
						TileManager.Tile startTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						TileManager.Tile endTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[2].Split(',')[1]), float.Parse(lineData[2].Split(',')[2])));
						List<TileManager.Tile> riverTiles = new List<TileManager.Tile>();
						foreach (string riverTilePositionString in lineData.Skip(3)) {
							riverTiles.Add(tileM.map.GetTileFromPosition(new Vector2(float.Parse(riverTilePositionString.Split(',')[0]), float.Parse(riverTilePositionString.Split(',')[1]))));
						}
						tileM.map.rivers.Add(new TileManager.Map.River(startTile, endTile, riverTiles));
					} else if (sectionIndex == 5) { // Resources
						foreach (string resourceData in lineData.Skip(1)) {
							ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceData.Split(',')[0]));
							resource.ChangeDesiredAmount(int.Parse(resourceData.Split(',')[1]));
						}
					} else if (sectionIndex == 6) { // Object
						TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						ResourceManager.TileObjectPrefab tileObjectPrefab = resourceM.GetTileObjectPrefabByEnum((ResourceManager.TileObjectPrefabsEnum)System.Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), lineData[2].Split(',')[1]));
						int rotationIndex = int.Parse(lineData[3].Split(',')[1]);
						float integrity = int.Parse(lineData[4].Split(',')[1]);
						tile.SetTileObject(tileObjectPrefab, rotationIndex);
						tile.GetObjectInstanceAtLayer(tileObjectPrefab.layer).integrity = integrity;
					} else if (sectionIndex == 7) { // Manufacturing Tile Object
						TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == tile);
						if (lineData[2] != "None") {
							mto.createResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[2].Split(',')[1]));
						}
						if (lineData[3] != "None") {
							mto.fuelResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), lineData[3].Split(',')[1]));
						}
						mto.active = bool.Parse(lineData[4].Split(',')[1]);
					} else if (sectionIndex == 8) { // Farm
						TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						ResourceManager.Farm farm = resourceM.farms.Find(findFarm => findFarm.tile == tile);
						farm.growTimer = float.Parse(lineData[3].Split(',')[1]);
						farm.maxGrowthTime = float.Parse(lineData[4].Split(',')[1]);
						farm.growProgressSpriteIndex = -1;
						farm.Update();
					} else if (sectionIndex == 9) { // Container
						TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == tile);
						ResourceManager.Inventory inventory = new ResourceManager.Inventory(null, container, int.Parse(lineData[2].Split(',')[1]));
						foreach (string inventoryResourceString in lineData[3].Split(',').Skip(1)) {
							ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), inventoryResourceString.Split(':')[1]));
							int amount = int.Parse(inventoryResourceString.Split(':')[2]);
							inventory.ChangeResourceAmount(resource, amount);
						}
						container.inventory = inventory;
						containerReservedResourcesData.Add(container,lineData[4]);
					} else if (sectionIndex == 10) { // Colonist
						Vector2 position = new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2]));
						TileManager.Tile tile = tileM.map.GetTileFromPosition(position);

						string name = lineData[2].Split(',')[1];

						ColonistManager.Life.Gender gender = (ColonistManager.Life.Gender)System.Enum.Parse(typeof(ColonistManager.Life.Gender), lineData[3].Split(',')[1]);

						Dictionary<ColonistManager.Human.HumanLook, int> humanLookIndices = new Dictionary<ColonistManager.Human.HumanLook, int>() {
							{ ColonistManager.Human.HumanLook.Skin, int.Parse(lineData[4].Split(',')[1]) },
							{ ColonistManager.Human.HumanLook.Hair, int.Parse(lineData[5].Split(',')[1]) },
							{ ColonistManager.Human.HumanLook.Shirt, int.Parse(lineData[6].Split(',')[1]) },
							{ ColonistManager.Human.HumanLook.Pants, int.Parse(lineData[7].Split(',')[1]) },
						};

						float health = float.Parse(lineData[8].Split(',')[1]);

						bool playerMoved = bool.Parse(lineData[9].Split(',')[1]);

						ColonistManager.Profession profession = colonistM.professions.Find(p => p.type.ToString() == lineData[10].Split(',')[1]);
						ColonistManager.Profession oldProfession = colonistM.professions.Find(p => p.type.ToString() == lineData[11].Split(',')[1]);

						ColonistManager.Colonist colonist = new ColonistManager.Colonist(tile, profession, health) {
							humanLookIndices = humanLookIndices,
							gender = gender
						};

						ResourceManager.Inventory inventory = new ResourceManager.Inventory(colonist, null, int.Parse(lineData[12].Split(',')[1]));
						foreach (string inventoryResourceString in lineData[13].Split(',').Skip(1)) {
							ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), inventoryResourceString.Split(':')[1]));
							int amount = int.Parse(inventoryResourceString.Split(':')[2]);
							inventory.ChangeResourceAmount(resource, amount);
						}
						colonistReservedResourcesData.Add(colonist,lineData[14]);

						JobManager.Job job = null;
						if (lineData[15] != "None") {
							List<string> jobDataSplit = lineData[15].Split('~').ToList();
							job = LoadJob(jobDataSplit);
							if (job.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
								ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == job.tile);
								mto.jobBacklog.Add(job);
							}
						}
						JobManager.Job storedJob = null;
						if (lineData[16] != "None") {
							List<string> jobDataSplit = lineData[16].Split('~').ToList();
							storedJob = LoadJob(jobDataSplit);
							if (storedJob.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
								ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == storedJob.tile);
								mto.jobBacklog.Add(storedJob);
							}
						}

						List<ColonistManager.SkillInstance> skills = new List<ColonistManager.SkillInstance>();
						foreach (string skillDataString in lineData[17].Split(',').Skip(1)) {
							ColonistManager.SkillPrefab skillPrefab = colonistM.GetSkillPrefabFromString(skillDataString.Split(':')[1]);
							int level = int.Parse(skillDataString.Split(':')[2]);
							float nextLevelExperience = float.Parse(skillDataString.Split(':')[3]);
							float currentExperience = float.Parse(skillDataString.Split(':')[4]);
							ColonistManager.SkillInstance skill = new ColonistManager.SkillInstance(colonist, skillPrefab, false, level) {
								colonist = colonist,
								prefab = skillPrefab,
								level = level,
								nextLevelExperience = nextLevelExperience,
								currentExperience = currentExperience
							};
							skills.Add(skill);
						}
						foreach (ColonistManager.SkillPrefab skillPrefab in colonistM.skillPrefabs.Where(sP => skills.Find(skill => skill.prefab == sP) == null)) {
							ColonistManager.SkillInstance skill = new ColonistManager.SkillInstance(colonist, skillPrefab, true, 0);
							skills.Add(skill);
						}

						List<ColonistManager.TraitInstance> traits = new List<ColonistManager.TraitInstance>();
						foreach (string traitDataString in lineData[18].Split(',').Skip(1)) {
							ColonistManager.TraitPrefab traitPrefab = colonistM.GetTraitPrefabFromString(traitDataString.Split(':')[1]);
							traits.Add(new ColonistManager.TraitInstance(colonist, traitPrefab));
						}
						foreach (ColonistManager.TraitPrefab traitPrefab in colonistM.traitPrefabs.Where(tP => traits.Find(trait => trait.prefab == tP) == null)) {
							ColonistManager.TraitInstance trait = new ColonistManager.TraitInstance(colonist, traitPrefab);
							traits.Add(trait);
						}

						List<ColonistManager.NeedInstance> needs = new List<ColonistManager.NeedInstance>();
						foreach (string needDataString in lineData[19].Split(',').Skip(1)) {
							ColonistManager.NeedPrefab needPrefab = colonistM.GetNeedPrefabFromString(needDataString.Split(':')[1]);
							float value = float.Parse(needDataString.Split(':')[2]);
							needs.Add(new ColonistManager.NeedInstance(colonist, needPrefab) { value = value });
						}
						foreach (ColonistManager.NeedPrefab needPrefab in colonistM.needPrefabs.Where(nP => needs.Find(need => need.prefab == nP) == null)) {
							ColonistManager.NeedInstance need = new ColonistManager.NeedInstance(colonist, needPrefab);
							needs.Add(need);
						}
						needs.OrderBy(need => need.prefab.priority);
						float baseHappiness = float.Parse(lineData[20].Split(',')[1]);
						float effectiveHappiness = float.Parse(lineData[21].Split(',')[1]);
						List<ColonistManager.HappinessModifierInstance> happinessModifiers = new List<ColonistManager.HappinessModifierInstance>();
						foreach (string happinessModifierString in lineData[22].Split(',').Skip(1)) {
							ColonistManager.HappinessModifierPrefab happinessModifierPrefab = colonistM.GetHappinessModifierPrefabFromString(happinessModifierString.Split(':')[1]);
							float timer = float.Parse(happinessModifierString.Split(':')[2]);
							happinessModifiers.Add(new ColonistManager.HappinessModifierInstance(colonist, happinessModifierPrefab) { timer = timer });
						}
						TileManager.Tile pathEndTile = null;
						if (lineData[23] != "None") {
							pathEndTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(lineData[23].Split(',')[1]), float.Parse(lineData[23].Split(',')[2])));
						}
						colonist.LoadColonistData(
							position,
							name,
							humanLookIndices,
							health,
							profession,
							oldProfession,
							inventory,
							job,
							storedJob,
							skills,
							traits,
							needs,
							baseHappiness,
							effectiveHappiness,
							happinessModifiers,
							playerMoved,
							pathEndTile
						);
						colonistM.AddColonist(colonist);
						if (lineIndex == sectionEnd - 1) {
							foreach (KeyValuePair<ColonistManager.Colonist, string> reservedResourcesStringKVP in colonistReservedResourcesData) {
								if (int.Parse(reservedResourcesStringKVP.Value.Split(',')[0].Split(':')[1]) > 0) {
									ColonistManager.Colonist reservedResourcesColonist = colonistM.colonists.Find(findColonist => findColonist.name == reservedResourcesStringKVP.Value.Split(';')[0].Split(':')[1]);
									List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
									foreach (string reservedResourceString in reservedResourcesStringKVP.Value.Split(';').Skip(1)) {
										ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), reservedResourceString.Split(':')[1]));
										int amount = int.Parse(reservedResourceString.Split(':')[2]);
										resourcesToReserve.Add(new ResourceManager.ResourceAmount(resource, amount));
									}
									reservedResourcesStringKVP.Key.inventory.ReserveResources(resourcesToReserve, reservedResourcesColonist);
								}
							}
							/*
							foreach (KeyValuePair<ResourceManager.Container, string> reservedResourcesStringKVP in containerReservedResourcesData) {
								if (int.Parse(reservedResourcesStringKVP.Value.Split(',')[0].Split(':')[1]) > 0) {
									ColonistManager.Colonist reservedResourcesColonist = colonistM.colonists.Find(findColonist => findColonist.name == reservedResourcesStringKVP.Value.Split(',')[1].Split(';')[0].Split(':')[1]);
									List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
									foreach (string reservedResourceString in reservedResourcesStringKVP.Value.Split(';').Skip(1)) {
										ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), reservedResourceString.Split(':')[1]));
										int amount = int.Parse(reservedResourceString.Split(':')[2]);
										resourcesToReserve.Add(new ResourceManager.ResourceAmount(resource, amount));
									}
									reservedResourcesStringKVP.Key.inventory.ReserveResources(resourcesToReserve, reservedResourcesColonist);
								}
							}
							*/
						}
					} else if (sectionIndex == 11) { // Job
						JobManager.Job job = LoadJob(lineData);
						jobM.AddExistingJob(job);
						if (job.prefab.jobType == JobManager.JobTypesEnum.CreateResource) {
							ResourceManager.ManufacturingTileObject mto = resourceM.manufacturingTileObjectInstances.Find(findMTO => findMTO.parentObject.tile == job.tile);
							mto.jobBacklog.Add(job);
						}
					}
					innerSectionIndex += 1;
				}
				lastSectionEnd = sectionEnd - 1;
			}
			sectionIndex += 1;
		}

		tileM.map.SetTileRegions(false);
		tileM.map.CreateRegionBlocks();
		tileM.map.DetermineShadowTiles(tileM.map.tiles, false);
		tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
		tileM.map.DetermineVisibleRegionBlocks();
		tileM.map.Bitmasking(tileM.map.tiles);
		resourceM.Bitmask(tileM.map.tiles);

		uiM.SetLoadMenuActive(false, false);
		if (!fromMainMenu) {
			uiM.TogglePauseMenu();
		}
		timeM.SetPaused(true);

		tileM.generated = true;
		tileM.generating = false;
	}

	public JobManager.Job LoadJob(List<string> jobDataSplit) {
		TileManager.Tile jobTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(jobDataSplit[1].Split(',')[1]), float.Parse(jobDataSplit[1].Split(',')[2])));
		ResourceManager.TileObjectPrefab jobPrefab = resourceM.GetTileObjectPrefabByEnum((ResourceManager.TileObjectPrefabsEnum)System.Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), jobDataSplit[2].Split(',')[1]));
		int rotationIndex = int.Parse(jobDataSplit[3].Split(',')[1]);
		bool started = bool.Parse(jobDataSplit[4].Split(',')[1]);
		float progress = float.Parse(jobDataSplit[5].Split(',')[1]);
		float colonistBuildTime = float.Parse(jobDataSplit[6].Split(',')[1]);
		List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();
		foreach (string resourceToBuildString in jobDataSplit[7].Split(',').Skip(1)) {
			ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceToBuildString.Split(':')[1]));
			int amount = int.Parse(resourceToBuildString.Split(':')[2]);
			resourcesToBuild.Add(new ResourceManager.ResourceAmount(resource, amount));
		}
		List<ResourceManager.ResourceAmount> colonistResources = new List<ResourceManager.ResourceAmount>();
		List<JobManager.ContainerPickup> containerPickups = new List<JobManager.ContainerPickup>();
		if (jobDataSplit[8] != "None") {
			List<string> onColonistDataSplit = jobDataSplit[8].Split(',').ToList();
			if (onColonistDataSplit[1] != "None") {
				foreach (string colonistResourceString in onColonistDataSplit[1].Split(';').Skip(1)) {
					ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), colonistResourceString.Split(':')[1]));
					int amount = int.Parse(colonistResourceString.Split(':')[2]);
					colonistResources.Add(new ResourceManager.ResourceAmount(resource, amount));
				}
			}
			if (onColonistDataSplit[2] != "None") {
				foreach (string containerPickupString in onColonistDataSplit[2].Split(';').Skip(1)) {
					List<string> containerPickupDataSplit = containerPickupString.Split(':').ToList();
					Vector2 containerPosition = new Vector2(float.Parse(containerPickupDataSplit[1].Split('`')[0]), float.Parse(containerPickupDataSplit[1].Split('`')[1]));
					TileManager.Tile containerTile = tileM.map.GetTileFromPosition(containerPosition);
					ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == containerTile);
					List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
					foreach (string resourceToPickupString in containerPickupDataSplit.Skip(2).ToList()) {
						ResourceManager.Resource resource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), resourceToPickupString.Split('`')[1]));
						int amount = int.Parse(resourceToPickupString.Split('`')[2]);
						resourcesToPickup.Add(new ResourceManager.ResourceAmount(resource, amount));
					}
					containerPickups.Add(new JobManager.ContainerPickup(container, resourcesToPickup));
				}
			}
		}

		JobManager.Job job = new JobManager.Job(jobTile, jobPrefab, rotationIndex) {
			started = started,
			jobProgress = progress,
			colonistBuildTime = colonistBuildTime,
			resourcesToBuild = resourcesToBuild,
			colonistResources = (colonistResources.Count > 0 ? colonistResources : null),
			containerPickups = (containerPickups.Count > 0 ? containerPickups : null)
		};

		if (jobDataSplit[9] != "None") {
			job.plant = new ResourceManager.Plant(resourceM.GetPlantGroupByEnum((ResourceManager.PlantGroupsEnum)System.Enum.Parse(typeof(ResourceManager.PlantGroupsEnum),jobDataSplit[9].Split(',')[1])), jobTile, false, true, tileM.map.smallPlants,false,(jobDataSplit[9].Split(',')[2] != "None" ? resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), jobDataSplit[9].Split(',')[2])) : null), resourceM);
		}
		if (jobDataSplit[10] != "None") {
			job.createResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), jobDataSplit[10].Split(',')[1]));
		}
		if (jobDataSplit[11] != "None") {
			ResourceManager.TileObjectPrefabsEnum activeTileObjectPrefab = (ResourceManager.TileObjectPrefabsEnum)System.Enum.Parse(typeof(ResourceManager.TileObjectPrefabsEnum), jobDataSplit[11].Split(',')[3]);
			TileManager.Tile activeTileObjectTile = tileM.map.GetTileFromPosition(new Vector2(float.Parse(jobDataSplit[11].Split(',')[1]), float.Parse(jobDataSplit[11].Split(',')[2])));
			ResourceManager.TileObjectInstance activeTileObject = activeTileObjectTile.objectInstances.Values.ToList().Find(oi => oi.tile == activeTileObjectTile && oi.prefab.type == activeTileObjectPrefab);
			job.activeTileObject = activeTileObject;
		}
		job.priority = int.Parse(jobDataSplit[12].Split(',')[1]);
		return job;
	}
}
