using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

	public GameObject tilePrefab;
	public GameObject humanPrefab;
	public Sprite selectionCornersSprite;
	public Sprite whiteSquareSprite;
	public Sprite clearSquareSprite;
	public GameObject planetTilePrefab;
	public GameObject tileImage;
	public GameObject objectDataPanel;

	public void GetResourceReferences() {
		tilePrefab = Resources.Load<GameObject>(@"Prefabs/Tile");
		humanPrefab = Resources.Load<GameObject>(@"Prefabs/Human");
		selectionCornersSprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		whiteSquareSprite = Resources.Load<Sprite>(@"UI/white-square");
		clearSquareSprite = Resources.Load<Sprite>(@"UI/clear-square");
		planetTilePrefab = Resources.Load<GameObject>(@"UI/UIElements/PlanetTile");
		tileImage = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage");
		objectDataPanel = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel");
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
		WheatSeed, CottonSeed, TreeSeed, AppleSeed, BushSeed, CactusSeed,
		Wheat,
		Potato, BakedPotato, Blueberry, Apple, BakedApple,
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
			name = UIManager.SplitByCapitals(type.ToString());

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
								type = (ResourcesEnum)Enum.Parse(typeof(ResourcesEnum), value);
								break;
							case "Weight":
								weight = float.Parse(value);
								break;
							case "Price":
								int gold = 0;
								int silver = 0;
								int bronze = 0;
								foreach (string priceValue in value.Split(',')) {
									string priceValueAmount = UIManager.RemoveNonAlphanumericChars(value.Split(':')[0]);
									string priceValueDenomination = UIManager.RemoveNonAlphanumericChars(value.Split(':')[1]);
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
									ResourcesEnum resourceType = (ResourcesEnum)Enum.Parse(typeof(ResourcesEnum), resourceName);
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

		private int worldTotalAmount;
		private int colonistsTotalAmount;
		private int containerTotalAmount;
		private int unreservedContainerTotalAmount;
		private int availableAmount;

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
			name = UIManager.SplitByCapitals(type.ToString());

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
			public int gold = 0; // Amount of Gold in this price
			public int silver = 0; // Amount of Silver in this Price
			public int bronze = 0; // Amount of Bronze in this Price

			public float relativeGold = 0; // Entire value of this Price, represented as only Gold
			public float relativeSilver = 0; // Entire value of this Price, represented as only Silver
			public float relativeBronze = 0; // Entire value of this Price, represented as only Bronze

			public enum PriceTypeEnum { Gold, Silver, Bronze };
			public enum RelativePriceTypeEnum { RelativeGold, RelativeSilver, RelativeBronze };

			public Price() {

			}

			public Price(int gold, int silver, int bronze) {
				this.gold = gold;
				this.silver = silver;
				this.bronze = bronze;
			}

			public void CalculateRelativePrices() {
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

			public void Redistribute() {
				silver += (bronze > 0 ? Mathf.FloorToInt(bronze / 100f) : Mathf.CeilToInt(bronze / 100f));
				bronze %= 100; // Does this work with negatives?

				gold += (silver > 0 ? Mathf.FloorToInt(silver / 100f) : Mathf.CeilToInt(silver / 100f));
				silver %= 100; // Does this work with negatives?
			}

			public void ChangePrice(Price price, int multiple) {
				gold += price.gold * multiple;
				silver += price.silver * multiple;
				bronze += price.bronze * multiple;

				Redistribute();

				CalculateRelativePrices();
			}

			public override string ToString() {
				return string.Format("<color=\"#f39c12\">{0}</color> <color=\"#7f8c8d\">{1}</color> <color=\"#d35400\">{2}</color>", gold, silver, bronze);
			}
		}

		public int GetWorldTotalAmount() {
			return worldTotalAmount;
		}

		public void SetWorldTotalAmount(int worldTotalAmount) {
			this.worldTotalAmount = worldTotalAmount;
		}

		public void AddToWorldTotalAmount(int amount) {
			SetWorldTotalAmount(GetWorldTotalAmount() + amount);
		}

		public int GetColonistsTotalAmount() {
			return colonistsTotalAmount;
		}

		public void SetColonistsTotalAmount(int colonistsTotalAmount) {
			this.colonistsTotalAmount = colonistsTotalAmount;
		}

		public void AddToColonistsTotalAmount(int amount) {
			SetColonistsTotalAmount(GetColonistsTotalAmount() + amount);
		}

		public int GetContainerTotalAmount() {
			return containerTotalAmount;
		}

		public void SetContainerTotalAmount(int containerTotalAmount) {
			this.containerTotalAmount = containerTotalAmount;
		}

		public void AddToContainerTotalAmount(int amount) {
			SetContainerTotalAmount(GetContainerTotalAmount() + amount);
		}

		public int GetUnreservedContainerTotalAmount() {
			return unreservedContainerTotalAmount;
		}

		public void SetUnreservedContainerTotalAmount(int unreservedContainerTotalAmount) {
			this.unreservedContainerTotalAmount = unreservedContainerTotalAmount;
		}

		public void AddToUnreservedContainerTotalAmount(int amount) {
			SetUnreservedContainerTotalAmount(GetUnreservedContainerTotalAmount() + amount);
		}

		public void SetAvailableAmount(int availableAmount) {
			this.availableAmount = availableAmount;
		}

		public void CalculateAvailableAmount() {
			availableAmount = GetColonistsTotalAmount() + GetUnreservedContainerTotalAmount();
		}

		public int GetAvailableAmount() {
			return availableAmount;
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

	public class TradeResourceAmount {
		public Resource resource;

		public int caravanAmount;

		private int oldTradeAmount = 0;
		private int tradeAmount;

		public Resource.Price caravanResourcePrice;

		public ColonistManager.Caravan caravan;

		public UIManager.TradeResourceElement tradeResourceElement;

		public TradeResourceAmount(Resource resource, int caravanAmount, Resource.Price caravanResourcePrice, ColonistManager.Caravan caravan) {
			this.resource = resource;

			this.caravanAmount = caravanAmount;

			this.caravanResourcePrice = caravanResourcePrice;

			this.caravan = caravan;
		}

		public void Update() {
			UpdateCaravanAmount();
		}

		private void UpdateCaravanAmount() {
			caravanAmount = 0;
			ResourceAmount caravanResourceAmount = caravan.inventory.resources.Find(ra => ra.resource == resource);
			if (caravanResourceAmount != null) {
				caravanAmount = caravanResourceAmount.amount;
			}
		}

		public int GetTradeAmount() {
			return tradeAmount;
		}

		public void SetTradeAmount(int tradeAmount) {
			oldTradeAmount = this.tradeAmount;
			this.tradeAmount = tradeAmount;
			if (oldTradeAmount != this.tradeAmount) {
				caravan.SetSelectedResource(this);
			}
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

		public void ChangeResourceAmount(Resource resource, int amount) {
			ResourceAmount existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (amount >= 0 || (amount - existingResourceAmount.amount) >= 0) {
					existingResourceAmount.amount += amount;
				} else if (amount < 0 && (existingResourceAmount.amount + amount) >= 0) {
					existingResourceAmount.amount += amount;
				}
			} else {
				if (amount > 0) {
					resources.Add(new ResourceAmount(resource, amount));
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
			uiM.SetSelectedTraderMenu();
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
					ChangeResourceAmount(raInventory.resource, -raReserve.amount);
				}
				reservedResources.Add(new ReservedResources(resourcesToReserve, colonistReservingResources));
			}
			uiM.SetSelectedColonistInformation();
			uiM.SetSelectedTraderMenu();
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
			uiM.SetSelectedTraderMenu();
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
			uiM.SetSelectedTraderMenu();
			uiM.SetSelectedContainerInfo();
		}
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in resources) {
			resource.SetWorldTotalAmount(0);
			resource.SetColonistsTotalAmount(0);
			resource.SetContainerTotalAmount(0);
			resource.SetUnreservedContainerTotalAmount(0);
			resource.SetAvailableAmount(0);
		}

		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			foreach (ResourceAmount resourceAmount in colonist.inventory.resources) {
				resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToColonistsTotalAmount(resourceAmount.amount);
			}
			foreach (ReservedResources reservedResources in colonist.inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
					resourceAmount.resource.AddToColonistsTotalAmount(resourceAmount.amount);
				}
			}
		}
		foreach (Container container in containers) {
			foreach (ResourceAmount resourceAmount in container.inventory.resources) {
				resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToContainerTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToUnreservedContainerTotalAmount(resourceAmount.amount);
			}
			foreach (ReservedResources reservedResources in container.inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
					resourceAmount.resource.AddToContainerTotalAmount(resourceAmount.amount);
				}
			}
		}

		foreach (Resource resource in resources) {
			resource.CalculateAvailableAmount();
		}
	}

	public List<ResourceAmount> GetFilteredResources(bool colonistInventory, bool colonistReserved, bool containerInventory, bool containerReserved) {
		List<ResourceAmount> returnResources = new List<ResourceAmount>();
		if (colonistInventory || colonistReserved) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonistInventory) {
					foreach (ResourceAmount resourceAmount in colonist.inventory.resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
						} else {
							existingResourceAmount.amount += resourceAmount.amount;
						}
					}
				}
				if (colonistReserved) {
					foreach (ReservedResources reservedResources in colonist.inventory.reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
							} else {
								existingResourceAmount.amount += resourceAmount.amount;
							}
						}
					}
				}
			}
		}
		if (containerInventory || containerReserved) {
			foreach (Container container in containers) {
				if (containerInventory) {
					foreach (ResourceAmount resourceAmount in container.inventory.resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
						} else {
							existingResourceAmount.amount += resourceAmount.amount;
						}
					}
				}
				if (containerReserved) {
					foreach (ReservedResources reservedResources in container.inventory.reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
							} else {
								existingResourceAmount.amount += resourceAmount.amount;
							}
						}
					}
				}
			}
		}
		return returnResources;
	}

	public List<ClothingPrefab> clothingPrefabs = new List<ClothingPrefab>();

	public class ClothingPrefab {
		public ColonistManager.Human.Appearance type;
		public string name;
		public int insulation;
		public int waterResistance;
		public List<string> colours;

		public List<List<Sprite>> moveSprites = new List<List<Sprite>>();

		public ClothingPrefab(ColonistManager.Human.Appearance type, string name, int insulation, int waterResistance, List<string> colours, int typeIndex) {
			this.type = type;
			this.name = name;
			this.insulation = insulation;
			this.waterResistance = waterResistance;
			this.colours = colours;

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(Resources.LoadAll<Sprite>(@"Sprites/Clothes/" + type + "/clothes-" + type.ToString().ToLower() + "-" + i).Skip(typeIndex).Take(colours.Count).ToList());
			}
		}
	}

	public void CreateClothingPrefabs() {
		List<string> clothingDataStringList = Resources.Load<TextAsset>(@"Data/clothes").text.Replace("\t", string.Empty).Split(new string[] { "<Clothing>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string singleClothingDataString in clothingDataStringList) {

			ColonistManager.Human.Appearance type = ColonistManager.Human.Appearance.Hat;
			string name = string.Empty;
			int insulation = 0;
			int waterResistance = 0;
			List<string> colours = new List<string>();
			List<ResourceAmount> requiredResources = new List<ResourceAmount>();

			List<string> singleClothingDataLineStringList = singleClothingDataString.Split('\n').ToList();
			foreach (string singleClothingDataLineString in singleClothingDataLineStringList.Skip(1)) {
				if (!string.IsNullOrEmpty(singleClothingDataLineString)) {

					string label = singleClothingDataLineString.Split('>')[0].Replace("<", string.Empty);
					string value = singleClothingDataLineString.Split('>')[1];

					switch (label) {
						case "Type":
							type = (ColonistManager.Human.Appearance)System.Enum.Parse(typeof(ColonistManager.Human.Appearance), value);
							break;
						case "Name":
							name = UIManager.RemoveNonAlphanumericChars(value);
							break;
						case "Insulation":
							insulation = int.Parse(value);
							break;
						case "WaterResistance":
							waterResistance = int.Parse(value);
							break;
						case "Colours":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string colour in value.Split(',')) {
									colours.Add(UIManager.RemoveNonAlphanumericChars(colour));
								}
							}
							break;
						case "RequiredResources":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string requiredResourceString in value.Split(',')) {
									Resource resource = GetResourceByEnum((ResourcesEnum)Enum.Parse(typeof(ResourcesEnum), requiredResourceString.Split(':')[0]));
									int amount = int.Parse(requiredResourceString.Split(':')[1]);
									requiredResources.Add(new ResourceAmount(resource, amount));
								}
							}
							break;
						default:
							print("Unknown clothing label: \"" + singleClothingDataLineString + "\"");
							break;
					}
				}
			}

			ClothingPrefab clothingPrefab = new ClothingPrefab(type, name, insulation, waterResistance, colours, clothingPrefabs.Sum(c => c.colours.Count));
			clothingPrefabs.Add(clothingPrefab);

			//foreach (string colour in clothingPrefab.colours) {
			//	clothingInstances.Add(new ClothingInstance(clothingPrefab, colour, null));
			//}
		}
	}

	public List<ClothingPrefab> GetClothingPrefabsByAppearance(ColonistManager.Human.Appearance appearance) {
		return clothingPrefabs.FindAll(c => c.type == appearance);
	}

	public List<ClothingInstance> clothingInstances = new List<ClothingInstance>();

	public class ClothingInstance {
		public ClothingPrefab clothingPrefab;
		public string colour;
		public string name;
		public ColonistManager.Human human;
		public List<Sprite> moveSprites = new List<Sprite>();

		public ClothingInstance(ClothingPrefab clothingPrefab, string colour, ColonistManager.Human human) {
			this.clothingPrefab = clothingPrefab;
			this.colour = colour;
			this.human = human;

			name = UIManager.SplitByCapitals(colour + clothingPrefab.name);

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(clothingPrefab.moveSprites[i][clothingPrefab.colours.IndexOf(colour)]);
			}

			if (human != null) {
				human.ChangeClothing(clothingPrefab.type, this);
			}
		}
	}

	public List<ClothingInstance> GetClothingInstancesByAppearance(ColonistManager.Human.Appearance appearance) {
		return clothingInstances.FindAll(c => c.clothingPrefab.type == appearance);
	}

	public enum TileObjectPrefabGroupsEnum {
		Structure, Furniture, Industrial,
		Command,
		Farm,
		None,
	};
	public enum TileObjectPrefabSubGroupsEnum {
		Walls, Fences, Doors, Floors,
		Containers, Beds, Chairs, Tables, Lights,
		Furnaces,Processing,
		Plants, Terrain, Remove, Cancel, Priority,
		PlantFarm, HarvestFarm,
		None
	};
	public enum TileObjectPrefabsEnum {
		StoneWall, WoodenWall, BrickWall,
		WoodenFence,
		WoodenDoor,
		StoneFloor, WoodenFloor, BrickFloor,
		Basket, WoodenChest, WoodenDrawers,
		WoodenBed,
		WoodenChair,
		WoodenTable,
		Torch, WoodenLamp,
		StoneFurnace,
		CottonGin, SplittingBlock, SplittingLog, Anvil,
		ChopPlant, PlantPlant, PlantAppleTree, PlantBlueberryBush,
		Mine, Dig,
		RemoveLayer1, RemoveLayer2, RemoveAll,
		Cancel,
		IncreasePriority, DecreasePriority,
		WheatFarm, PotatoFarm, CottonFarm,
		HarvestFarm,
		CreateResource, PickupResources, EmptyInventory, Sleep, CollectWater, Drink, CollectFood, Eat, 
		PlantTree, PlantBush, PlantCactus
	};

	public static readonly List<TileObjectPrefabsEnum> bitmaskingTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor, TileObjectPrefabsEnum.WoodenFence,
		TileObjectPrefabsEnum.BrickWall, TileObjectPrefabsEnum.BrickFloor,
		TileObjectPrefabsEnum.WoodenTable
	};

	public static readonly List<TileObjectPrefabsEnum> FloorEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneFloor, TileObjectPrefabsEnum.WoodenFloor, TileObjectPrefabsEnum.BrickFloor
	};
	public static readonly List<TileObjectPrefabsEnum> WallEquivalentTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.WoodenFence, TileObjectPrefabsEnum.BrickWall
	};

	public static readonly Dictionary<TileObjectPrefabSubGroupsEnum, List<TileObjectPrefabsEnum>> manufacturingTileObjects = new Dictionary<TileObjectPrefabSubGroupsEnum, List<TileObjectPrefabsEnum>>() {
		{ TileObjectPrefabSubGroupsEnum.Furnaces, new List<TileObjectPrefabsEnum>() {
			TileObjectPrefabsEnum.StoneFurnace
		} },
		{ TileObjectPrefabSubGroupsEnum.Processing, new List<TileObjectPrefabsEnum>() {
			TileObjectPrefabsEnum.CottonGin, TileObjectPrefabsEnum.SplittingBlock, TileObjectPrefabsEnum.SplittingLog, TileObjectPrefabsEnum.Anvil
		} }
	};
	public static readonly List<TileObjectPrefabsEnum> containerTileObjectTypes = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.Basket, TileObjectPrefabsEnum.WoodenChest
	};
	public static readonly List<TileObjectPrefabsEnum> sleepSpotTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.WoodenBed
	};
	public static readonly List<TileObjectPrefabsEnum> manufacturingTileObjectsFuel = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.StoneFurnace
	};
	public static readonly List<TileObjectPrefabsEnum> manufacturingTileObjectsNoFuel = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.CottonGin, TileObjectPrefabsEnum.SplittingBlock, TileObjectPrefabsEnum.SplittingLog, TileObjectPrefabsEnum.Anvil
	};

	public static readonly List<TileObjectPrefabsEnum> lightSourceTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.WoodenLamp, TileObjectPrefabsEnum.Torch
	};

	public static readonly List<TileObjectPrefabsEnum> lightBlockingTileObjects = new List<TileObjectPrefabsEnum>() {
		TileObjectPrefabsEnum.BrickWall, TileObjectPrefabsEnum.StoneWall, TileObjectPrefabsEnum.WoodenWall, TileObjectPrefabsEnum.WoodenDoor
	};

	public void CreateTileObjectPrefabs() {
		List <string> tileObjectPrefabGroupsData = Resources.Load<TextAsset>(@"Data/tileObjectPrefabs").text.Replace("\t",string.Empty).Split(new string[] { "<Group>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string tileObjectPrefabGroupDataString in tileObjectPrefabGroupsData) {
			tileObjectPrefabGroups.Add(new TileObjectPrefabGroup(tileObjectPrefabGroupDataString, this));
		}
		foreach (Resource resource in resources) {
			resource.SetInitialMTOReferences();
		}
		uiM.CreateMenus();
	}

	public List<TileObjectPrefabGroup> tileObjectPrefabGroups = new List<TileObjectPrefabGroup>();

	public class TileObjectPrefabGroup {
		public TileObjectPrefabGroupsEnum type;
		public string name;

		public List<TileObjectPrefabSubGroup> tileObjectPrefabSubGroups = new List<TileObjectPrefabSubGroup>();

		public TileObjectPrefabGroup(string data, ResourceManager resourceM) {
			List<string> tileObjectPrefabSubGroupsData = data.Split(new string[] { "<SubGroup>" },System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabGroupsEnum),tileObjectPrefabSubGroupsData[0]);
			name = UIManager.SplitByCapitals(type.ToString());

			foreach (string tileObjectPrefabSubGroupDataString in tileObjectPrefabSubGroupsData.Skip(1)) {
				tileObjectPrefabSubGroups.Add(new TileObjectPrefabSubGroup(tileObjectPrefabSubGroupDataString, this, resourceM));
			}
		}
	}

	public class TileObjectPrefabSubGroup {
		public TileObjectPrefabSubGroupsEnum type;
		public string name;

		public TileObjectPrefabGroup tileObjectPrefabGroup;
		public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

		public TileObjectPrefabSubGroup(string data, TileObjectPrefabGroup tileObjectPrefabGroup, ResourceManager resourceM) {
			this.tileObjectPrefabGroup = tileObjectPrefabGroup;

			List<string> tileObjectPrefabsData = data.Split(new string[] { "<Object>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (TileObjectPrefabSubGroupsEnum)System.Enum.Parse(typeof(TileObjectPrefabSubGroupsEnum), tileObjectPrefabsData[0]);
			name = UIManager.SplitByCapitals(type.ToString());

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
									Vector2 multiTilePosition = new Vector2(float.Parse(multiTilePositionString.Split(',')[0]), float.Parse(multiTilePositionString.Split(',')[1]));
									multiTilePositions.Add(multiTilePosition);
								}
								break;
							case "MaxInventoryAmount":
								maxInventoryAmount = int.Parse(value);
								break;
							case "MaxLightDistance":
								maxLightDistance = int.Parse(value);
								break;
							case "LightColour":
								lightColour = UIManager.HexToColor(value);
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

	public List<TileObjectPrefab> tileObjectPrefabs = new List<TileObjectPrefab>();

	public class TileObjectPrefab {

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

		public Dictionary<int, List<Vector2>> multiTilePositions = new Dictionary<int, List<Vector2>>();
		public Dictionary<int, Vector2> anchorPositionOffset = new Dictionary<int, Vector2>();
		public Dictionary<int, Vector2> dimensions = new Dictionary<int, Vector2>();

		public int maxInventoryAmount;

		public int maxLightDistance;
		public Color lightColour;

		public float restComfortAmount;

		public bool canRotate;

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
			this.tileObjectPrefabSubGroup = tileObjectPrefabSubGroup;

			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

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

			canRotate = (!bitmaskingTileObjects.Contains(type) && bitmaskSprites.Count > 0);

			float largestX = 0;
			float largestY = 0;

			if (multiTilePositions.Count <= 0) {
				multiTilePositions.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < (bitmaskSprites.Count > 0 ? bitmaskSprites.Count : 1); i++) {
				if (i == 0 || i > 0 && canRotate) {
					this.multiTilePositions.Add(i, new List<Vector2>());
					if (i == 0) {
						this.multiTilePositions[i].AddRange(multiTilePositions);
					} else {
						foreach (Vector2 oldMultiTilePosition in this.multiTilePositions[i-1]) {
							Vector2 newMultiTilePosition = new Vector2(oldMultiTilePosition.y, largestX - oldMultiTilePosition.x);
							this.multiTilePositions[i].Add(newMultiTilePosition);
						}
					}
					largestX = this.multiTilePositions[i].OrderByDescending(MTP => MTP.x).ToList()[0].x;
					largestY = this.multiTilePositions[i].OrderByDescending(MTP => MTP.y).ToList()[0].y;
					dimensions.Add(i, new Vector2(largestX + 1, largestY + 1));
					anchorPositionOffset.Add(i, new Vector2(largestX / 2, largestY / 2));
				}
			}
		}
	}

	public TileObjectPrefab GetTileObjectPrefabByEnum(TileObjectPrefabsEnum topEnum) {
		return tileObjectPrefabs.Find(top => top.type == topEnum);
	}

	public Dictionary<TileObjectPrefab, List<TileObjectInstance>> tileObjectInstances = new Dictionary<TileObjectPrefab, List<TileObjectInstance>>();

	public List<TileObjectInstance> GetTileObjectInstanceList(TileObjectPrefab prefab) {
		if (tileObjectInstances.ContainsKey(prefab)) {
			return tileObjectInstances[prefab];
		}
		Debug.LogWarning("Tried accessing a tile object instance which isn't already in the list.");
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
			if (containerTileObjectTypes.Contains(tileObjectInstance.prefab.type)) {
				Container targetContainer = containers.Find(container => container.parentObject == tileObjectInstance);
				if (uiM.selectedContainer == targetContainer) {
					uiM.SetSelectedContainer(null);
				}
				containers.Remove(targetContainer);
			}
			if (manufacturingTileObjects.ContainsKey(tileObjectInstance.prefab.tileObjectPrefabSubGroup.type)) {
				if (manufacturingTileObjects[tileObjectInstance.prefab.tileObjectPrefabSubGroup.type].Contains(tileObjectInstance.prefab.type)) {
					ManufacturingTileObject targetMTO = manufacturingTileObjectInstances.Find(mto => mto.parentObject == tileObjectInstance);
					if (uiM.selectedMTO == targetMTO) {
						uiM.SetSelectedManufacturingTileObject(null);
					}
					manufacturingTileObjectInstances.Remove(targetMTO);
				}
			}
			if (lightSourceTileObjects.Contains(tileObjectInstance.prefab.type)) {
				LightSource targetLightSource = lightSources.Find(lightSource => lightSource.parentObject == tileObjectInstance);
				targetLightSource.RemoveTileBrightnesses();
				lightSources.Remove(targetLightSource);
			}
			if (sleepSpotTileObjects.Contains(tileObjectInstance.prefab.type)) {
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

		public TileManager.Tile tile; // The tile that this object covers that is closest to the zeroPointTile (usually they are the same tile)
		public List<TileManager.Tile> additionalTiles = new List<TileManager.Tile>();
		public TileManager.Tile zeroPointTile; // The tile representing the (0,0) position of the object even if the object doesn't cover it
		
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
			zeroPointTile = tile;
			foreach (Vector2 multiTilePosition in prefab.multiTilePositions[rotationIndex]) {
				TileManager.Tile additionalTile = zeroPointTile.map.GetTileFromPosition(zeroPointTile.obj.transform.position + (Vector3)multiTilePosition);
				additionalTiles.Add(additionalTile);
				if (additionalTile != zeroPointTile) {
					additionalTile.SetTileObjectInstanceReference(this);
				}
			}
			if (additionalTiles.Count > 0 && !additionalTiles.Contains(tile)) {
				tile = additionalTiles.OrderBy(additionalTile => Vector2.Distance(tile.obj.transform.position, additionalTile.obj.transform.position)).ToList()[0];
			} else if (additionalTiles.Count <= 0) {
				additionalTiles.Add(tile);
			}

			this.rotationIndex = rotationIndex;

			obj = Instantiate(resourceM.tilePrefab, zeroPointTile.obj.transform,false);
			obj.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			obj.name = "Tile Object Instance: " + prefab.name;
			obj.GetComponent<SpriteRenderer>().sortingOrder = 1 + prefab.layer; // Tile Object Sprite
			obj.GetComponent<SpriteRenderer>().sprite = prefab.baseSprite;

			if (containerTileObjectTypes.Contains(prefab.type)) {
				resourceM.containers.Add(new Container(this,prefab.maxInventoryAmount));
			}
			if (manufacturingTileObjects.ContainsKey(prefab.tileObjectPrefabSubGroup.type)) {
				resourceM.manufacturingTileObjectInstances.Add(new ManufacturingTileObject(this));
			}
			if (lightSourceTileObjects.Contains(prefab.type)) {
				resourceM.lightSources.Add(new LightSource(this));
			}
			if (sleepSpotTileObjects.Contains(prefab.type)) {
				resourceM.sleepSpots.Add(new SleepSpot(this));
			}

			if (lightBlockingTileObjects.Contains(prefab.type)) {
				foreach (LightSource lightSource in resourceM.lightSources) {
					foreach (TileManager.Tile objectTile in additionalTiles) {
						if (lightSource.litTiles.Contains(objectTile)) {
							lightSource.RemoveTileBrightnesses();
							lightSource.SetTileBrightnesses();
						}
					}
				}
			}

			SetColour(tile.sr.color);

			integrity = prefab.maxIntegrity;
		}

		public void FinishCreation() {
			List<TileManager.Tile> bitmaskingTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile additionalTile in additionalTiles) {
				bitmaskingTiles.Add(additionalTile);
				bitmaskingTiles.AddRange(additionalTile.surroundingTiles);
			}
			bitmaskingTiles = bitmaskingTiles.Distinct().ToList();
			resourceM.Bitmask(bitmaskingTiles);
			foreach (TileManager.Tile tile in additionalTiles) {
				SetColour(tile.sr.color);
			}
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

	public List<Container> containers = new List<Container>();

	public class Container {
		public TileObjectInstance parentObject;
		public Inventory inventory;
		public int maxAmount;
		public Container(TileObjectInstance parentObject, int maxAmount) {
			this.parentObject = parentObject;
			this.maxAmount = maxAmount;
			inventory = new Inventory(null, this, maxAmount);
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
					if (resourceAmount.resource.GetWorldTotalAmount() < resourceAmount.amount) {
						hasEnoughRequiredResources = false;
					}
				}
			}
			if (createResource != null) {
				if (createResource.requiredEnergy != 0) {
					hasEnoughFuel = fuelResource != null;
					if (fuelResource != null && createResource != null) {
						fuelResourcesRequired = Mathf.CeilToInt((createResource.requiredEnergy) / ((float)fuelResource.fuelEnergy));
						if (fuelResource.GetWorldTotalAmount() < fuelResourcesRequired) {
							hasEnoughFuel = false;
						}
					}
					canActivate = hasEnoughRequiredResources && hasEnoughFuel;
				} else {
					canActivate = hasEnoughRequiredResources;
				}
			}
			if (active) {
				if (canActivate && createResource.desiredAmount > createResource.GetWorldTotalAmount() && jobBacklog.Count < 1) {
					resourceM.CreateResource(createResource, 1, parentObject);
				}
			}
			parentObject.active = active;
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
			List<TileManager.Tile> newLitTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile tile in tileM.map.tiles) {
				float distance = Vector2.Distance(tile.obj.transform.position, parentTile.obj.transform.position);
				if (distance <= parentObject.prefab.maxLightDistance) {
					float intensityAtTile = Mathf.Clamp(parentObject.prefab.maxLightDistance * (1f / Mathf.Pow(distance, 2f)), 0f, 1f);
					if (tile != parentTile) {
						bool lightTile = true;
						Vector3 lightVector = parentObject.obj.transform.position;
						while ((parentObject.obj.transform.position - lightVector).magnitude <= distance) {
							TileManager.Tile lightVectorTile = tileM.map.GetTileFromPosition(lightVector);
							if (lightVectorTile != parentTile) {
								if (tileM.map.TileBlocksLight(lightVectorTile)) {
									/*
									if (!lightVectorTile.horizontalSurroundingTiles.Any(t => newLitTiles.Contains(t) && !tileM.map.TileBlocksLight(t))) {
										lightTile = false;
										break;
									}
									*/
									lightTile = false;
									break;
								}
							}
							lightVector += (tile.obj.transform.position - parentObject.obj.transform.position).normalized * 0.1f;
						}
						if (lightTile) {
							tile.AddLightSourceBrightness(this, intensityAtTile);
							newLitTiles.Add(tile);
						}
					} else {
						parentTile.AddLightSourceBrightness(this, intensityAtTile);
					}
				}
			}
			tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
			litTiles.AddRange(newLitTiles);
		}

		public void RemoveTileBrightnesses() {
			foreach (TileManager.Tile tile in litTiles) {
				tile.RemoveLightSourceBrightness(this);
			}
			litTiles.Clear();
			parentTile.RemoveLightSourceBrightness(this);
			tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
		}
	}

	public enum PlantGroupsEnum { Cactus, ColourfulBush, ColourfulTree, DeadTree, Bush, SnowTree, ThinTree, WideTree };

	public static readonly List<PlantGroupsEnum> livingTreesAndBushes = new List<PlantGroupsEnum>() {
		PlantGroupsEnum.ColourfulBush, PlantGroupsEnum.ColourfulTree, PlantGroupsEnum.Bush, PlantGroupsEnum.SnowTree, PlantGroupsEnum.ThinTree, PlantGroupsEnum.WideTree
	};

	public static readonly Dictionary<ResourcesEnum, ResourcesEnum> seedToHarvestResource = new Dictionary<ResourcesEnum, ResourcesEnum>() {
		{ ResourcesEnum.AppleSeed,ResourcesEnum.Apple },
		{ ResourcesEnum.Blueberry,ResourcesEnum.Blueberry }
	};

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
							name = UIManager.RemoveNonAlphanumericChars(value);
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

			obj = Instantiate(resourceM.tilePrefab, tile.obj.transform.position, Quaternion.identity);

			SpriteRenderer pSR = obj.GetComponent<SpriteRenderer>();

			small = (randomSmall ? UnityEngine.Random.Range(0f, 1f) < 0.1f : smallValue);

			harvestResource = null;
			if (giveHarvestResource) {
				if (group.type == PlantGroupsEnum.WideTree && !small) {
					if (UnityEngine.Random.Range(0f, 1f) <= 0.05f) {
						harvestResource = resourceM.GetResourceByEnum(ResourcesEnum.Apple);
					}
				} else if (group.type == PlantGroupsEnum.Bush && !small) {
					if (UnityEngine.Random.Range(0f, 1f) <= 0.05f) {
						harvestResource = resourceM.GetResourceByEnum(ResourcesEnum.Blueberry);
					}
				}
			}
			if (specificHarvestResource != null) {
				harvestResource = specificHarvestResource;
			}

			pSR.sprite = (small ? group.smallPlants[UnityEngine.Random.Range(0, group.smallPlants.Count)] : group.fullPlants[UnityEngine.Random.Range(0, group.fullPlants.Count)]);
			if (harvestResource != null) {
				name = harvestResource.name + " " + name;
				if (group.harvestResourceSprites.ContainsKey(harvestResource.type)) {
					if (group.harvestResourceSprites[harvestResource.type].ContainsKey(small)) {
						pSR.sprite = group.harvestResourceSprites[harvestResource.type][small][UnityEngine.Random.Range(0, group.harvestResourceSprites[harvestResource.type][small].Count)];
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
				int amount = Mathf.Clamp(resourceAmount.amount + UnityEngine.Random.Range(-2, 2), 1, int.MaxValue);
				if (small && amount > 0) {
					amount = Mathf.CeilToInt(amount / 2f);
				}
				resourcesToReturn.Add(new ResourceAmount(resourceAmount.resource, amount));
			}
			if (harvestResource != null) {
				int randomRangeAmount = 1;
				if (harvestResource.type == ResourcesEnum.Apple) {
					randomRangeAmount = UnityEngine.Random.Range(1, 6);
				} else if (harvestResource.type == ResourcesEnum.Blueberry) {
					randomRangeAmount = UnityEngine.Random.Range(5, 20);
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
			obj.GetComponent<SpriteRenderer>().sprite = group.fullPlants[UnityEngine.Random.Range(0, group.fullPlants.Count)];
			if (harvestResource != null) {
				if (group.harvestResourceSprites.ContainsKey(harvestResource.type)) {
					if (group.harvestResourceSprites[harvestResource.type].ContainsKey(small)) {
						obj.GetComponent<SpriteRenderer>().sprite = group.harvestResourceSprites[harvestResource.type][small][UnityEngine.Random.Range(0, group.harvestResourceSprites[harvestResource.type][small].Count)];
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
				return GetPlantGroupByEnum(biomePlantGroupsEnums[UnityEngine.Random.Range(0, biomePlantGroupsEnums.Count)]);
			} else {
				return null;
			}
		} else {
			foreach (KeyValuePair<PlantGroupsEnum, float> kvp in biome.vegetationChances) {
				PlantGroupsEnum plantGroup = kvp.Key;
				if (UnityEngine.Random.Range(0f, 1f) < biome.vegetationChances[plantGroup]) {
					return GetPlantGroupByEnum(plantGroup);
				}
			}
		}
		return null;
	}

	public static readonly Dictionary<ResourcesEnum, int> farmGrowTime = new Dictionary<ResourcesEnum, int>() {
		{ ResourcesEnum.WheatSeed, 5760 },
		{ ResourcesEnum.Potato, 2880 },
		{ ResourcesEnum.CottonSeed, 5760 }
	};
	public static readonly Dictionary<ResourcesEnum,ResourcesEnum> farmSeedReturnResource = new Dictionary<ResourcesEnum,ResourcesEnum>() {
		{ ResourcesEnum.WheatSeed, ResourcesEnum.Wheat },
		{ ResourcesEnum.Potato, ResourcesEnum.Potato },
		{ ResourcesEnum.CottonSeed, ResourcesEnum.Cotton }
	};
	public static readonly Dictionary<ResourcesEnum, TileObjectPrefabsEnum> farmSeedsTileObject = new Dictionary<ResourcesEnum, TileObjectPrefabsEnum>() {
		{ ResourcesEnum.WheatSeed, TileObjectPrefabsEnum.WheatFarm },
		{ ResourcesEnum.Potato, TileObjectPrefabsEnum.PotatoFarm },
		{ ResourcesEnum.CottonSeed, TileObjectPrefabsEnum.CottonFarm }
	};

	public List<Farm> farms = new List<Farm>();

	public class Farm : TileObjectInstance {

		private TileManager tileM;
		private TimeManager timeM;
		private JobManager jobM;
		private ResourceManager resourceM;

		void GetScriptReferencecs() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			timeM = GM.GetComponent<TimeManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
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

		public Farm(TileObjectPrefab prefab, TileManager.Tile tile) : base(prefab, tile, 0) {

			GetScriptReferencecs();

			seedType = prefab.resourcesToBuild[0].resource.type;
			name = (UIManager.SplitByCapitals(seedType.ToString()).Split(' ')[0]).Replace(" ", "") + " Farm";
			maxGrowthTime = farmGrowTime[seedType] * UnityEngine.Random.Range(0.9f, 1.1f);

			growProgressSprites = prefab.bitmaskSprites;
			maxSpriteIndex = growProgressSprites.Count - 1;

			precipitationGrowthMultiplier = Mathf.Min((-2 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 2) + 1), (-30 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 3) + 1));
			temperatureGrowthMultipler = Mathf.Clamp(Mathf.Min(((tile.temperature - 10) / 15f + 1), (-((tile.temperature - 50) / 20f))), 0, 1);

			Update();
		}

		public void Update() {
			if (growTimer >= maxGrowthTime) {
				if (!jobM.JobOfTypeExistsAtTile(JobManager.JobTypesEnum.HarvestFarm, tile)) {
					jobM.CreateJob(new JobManager.Job(tile, resourceM.GetTileObjectPrefabByEnum(TileObjectPrefabsEnum.HarvestFarm), 0));
				}
			} else {
				growTimer += CalculateGrowthRate();
				int newGrowProgressSpriteIndex = Mathf.FloorToInt((growTimer / (maxGrowthTime + 10)) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[Mathf.Clamp(growProgressSpriteIndex, 0, maxSpriteIndex)];
				}
			}
		}

		public float CalculateGrowthRate() {
			float growthRate = timeM.deltaTime;
			growthRate *= Mathf.Max(tileM.map.CalculateBrightnessLevelAtHour(timeM.tileBrightnessTime), tile.lightSourceBrightness);
			growthRate *= precipitationGrowthMultiplier;
			growthRate *= temperatureGrowthMultipler;
			growthRate = Mathf.Clamp(growthRate, 0, 1);
			return growthRate;
		}
	}

	private int BitSumTileObjects(List<TileObjectPrefabsEnum> compareTileObjectTypes,List<TileManager.Tile> tileSurroundingTiles) {
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
			List<int> layerSumTiles = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0 };
			for (int i = 0;i < tileSurroundingTiles.Count;i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && TileManager.Map.diagonalCheckMap.ContainsKey(i)) {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]],tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
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
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]],tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareTileObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer,layerSumTiles);
		}

		List<bool> sumTiles = new List<bool>() { false, false, false, false, false, false, false, false };

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

	private void BitmaskTileObjects(TileObjectInstance objectInstance,bool includeDiagonalSurroundingTiles,bool customBitSumInputs,bool compareEquivalentTileObjects, List<TileObjectPrefabsEnum> customCompareTileObjectTypes) {
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
			oISR.sprite = objectInstance.prefab.bitmaskSprites[TileManager.Map.bitmaskMap[sum]];
		} else {
			oISR.sprite = objectInstance.prefab.bitmaskSprites[sum];
		}
	}

	public void Bitmask(List<TileManager.Tile> tilesToBitmask) {
		foreach (TileManager.Tile tile in tilesToBitmask) {
			if (tile != null && tile.GetAllObjectInstances().Count > 0) {
				foreach (TileObjectInstance tileObjectInstance in tile.GetAllObjectInstances()) {
					if (bitmaskingTileObjects.Contains(tileObjectInstance.prefab.type)) {
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