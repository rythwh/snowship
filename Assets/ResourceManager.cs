using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : MonoBehaviour {

	private UIManager uiM;
	private TileManager tileM;

	void Awake() {
		uiM = GetComponent<UIManager>();
		tileM = GetComponent<TileManager>();
	}

	void Update() {
		foreach (Farm farm in farms) {
			farm.Update();
		}
	}

	public enum ResourceGroupsEnum {
		Natural, Materials,
		Seeds, RawFoods, Foods
	};

	public enum ResourcesEnum {
		Wood, Stone, Cloth, Dirt, Granite, Limestone, Marble, Sandstone, Slate, Clay, Firewood,
		WheatSeeds, Wheat, PotatoSeeds, Potatoes, Berries, Apples
	};

	public List<ResourceGroup> resourceGroups = new List<ResourceGroup>();

	public class ResourceGroup {

		public ResourceGroupsEnum type;
		public string name;

		public List<ResourcesEnum> resourceTypes = new List<ResourcesEnum>();
		public List<Resource> resources = new List<Resource>();

		public ResourceGroup(List<string> resourceGroupData, ResourceManager rm) {
			type = (ResourceGroupsEnum)System.Enum.Parse(typeof(ResourceGroupsEnum),resourceGroupData[0]);
			name = type.ToString();

			List<string> resourceData = resourceGroupData[1].Split('`').ToList();
			foreach (string resourceString in resourceData) {
				Resource resource = new Resource(resourceString.Split('/').ToList(),this,rm);
				resourceTypes.Add(resource.type);
				resources.Add(resource);
				rm.resources.Add(resource);
			}
		}
	}

	public List<Resource> resources = new List<Resource>();

	public class Resource {
		public ResourcesEnum type;
		public string name;
		
		public ResourceGroup resourceGroup;

		public int value;

		public int nutrition = 0;

		public Resource(List<string> resourceData,ResourceGroup resourceGroup, ResourceManager rm) {
			type = (ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum),resourceData[0]);
			name = type.ToString();

			this.resourceGroup = resourceGroup;

			value = int.Parse(resourceData[1]);

			if (resourceGroup.type == ResourceGroupsEnum.Foods) {
				nutrition = int.Parse(resourceData[2]);
			}
		}
	}

	public void CreateResources() {
		List<string> resourceGroupsDataString = UnityEngine.Resources.Load<TextAsset>(@"Data/resources").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('~').ToList();
		foreach (string resourceGroupDataString in resourceGroupsDataString) {
			List<string> resourceGroupData = resourceGroupDataString.Split(':').ToList();
			ResourceGroup resourceGroup = new ResourceGroup(resourceGroupData,this);
			resourceGroups.Add(resourceGroup);
		}
		tileM.CreatePlantResources();
	}

	public Resource GetResourceByEnum(ResourcesEnum resourceEnum) {
		return resources.Find(resource => resource.type == resourceEnum);
	}

	public class ResourceAmount {
		public Resource resource;
		public int amount;
		public ResourceAmount(Resource resource,int amount) {
			this.resource = resource;
			this.amount = amount;
		}
	}

	public class ReservedResources {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public ColonistManager.Colonist colonist;

		public ReservedResources(List<ResourceAmount> resourcesToReserve,ColonistManager.Colonist colonistReservingResources) {
			resources.AddRange(resourcesToReserve);
			colonist = colonistReservingResources;
		}
	}

	/*
	 * <Type> -> <SubType> -> <Object>
	*/
	public enum TileObjectPrefabGroupsEnum {
		Structure, Furniture,
		Command,
		Farm,
		None,
	};
	public enum TileObjectPrefabSubGroupsEnum {
		Walls, Doors, Floors, Containers, Beds,
		Plants, Terrain, Remove,
		PlantFarm, HarvestFarm,
		None
	};
	public enum TileObjectPrefabsEnum {
		StoneWall, WoodenWall, WoodenFence,
		WoodenDoor,
		StoneFloor, WoodenFloor,
		Basket, WoodenChest,
		WoodenBed,
		RemoveLayer1, RemoveLayer2, RemoveAll,
		ChopPlant, Mine,
		WheatFarm, PotatoFarm, HarvestFarm,
		PickupResources, EmptyInventory, Cancel, CollectFood, Eat, Sleep
	};

	List<TileObjectPrefabsEnum> BitmaskingTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor
	};

	public List<TileObjectPrefabsEnum> GetBitmaskingTileObjects() {
		return BitmaskingTileObjects;
	}

	List<TileObjectPrefabsEnum> FloorEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor
	};
	List<TileObjectPrefabsEnum> WallEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall
	};

	public List<TileObjectPrefabGroup> tileObjectPrefabGroups = new List<TileObjectPrefabGroup>();
	public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

	public void CreateTileObjectPrefabs() {
		List <string> tileObjectPrefabGroupsData = Resources.Load<TextAsset>(@"Data/tileobjectprefabs").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split(new string[] { "<Group>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string tileObjectPrefabGroupDataString in tileObjectPrefabGroupsData) {
			tileObjectPrefabGroups.Add(new TileObjectPrefabGroup(tileObjectPrefabGroupDataString));
		}
		uiM.CreateMenus();
	}

	public TileObjectPrefab GetTileObjectPrefabByEnum(TileObjectPrefabsEnum topEnum) {
		return tileObjectPrefabs.Find(top => top.type == topEnum);
	}

	public class TileObjectPrefabGroup {
		public TileObjectPrefabGroupsEnum type;
		public string name;

		public List<TileObjectPrefabSubGroup> tileObjectPrefabSubGroups = new List<TileObjectPrefabSubGroup>();

		public TileObjectPrefabGroup(string data) {
			List<string> tileObjectPrefabSubGroupsData = data.Split(new string[] { "<SubGroup>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabGroupsEnum),tileObjectPrefabSubGroupsData[0]);
			name = type.ToString();

			foreach (string tileObjectPrefabSubGroupDataString in tileObjectPrefabSubGroupsData.Skip(1)) {
				tileObjectPrefabSubGroups.Add(new TileObjectPrefabSubGroup(tileObjectPrefabSubGroupDataString,this));
			}
		}
	}

	public class TileObjectPrefabSubGroup {
		public TileObjectPrefabSubGroupsEnum type;
		public string name;

		public TileObjectPrefabGroup tileObjectPrefabGroup;
		public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

		public TileObjectPrefabSubGroup(string data,TileObjectPrefabGroup tileObjectPrefabGroup) {
			this.tileObjectPrefabGroup = tileObjectPrefabGroup;

			List<string> tileObjectPrefabsData = data.Split(new string[] { "<Object>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabSubGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabSubGroupsEnum),tileObjectPrefabsData[0]);
			name = type.ToString();

			foreach (string tileObjectPrefabDataString in tileObjectPrefabsData.Skip(1)) {
				tileObjectPrefabs.Add(new TileObjectPrefab(tileObjectPrefabDataString,this));
			}
		}
	}

	

	public class TileObjectPrefab {

		private ResourceManager resourceM;
		private UIManager uiM;

		void GetScriptReferences() {

			GameObject GM = GameObject.Find("GM");

			resourceM = GM.GetComponent<ResourceManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public TileObjectPrefabsEnum type;
		public string name;

		public TileObjectPrefabSubGroup tileObjectPrefabSubGroup;

		public Sprite baseSprite;
		public List<Sprite> bitmaskSprites = new List<Sprite>();

		public int timeToBuild;
		public List<ResourceAmount> resourcesToBuild = new List<ResourceAmount>();
		public List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
		public JobManager.JobTypesEnum jobType;

		public float flammability;

		public bool walkable;
		public float walkSpeed;

		public int layer;

		public bool addToTileWhenBuilt;

		public int maxInventoryAmount;

		public TileObjectPrefab(string data,TileObjectPrefabSubGroup tileObjectPrefabSubGroup) {

			GetScriptReferences();

			this.tileObjectPrefabSubGroup = tileObjectPrefabSubGroup;

			List<string> properties = data.Split('/').ToList();

			type = (TileObjectPrefabsEnum)System.Enum.Parse(typeof(TileObjectPrefabsEnum),properties[0]);
			name = uiM.SplitByCapitals(type.ToString());

			timeToBuild = int.Parse(properties[1]);

			if (float.Parse(properties[2].Split(',')[0]) != 0) {
				int resourceIndex = 0;
				foreach (string resourceName in properties[3].Split(',').ToList()) {
					resourcesToBuild.Add(new ResourceAmount(resourceM.GetResourceByEnum((ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum),resourceName)),int.Parse(properties[2].Split(',')[resourceIndex])));
					resourceIndex += 1;
				}
			}

			if (bool.Parse(properties[4])) {
				foreach (string selectionModifierString in properties[5].Split(',')) {
					selectionModifiers.Add((JobManager.SelectionModifiersEnum)System.Enum.Parse(typeof(JobManager.SelectionModifiersEnum),selectionModifierString));
				}
			}

			jobType = (JobManager.JobTypesEnum)System.Enum.Parse(typeof(JobManager.JobTypesEnum),properties[6]);

			flammability = float.Parse(properties[7]);

			walkable = bool.Parse(properties[8]);
			walkSpeed = float.Parse(properties[9]);

			layer = int.Parse(properties[10]);

			addToTileWhenBuilt = bool.Parse(properties[11]);

			baseSprite = Resources.Load<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ','-') + "-base");
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ','-') + "-bitmask").ToList();
			if (baseSprite == null && bitmaskSprites.Count > 0) {
				baseSprite = bitmaskSprites[0];
			}
			if (jobType == JobManager.JobTypesEnum.PlantFarm) {
				baseSprite = bitmaskSprites[bitmaskSprites.Count - 1];
			}

			if (resourceM.ContainerTileObjectTypes.Contains(type)) {
				maxInventoryAmount = int.Parse(properties[12]);
			}

			resourceM.tileObjectPrefabs.Add(this);
		}
	}

	public Dictionary<TileObjectPrefab,List<TileObjectInstance>> tileObjectInstances = new Dictionary<TileObjectPrefab,List<TileObjectInstance>>();
	public List<Farm> farms = new List<Farm>();

	public List<TileObjectInstance> GetTileObjectInstanceList(TileObjectPrefab prefab) {
		if (tileObjectInstances.ContainsKey(prefab)) {
			return tileObjectInstances[prefab];
		}
		print("Tried accessing a tile object instance which isn't already in the list...");
		return null;
	}

	public void AddTileObjectInstance(TileObjectInstance tileObjectInstance) {
		if (tileObjectInstances.ContainsKey(tileObjectInstance.prefab)) {
			tileObjectInstances[tileObjectInstance.prefab].Add(tileObjectInstance);
			uiM.ChangeObjectPrefabElements(UIManager.ChangeTypesEnum.Update,tileObjectInstance.prefab);
		} else {
			tileObjectInstances.Add(tileObjectInstance.prefab,new List<TileObjectInstance>() { tileObjectInstance });
			uiM.ChangeObjectPrefabElements(UIManager.ChangeTypesEnum.Add,tileObjectInstance.prefab);
		}
	}

	public void RemoveTileObjectInstance(TileObjectInstance tileObjectInstance) {
		if (tileObjectInstances.ContainsKey(tileObjectInstance.prefab)) {
			tileObjectInstances[tileObjectInstance.prefab].Remove(tileObjectInstance);
			uiM.ChangeObjectPrefabElements(UIManager.ChangeTypesEnum.Remove,tileObjectInstance.prefab);
		} else {
			print("Tried removing a tile object instance which isn't in the list...");
		}
		if (tileObjectInstances[tileObjectInstance.prefab].Count <= 0) {
			tileObjectInstances.Remove(tileObjectInstance.prefab);
		}
	}

	public class TileObjectInstance {

		private ResourceManager resourceM;

		void GetScriptReferences() {

			GameObject GM = GameObject.Find("GM");

			resourceM = GM.GetComponent<ResourceManager>();
		}

		public TileManager.Tile tile;
		
		public TileObjectPrefab prefab;
		public GameObject obj;

		public int rotationIndex;

		public TileObjectInstance(TileObjectPrefab prefab, TileManager.Tile tile,int rotationIndex) {

			GetScriptReferences();

			this.prefab = prefab;
			this.tile = tile;
			this.rotationIndex = rotationIndex;

			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 1 + prefab.layer; // Tile Object Sprite
			obj.GetComponent<SpriteRenderer>().sprite = prefab.baseSprite;

			if (resourceM.ContainerTileObjectTypes.Contains(prefab.type)) {
				resourceM.containers.Add(new Container(this,prefab.maxInventoryAmount));
			}

			SetColour(tile.sr.color);
		}

		public void FinishCreation() {
			List<TileManager.Tile> bitmaskingTiles = new List<TileManager.Tile>() { tile };
			bitmaskingTiles.AddRange(tile.surroundingTiles);
			resourceM.Bitmask(bitmaskingTiles);
			SetColour(tile.sr.color);
		}

		public void SetColour(Color newColour) {
			obj.GetComponent<SpriteRenderer>().color = new Color(newColour.r,newColour.g,newColour.b,1f);
		}
	}

	List<TileObjectPrefabsEnum> ContainerTileObjectTypes = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.Basket, TileObjectPrefabsEnum.WoodenChest
	};

	public class Container {
		public TileObjectInstance parentObject;
		public Inventory inventory;
		public int maxAmount;
		public Container(TileObjectInstance parentObject, int maxAmount) {
			this.parentObject = parentObject;
			this.maxAmount = maxAmount;
			inventory = new Inventory(null,this,maxAmount);
		}
	}

	Dictionary<ResourcesEnum,int> FarmGrowTimes = new Dictionary<ResourcesEnum,int>() {
		{ResourcesEnum.WheatSeeds,5760 },{ResourcesEnum.PotatoSeeds,2880 }
	};
	public Dictionary<ResourcesEnum,int> GetFarmGrowTimes() {
		return FarmGrowTimes;
	}
	Dictionary<ResourcesEnum,ResourcesEnum> FarmSeedReturnResource = new Dictionary<ResourcesEnum,ResourcesEnum>() {
		{ResourcesEnum.WheatSeeds,ResourcesEnum.Wheat },{ResourcesEnum.PotatoSeeds,ResourcesEnum.Potatoes }
	};
	public Dictionary<ResourcesEnum,ResourcesEnum> GetFarmSeedReturnResource() {
		return FarmSeedReturnResource;
	}
	Dictionary<ResourcesEnum,TileObjectPrefabsEnum> FarmSeedsTileObject = new Dictionary<ResourcesEnum,TileObjectPrefabsEnum>() {
		{ResourcesEnum.WheatSeeds,TileObjectPrefabsEnum.WheatFarm },{ResourcesEnum.PotatoSeeds,TileObjectPrefabsEnum.PotatoFarm }
	};
	public Dictionary<ResourcesEnum,TileObjectPrefabsEnum> GetFarmSeedsTileObject() {
		return FarmSeedsTileObject;
	}

	public class Farm : TileObjectInstance {

		private TimeManager timeM;
		private JobManager jobM;
		private ResourceManager resourceM;
		private ColonistManager colonistM;
		private UIManager uiM;

		void GetScriptReferencecs() {
			GameObject GM = GameObject.Find("GM");

			timeM = GM.GetComponent<TimeManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			colonistM = GM.GetComponent<ColonistManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public ResourcesEnum seedType;
		public string name;

		public float growTimer = 0;
		public float maxGrowthTime = 0;

		public int growProgressSpriteIndex = 0;
		public List<Sprite> growProgressSprites = new List<Sprite>();

		public Farm(TileObjectPrefab prefab, TileManager.Tile tile) : base(prefab,tile,0) {

			GetScriptReferencecs();

			seedType = prefab.resourcesToBuild[0].resource.type;
			name = (uiM.SplitByCapitals(seedType.ToString()).Split(' ')[0]).Replace(" ","") + " Farm";
			maxGrowthTime = resourceM.GetFarmGrowTimes()[seedType];

			growProgressSprites = prefab.bitmaskSprites;
		}

		public void Update() {
			if (growTimer >= maxGrowthTime) {
				if (!jobM.JobOfTypeExistsAtTile(JobManager.JobTypesEnum.HarvestFarm,tile)) {
					jobM.CreateJob(new JobManager.Job(tile,resourceM.GetTileObjectPrefabByEnum(TileObjectPrefabsEnum.HarvestFarm),0,colonistM,resourceM));
				}
			} else {
				growTimer += 1 * timeM.deltaTime;
				int newGrowProgressSpriteIndex = Mathf.FloorToInt((growTimer / (maxGrowthTime + 10)) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[growProgressSpriteIndex];
				}
			}
		}
	}

	public List<Container> containers = new List<Container>();

	public class Inventory {

		private UIManager uiM;
		private JobManager jobM;

		private void GetScriptReferences() {

			GameObject GM = GameObject.Find("GM");

			uiM = GM.GetComponent<UIManager>();
			jobM = GM.GetComponent<JobManager>();
		}

		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public List<ReservedResources> reservedResources = new List<ReservedResources>();

		public ColonistManager.Human human;
		public Container container;

		public int maxAmount;

		public Inventory(ColonistManager.Human human, Container container, int maxAmount) {
			this.human = human;
			this.container = container;
			this.maxAmount = maxAmount;

			GetScriptReferences();
		}

		public int CountResources() {
			return (resources.Sum(resource => resource.amount) + reservedResources.Sum(reservedResource => reservedResource.resources.Sum(rr => rr.amount)));
		}

		public void ChangeResourceAmount(Resource resource,int amount) {
			ResourceAmount existingResourceAmount = resources.Find(ra => ra.resource == resource);
			//print(existingResourceAmount);
			if (existingResourceAmount != null) {
				if (amount >= 0 || (amount - existingResourceAmount.amount) >= 0) {
					//print("Added an additional " + amount + " of " + resource.name + " to " + human.name);
					existingResourceAmount.amount += amount;
				} else if (amount < 0 && (existingResourceAmount.amount + amount) >= 0) {
					//print("Removed " + amount + " of " + resource.name + " from " + human.name);
					existingResourceAmount.amount += amount;
				}/* else {
					Debug.LogError("Trying to remove " + amount + " of " + resource.name + " on " + human.name + " when only " + existingResourceAmount.amount + " of that resource exist in this inventory");
				}*/
			} else {
				if (amount > 0) {
					//print("Adding " + resource.name + " to " + human.name + " with a starting amount of " + amount);
					resources.Add(new ResourceAmount(resource,amount));
				}/* else if (amount < 0) {
					Debug.LogError("Trying to remove " + amount + " of " + resource.name + " that doesn't exist in " + human.name);
				}*/
			}
			existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (existingResourceAmount.amount == 0) {
					//print("Removed " + existingResourceAmount.resource.name + " from " + human.name + " as its amount was 0");
					resources.Remove(existingResourceAmount);
				}/* else if (existingResourceAmount.amount < 0) {
					Debug.LogError("There is a negative amount of " + resource.name + " on " + human.name + " with " + existingResourceAmount.amount);
				} else {
					print(human.name + " now has " + existingResourceAmount.amount + " of " + existingResourceAmount.resource.name);
				}*/
			}
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
			jobM.UpdateColonistJobs();
		}

		public bool ReserveResources(List<ResourceAmount> resourcesToReserve, ColonistManager.Colonist colonistReservingResources) {
			print(colonistReservingResources.name + " is trying to reserve " + resourcesToReserve.Count + " resources");
			bool allResourcesFound = true;
			foreach (ResourceAmount raReserve in resourcesToReserve) {
				ResourceAmount raInventory = resources.Find(ra => ra.resource == raReserve.resource);
				if (!(raInventory != null && raInventory.amount >= raReserve.amount)) {
					allResourcesFound = false;
				}
			}
			print("All Resources Found: " + allResourcesFound);
			if (allResourcesFound) {
				foreach (ResourceAmount raReserve in resourcesToReserve) {
					ResourceAmount raInventory = resources.Find(ra => ra.resource == raReserve.resource);
					ChangeResourceAmount(raInventory.resource,-raReserve.amount);
				}
				reservedResources.Add(new ReservedResources(resourcesToReserve,colonistReservingResources));
			}
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
			return allResourcesFound;
		}

		public List<ReservedResources> TakeReservedResources(ColonistManager.Colonist colonistReservingResources) {
			print(colonistReservingResources.name + " is taking reserved resources");
			List<ReservedResources> reservedResourcesByColonist = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.colonist == colonistReservingResources) {
					reservedResourcesByColonist.Add(rr);
					print("Found " + rr.resources.Count + " for " + colonistReservingResources.name);
				}
			}
			foreach (ReservedResources rr in reservedResourcesByColonist) {
				reservedResources.Remove(rr);
			}
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
			return reservedResourcesByColonist;
		}
	}

	Dictionary<int,int> bitmaskMap = new Dictionary<int,int>() {
		{ 19,16 },{ 23,17 },{ 27,18 },{ 31,19 },{ 38,20 },{ 39,21 },{ 46,22 },
		{ 47,23 },{ 55,24 },{ 63,25 },{ 76,26 },{ 77,27 },{ 78,28 },{ 79,29 },
		{ 95,30 },{ 110,31 },{ 111,32 },{ 127,33 },{ 137,34 },{ 139,35 },{ 141,36 },
		{ 143,37 },{ 155,38 },{ 159,39 },{ 175,40 },{ 191,41 },{ 205,42 },{ 207,43 },
		{ 223,44 },{ 239,45 },{ 255,46 }
	};
	Dictionary<int,List<int>> diagonalCheckMap = new Dictionary<int,List<int>>() {
		{4,new List<int>() {0,1 } },
		{5,new List<int>() {1,2 } },
		{6,new List<int>() {2,3 } },
		{7,new List<int>() {3,0 } }
	};

	int BitSumTileObjects(List<TileObjectPrefabsEnum> compareTileObjectTypes,List<TileManager.Tile> tileSurroundingTiles) {
		List<int> layers = new List<int>();
		foreach (TileManager.Tile tile in tileSurroundingTiles) {
			if (tile != null) {
				foreach (KeyValuePair<int,TileObjectInstance> kvp in tile.objectInstances) {
					if (!layers.Contains(kvp.Key)) {
						layers.Add(kvp.Key);
					}
				}
			}
		}
		layers.Sort();

		Dictionary<int,List<int>> layersSumTiles = new Dictionary<int,List<int>>();
		foreach (int layer in layers) {
			List<int> layerSumTiles = new List<int>() { 0,0,0,0,0,0,0,0 };
			for (int i = 0;i < tileSurroundingTiles.Count;i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && diagonalCheckMap.ContainsKey(i)) {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[diagonalCheckMap[i][0]],tileSurroundingTiles[diagonalCheckMap[i][1]] };
							List<TileManager.Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && compareTileObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							layerSumTiles[i] = 1;
						}
					}
				} else {
					if (tileSurroundingTiles.Find(tile => tile != null && tileSurroundingTiles.IndexOf(tile) <= 3 && tile.GetObjectInstanceAtLayer(layer) != null && !compareTileObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
						layerSumTiles[i] = 1;
					} else {
						if (i <= 3) {
							layerSumTiles[i] = 1;
						} else {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[diagonalCheckMap[i][0]],tileSurroundingTiles[diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareTileObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer,layerSumTiles);
		}

		List<bool> sumTiles = new List<bool>() { false,false,false,false,false,false,false,false };

		foreach (KeyValuePair<int,List<int>> layerSumTiles in layersSumTiles) {
			foreach (TileObjectPrefabsEnum topEnum in compareTileObjectTypes) {
				TileObjectPrefab top = GetTileObjectPrefabByEnum(topEnum);
				if (top.layer == layerSumTiles.Key) {
					foreach (TileManager.Tile tile in tileSurroundingTiles) {
						if (tile != null) {
							TileObjectInstance topInstance = tile.GetAllObjectInstances().Find(instances => instances.prefab == top);
							if (topInstance != null) {
								if (layerSumTiles.Value[tileSurroundingTiles.IndexOf(tile)] > 0) {
									sumTiles[tileSurroundingTiles.IndexOf(tile)] = true;
								}
							}
						}
					}
				}
			}
		}

		int sum = 0;

		for (int i = 0;i < sumTiles.Count;i++) {
			if (sumTiles[i]) {
				sum += Mathf.RoundToInt(Mathf.Pow(2,i));
			}
		}

		return sum;
	}

	void BitmaskTileObjects(TileObjectInstance objectInstance,bool includeDiagonalSurroundingTiles,bool customBitSumInputs,bool compareEquivalentTileObjects, List<TileObjectPrefabsEnum> customCompareTileObjectTypes) {
		int sum = 0;
		if (customBitSumInputs) {
			sum = BitSumTileObjects(customCompareTileObjectTypes,(includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
		} else {
			if (compareEquivalentTileObjects) {
				if (FloorEquivalentTileObjects.Contains(objectInstance.prefab.type)) {
					sum = BitSumTileObjects(FloorEquivalentTileObjects,(includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				} else if (WallEquivalentTileObjects.Contains(objectInstance.prefab.type)) {
					sum = BitSumTileObjects(WallEquivalentTileObjects,(includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				} else {
					sum = BitSumTileObjects(new List<TileObjectPrefabsEnum>() { objectInstance.prefab.type },(includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				}
			} else {
				sum = BitSumTileObjects(new List<TileObjectPrefabsEnum>() { objectInstance.prefab.type },(includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
			}
		}
		SpriteRenderer oISR = objectInstance.obj.GetComponent<SpriteRenderer>();
		if (sum >= 16) {
			oISR.sprite = objectInstance.prefab.bitmaskSprites[bitmaskMap[sum]];
		} else {
			oISR.sprite = objectInstance.prefab.bitmaskSprites[sum];
		}
	}

	public void Bitmask(List<TileManager.Tile> tilesToBitmask) {
		foreach (TileManager.Tile tile in tilesToBitmask) {
			if (tile != null && tile.GetAllObjectInstances().Count > 0) {
				foreach (TileObjectInstance tileObjectInstance in tile.GetAllObjectInstances()) {
					if (BitmaskingTileObjects.Contains(tileObjectInstance.prefab.type)) {
						BitmaskTileObjects(tileObjectInstance,true,false,false,null);
					} else {
						if (tileObjectInstance.prefab.bitmaskSprites.Count > 0) {
							if (tileObjectInstance.prefab.jobType != JobManager.JobTypesEnum.PlantFarm) {
								tileObjectInstance.obj.GetComponent<SpriteRenderer>().sprite = tileObjectInstance.prefab.bitmaskSprites[tileObjectInstance.rotationIndex];
							}
						} else {
							tileObjectInstance.obj.GetComponent<SpriteRenderer>().sprite = tileObjectInstance.prefab.baseSprite;
						}
					}
				}
			}
		}
	}
}