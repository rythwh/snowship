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

		StreamWriter file = new StreamWriter(fileName);
		//FileStream file = new FileStream(Application.persistentDataPath + "/Saves/snowship-save" + System.DateTime.Now.ToUniversalTime() + ".snowship",FileMode.Create);

		// Save the planet data

		// Save the tile data
		foreach (TileManager.Tile tile in tileM.map.tiles) {
			file.WriteLine(GetTileDataString(tile));
		}

		// Save the object data
		foreach (KeyValuePair<ResourceManager.TileObjectPrefab,List<ResourceManager.TileObjectInstance>> objectInstanceKVP in resourceM.tileObjectInstances) {
			foreach (ResourceManager.TileObjectInstance objectInstance in objectInstanceKVP.Value) {
				file.WriteLine(GetObjectInstanceDataString(objectInstance));
			}
		}

		// Save manufacturing tile object data
		foreach (ResourceManager.ManufacturingTileObject mto in resourceM.manufacturingTileObjectInstances) {
			file.WriteLine(GetManufacturingTileObjectDataString(mto));
		}

		// Save the colonist data
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			file.WriteLine(GetColonistDataString(colonist));
		}

		// Save the job data

		// Save the time data

		// Save the camera data

	}

	/*
		"xPos,yPos/height/temperature/precipitation/dugPreviously"

		Example: "35,45/0.25/23/0.4/false"
	*/
	public string GetTileDataString(TileManager.Tile tile) {
		string tileData = string.Empty;
		tileData += tile.obj.transform.position.x + "/" + tile.obj.transform.position.y;
		tileData += "/" + tile.height;
		tileData += "/" + tile.temperature;
		tileData += "/" + tile.precipitation;
		tileData += "/" + tile.dugPreviously;
		return tileData;
	}

	/*
		"xPos,yPos/prefabType/rotationIndex"

		Example: "35,45/WoodenChest/0"
	*/
	public string GetObjectInstanceDataString(ResourceManager.TileObjectInstance objectInstance) {
		string objectInstanceData = string.Empty;
		objectInstanceData += objectInstance.obj.transform.position.x + "," + objectInstance.obj.transform.position.y;
		objectInstanceData += "/" + objectInstance.prefab.type;
		objectInstanceData += "/" + objectInstance.rotationIndex;
		return objectInstanceData;
	}

	/*
		"xPos,yPos/createResourceType/fuelResourceType"

		Example: "35,45/Brick/Wood"
	*/
	public string GetManufacturingTileObjectDataString(ResourceManager.ManufacturingTileObject mto) {
		string mtoInstanceData = "MTO";
		mtoInstanceData += "/" + mto.parentObject.obj.transform.position.x + "," + mto.parentObject.obj.transform.position.y;
		mtoInstanceData += "/" + mto.createResource.type;
		mtoInstanceData += "/" + mto.fuelResource.type;
		return mtoInstanceData;
	}

	/*
		"Colonist/xPos,yPos/playerMoved/professionType/oldProfessionType/Job,jobData/StoredJob,storedJobData/Skill,skillData/Trait,traitData/Need,needData/Human,humanData/Life,lifeData"

		Example: "35,45/Brick/Wood"
	*/
	public string GetColonistDataString(ColonistManager.Colonist colonist) {
		string colonistData = "Colonist";
		colonistData += "/" + colonist.obj.transform.position.x + "," + colonist.obj.transform.position.y;
		colonistData += "/" + colonist.playerMoved;
		colonistData += "/" + colonist.profession.type;
		colonistData += "/" + colonist.oldProfession.type;
		if (colonist.job != null) {
			colonistData += "/Job," + GetJobDataString(colonist.job,false);
		}
		if (colonist.storedJob != null) {
			colonistData += "/StoredJob," + GetJobDataString(colonist.storedJob,false);
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
		
	}

	public void LoadGame() {
		// Load the tile data

		// Load the colonist data

		// Load the job data

		// Load the time data

		// Load the camera data

		// Load the planet data
	}
}
