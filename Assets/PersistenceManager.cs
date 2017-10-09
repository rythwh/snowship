using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

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
		//FileStream file = new FileStream(Application.persistentDataPath + "/Saves/snowship-save" + System.DateTime.Now.ToUniversalTime() + ".snowship",FileMode.Create);

		// Save the planet data
		file.WriteLine("PlanetTiles/" + uiM.planetTiles.Count);
		foreach (UIManager.PlanetTile planetTile in uiM.planetTiles) {

		}

		// Save the tile data
		string tileMapData = "Tiles";
		tileMapData += "/TileCount," + tileM.map.tiles.Count;
		tileMapData += "/MapSeed," + tileM.map.mapData.mapSeed;
		tileMapData += "/MapSize," + tileM.map.mapData.mapSize;
		tileMapData += "/EquatorOffset," + tileM.map.mapData.equatorOffset;
		tileMapData += "/AverageTemperature," + tileM.map.mapData.averageTemperature;
		tileMapData += "/AveragePrecipitation," + tileM.map.mapData.averagePrecipitation;
		foreach (KeyValuePair<TileManager.TileTypes, float> terrainTypeHeightsKVP in tileM.map.mapData.terrainTypeHeights) {
			tileMapData += "/TerrainTypeHeight," + terrainTypeHeightsKVP.Key + "," + terrainTypeHeightsKVP.Value;
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

		// Save manufacturing tile object data
		foreach (ResourceManager.ManufacturingTileObject mto in resourceM.manufacturingTileObjectInstances) {
			file.WriteLine(GetManufacturingTileObjectDataString(mto));
		}

		// Save farm object data
		foreach (ResourceManager.Farm farm in resourceM.farms) {
			file.WriteLine(GetFarmDataString(farm));
		}

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

		file.Close();
	}

	/*
		"xPos,yPos/height/temperature/precipitation/dugPreviously"

		Example: "35,45/0.25/23/0.4/false"
	*/
	public string GetTileDataString(TileManager.Tile tile) {
		string tileData = string.Empty;
		tileData += tile.obj.transform.position.x + "," + tile.obj.transform.position.y;
		tileData += "/" + tile.height;
		tileData += "/" + tile.temperature;
		tileData += "/" + tile.precipitation;
		tileData += "/" + tile.dugPreviously;
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
			riverData += "/RiverTile," + riverTile.obj.transform.position.x + "," + riverTile.obj.transform.position.y;
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
		colonistData += "/SkinIndex," + colonist.skinIndex;
		colonistData += "/HairIndex," + colonist.hairIndex;
		colonistData += "/ShirtIndex," + colonist.shirtIndex;
		colonistData += "/PantsIndex," + colonist.pantsIndex;
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

	/*
		"Colonist/xPos,yPos/playerMoved/professionType/oldProfessionType/Job,jobData/StoredJob,storedJobData/Skill,skillData/Trait,traitData/Need,needData/Human,humanData/Life,lifeData"

		Example: "35,45/Brick/Wood"
	*/
	/*
	public string GetColonistDataString(ColonistManager.Colonist colonist) {
		string colonistData = "Colonist";
		colonistData += "/" + colonist.obj.transform.position.x + "," + colonist.obj.transform.position.y;
		colonistData += "/" + colonist.playerMoved;
		colonistData += "/" + colonist.profession.type;
		colonistData += "/" + colonist.oldProfession.type;
		if (colonist.job != null) {
			colonistData += "/Job," + GetJobDataString(colonist.job, false);
		} else {
			colonistData += "/Job,None";
		}
		if (colonist.storedJob != null) {
			colonistData += "/StoredJob," + GetJobDataString(colonist.storedJob, false);
		} else {
			colonistData += "/StoredJob,None";
		}
		colonistData += "Skills`";
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
		colonistData += "/" + GetHumanDataString(colonist);
		colonistData += "/" + GetLifeDataString(colonist);
		return colonistData;
	}

	public string GetHumanDataString(ColonistManager.Human human) {
		string humanData = "Human";
		return humanData;
	}

	public string GetLifeDataString(ColonistManager.Life life) {
		string lifeData = "Life";
		return lifeData;
	}
	*/

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

	/*
	public string GetJobDataString(JobManager.Job job, bool includePosition) {
		string jobData = string.Empty;
		if (includePosition) {
			jobData += job.tile.obj.transform.position.x + "/" + job.tile.obj.transform.position.y;
		}
		jobData += "/" + job.prefab.type;
		jobData += "/" + job.rotationIndex;
		jobData += "/" + job.started;
		jobData += "/" + job.jobProgress;
		jobData += "/" + job.colonistBuildTime;
		foreach (ResourceManager.ResourceAmount resourceToBuild in job.resourcesToBuild) {
			jobData += "/ResourceToBuild," + GetResourceAmountDataString(resourceToBuild);
		}
		foreach (ResourceManager.ResourceAmount colonistResource in job.colonistResources) {
			jobData += "/ColonistResource," + GetResourceAmountDataString(colonistResource);
		}
		foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
			jobData += "/ContainerPickup," + GetContainerPickupDataString(containerPickup);
		}
		return jobData;
	}
	*/

	/*
	public string GetResourceAmountDataString(ResourceManager.ResourceAmount resourceAmount) {
		string resourceAmountData = string.Empty;
		resourceAmountData += resourceAmount.resource.type;
		resourceAmountData += "," + resourceAmount.amount;
		return resourceAmountData;
	}

	public string GetContainerPickupDataString(JobManager.ContainerPickup containerPickup) {
		string containerPickupData = string.Empty;
		containerPickupData += GetContainerDataString(containerPickup.container);
		foreach (ResourceManager.ResourceAmount resourceToPickup in containerPickup.resourcesToPickup) {
			containerPickupData += "`ResourceToPickup,";
			containerPickupData += "," + GetResourceAmountDataString(resourceToPickup);
		}
		return containerPickupData;
	}

	public string GetContainerDataString(ResourceManager.Container container) {
		string containerData = string.Empty;
		return containerData;
	}

	public string GetFarmDataString(ResourceManager.Farm farm) {
		string farmData = string.Empty;
		return farmData;
	}

	// "maxAmount/ReservedResources|Colonist:N,ResourceName:Amount,ResourceName:Amount/Resources,

	public string GetInventoryDataString(ResourceManager.Inventory inventory) {
		string inventoryData = string.Empty;
		inventoryData += inventory.maxAmount;

		return inventoryData;
	}

	public string GetReservedResourceDataString(ResourceManager.ReservedResources reservedResource) {
		string reservedResourceData = string.Empty;

		return reservedResourceData;
	}
	*/

	public void LoadGame() {
		// Load the tile data

		// Load the colonist data

		// Load the job data

		// Load the time data

		// Load the camera data

		// Load the planet data
	}
}
