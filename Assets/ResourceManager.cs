using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour {

	private UIManager uiM;
	private ColonistManager colonistM;
	private JobManager jobM;

	void Awake() {
		uiM = GetComponent<UIManager>();
		colonistM = GetComponent<ColonistManager>();
		jobM = GetComponent<JobManager>();
	}

	void Update() {
		CalculateResourceTotals();
		foreach (Farm farm in farms) {
			farm.Update();
		}
		foreach (ManufacturingTileObject mto in manufacturingTileObjectInstances) {
			mto.Update();
		}
	}

	public enum ResourceGroupsEnum {
		Natural,
		Ores,
		Metals,
		Materials,
		Seeds,
		RawFoods,
		Foods,
		Coins
	};

	public enum ResourcesEnum {
		Dirt, Stone, Granite, Limestone, Marble, Sandstone, Slate, Clay, Log, Wood, Firewood, Snow, Sand, Cactus, Leaf, Sap,
		GoldOre, SilverOre, BronzeOre, IronOre,
		Gold, Silver, Bronze, Iron,
		Brick, Glass, Cotton, Cloth,
		WheatSeed, CottonSeed, TreeSeed, AppleSeed, ShrubSeed, CactusSeed,
		Wheat,
		Potato, BakedPotato, Berries, Apple, BakedApple,
		GoldCoin, SilverCoin, BronzeCoin
	};

	List<ResourcesEnum> manufacturableResources = new List<ResourcesEnum>() {
		ResourcesEnum.Wood, ResourcesEnum.Firewood, ResourcesEnum.Brick, ResourcesEnum.Glass, ResourcesEnum.Cloth, ResourcesEnum.BakedPotato, ResourcesEnum.BakedApple,
		ResourcesEnum.Gold, ResourcesEnum.Silver, ResourcesEnum.Bronze, ResourcesEnum.Iron, ResourcesEnum.GoldCoin, ResourcesEnum.SilverCoin, ResourcesEnum.BronzeCoin
	};
	public List<ResourcesEnum> GetManufacturableResources() {
		return manufacturableResources;
	}

	List<ResourcesEnum> fuelResources = new List<ResourcesEnum>() {
		ResourcesEnum.Log, ResourcesEnum.Wood, ResourcesEnum.Firewood
	};
	public List<ResourcesEnum> GetFuelResources() {
		return fuelResources;
	}

	public void CreateResources() {
		List<string> resourceGroupsData = Resources.Load<TextAsset>(@"Data/resources").text.Replace("\t", string.Empty).Split(new string[] { "<Group>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string resourceGroupDataString in resourceGroupsData) {
			resourceGroups.Add(new ResourceGroup(resourceGroupDataString));
		}
		foreach (Resource resource in resources) {
			resource.SetInitialRequiredResourceReferences();
		}
	}

	public List<ResourceGroup> resourceGroups = new List<ResourceGroup>();

	public class ResourceGroup {

		public ResourceManager resourceM;
		public TileManager tileM;
		public UIManager uiM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			resourceM = GM.GetComponent<ResourceManager>();
			tileM = GM.GetComponent<TileManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public ResourceGroupsEnum type;
		public string name;

		public List<ResourcesEnum> resourceTypes = new List<ResourcesEnum>();
		public List<Resource> resources = new List<Resource>();

		public ResourceGroup(string resourceDataString) {
			GetScriptReferences();

			List<string> resourceGroupResourcesData = resourceDataString.Split(new string[] { "<Resource>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

			this.type = (ResourceGroupsEnum)System.Enum.Parse(typeof(ResourceGroupsEnum), resourceGroupResourcesData[0]);
			name = uiM.SplitByCapitals(type.ToString());

			foreach (string resourceGroupResourceString in resourceGroupResourcesData.Skip(1)) {

				ResourcesEnum type = ResourcesEnum.Apple;
				float weight = 0;
				Resource.Price price = null;
				int nutrition = 0;
				int fuelEnergy = 0;
				List<TileObjectPrefabSubGroupsEnum> requiredMTOSubGroups = new List<TileObjectPrefabSubGroupsEnum>();
				List<TileObjectPrefabsEnum> requiredMTOs = new List<TileObjectPrefabsEnum>();
				int requiredEnergy = 0;
				Dictionary<ResourcesEnum, float> requiredResources = new Dictionary<ResourcesEnum, float>();

				List<string> singleResourceDataLineStringList = resourceGroupResourceString.Split('\n').ToList();
				foreach (string singleResourceDataLineString in singleResourceDataLineStringList.Skip(1)) {
					if (!string.IsNullOrEmpty(singleResourceDataLineString)) {

						string label = singleResourceDataLineString.Split('>')[0].Replace("<", string.Empty);
						string value = singleResourceDataLineString.Split('>')[1];

						switch (label) {
							case "Type":
								type = (ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum), value);
								break;
							case "Weight":
								weight = float.Parse(value);
								break;
							case "Price":
								int gold = 0;
								int silver = 0;
								int bronze = 0;
								foreach (string priceValue in value.Split(',')) {
									string priceValueAmount = uiM.RemoveNonAlphanumericChars(value.Split(':')[0]);
									string priceValueDenomination = uiM.RemoveNonAlphanumericChars(value.Split(':')[1]);
									switch (priceValueDenomination) {
										case "G":
											gold = int.Parse(priceValueAmount);
											break;
										case "S":
											silver = int.Parse(priceValueAmount);
											break;
										case "B":
											bronze = int.Parse(priceValueAmount);
											break;
										default:
											print("Unknown resource-price label: \"" + value + "\"");
											break;
									}
									price = new Resource.Price(gold, silver, bronze);
								}
								break;
							case "Nutrition":
								nutrition = int.Parse(value);
								break;
							case "FuelEnergy":
								fuelEnergy = int.Parse(value);
								break;
							case "RequiredMTOSubGroups":
								foreach (string requiredMTOSubGroupString in value.Split(',')) {
									requiredMTOSubGroups.Add((TileObjectPrefabSubGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabSubGroupsEnum), value));
								}
								break;
							case "RequiredMTOs":
								foreach (string requiredMTOString in value.Split(',')) {
									requiredMTOs.Add((TileObjectPrefabsEnum)System.Enum.Parse(typeof(TileObjectPrefabsEnum), value));
								}
								break;
							case "RequiredEnergy":
								requiredEnergy = int.Parse(value);
								break;
							case "RequiredResources":
								foreach (string requiredResourceString in value.Split(',')) {
									string resourceName = requiredResourceString.Split(':')[0];
									ResourcesEnum resourceType = (ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum), resourceName);
									float amount = float.Parse(requiredResourceString.Split(':')[1]);
									requiredResources.Add(resourceType,amount);
								}
								break;
							default:
								print("Unknown resource label: \"" + singleResourceDataLineString + "\"");
								break;
						}
					}
				}

				Resource newResource = new Resource(
					this,
					type,
					weight,
					price,
					nutrition,
					fuelEnergy,
					requiredMTOSubGroups,
					requiredMTOs,
					requiredEnergy,
					requiredResources
				);
				resourceTypes.Add(type);
				resources.Add(newResource);
				resourceM.resources.Add(newResource);
			}
		}
	}

	public List<Resource> resources = new List<Resource>();

	public class Resource {
		public ResourcesEnum type;
		public string name;

		public Sprite image;

		public ResourceGroup resourceGroup;

		public float weight = 0;

		public Price price;

		public int nutrition = 0;

		public int desiredAmount = 0;

		public int worldTotalAmount;
		public int colonistsTotalAmount;
		public int containerTotalAmount;
		public int unreservedContainerTotalAmount;

		public List<TileObjectPrefabSubGroup> requiredMTOSubGroups = new List<TileObjectPrefabSubGroup>();
		public List<TileObjectPrefab> requiredMTOs = new List<TileObjectPrefab>();
		public int requiredEnergy = 0;
		public List<ResourceAmount> requiredResources = new List<ResourceAmount>();
		public int fuelEnergy = 0;
		public int amountCreated = 1;

		// Not possible to immediately fill requiredResources since all resources don't exist at this point in code execution
		public Dictionary<ResourcesEnum, float> requiredResourcesTemp = new Dictionary<ResourcesEnum, float>();

		// Not possible to immediately fill MTOSubGroups and MTOs since all tile object prefabs don't exist at this point in code execution
		public List<TileObjectPrefabSubGroupsEnum> requiredMTOSubGroupsTemp = new List<TileObjectPrefabSubGroupsEnum>();
		public List<TileObjectPrefabsEnum> requiredMTOsTemp = new List<TileObjectPrefabsEnum>();

		public UIManager.ResourceInstanceElement resourceListElement;

		public Resource(ResourceGroup resourceGroup, ResourcesEnum type, float weight, Price price, int nutrition, int fuelEnergy, List<TileObjectPrefabSubGroupsEnum> requiredMTOSubGroupsTemp, List<TileObjectPrefabsEnum> requiredMTOsTemp, int requiredEnergy, Dictionary<ResourcesEnum,float> requiredResourcesTemp) {
			this.resourceGroup = resourceGroup;

			this.type = type;
			name = resourceGroup.uiM.SplitByCapitals(type.ToString());

			this.weight = weight;

			this.price = price;

			this.nutrition = nutrition;

			this.fuelEnergy = fuelEnergy;

			this.requiredMTOSubGroupsTemp = requiredMTOSubGroupsTemp;
			this.requiredMTOsTemp= requiredMTOsTemp;

			this.requiredEnergy = requiredEnergy;

			this.requiredResourcesTemp = requiredResourcesTemp;

			image = Resources.Load<Sprite>(@"Sprites/Resources/" + name + "/" + name.Replace(' ', '-') + "-base");
		}

		public void SetInitialRequiredResourceReferences() {
			foreach (KeyValuePair<ResourcesEnum, float> requiredResourcesTempKVP in requiredResourcesTemp) {
				Resource resource = resourceGroup.resourceM.GetResourceByEnum(requiredResourcesTempKVP.Key);
				float amount = requiredResourcesTempKVP.Value;
				if (amount < 1 && amount > 0) {
					amountCreated = Mathf.RoundToInt(1f / amount);
					requiredResources.Add(new ResourceAmount(resource, 1));
				} else {
					amountCreated = 1;
					requiredResources.Add(new ResourceAmount(resource, Mathf.RoundToInt(amount)));
				}
			}
			requiredResourcesTemp.Clear();
		}

		public void SetInitialMTOReferences() {
			foreach (TileObjectPrefabSubGroupsEnum requiredMTOSubGroupTempEnum in requiredMTOSubGroupsTemp) {
				requiredMTOSubGroups.Add(resourceGroup.resourceM.GetTileObjectPrefabSubGroupByEnum(requiredMTOSubGroupTempEnum));
			}
			foreach (TileObjectPrefabsEnum requiredMTOTempEnum in requiredMTOsTemp) {
				requiredMTOs.Add(resourceGroup.resourceM.GetTileObjectPrefabByEnum(requiredMTOTempEnum));
			}
		}

		public void ChangeDesiredAmount(int newDesiredAmount) {
			desiredAmount = newDesiredAmount;
			UpdateDesiredAmountText();
		}

		public void UpdateDesiredAmountText() {
			if (resourceGroup.uiM.selectedMTO != null && resourceGroup.uiM.selectedMTO.createResource == this) {
				resourceGroup.uiM.selectedMTOPanel.obj.transform.Find("ResourceTargetAmount-Panel/TargetAmount-Input").GetComponent<InputField>().text = desiredAmount.ToString();
			}
			resourceListElement.desiredAmountInput.text = desiredAmount.ToString();
		}

		public class Price {
			public int gold = 0;
			public int silver = 0;
			public int bronze = 0;

			public float relativeGold = 0;
			public float relativeSilver = 0;
			public float relativeBronze = 0;

			public enum PriceTypeEnum { Gold, Silver, Bronze };
			public enum RelativePriceTypeEnum {	RelativeGold, RelativeSilver, RelativeBronze };

			public Price(int gold, int silver, int bronze) {
				this.gold = gold;
				this.silver = silver;
				this.bronze = bronze;

				relativeGold = gold * (silver / 100f) * (bronze / 10000f);
				relativeSilver = (gold * 100f) + silver + (bronze / 100f);
				relativeBronze = (gold * 10000f) + (silver * 100f) + bronze;
			}

			public int GetPrice(PriceTypeEnum priceType) {
				switch (priceType) {
					case PriceTypeEnum.Gold:
						return gold;
					case PriceTypeEnum.Silver:
						return silver;
					case PriceTypeEnum.Bronze:
						return bronze;
				}
				return -1;
			}

			public float GetRelativePrice(RelativePriceTypeEnum relativePriceType) {
				switch (relativePriceType) {
					case RelativePriceTypeEnum.RelativeGold:
						return relativeGold;
					case RelativePriceTypeEnum.RelativeSilver:
						return relativeSilver;
					case RelativePriceTypeEnum.RelativeBronze:
						return relativeBronze;
				}
				return -1;
			}
		}
	}

	public Resource GetResourceByEnum(ResourcesEnum resourceEnum) {
		return resources.Find(resource => resource.type == resourceEnum);
	}

	public class ResourceAmount {
		public Resource resource;
		public int amount;
		public ResourceAmount(Resource resource, int amount) {
			this.resource = resource;
			this.amount = amount;
		}
	}

	public class ReservedResources {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public ColonistManager.Colonist colonist;

		public ReservedResources(List<ResourceAmount> resourcesToReserve, ColonistManager.Colonist colonistReservingResources) {
			resources.AddRange(resourcesToReserve);
			colonist = colonistReservingResources;
		}
	}

	public List<ManufacturingTileObject> manufacturingTileObjectInstances = new List<ManufacturingTileObject>();
	public class ManufacturingTileObject {
		private ResourceManager resourceM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			resourceM = GM.GetComponent<ResourceManager>();
		}

		public TileObjectInstance parentObject;

		public Resource createResource;
		public bool hasEnoughRequiredResources;

		public Resource fuelResource;
		public bool hasEnoughFuel;
		public int fuelResourcesRequired = 0;

		public bool canActivate;
		public bool active;

		public List<JobManager.Job> jobBacklog = new List<JobManager.Job>();

		public ManufacturingTileObject(TileObjectInstance parentObject) {
			GetScriptReferences();

			this.parentObject = parentObject;
			this.parentObject.mto = this;
		}

		public void Update() {
			hasEnoughRequiredResources = createResource != null;
			if (createResource != null) {
				foreach (ResourceAmount resourceAmount in createResource.requiredResources) {
					if (resourceAmount.resource.worldTotalAmount < resourceAmount.amount) {
						hasEnoughRequiredResources = false;
					}
				}
			}
			if (createResource != null) {
				if (createResource.requiredEnergy != 0) {
					hasEnoughFuel = fuelResource != null;
					if (fuelResource != null && createResource != null) {
						fuelResourcesRequired = Mathf.CeilToInt((createResource.requiredEnergy) / ((float)fuelResource.fuelEnergy));
						if (fuelResource.worldTotalAmount < fuelResourcesRequired) {
							hasEnoughFuel = false;
						}
					}
					canActivate = hasEnoughRequiredResources && hasEnoughFuel;
				} else {
					canActivate = hasEnoughRequiredResources;
				}
			}
			if (active) {
				if (canActivate && createResource.desiredAmount > createResource.worldTotalAmount && jobBacklog.Count < 1) {
					resourceM.CreateResource(createResource, 1, parentObject);
				}
			}
			parentObject.active = active;
		}
	}

	public void CreateResource(Resource resource, int amount, TileObjectInstance manufacturingTileObject) {
		for (int i = 0; i < amount; i++) {
			JobManager.Job job = new JobManager.Job(manufacturingTileObject.tile, GetTileObjectPrefabByEnum(TileObjectPrefabsEnum.CreateResource), 0);
			job.SetCreateResourceData(resource, manufacturingTileObject);
			jobM.CreateJob(job);
			manufacturingTileObject.mto.jobBacklog.Add(job);
		}
	}

	public List<SleepSpot> sleepSpots = new List<SleepSpot>();
	public class SleepSpot {
		public TileObjectInstance parentObject;
		public ColonistManager.Colonist occupyingColonist;

		public SleepSpot(TileObjectInstance parentObject) {
			this.parentObject = parentObject;
		}

		public void StartSleeping(ColonistManager.Colonist colonist) {
			occupyingColonist = colonist;
		}

		public void StopSleeping() {
			occupyingColonist = null;
		}
	}

	public enum TileObjectPrefabGroupsEnum {
		Structure, Furniture, Industrial,
		Command,
		Farm,
		None,
	};
	public enum TileObjectPrefabSubGroupsEnum {
		Walls, Doors, Floors, Containers, Beds, Chairs, Tables, Lights,
		Furnaces,Processing,
		Plants, Terrain, Remove, Cancel, Priority,
		PlantFarm, HarvestFarm,
		None
	};
	public enum TileObjectPrefabsEnum {
		StoneWall, WoodenWall, WoodenFence, BrickWall,
		WoodenDoor,
		StoneFloor, WoodenFloor, BrickFloor,
		Basket, WoodenChest, WoodenDrawers,
		WoodenBed,
		WoodenChair,
		WoodenTable,
		Torch, WoodenLamp,
		StoneFurnace,
		CottonGin, SplittingBlock, SplittingLog, Anvil,
		ChopPlant, PlantPlant, PlantAppleTree, PlantBerryBush,
		Mine, Dig,
		RemoveLayer1, RemoveLayer2, RemoveAll,
		Cancel,
		IncreasePriority, DecreasePriority,
		WheatFarm, PotatoFarm, CottonFarm,
		HarvestFarm,
		CreateResource, PickupResources, EmptyInventory, CollectFood, Eat, Sleep,
		PlantTree, PlantShrub, PlantCactus
	};

	List<TileObjectPrefabsEnum> BitmaskingTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor, TileObjectPrefabsEnum.WoodenFence,
		TileObjectPrefabsEnum.BrickWall, TileObjectPrefabsEnum.BrickFloor,
		TileObjectPrefabsEnum.WoodenTable
	};

	public List<TileObjectPrefabsEnum> GetBitmaskingTileObjects() {
		return BitmaskingTileObjects;
	}

	List<TileObjectPrefabsEnum> FloorEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor, TileObjectPrefabsEnum.BrickFloor
	};
	List<TileObjectPrefabsEnum> WallEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.WoodenFence, TileObjectPrefabsEnum.BrickWall
	};

	Dictionary<TileObjectPrefabSubGroupsEnum,List<TileObjectPrefabsEnum>> ManufacturingTileObjects = new Dictionary<TileObjectPrefabSubGroupsEnum,List<TileObjectPrefabsEnum>>() {
		{TileObjectPrefabSubGroupsEnum.Furnaces,new List<TileObjectPrefabsEnum>() { TileObjectPrefabsEnum.StoneFurnace } },
		{TileObjectPrefabSubGroupsEnum.Processing,new List<TileObjectPrefabsEnum>() { TileObjectPrefabsEnum.CottonGin, TileObjectPrefabsEnum.SplittingBlock, TileObjectPrefabsEnum.SplittingLog, TileObjectPrefabsEnum.Anvil} }
	};
	public Dictionary<TileObjectPrefabSubGroupsEnum, List<TileObjectPrefabsEnum>> GetManufacturingTileObjects() {
		return ManufacturingTileObjects;
	}
	List<TileObjectPrefabsEnum> ManufacturingTileObjectsFuel = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneFurnace
	};
	public List<TileObjectPrefabsEnum> GetManufacturingTileObjectsFuel() {
		return ManufacturingTileObjectsFuel;
	}
	List<TileObjectPrefabsEnum> ManufacturingTileObjectsNoFuel = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.CottonGin, TileObjectPrefabsEnum.SplittingBlock, TileObjectPrefabsEnum.SplittingLog, TileObjectPrefabsEnum.Anvil
	};
	public List<TileObjectPrefabsEnum> GetManufacturingTileObjectsNoFuel() {
		return ManufacturingTileObjectsNoFuel;
	}

	List<TileObjectPrefabsEnum> LightSourceTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.WoodenLamp, TileObjectPrefabsEnum.Torch
	};

	List<TileObjectPrefabsEnum> SleepSpotTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.WoodenBed
	};

	List<TileObjectPrefabsEnum> LightBlockingTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.BrickWall, TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.WoodenDoor
	};
	public List<TileObjectPrefabsEnum> GetLightBlockingTileObjects () {
		return LightBlockingTileObjects;
	}

	public List<TileObjectPrefabGroup> tileObjectPrefabGroups = new List<TileObjectPrefabGroup>();
	public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

	public void CreateTileObjectPrefabs() {
		List <string> tileObjectPrefabGroupsData = Resources.Load<TextAsset>(@"Data/tileobjectprefabs").text.Replace("\t",string.Empty).Split(new string[] { "<Group>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string tileObjectPrefabGroupDataString in tileObjectPrefabGroupsData) {
			tileObjectPrefabGroups.Add(new TileObjectPrefabGroup(tileObjectPrefabGroupDataString,uiM,this));
		}
		foreach (Resource resource in resources) {
			resource.SetInitialMTOReferences();
		}
		uiM.CreateMenus();
	}

	public TileObjectPrefab GetTileObjectPrefabByEnum(TileObjectPrefabsEnum topEnum) {
		return tileObjectPrefabs.Find(top => top.type == topEnum);
	}

	public TileObjectPrefabSubGroup GetTileObjectPrefabSubGroupByEnum(TileObjectPrefabSubGroupsEnum topsgEnum) {
		foreach (TileObjectPrefabGroup topg in tileObjectPrefabGroups) {
			foreach (TileObjectPrefabSubGroup topsg in topg.tileObjectPrefabSubGroups) {
				if (topsg.type == topsgEnum) {
					return topsg;
				}
			}
		}
		return null;
	}

	public class TileObjectPrefabGroup {
		public TileObjectPrefabGroupsEnum type;
		public string name;

		public List<TileObjectPrefabSubGroup> tileObjectPrefabSubGroups = new List<TileObjectPrefabSubGroup>();

		public TileObjectPrefabGroup(string data, UIManager uiM, ResourceManager resourceM) {
			List<string> tileObjectPrefabSubGroupsData = data.Split(new string[] { "<SubGroup>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabGroupsEnum),tileObjectPrefabSubGroupsData[0]);
			name = uiM.SplitByCapitals(type.ToString());

			foreach (string tileObjectPrefabSubGroupDataString in tileObjectPrefabSubGroupsData.Skip(1)) {
				tileObjectPrefabSubGroups.Add(new TileObjectPrefabSubGroup(tileObjectPrefabSubGroupDataString,this,uiM,resourceM));
			}
		}
	}

	public class TileObjectPrefabSubGroup {
		public TileObjectPrefabSubGroupsEnum type;
		public string name;

		public TileObjectPrefabGroup tileObjectPrefabGroup;
		public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

		public TileObjectPrefabSubGroup(string data, TileObjectPrefabGroup tileObjectPrefabGroup, UIManager uiM, ResourceManager resourceM) {
			this.tileObjectPrefabGroup = tileObjectPrefabGroup;

			List<string> tileObjectPrefabsData = data.Split(new string[] { "<Object>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabSubGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabSubGroupsEnum), tileObjectPrefabsData[0]);
			name = uiM.SplitByCapitals(type.ToString());

			foreach (string tileObjectPrefabDataString in tileObjectPrefabsData.Skip(1)) {
				TileObjectPrefabsEnum type = TileObjectPrefabsEnum.Cancel;
				int timeToBuild = -1;
				List<ResourceAmount> resourcesToBuild = new List<ResourceAmount>();
				List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
				JobManager.JobTypesEnum jobType = JobManager.JobTypesEnum.Cancel;
				float flammability = 0;
				int maxIntegrity = 0;
				bool walkable = true;
				float walkSpeed = 1;
				int layer = -1;
				bool addToTileWhenBuilt = true;
				Vector2 blockingAmount = new Vector2(0, 0);
				List<Vector2> multiTilePositions = new List<Vector2>();
				int maxInventoryAmount = -1;
				int maxLightDistance = -1;
				Color lightColour = Color.black;
				float restComfortAmount = -1;

				List<string> singleTileObjectPrefabDataLineStringList = tileObjectPrefabDataString.Split('\n').ToList();
				foreach (string singleTileObjectPrefabDataLineString in singleTileObjectPrefabDataLineStringList.Skip(1)) {
					if (!string.IsNullOrEmpty(singleTileObjectPrefabDataLineString)) {

						string label = singleTileObjectPrefabDataLineString.Split('>')[0].Replace("<", string.Empty);
						string value = singleTileObjectPrefabDataLineString.Split('>')[1];

						switch (label) {
							case "Type":
								type = (TileObjectPrefabsEnum)System.Enum.Parse(typeof(TileObjectPrefabsEnum), value);
								break;
							case "TimeToBuild":
								timeToBuild = int.Parse(value);
								break;
							case "ResourcesToBuild":
								foreach (string resourceToBuildString in value.Split(',')) {
									string resourceName = resourceToBuildString.Split(':')[0];
									Resource resource = resourceM.GetResourceByEnum((ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum), resourceName));
									int amount = int.Parse(resourceToBuildString.Split(':')[1]);
									resourcesToBuild.Add(new ResourceAmount(resource, amount));
								}
								break;
							case "SelectionModifiers":
								foreach (string selectionModifierString in value.Split(',')) {
									string selectionModifierName = selectionModifierString.Split(':')[0];
									JobManager.SelectionModifiersEnum selectionModifier = (JobManager.SelectionModifiersEnum)System.Enum.Parse(typeof(JobManager.SelectionModifiersEnum), selectionModifierName);
									selectionModifiers.Add(selectionModifier);
								}
								break;
							case "JobType":
								jobType = (JobManager.JobTypesEnum)System.Enum.Parse(typeof(JobManager.JobTypesEnum), value);
								break;
							case "Flammability":
								flammability = float.Parse(value);
								break;
							case "MaxIntegrity":
								maxIntegrity = int.Parse(value);
								break;
							case "Walkable":
								walkable = bool.Parse(value);
								break;
							case "WalkSpeed":
								walkSpeed = float.Parse(value);
								break;
							case "Layer":
								layer = int.Parse(value);
								break;
							case "AddToTileWhenBuilt":
								addToTileWhenBuilt = bool.Parse(value);
								break;
							case "BlockingAmount":
								List<string> blockingDirections = value.Split(',').ToList();
								blockingAmount = new Vector2(float.Parse(blockingDirections[0]), float.Parse(blockingDirections[1]));
								break;
							case "MultiTilePositions":
								List<string> multiTilePositionStrings = value.Split(';').ToList();
								foreach (string multiTilePositionString in multiTilePositionStrings) {
									multiTilePositions.Add(new Vector2(float.Parse(multiTilePositionString.Split(',')[0]), float.Parse(multiTilePositionString.Split(',')[1])));
								}
								break;
							case "MaxInventoryAmount":
								maxInventoryAmount = int.Parse(value);
								break;
							case "MaxLightDistance":
								maxLightDistance = int.Parse(value);
								break;
							case "LightColour":
								lightColour = uiM.HexToColor(value);
								break;
							case "RestComfortAmount":
								restComfortAmount = float.Parse(value);
								break;
							default:
								print("Unknown tile object prefab label: \"" + singleTileObjectPrefabDataLineString + "\"");
								break;
						}
					}
				}

				TileObjectPrefab newTOP = new TileObjectPrefab(
					this,
					type,
					timeToBuild,
					resourcesToBuild,
					selectionModifiers,
					jobType,
					flammability,
					maxIntegrity,
					walkable,
					walkSpeed,
					layer,
					addToTileWhenBuilt,
					blockingAmount,
					multiTilePositions,
					maxInventoryAmount,
					maxLightDistance,
					lightColour,
					restComfortAmount
				);
				tileObjectPrefabs.Add(newTOP);
				resourceM.tileObjectPrefabs.Add(newTOP);
			}
		}
	}

	public class TileObjectPrefab {

		private UIManager uiM;

		void GetScriptReferences() {

			GameObject GM = GameObject.Find("GM");

			uiM = GM.GetComponent<UIManager>();
		}

		public TileObjectPrefabsEnum type;
		public string name;

		public TileObjectPrefabSubGroup tileObjectPrefabSubGroup;

		public Sprite baseSprite;
		public List<Sprite> bitmaskSprites = new List<Sprite>();
		public List<Sprite> activeSprites = new List<Sprite>();

		public int timeToBuild;
		public List<ResourceAmount> resourcesToBuild = new List<ResourceAmount>();
		public List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
		public JobManager.JobTypesEnum jobType;

		public float flammability;

		public int maxIntegrity;

		public bool walkable;
		public float walkSpeed;

		public int layer;

		public bool addToTileWhenBuilt;

		public Vector2 blockingAmount;

		public List<Vector2> multiTilePositions = new List<Vector2>() { new Vector2(0, 0) };

		public int maxInventoryAmount;

		public int maxLightDistance;
		public Color lightColour;

		public float restComfortAmount;

		public TileObjectPrefab(
			TileObjectPrefabSubGroup tileObjectPrefabSubGroup,
			TileObjectPrefabsEnum type,
			int timeToBuild,
			List<ResourceAmount> resourcesToBuild,
			List<JobManager.SelectionModifiersEnum> selectionModifiers,
			JobManager.JobTypesEnum jobType,
			float flammability,
			int maxIntegrity,
			bool walkable,
			float walkSpeed,
			int layer,
			bool addToTileWhenBuilt,
			Vector2 blockingAmount,
			List<Vector2> multiTilePositions,
			int maxInventoryAmount,
			int maxLightDistance,
			Color lightColour,
			float restComfortAmount
			) {

			GetScriptReferences();

			this.tileObjectPrefabSubGroup = tileObjectPrefabSubGroup;

			this.type = type;
			name = uiM.SplitByCapitals(type.ToString());

			this.timeToBuild = timeToBuild;

			this.resourcesToBuild = resourcesToBuild;

			this.selectionModifiers = selectionModifiers;

			this.jobType = jobType;

			this.flammability = flammability;

			this.maxIntegrity = maxIntegrity;

			this.walkable = walkable;
			this.walkSpeed = walkSpeed;

			this.layer = layer;

			this.addToTileWhenBuilt = addToTileWhenBuilt;

			this.blockingAmount = blockingAmount;

			this.multiTilePositions.AddRange(multiTilePositions);

			this.maxInventoryAmount = maxInventoryAmount;

			this.maxLightDistance = maxLightDistance;
			this.lightColour = lightColour;

			this.restComfortAmount = restComfortAmount;

			baseSprite = Resources.Load<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-base");
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-bitmask").ToList();
			activeSprites = Resources.LoadAll<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-active").ToList();

			if (baseSprite == null && bitmaskSprites.Count > 0) {
				baseSprite = bitmaskSprites[0];
			}
			if (jobType == JobManager.JobTypesEnum.PlantFarm) {
				baseSprite = bitmaskSprites[bitmaskSprites.Count - 1];
			}
		}
	}

	public Dictionary<TileObjectPrefab,List<TileObjectInstance>> tileObjectInstances = new Dictionary<TileObjectPrefab,List<TileObjectInstance>>();
	public List<Farm> farms = new List<Farm>();

	public List<TileObjectInstance> GetTileObjectInstanceList(TileObjectPrefab prefab) {
		if (tileObjectInstances.ContainsKey(prefab)) {
			return tileObjectInstances[prefab];
		}
		Debug.LogWarning("Tried accessing a tile object instance which isn't already in the list");
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
			if (ContainerTileObjectTypes.Contains(tileObjectInstance.prefab.type)) {
				Container targetContainer = containers.Find(container => container.parentObject == tileObjectInstance);
				if (uiM.selectedContainer == targetContainer) {
					uiM.SetSelectedContainer(null);
				}
				containers.Remove(targetContainer);
			}
			if (ManufacturingTileObjects.ContainsKey(tileObjectInstance.prefab.tileObjectPrefabSubGroup.type)) {
				if (ManufacturingTileObjects[tileObjectInstance.prefab.tileObjectPrefabSubGroup.type].Contains(tileObjectInstance.prefab.type)) {
					ManufacturingTileObject targetMTO = manufacturingTileObjectInstances.Find(mto => mto.parentObject == tileObjectInstance);
					if (uiM.selectedMTO == targetMTO) {
						uiM.SetSelectedManufacturingTileObject(null);
					}
					manufacturingTileObjectInstances.Remove(targetMTO);
				}
			}
			if (LightSourceTileObjects.Contains(tileObjectInstance.prefab.type)) {
				LightSource targetLightSource = lightSources.Find(lightSource => lightSource.parentObject == tileObjectInstance);
				targetLightSource.RemoveTileBrightnesses();
				lightSources.Remove(targetLightSource);
			}
			if (SleepSpotTileObjects.Contains(tileObjectInstance.prefab.type)) {
				SleepSpot targetSleepSpot = sleepSpots.Find(sleepSpot => sleepSpot.parentObject == tileObjectInstance);
				sleepSpots.Remove(targetSleepSpot);
			}
			tileObjectInstances[tileObjectInstance.prefab].Remove(tileObjectInstance);
			uiM.ChangeObjectPrefabElements(UIManager.ChangeTypesEnum.Update,tileObjectInstance.prefab);
		} else {
			Debug.LogWarning("Tried removing a tile object instance which isn't in the list");
		}
		if (tileObjectInstances[tileObjectInstance.prefab].Count <= 0) {
			tileObjectInstances.Remove(tileObjectInstance.prefab);
			uiM.ChangeObjectPrefabElements(UIManager.ChangeTypesEnum.Remove,tileObjectInstance.prefab);
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

		public bool active;

		public ManufacturingTileObject mto;

		public float integrity;

		public TileObjectInstance(TileObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) {

			GetScriptReferences();

			this.prefab = prefab;
			this.tile = tile;
			this.rotationIndex = rotationIndex;

			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
			obj.name = "Tile Object Instance: " + prefab.name;
			obj.GetComponent<SpriteRenderer>().sortingOrder = 1 + prefab.layer; // Tile Object Sprite
			obj.GetComponent<SpriteRenderer>().sprite = prefab.baseSprite;

			if (resourceM.ContainerTileObjectTypes.Contains(prefab.type)) {
				resourceM.containers.Add(new Container(this,prefab.maxInventoryAmount));
			}
			if (resourceM.ManufacturingTileObjects.ContainsKey(prefab.tileObjectPrefabSubGroup.type)) {
				resourceM.manufacturingTileObjectInstances.Add(new ManufacturingTileObject(this));
			}
			if (resourceM.LightSourceTileObjects.Contains(prefab.type)) {
				resourceM.lightSources.Add(new LightSource(this));
			}
			if (resourceM.SleepSpotTileObjects.Contains(prefab.type)) {
				resourceM.sleepSpots.Add(new SleepSpot(this));
			}

			if (resourceM.LightBlockingTileObjects.Contains(prefab.type)) {
				foreach (LightSource lightSource in resourceM.lightSources) {
					if (lightSource.litTiles.Contains(tile)) {
						lightSource.RemoveTileBrightnesses();
						lightSource.SetTileBrightnesses();
					}
				}
			}

			SetColour(tile.sr.color);

			integrity = prefab.maxIntegrity;
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

		public void SetActiveSprite(bool newActiveValue, JobManager.Job job) {
			active = newActiveValue;
			if (active) {
				if (prefab.activeSprites.Count > 0) {
					if (prefab.type == TileObjectPrefabsEnum.SplittingBlock) {
						int customActiveSpriteIndex = 0;
						if (job.createResource.type == ResourcesEnum.Wood) {
							customActiveSpriteIndex = 0;
						} else if (job.createResource.type == ResourcesEnum.Firewood) {
							customActiveSpriteIndex = 1;
						}
						obj.GetComponent<SpriteRenderer>().sprite = prefab.activeSprites[4 * customActiveSpriteIndex + rotationIndex];
					} else {
						obj.GetComponent<SpriteRenderer>().sprite = prefab.activeSprites[rotationIndex];
					}
				}
			} else {
				if (prefab.bitmaskSprites.Count > 0) {
					obj.GetComponent<SpriteRenderer>().sprite = prefab.bitmaskSprites[rotationIndex];
				} else {
					obj.GetComponent<SpriteRenderer>().sprite = prefab.baseSprite;
				}
			}
		}
	}

	List<TileObjectPrefabsEnum> ContainerTileObjectTypes = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.Basket, TileObjectPrefabsEnum.WoodenChest
	};
	public List<TileObjectPrefabsEnum> GetContainerTileObjectTypes() {
		return ContainerTileObjectTypes;
	}

	public enum PlantGroupsEnum { Cactus, ColourfulShrub, ColourfulTree, DeadTree, Shrub, SnowTree, ThinTree, WideTree };

	private Dictionary<ResourcesEnum, ResourcesEnum> seedToHarvestResource = new Dictionary<ResourcesEnum, ResourcesEnum>() {
		{ ResourcesEnum.AppleSeed,ResourcesEnum.Apple },
		{ ResourcesEnum.Berries,ResourcesEnum.Berries }
	};
	public Dictionary<ResourcesEnum, ResourcesEnum> GetSeedToHarvestResource() {
		return seedToHarvestResource;
	}

	public List<PlantGroup> plantGroups = new List<PlantGroup>();

	public class PlantGroup {
		public PlantGroupsEnum type;
		public string name;

		public Resource seed;

		public TileObjectPrefab plantPlantObjectPrefab;

		public List<ResourceAmount> returnResources = new List<ResourceAmount>();

		public List<Sprite> smallPlants = new List<Sprite>();
		public List<Sprite> fullPlants = new List<Sprite>();

		public Dictionary<ResourcesEnum, Dictionary<bool, List<Sprite>>> harvestResourceSprites = new Dictionary<ResourcesEnum, Dictionary<bool, List<Sprite>>>();

		public float maxIntegrity;

		public PlantGroup(PlantGroupsEnum type, string simpleName, Resource seed, TileObjectPrefab plantPlantObjectPrefab, List<ResourceAmount> returnResources, float maxIntegrity, ResourceManager resourceM) {
			this.type = type;
			name = simpleName;

			this.seed = seed;

			this.plantPlantObjectPrefab = plantPlantObjectPrefab;

			this.returnResources = returnResources;

			this.maxIntegrity = maxIntegrity;

			string typeString = type.ToString();
			smallPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + typeString + "/" + typeString + "-small").ToList();
			fullPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + typeString + "/" + typeString + "-full").ToList();

			foreach (Resource resource in resourceM.resources) {
				Dictionary<bool, List<Sprite>> foundSpriteSizes = new Dictionary<bool, List<Sprite>>();
				List<Sprite> smallResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + typeString + "/" + resource.type.ToString() + "/" + typeString + "-small-" + resource.type.ToString().ToLower()).ToList();
				if (smallResourceSprites != null && smallResourceSprites.Count > 0) {
					foundSpriteSizes.Add(true, smallResourceSprites);
				}
				List<Sprite> fullResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + typeString + "/" + resource.type.ToString() + "/" + typeString + "-full-" + resource.type.ToString().ToLower()).ToList();
				if (fullResourceSprites != null && fullResourceSprites.Count > 0) {
					foundSpriteSizes.Add(false, fullResourceSprites);
				}
				if (foundSpriteSizes.Count > 0) {
					harvestResourceSprites.Add(resource.type, foundSpriteSizes);
				}
			}
		}
	}

	public void CreatePlantGroups() {
		List<string> plantDataStringList = Resources.Load<TextAsset>(@"Data/plants").text.Replace("\t", string.Empty).Split(new string[] { "<PlantGroup>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string singlePlantDataString in plantDataStringList) {

			PlantGroupsEnum type = PlantGroupsEnum.Cactus;
			string name = string.Empty;
			Resource seed = null;
			TileObjectPrefab plantPlantObjectPrefab = null;
			List<ResourceAmount> returnResources = new List<ResourceAmount>();
			float maxIntegrity = 0;

			List<string> singlePlantDataLineStringList = singlePlantDataString.Split('\n').ToList();
			foreach (string singlePlantDataLineString in singlePlantDataLineStringList.Skip(1)) {
				if (!string.IsNullOrEmpty(singlePlantDataLineString)) {

					string label = singlePlantDataLineString.Split('>')[0].Replace("<", string.Empty);
					string value = singlePlantDataLineString.Split('>')[1];

					switch (label) {
						case "Type":
							type = (PlantGroupsEnum)System.Enum.Parse(typeof(PlantGroupsEnum), value);
							break;
						case "SimpleName":
							name = uiM.RemoveNonAlphanumericChars(value);
							break;
						case "Seed":
							seed = GetResourceByEnum((ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum), value));
							break;
						case "PlantPlantObjectPrefab":
							plantPlantObjectPrefab = GetTileObjectPrefabByEnum((TileObjectPrefabsEnum)System.Enum.Parse(typeof(TileObjectPrefabsEnum), value));
							break;
						case "ReturnResources":
							foreach (string returnResourceAmountString in value.Split(',')) {
								Resource resource = GetResourceByEnum((ResourcesEnum)System.Enum.Parse(typeof(ResourcesEnum), returnResourceAmountString.Split(':')[0]));
								int amount = int.Parse(returnResourceAmountString.Split(':')[1]);
								returnResources.Add(new ResourceAmount(resource, amount));
							}
							break;
						case "MaxIntegrity":
							maxIntegrity = float.Parse(value);
							break;
						default:
							print("Unknown plant label: \"" + singlePlantDataLineString + "\"");
							break;
					}
				}
			}

			plantGroups.Add(new PlantGroup(type, name, seed, plantPlantObjectPrefab, returnResources, maxIntegrity, this));
		}
	}

	public PlantGroup GetPlantGroupByEnum(PlantGroupsEnum plantGroup) {
		return plantGroups.Find(group => group.type == plantGroup);
	}

	public class Plant {
		public string name;

		public PlantGroup group;
		public TileManager.Tile tile;
		public GameObject obj;

		public bool small;

		public float growthProgress = 0;

		public Resource harvestResource;

		public float integrity;

		public Plant(PlantGroup group, TileManager.Tile tile, bool randomSmall, bool smallValue, List<Plant> smallPlants, bool giveHarvestResource, ResourceManager.Resource specificHarvestResource, ResourceManager resourceM) {
			name = group.name;

			this.group = group;
			this.tile = tile;

			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"), tile.obj.transform.position, Quaternion.identity);

			SpriteRenderer pSR = obj.GetComponent<SpriteRenderer>();

			small = (randomSmall ? Random.Range(0f, 1f) < 0.1f : smallValue);

			harvestResource = null;
			if (giveHarvestResource) {
				if (group.type == PlantGroupsEnum.WideTree && !small) {
					if (Random.Range(0f, 1f) <= 0.05f) {
						harvestResource = resourceM.GetResourceByEnum(ResourcesEnum.Apple);
					}
				} else if (group.type == PlantGroupsEnum.Shrub && !small) {
					if (Random.Range(0f, 1f) <= 0.05f) {
						harvestResource = resourceM.GetResourceByEnum(ResourcesEnum.Berries);
					}
				}
			}
			if (specificHarvestResource != null) {
				harvestResource = specificHarvestResource;
			}

			pSR.sprite = (small ? group.smallPlants[Random.Range(0, group.smallPlants.Count)] : group.fullPlants[Random.Range(0, group.fullPlants.Count)]);
			if (harvestResource != null) {
				name = harvestResource.name + " " + name;
				if (group.harvestResourceSprites.ContainsKey(harvestResource.type)) {
					if (group.harvestResourceSprites[harvestResource.type].ContainsKey(small)) {
						pSR.sprite = group.harvestResourceSprites[harvestResource.type][small][Random.Range(0, group.harvestResourceSprites[harvestResource.type][small].Count)];
					}
				}
			}
			pSR.sortingOrder = 1; // Plant Sprite

			obj.name = "Plant: " + name + " " + pSR.sprite.name;
			obj.transform.parent = tile.obj.transform;

			if (small) {
				smallPlants.Add(this);
			}

			integrity = group.maxIntegrity;
		}

		public List<ResourceAmount> GetResources() {
			List<ResourceAmount> resourcesToReturn = new List<ResourceAmount>();
			foreach (ResourceAmount resourceAmount in group.returnResources) {
				int amount = Mathf.Clamp(resourceAmount.amount + Random.Range(-2, 2), 1, int.MaxValue);
				if (small && amount > 0) {
					amount = Mathf.CeilToInt(amount / 2f);
				}
				resourcesToReturn.Add(new ResourceAmount(resourceAmount.resource, amount));
			}
			if (harvestResource != null) {
				int randomRangeAmount = 1;
				if (harvestResource.type == ResourcesEnum.Apple) {
					randomRangeAmount = Random.Range(1, 6);
				} else if (harvestResource.type == ResourcesEnum.Berries) {
					randomRangeAmount = Random.Range(5, 20);
				}
				int amount = Mathf.Clamp(randomRangeAmount, 1, int.MaxValue);
				if (small && amount > 0) {
					amount = Mathf.CeilToInt(amount / 2f);
				}
				resourcesToReturn.Add(new ResourceAmount(harvestResource, amount));
			}
			return resourcesToReturn;
		}

		public void Grow(List<Plant> smallPlants) {
			small = false;
			obj.GetComponent<SpriteRenderer>().sprite = group.fullPlants[Random.Range(0, group.fullPlants.Count)];
			if (harvestResource != null) {
				if (group.harvestResourceSprites.ContainsKey(harvestResource.type)) {
					if (group.harvestResourceSprites[harvestResource.type].ContainsKey(small)) {
						obj.GetComponent<SpriteRenderer>().sprite = group.harvestResourceSprites[harvestResource.type][small][Random.Range(0, group.harvestResourceSprites[harvestResource.type][small].Count)];
					}
				}
			}
			smallPlants.Remove(this);
		}
	}

	public PlantGroup GetPlantGroupByBiome(TileManager.Biome biome, bool guaranteedTree) {
		if (guaranteedTree) {
			List<PlantGroupsEnum> biomePlantGroupsEnums = biome.vegetationChances.Keys.Where(group => group != PlantGroupsEnum.DeadTree).ToList();
			if (biomePlantGroupsEnums.Count > 0) {
				return GetPlantGroupByEnum(biomePlantGroupsEnums[Random.Range(0, biomePlantGroupsEnums.Count)]);
			} else {
				return null;
			}
		} else {
			foreach (KeyValuePair<PlantGroupsEnum, float> kvp in biome.vegetationChances) {
				PlantGroupsEnum plantGroup = kvp.Key;
				if (Random.Range(0f, 1f) < biome.vegetationChances[plantGroup]) {
					return GetPlantGroupByEnum(plantGroup);
				}
			}
		}
		return null;
	}

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

	public List<LightSource> lightSources = new List<LightSource>();
	public class LightSource {

		private TileManager tileM;
		private TimeManager timeM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			timeM = GM.GetComponent<TimeManager>();
		}

		public TileManager.Tile parentTile;
		public TileObjectInstance parentObject;

		public List<TileManager.Tile> litTiles = new List<TileManager.Tile>();

		public LightSource(TileObjectInstance parentObject) {
			GetScriptReferences();

			parentTile = parentObject.tile;
			this.parentObject = parentObject;

			SetTileBrightnesses();
		}

		public void SetTileBrightnesses() {
			foreach (TileManager.Tile tile in tileM.map.tiles) {
				float distance = Vector2.Distance(tile.obj.transform.position, parentTile.obj.transform.position);
				if (distance <= parentObject.prefab.maxLightDistance) {
					float intensityAtTile = Mathf.Clamp(parentObject.prefab.maxLightDistance * (1f / Mathf.Pow(distance, 2f)), 0f, 1f);
					if (tile != parentTile) {
						bool lightTile = true;
						Vector3 lightVector = parentObject.obj.transform.position;
						while ((parentObject.obj.transform.position - lightVector).magnitude <= distance) {
							if (tileM.map.GetTileFromPosition(lightVector) != parentTile) {
								if (tileM.map.TileBlocksLight(tileM.map.GetTileFromPosition(lightVector))) {
									lightTile = false;
									break;
								}
							}
							lightVector += (tile.obj.transform.position - parentObject.obj.transform.position).normalized * 0.1f;
						}
						if (lightTile) {
							tile.AddLightSourceBrightness(this, intensityAtTile);
							litTiles.Add(tile);
						}
					} else {
						parentTile.AddLightSourceBrightness(this, intensityAtTile);
					}
				}
			}
			tileM.map.SetTileBrightness(timeM.GetTileBrightnessTime());
		}

		public void RemoveTileBrightnesses() {
			foreach (TileManager.Tile tile in litTiles) {
				tile.RemoveLightSourceBrightness(this);
			}
			litTiles.Clear();
			parentTile.RemoveLightSourceBrightness(this);
			tileM.map.SetTileBrightness(timeM.GetTileBrightnessTime());
		}
	}

	Dictionary<ResourcesEnum, int> FarmGrowTimes = new Dictionary<ResourcesEnum, int>() {
		{ ResourcesEnum.WheatSeed,5760 },
		{ ResourcesEnum.Potato,2880 },
		{ ResourcesEnum.CottonSeed,5760 }
	};
	public Dictionary<ResourcesEnum,int> GetFarmGrowTimes() {
		return FarmGrowTimes;
	}
	Dictionary<ResourcesEnum,ResourcesEnum> FarmSeedReturnResource = new Dictionary<ResourcesEnum,ResourcesEnum>() {
		{ ResourcesEnum.WheatSeed,ResourcesEnum.Wheat },
		{ ResourcesEnum.Potato,ResourcesEnum.Potato },
		{ ResourcesEnum.CottonSeed,ResourcesEnum.Cotton }
	};
	public Dictionary<ResourcesEnum,ResourcesEnum> GetFarmSeedReturnResource() {
		return FarmSeedReturnResource;
	}
	Dictionary<ResourcesEnum, TileObjectPrefabsEnum> FarmSeedsTileObject = new Dictionary<ResourcesEnum, TileObjectPrefabsEnum>() {
		{ ResourcesEnum.WheatSeed,TileObjectPrefabsEnum.WheatFarm },
		{ ResourcesEnum.Potato,TileObjectPrefabsEnum.PotatoFarm },
		{ ResourcesEnum.CottonSeed,TileObjectPrefabsEnum.CottonFarm }
	};
	public Dictionary<ResourcesEnum,TileObjectPrefabsEnum> GetFarmSeedsTileObject() {
		return FarmSeedsTileObject;
	}

	public class Farm : TileObjectInstance {

		private TileManager tileM;
		private TimeManager timeM;
		private JobManager jobM;
		private ResourceManager resourceM;
		private UIManager uiM;

		void GetScriptReferencecs() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			timeM = GM.GetComponent<TimeManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public ResourcesEnum seedType;
		public string name;

		public float growTimer = 0;
		public float maxGrowthTime = 0;

		public int growProgressSpriteIndex = -1;
		public List<Sprite> growProgressSprites = new List<Sprite>();
		public int maxSpriteIndex = 0;

		private float precipitationGrowthMultiplier = 0;
		private float temperatureGrowthMultipler = 0;

		public Farm(TileObjectPrefab prefab, TileManager.Tile tile) : base(prefab,tile,0) {

			GetScriptReferencecs();

			seedType = prefab.resourcesToBuild[0].resource.type;
			name = (uiM.SplitByCapitals(seedType.ToString()).Split(' ')[0]).Replace(" ","") + " Farm";
			maxGrowthTime = resourceM.GetFarmGrowTimes()[seedType] * Random.Range(0.9f, 1.1f);

			growProgressSprites = prefab.bitmaskSprites;
			maxSpriteIndex = growProgressSprites.Count - 1;

			precipitationGrowthMultiplier = Mathf.Min((-2 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 2) + 1), (-30 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 3) + 1));
			temperatureGrowthMultipler = Mathf.Clamp(Mathf.Min(((tile.temperature - 10) / 15f + 1), (-((tile.temperature - 50) / 20f))), 0, 1);

			Update();
		}

		public void Update() {
			if (growTimer >= maxGrowthTime) {
				if (!jobM.JobOfTypeExistsAtTile(JobManager.JobTypesEnum.HarvestFarm,tile)) {
					jobM.CreateJob(new JobManager.Job(tile,resourceM.GetTileObjectPrefabByEnum(TileObjectPrefabsEnum.HarvestFarm),0));
				}
			} else {
				growTimer += CalculateGrowthRate();
				int newGrowProgressSpriteIndex = Mathf.FloorToInt((growTimer / (maxGrowthTime + 10)) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[Mathf.Clamp(growProgressSpriteIndex,0,maxSpriteIndex)];
				}
			}
		}

		public float CalculateGrowthRate() {
			float growthRate = timeM.deltaTime;
			growthRate *= Mathf.Max(tileM.map.CalculateBrightnessLevelAtHour(timeM.GetTileBrightnessTime()), tile.lightSourceBrightness);
			growthRate *= precipitationGrowthMultiplier;
			growthRate *= temperatureGrowthMultipler;
			growthRate = Mathf.Clamp(growthRate, 0, 1);
			return growthRate;
		}
	}

	public List<Container> containers = new List<Container>();

	public class Inventory {

		private UIManager uiM;
		private JobManager jobM;
		private ResourceManager resourceM;

		private void GetScriptReferences() {

			GameObject GM = GameObject.Find("GM");

			uiM = GM.GetComponent<UIManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
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
			if (existingResourceAmount != null) {
				if (amount >= 0 || (amount - existingResourceAmount.amount) >= 0) {
					existingResourceAmount.amount += amount;
				} else if (amount < 0 && (existingResourceAmount.amount + amount) >= 0) {
					existingResourceAmount.amount += amount;
				}
			} else {
				if (amount > 0) {
					resources.Add(new ResourceAmount(resource,amount));
				}
			}
			existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (existingResourceAmount.amount == 0) {
					resources.Remove(existingResourceAmount);
				}
			}
			resourceM.CalculateResourceTotals();
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
			jobM.UpdateColonistJobs();
		}

		public bool ReserveResources(List<ResourceAmount> resourcesToReserve, ColonistManager.Colonist colonistReservingResources) {
			bool allResourcesFound = true;
			foreach (ResourceAmount raReserve in resourcesToReserve) {
				ResourceAmount raInventory = resources.Find(ra => ra.resource == raReserve.resource);
				if (!(raInventory != null && raInventory.amount >= raReserve.amount)) {
					allResourcesFound = false;
				}
			}
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
			List<ReservedResources> reservedResourcesByColonist = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.colonist == colonistReservingResources) {
					reservedResourcesByColonist.Add(rr);
				}
			}
			foreach (ReservedResources rr in reservedResourcesByColonist) {
				reservedResources.Remove(rr);
			}
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
			return reservedResourcesByColonist;
		}

		public void ReleaseReservedResources(ColonistManager.Colonist colonist) {
			List<ReservedResources> reservedResourcesToRemove = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.colonist == colonist) {
					foreach (ResourceAmount ra in rr.resources) {
						ChangeResourceAmount(ra.resource, ra.amount);
					}
					reservedResourcesToRemove.Add(rr);
				}
			}
			foreach (ReservedResources rrRemove in reservedResourcesToRemove) {
				reservedResources.Remove(rrRemove);
			}
			reservedResourcesToRemove.Clear();
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedContainerInfo();
		}
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in resources) {
			resource.worldTotalAmount = 0;
			resource.colonistsTotalAmount = 0;
			resource.containerTotalAmount = 0;
			resource.unreservedContainerTotalAmount = 0;
		}
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			foreach (ResourceAmount resourceAmount in colonist.inventory.resources) {
				resourceAmount.resource.worldTotalAmount += resourceAmount.amount;
				resourceAmount.resource.colonistsTotalAmount += resourceAmount.amount;
			}
			foreach (ReservedResources reservedResources in colonist.inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.worldTotalAmount += resourceAmount.amount;
					resourceAmount.resource.colonistsTotalAmount += resourceAmount.amount;
				}
			}
		}
		foreach (Container container in containers) {
			foreach (ResourceAmount resourceAmount in container.inventory.resources) {
				resourceAmount.resource.worldTotalAmount += resourceAmount.amount;
				resourceAmount.resource.containerTotalAmount += resourceAmount.amount;
				resourceAmount.resource.unreservedContainerTotalAmount += resourceAmount.amount;
			}
			foreach (ReservedResources reservedResources in container.inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.worldTotalAmount += resourceAmount.amount;
					resourceAmount.resource.containerTotalAmount += resourceAmount.amount;
				}
			}
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