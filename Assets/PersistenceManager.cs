using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;

public class PersistenceManager : MonoBehaviour {

	private int gameVersion;
	private int saveVersion;

	public int GetGameVersion() {
		return gameVersion;
	}

	public int GetSaveVersion() {
		return saveVersion;
	}

	private CameraManager cameraM; // Save the camera zoom and position
	private ColonistManager colonistM; // Save all instances of colonists, humans, life, etc.
	private JobManager jobM; // Save all non-taken jobs
	private TileManager tileM; // Save the tile data
	private TimeManager timeM; // Save the date/time
	private UIManager uiM; // Save the planet data
	private ResourceManager resourceM; // Save the object data

	void Awake() {
		gameVersion = 1;
		saveVersion = 1;

		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		jobM = GetComponent<JobManager>();
		tileM = GetComponent<TileManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
		resourceM = GetComponent<ResourceManager>();
	}

	public void SaveSettings() {
		string settingsFilePath = Application.persistentDataPath + "/Settings/settings.txt";
		FileStream settingsFile = new FileStream(settingsFilePath, FileMode.OpenOrCreate);
		File.WriteAllText(@settingsFilePath, string.Empty);
	}

	public void LoadSettings() {

	}

	/* https://stackoverflow.com/questions/13266496/easily-write-a-whole-class-instance-to-xml-file-and-read-back-in */

	public string GenerateSaveFileName() {
		System.DateTime now = System.DateTime.Now;
		string dateTime = now.Year + "y" + now.Month + "m" + now.Day + "d" + now.Hour + "h" + now.Minute + "m" + now.Second + "s" + now.Millisecond + "m";
		string fileName = "snowship-save-" + uiM.colonyName + "-" + dateTime + ".snowship";
		return fileName;
	}

	public string GenerateSavePath(string fileName) {
		return Application.persistentDataPath + "/Saves/" + fileName;
	}

	public void SaveGame(string fileName) {
		fileName = GenerateSavePath(fileName);

		string directory = GenerateSavePath(string.Empty);
		print(directory);
		Directory.CreateDirectory(directory);
		print(fileName);
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
		saveFileFormatData += "/ObjectInstances," + resourceM.tileObjectInstances.Values.Sum(objList => objList.Count);
		saveFileFormatData += "/MTOs," + resourceM.manufacturingTileObjectInstances.Count;
		saveFileFormatData += "/Farms," + resourceM.farms.Count;
		saveFileFormatData += "/Container," + resourceM.containers.Count;
		saveFileFormatData += "/Colonists," + colonistM.colonists.Count;
		saveFileFormatData += "/Jobs," + jobM.jobs.Count;
		file.WriteLine(saveFileFormatData);

		// Save the time data
		string timeData = "Time";
		timeData += "/Time," + timeM.GetTileBrightnessTime();
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
		file.WriteLine(planetData);
		/*
		foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
			file.WriteLine(GetPlanetTileDataString(planetTile));
		}
		*/

		// Save the tile data
		string tileMapData = "Tiles";
		tileMapData += "/MapSeed," + tileM.map.mapData.mapSeed;
		tileMapData += "/MapSize," + tileM.map.mapData.mapSize;
		tileMapData += "/EquatorOffset," + tileM.map.mapData.equatorOffset;
		tileMapData += "/AverageTemperature," + tileM.map.mapData.averageTemperature;
		tileMapData += "/AveragePrecipitation," + tileM.map.mapData.averagePrecipitation;
		tileMapData += "/TerrainTypeHeights";
		foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightsKVP in tileM.map.mapData.terrainTypeHeights) {
			tileMapData += "," + terrainTypeHeightsKVP.Key + ":" + terrainTypeHeightsKVP.Value;
		}
		tileMapData += "/SurroundingPlanetTileHeightDirections";
		foreach (int surroundingPlanetTileHeightDirection in tileM.map.mapData.surroundingPlanetTileHeightDirections) {
			tileMapData += "," + surroundingPlanetTileHeightDirection;
		}
		file.WriteLine(tileMapData);
		foreach (TileManager.Tile tile in tileM.map.tiles) {
			file.WriteLine(GetTileDataString(tile));
		}

		// Save the river data
		foreach (TileManager.Map.River river in tileM.map.rivers) {
			file.WriteLine(GetRiverDataString(river));
		}

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
		/*
		tileData += tile.obj.transform.position.x + "," + tile.obj.transform.position.y;
		tileData += "/" + tile.height;
		tileData += "/" + tile.temperature;
		tileData += "/" + tile.precipitation;
		*/
		tileData += tile.tileType.type;
		if (tile.plant != null) {
			tileData += "/" + tile.plant.group.type + "," + tile.plant.small;
		} else {
			tileData += "/None/";
		}
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
		objectInstanceData += "/Position," + objectInstance.obj.transform.position.x + "," + objectInstance.obj.transform.position.y;
		objectInstanceData += "/PrefabType," + objectInstance.prefab.type;
		objectInstanceData += "/RotationIndex," + objectInstance.rotationIndex;
		return objectInstanceData;
	}

	/*
		"MTO/Position,x,y/CreateResource,resourceType/FuelResource,resourceType"

		Example: "MTO/Position,35.0,45.0/CreateResource,Brick/FuelResource,Wood"
	*/
	public string GetManufacturingTileObjectDataString(ResourceManager.ManufacturingTileObject mto) {
		string mtoData = "MTO";
		mtoData += "/Position," + mto.parentObject.obj.transform.position.x + "," + mto.parentObject.obj.transform.position.y;
		if (mto.createResource != null) {
			mtoData += "/CreateResource," + mto.createResource.type;
		}
		if (mto.fuelResource != null) {
			mtoData += "/FuelResource," + mto.fuelResource.type;
		}
		return mtoData;
	}

	/*
		"Farm/Position,x,y/SeedType,seedType/GrowTimer,growTimer/MaxGrowthTime,maxGrowthTime"

		Example: "Farm/Position,35.0,45.0/SeedType,PotatoSeeds/GrowTimer,100.51/MaxGrowthTime,1440"
	*/
	public string GetFarmDataString(ResourceManager.Farm farm) {
		string farmData = "Farm";
		farmData += "/Position," + farm.obj.transform.position.x + "," + farm.obj.transform.position.y;
		farmData += "/SeedType," + farm.seedType;
		farmData += "/GrowTimer," + farm.growTimer;
		farmData += "/MaxGrowthTime," + farm.maxGrowthTime;
		return farmData;
	}

	public string GetContainerDataString(ResourceManager.Container container) {
		string containerData = "Container";
		containerData += "/Position," + container.parentObject.obj.transform.position.x + "," + container.parentObject.obj.transform.position.y;
		containerData += "/InventoryMaxAmount," + container.inventory.maxAmount;
		foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources) {
			containerData += "/InventoryResource";
			containerData += "," + resourceAmount.resource.type;
			containerData += "," + resourceAmount.amount;
		}
		foreach (ResourceManager.ReservedResources reservedResources in container.inventory.reservedResources) {
			containerData += "/ReservedResourcesColonist," + reservedResources.colonist;
			foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
				containerData += ",ReservedResource";
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
		colonistData += "/SkinIndex," + colonist.colonistLookIndexes[ColonistManager.ColonistLook.Skin];
		colonistData += "/HairIndex," + colonist.colonistLookIndexes[ColonistManager.ColonistLook.Hair];
		colonistData += "/ShirtIndex," + colonist.colonistLookIndexes[ColonistManager.ColonistLook.Shirt];
		colonistData += "/PantsIndex," + colonist.colonistLookIndexes[ColonistManager.ColonistLook.Pants];
		colonistData += "/Health," + colonist.health;
		colonistData += "/PlayerMoved," + colonist.playerMoved;
		colonistData += "/Profession," + colonist.profession.type;
		colonistData += "/OldProfession," + colonist.oldProfession.type;
		colonistData += "/InventoryMaxAmount," + colonist.inventory.maxAmount;
		foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources) {
			colonistData += "/InventoryResource";
			colonistData += "," + resourceAmount.resource.type;
			colonistData += "," + resourceAmount.amount;
		}
		foreach (ResourceManager.ReservedResources reservedResources in colonist.inventory.reservedResources) {
			colonistData += "/ReservedResourcesColonist," + reservedResources.colonist;
			foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
				colonistData += ",ReservedResource";
				colonistData += ":" + resourceAmount.resource.type;
				colonistData += ":" + resourceAmount.amount;
			}
		}
		if (colonist.job != null) {
			colonistData += "/" + GetJobDataString(colonist.job, true).Replace('/', '`');
		}
		if (colonist.storedJob != null) {
			colonistData += "/" + GetJobDataString(colonist.storedJob, true).Replace('/', '`');
		}
		foreach (ColonistManager.SkillInstance skill in colonist.skills) {
			colonistData += "/Skill";
			colonistData += "," + skill.prefab.type;
			colonistData += "," + skill.level;
			colonistData += "," + skill.nextLevelExperience;
			colonistData += "," + skill.currentExperience;
		}
		foreach (ColonistManager.TraitInstance trait in colonist.traits) {
			colonistData += "/Trait";
			colonistData += "," + trait.prefab.type;
		}
		foreach (ColonistManager.NeedInstance need in colonist.needs) {
			colonistData += "/Need";
			colonistData += "," + need.prefab.type;
			colonistData += "," + need.value;
		}
		colonistData += "/BaseHappiness," + colonist.baseHappiness;
		colonistData += "/EffectiveHappiness," + colonist.effectiveHappiness;
		foreach (ColonistManager.HappinessModifierInstance happinessModifier in colonist.happinessModifiers) {
			colonistData += "/HappinessModifier";
			colonistData += "," + happinessModifier.prefab.type;
			colonistData += "," + happinessModifier.timer;
		}
		return colonistData;
	}

	public string GetJobDataString(JobManager.Job job, bool onColonist) {
		string jobData = "Job";
		jobData += "/Position," + job.tile.obj.transform.position.x + "," + job.tile.obj.transform.position.y;
		jobData += "/PrefabType," + job.prefab.type;
		jobData += "/RotationIndex," + job.rotationIndex;
		jobData += "/Started," + job.started;
		jobData += "/Progress," + job.jobProgress;
		jobData += "/ColonistBuildTime" + job.colonistBuildTime;
		// "/ResourceToBuild,ResourceType,Amount"
		foreach (ResourceManager.ResourceAmount resourceToBuild in job.resourcesToBuild) {
			jobData += "/ResourceToBuild";
			jobData += "," + resourceToBuild.resource.type;
			jobData += "," + resourceToBuild.amount;
		}
		if (onColonist) {
			if (job.colonistResources != null) {
				// "/ColonistResource,ResourceType,Amount"
				foreach (ResourceManager.ResourceAmount colonistResource in job.colonistResources) {
					jobData += "/ColonistResource";
					jobData += "," + colonistResource.resource.type;
					jobData += "," + colonistResource.amount;
				}
			}
			if (job.containerPickups != null) {
				// "/ContainerPickup,xPos,yPos,ResourceToPickup:ResourceType:Amount,ResourceToPickup:ResourceType:Amount,..."
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					jobData += "/ContainerPickup";
					jobData += "," + containerPickup.container.parentObject.obj.transform.position.x + "," + containerPickup.container.parentObject.obj.transform.position.y;
					foreach (ResourceManager.ResourceAmount resourceToPickup in containerPickup.resourcesToPickup) {
						jobData += ",ResourceToPickup:" + resourceToPickup.resource.type + ":" + resourceToPickup.amount;
					}
				}
			}
		}
		if (job.plant != null) {
			jobData += "/Plant," + job.plant.group.type;
		}
		if (job.createResource != null) {
			jobData += "/CreateResource," + job.createResource.type;
		}
		if (job.activeTileObject != null) {
			jobData += "/ActiveTileObject," + job.activeTileObject.obj.transform.position.x + "," + job.activeTileObject.obj.transform.position.y + "," + job.activeTileObject.prefab.type;
		}
		return jobData;
	}

	public void LoadGame(string fileName) {

		tileM.generated = false;

		foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {
			Destroy(planetTile.obj);
		}

		foreach (TileManager.Tile tile in tileM.map.tiles) {
			if (tile.plant != null) {
				Destroy(tile.plant.obj);
				tile.plant = null;
			}
			Destroy(tile.obj);
		}
		tileM.map.tiles.Clear();
		tileM.map = null;

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
		TileManager.Map planet = null;

		// Map Data
		TileManager.MapData mapData = null;

		sectionIndex = 0;
		int lastSectionEnd = 2;
		foreach (int sectionLength in sectionLengths) {
			if (sectionLength > 0) {
				int sectionStart = lastSectionEnd + 1;
				int sectionEnd = sectionStart + sectionLength;
				print(sectionStart + " -> " + (sectionEnd - 1));
				int innerSectionIndex = 0;
				for (int lineIndex = sectionStart; lineIndex < sectionEnd; lineIndex++) {
					List<string> lineData = lines[lineIndex - 1].Split('/').ToList();
					if (sectionIndex == 0) { // Time/Date
						timeM.SetTime(float.Parse(lineData[1].Split(',')[1]));
						timeM.SetDate(int.Parse(lineData[2].Split(',')[1]), int.Parse(lineData[2].Split(',')[1]), int.Parse(lineData[2].Split(',')[1]));
					} else if (sectionIndex == 1) { // Camera
						print(lineData[1].Split(',')[1]);
						cameraM.SetCameraPosition(new Vector2(float.Parse(lineData[1].Split(',')[1]), float.Parse(lineData[1].Split(',')[2])));
						cameraM.SetCameraZoom(float.Parse(lineData[2].Split(',')[1]));
					} else if (sectionIndex == 2) { // Planet
						uiM.mainMenu.SetActive(true);
						int planetSeed = int.Parse(lineData[1].Split(',')[1]);
						int planetSize = int.Parse(lineData[2].Split(',')[1]);
						float planetDistance = float.Parse(lineData[3].Split(',')[1]);
						float planetTemperature = uiM.CalculatePlanetTemperature(planetDistance);
						int temperatureRange = int.Parse(lineData[4].Split(',')[1]);
						planetData = new TileManager.MapData(
							planetSeed,
							planetSize,
							UIManager.StaticPlanetMapDataValues.actualMap,
							UIManager.StaticPlanetMapDataValues.equatorOffset,
							UIManager.StaticPlanetMapDataValues.planetTemperature,
							temperatureRange,
							planetTemperature,
							UIManager.StaticPlanetMapDataValues.averageTemperature,
							UIManager.StaticPlanetMapDataValues.averagePrecipitation,
							UIManager.StaticPlanetMapDataValues.terrainTypeHeights,
							UIManager.StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
							UIManager.StaticPlanetMapDataValues.preventEdgeTouching
						);
						planet = new TileManager.Map(planetData, false);
						foreach (TileManager.Tile tile in planet.tiles) {
							uiM.planetTiles.Add(new UIManager.PlanetTile(tile, uiM.planetPreviewPanel.transform, tile.position, planetData.mapSize, planetData.temperatureOffset));
						}
						uiM.mainMenu.SetActive(false);
					} else if (sectionIndex == 3) { // Tile
						if (innerSectionIndex == 0) {
							int mapSeed = int.Parse(lineData[1].Split(',')[1]);
							int mapSize = int.Parse(lineData[2].Split(',')[1]);
							float equatorOffset = float.Parse(lineData[3].Split(',')[1]);
							float averageTemperature = float.Parse(lineData[4].Split(',')[1]);
							float averagePrecipitation = float.Parse(lineData[5].Split(',')[1]);
							Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>();
							foreach (string terrainTypeHeightString in lineData[6].Split(',').Skip(1)) {
								terrainTypeHeights.Add((TileManager.TileTypes)System.Enum.Parse(typeof(TileManager.TileTypes), terrainTypeHeightString.Split(':')[0]), float.Parse(terrainTypeHeightString.Split(':')[1]));
							}
							List<int> surroundingPlanetTileHeightDirections = new List<int>();
							foreach (string surroundingPlanetTileHeightDirectionString in lineData[7].Split(',').Skip(1)) {
								surroundingPlanetTileHeightDirections.Add(int.Parse(surroundingPlanetTileHeightDirectionString));
							}
							mapData = new TileManager.MapData(
								mapSeed,
								mapSize,
								true,
								equatorOffset,
								false,
								0,
								0,
								averageTemperature,
								averagePrecipitation,
								terrainTypeHeights,
								surroundingPlanetTileHeightDirections,
								false
							);
							tileM.map = new TileManager.Map(mapData, true);
						} else {
							TileManager.Tile tile = tileM.map.tiles[innerSectionIndex-1];

							TileManager.TileType savedTileType = tileM.GetTileTypeByEnum((TileManager.TileTypes)System.Enum.Parse(typeof(TileManager.TileTypes), lineData[0]));
							if (savedTileType != tile.tileType) {
								tile.SetTileType(savedTileType,true,false,false,false);
								if (tileM.GetHoleTileTypes().Contains(savedTileType.type)) {
									tile.dugPreviously = true;
								}
							}

							if (lineData[1] == "None") {
								if (tile.plant != null) {
									tile.SetPlant(true, null);
								}
							} else {
								TileManager.PlantGroup savedPlantGroup = tileM.GetPlantGroupByEnum((TileManager.PlantGroupsEnum)System.Enum.Parse(typeof(TileManager.PlantGroupsEnum), lineData[1].Split(',')[0]));
								bool savedPlantSmall = bool.Parse(lineData[1].Split(',')[1]);
								TileManager.Plant savedPlant = new TileManager.Plant(savedPlantGroup, tile, false, savedPlantSmall);
								tile.SetPlant(false, savedPlant);
							}
						}
					} else if (sectionIndex == 4) { // River

					} else if (sectionIndex == 5) { // Object

					} else if (sectionIndex == 6) { // Manufacturing Tile Object

					} else if (sectionIndex == 7) { // Farm

					} else if (sectionIndex == 8) { // Container

					} else if (sectionIndex == 9) { // Colonist

					} else if (sectionIndex == 10) { // Job

					}
					innerSectionIndex += 1;
				}
				lastSectionEnd = sectionEnd - 1;
			}
			sectionIndex += 1;
		}

		tileM.map.DetermineShadowTiles(tileM.map.tiles, false);
		tileM.map.SetTileBrightness(timeM.GetTileBrightnessTime());
		tileM.map.DetermineVisibleRegionBlocks();
		tileM.map.Bitmasking(tileM.map.tiles);

		return;

		// Load the time data

		// Load the camera data

		// Load the planet data

		// Load the tile data

		// Load the river data

		// Load the object data

		// Load the manufacturing tile object data

		// Load the farm data

		// Load the container data

		// Load the colonist data

		// Load the job data
	}
}
