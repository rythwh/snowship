using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : BaseManager {

	public GameObject tilePrefab;
	public GameObject humanPrefab;
	public Sprite selectionCornersSprite;
	public Sprite whiteSquareSprite;
	public Sprite clearSquareSprite;
	public GameObject planetTilePrefab;
	public GameObject colonyObj;
	public GameObject tileImage;
	public GameObject objectDataPanel;

	public void SetResourceReferences() {
		tilePrefab = Resources.Load<GameObject>(@"Prefabs/Tile");
		humanPrefab = Resources.Load<GameObject>(@"Prefabs/Human");
		selectionCornersSprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		whiteSquareSprite = Resources.Load<Sprite>(@"UI/white-square");
		clearSquareSprite = Resources.Load<Sprite>(@"UI/clear-square");
		planetTilePrefab = Resources.Load<GameObject>(@"UI/UIElements/PlanetTile");
		colonyObj = Resources.Load<GameObject>(@"UI/UIElements/ColonyObj");
		tileImage = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage");
		objectDataPanel = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel");
	}

	public override void Update() {
		CalculateResourceTotals();

		GrowPlants();

		foreach (Farm farm in farms) {
			farm.Update();
		}
		foreach (ManufacturingObject manufacturingObject in manufacturingObjectInstances) {
			manufacturingObject.Update();
		}
	}

	public enum ResourceGroupPropertyEnum {
		ResourceGroup,
		Type,
		Resources
	}

	public enum ResourcePropertyEnum {
		Resource,
		Type,
		Classes,
		Weight,
		Volume,
		Price,
		Food,
		Fuel,
		Manufacturing,
		Clothing
	}

	public enum ResourceFoodPropertyEnum {
		Nutrition
	}

	public enum ResourceFuelPropertyEnum {
		FuelEnergy
	}

	public enum ResourceManufacturingPropertyEnum {
		ObjectSubGroups,
		Objects,
		ManufacturingEnergy,
		Resources
	}

	public enum ResourceClothingPropertyEnum {
		Appearance,
		ClothingType,
		Insulation,
		WaterResistance,
		WeightCapacity,
		VolumeCapacity,
		Colours
	}

	public enum ResourceGroupEnum {
		None,
		NaturalResources,
		Ores,
		Metals,
		Materials,
		Seeds,
		RawFoods,
		Foods,
		Coins,
		Clothing
	}

	public enum ResourceEnum {
		None,
		Dirt, Mud, Andesite, Basalt, Diorite, Granite, Kimberlite, Obsidian, Rhyolite, Chalk, Claystone, Coal, Flint, Lignite, Limestone, Sandstone, Anthracite, Marble, Quartz, Slate,
			Clay, Log, Snow, Sand, Cactus, Leaf, Sap,
		GoldOre, SilverOre, BronzeOre, IronOre, CopperOre,
		Gold, Silver, Bronze, Iron, Copper,
		Wood, Firewood, Charcoal, Brick, Glass, Cotton, Cloth,
		WheatSeed, CottonSeed, TreeSeed, AppleSeed, BushSeed, CactusSeed,
		Wheat,
		Potato, BakedPotato, Blueberry, Apple, BakedApple,
		GoldCoin, SilverCoin, BronzeCoin,
		CyanSweater, GreenSweater, BlueSweater, PurpleSweater, NavySweater, YellowSweater, OrangeSweater, RedSweater, WhiteSweater, GraySweater,
		CyanShirt, GreenShirt, BlueShirt, PurpleShirt, NavyShirt, YellowShirt, OrangeShirt, RedShirt, WhiteShirt, GrayShirt,
		BrownSmallBackpack
	}

	public enum ResourceClassEnum {
		Food,
		Manufacturable,
		Fuel,
		Clothing,
		Bag
	}

	public enum ClothingEnum {
		Sweater, Shirt, SmallBackpack
	}

	public void CreateResources() {

		Dictionary<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>> manufacturingResourcesTemp = new Dictionary<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>>();

		List<KeyValuePair<string, object>> resourceGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/resources").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> resourceGroupProperty in resourceGroupProperties) {
			switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupProperty.Key)) {
				case ResourceGroupPropertyEnum.ResourceGroup:

					ResourceGroupEnum? groupType = null;
					List<Resource> resources = new List<Resource>();

					foreach (KeyValuePair<string, object> resourceGroupSubProperty in (List<KeyValuePair<string, object>>)resourceGroupProperty.Value) {
						switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupSubProperty.Key)) {
							case ResourceGroupPropertyEnum.Type:
								groupType = (ResourceGroupEnum)Enum.Parse(typeof(ResourceGroupEnum), (string)resourceGroupSubProperty.Value);
								break;
							case ResourceGroupPropertyEnum.Resources:
								foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)resourceGroupSubProperty.Value) {
									switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceProperty.Key)) {
										case ResourcePropertyEnum.Resource:

											ResourceEnum? type = null;
											List<ResourceClassEnum> classes = new List<ResourceClassEnum>();
											int? weight = null;
											int? volume = null;
											int? price = null;

											// Food
											int? foodNutrition = null;

											// Fuel
											int? fuelEnergy = 0;

											// Manufacturing
											List<ObjectSubGroupEnum> manufacturingObjectSubGroups = new List<ObjectSubGroupEnum>();
											List<ObjectEnum> manufacturingObjects = new List<ObjectEnum>();
											int? manufacturingEnergy = 0;
											// manufacturingResources -> manufacturingResourcesTemp

											// Clothing
											HumanManager.Human.Appearance? clothingAppearance = null;
											ClothingEnum? clothingType = null;
											int? clothingInsulation = null;
											int? clothingWaterResistance = null;
											int? clothingWeightCapacity = 0;
											int? clothingVolumeCapacity = 0;
											List<string> clothingColours = new List<string>();

											foreach (KeyValuePair<string, object> resourceSubProperty in (List<KeyValuePair<string, object>>)resourceProperty.Value) {
												switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceSubProperty.Key)) {
													case ResourcePropertyEnum.Type:
														type = (ResourceEnum)Enum.Parse(typeof(ResourceEnum), (string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Classes:
														foreach (string classString in ((string)resourceSubProperty.Value).Split(',')) {
															classes.Add((ResourceClassEnum)Enum.Parse(typeof(ResourceClassEnum), classString));
														}
														break;
													case ResourcePropertyEnum.Weight:
														weight = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Volume:
														volume = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Price:
														price = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Food:
														foreach (KeyValuePair<string, object> foodProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceFoodPropertyEnum)Enum.Parse(typeof(ResourceFoodPropertyEnum), foodProperty.Key)) {
																case ResourceFoodPropertyEnum.Nutrition:
																	foodNutrition = int.Parse((string)foodProperty.Value);
																	break;
																default:
																	Debug.LogError("Unknown resource food property: " + foodProperty.Key + " " + foodProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Fuel:
														foreach (KeyValuePair<string, object> fuelProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceFuelPropertyEnum)Enum.Parse(typeof(ResourceFuelPropertyEnum), fuelProperty.Key)) {
																case ResourceFuelPropertyEnum.FuelEnergy:
																	fuelEnergy = int.Parse((string)fuelProperty.Value);
																	break;
																default:
																	Debug.LogError("Unknown resource fuel property: " + fuelProperty.Key + " " + fuelProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Manufacturing:
														foreach (KeyValuePair<string, object> manufacturingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceManufacturingPropertyEnum)Enum.Parse(typeof(ResourceManufacturingPropertyEnum), manufacturingProperty.Key)) {
																case ResourceManufacturingPropertyEnum.ObjectSubGroups:
																	foreach (string objectSubGroupString in ((string)manufacturingProperty.Value).Split(',')) {
																		manufacturingObjectSubGroups.Add((ObjectSubGroupEnum)Enum.Parse(typeof(ObjectSubGroupEnum), objectSubGroupString));
																	}
																	break;
																case ResourceManufacturingPropertyEnum.Objects:
																	foreach (string objectString in ((string)manufacturingProperty.Value).Split(',')) {
																		manufacturingObjects.Add((ObjectEnum)Enum.Parse(typeof(ObjectEnum), objectString));
																	}
																	break;
																case ResourceManufacturingPropertyEnum.ManufacturingEnergy:
																	manufacturingEnergy = int.Parse((string)manufacturingProperty.Value);
																	break;
																case ResourceManufacturingPropertyEnum.Resources:
																	manufacturingResourcesTemp.Add(type.Value, new List<KeyValuePair<ResourceEnum, float>>());
																	foreach (string resourceAmountString in ((string)manufacturingProperty.Value).Split(',')) {
																		float amount = float.Parse(resourceAmountString.Split(':')[1]);
																		manufacturingResourcesTemp[type.Value].Add(new KeyValuePair<ResourceEnum, float>(
																			(ResourceEnum)Enum.Parse(typeof(ResourceEnum), resourceAmountString.Split(':')[0]),
																			amount
																		));
																	}
																	break;
																default:
																	Debug.LogError("Unknown resource manufacturing property: " + manufacturingProperty.Key + " " + manufacturingProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Clothing:
														foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceClothingPropertyEnum)Enum.Parse(typeof(ResourceClothingPropertyEnum), clothingProperty.Key)) {
																case ResourceClothingPropertyEnum.Appearance:
																	clothingAppearance = (HumanManager.Human.Appearance)Enum.Parse(typeof(HumanManager.Human.Appearance), (string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.ClothingType:
																	clothingType = (ClothingEnum)Enum.Parse(typeof(ClothingEnum), (string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.Insulation:
																	clothingInsulation = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.WaterResistance:
																	clothingWaterResistance = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.WeightCapacity:
																	clothingWeightCapacity = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.VolumeCapacity:
																	clothingVolumeCapacity = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.Colours:
																	foreach (string colourString in ((string)clothingProperty.Value).Split(',')) {
																		clothingColours.Add(UIManager.RemoveNonAlphanumericChars(colourString));
																	}
																	break;
																default:
																	Debug.LogError("Unknown resource clothing property: " + clothingProperty.Key + " " + clothingProperty.Value);
																	break;
															}
														}
														break;
													default:
														Debug.LogError("Unknown resource sub property: " + resourceSubProperty.Key + " " + resourceSubProperty.Value);
														break;
												}
											}

											if (classes.Contains(ResourceClassEnum.Food)) {
												Food food = new Food(
													type.Value,
													groupType.Value,
													classes,
													weight.Value,
													volume.Value,
													price.Value,
													fuelEnergy.Value,
													manufacturingObjectSubGroups,
													manufacturingObjects,
													manufacturingEnergy.Value,
													foodNutrition.Value
												);
												resources.Add(food);
												this.resources.Add(type.Value, food);
											} else if (classes.Contains(ResourceClassEnum.Clothing)) {
												ClothingPrefab clothingPrefab = new ClothingPrefab(
													clothingAppearance.Value,
													clothingType.Value,
													clothingInsulation.Value,
													clothingWaterResistance.Value,
													clothingWeightCapacity.Value,
													clothingVolumeCapacity.Value,
													clothingColours,
													clothingPrefabs.Where(c => c.appearance == clothingAppearance.Value).Sum(c => c.colours.Count)
												);
												clothingPrefabs.Add(clothingPrefab);
												foreach (string colour in clothingPrefab.colours) {
													Clothing clothing = new Clothing(
														(ResourceEnum)Enum.Parse(typeof(ResourceEnum), colour + clothingType.Value),
														groupType.Value,
														classes,
														weight.Value,
														volume.Value,
														price.Value,
														fuelEnergy.Value,
														manufacturingObjectSubGroups,
														manufacturingObjects,
														manufacturingEnergy.Value,
														clothingPrefab,
														colour
													);
													resources.Add(clothing);
													this.resources.Add(clothing.type, clothing);
												}
											} else {
												Resource resource = new Resource(
													type.Value,
													groupType.Value,
													classes,
													weight.Value,
													volume.Value,
													price.Value,
													fuelEnergy.Value,
													manufacturingObjectSubGroups,
													manufacturingObjects,
													manufacturingEnergy.Value
												);
												resources.Add(resource);
												this.resources.Add(type.Value, resource);
											}

											break;
										default:
											Debug.LogError("Unknown resource property: " + resourceProperty.Key + " " + resourceProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown resource group sub property: " + resourceGroupSubProperty.Key + " " + resourceGroupSubProperty.Value);
								break;
						}

					}

					ResourceGroup resourceGroup = new ResourceGroup(
						groupType.Value,
						resources
					);
					resourceGroups.Add(groupType.Value, resourceGroup);

					break;
				default:
					Debug.LogError("Unknown resource group property: " + resourceGroupProperty.Key + " " + resourceGroupProperty.Value);
					break;

			}
		}

		// Set Resource Classes
		foreach (ResourceClassEnum resourceClassEnum in Enum.GetValues(typeof(ResourceClassEnum))) {
			resourceClassToResources.Add(resourceClassEnum, new List<Resource>());
		}

		foreach (Resource resource in GetResources()) {
			foreach (ResourceClassEnum resourceClassEnum in resource.classes) {
				resourceClassToResources[resourceClassEnum].Add(resource);
			}
		}

		// Set Manufacturing Resources
		foreach (KeyValuePair<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>> manufacturingResourceToResourceAmount in manufacturingResourcesTemp) {
			List<Resource> resourcesToApplyTo = new List<Resource>();
			Resource manufacturableResource = GetResourceByEnum(manufacturingResourceToResourceAmount.Key);
			if (manufacturableResource.classes.Contains(ResourceClassEnum.Clothing)) {
				Clothing manufacturableClothing = (Clothing)manufacturableResource;
				foreach (Resource resource in GetResourcesInClass(ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.clothingType == manufacturableClothing.prefab.clothingType)) {
					resourcesToApplyTo.Add(resource);
				}
			} else {
				resourcesToApplyTo.Add(manufacturableResource);
			}
			foreach (Resource resource in resourcesToApplyTo) {
				foreach (KeyValuePair<ResourceEnum, float> resourceToAmount in manufacturingResourceToResourceAmount.Value) {
					float amount = resourceToAmount.Value;
					if (amount < 1 && amount > 0) {
						resource.amountCreated = Mathf.RoundToInt(1 / amount);
						resource.manufacturingResources.Add(new ResourceAmount(GetResourceByEnum(resourceToAmount.Key), 1));
					} else {
						resource.amountCreated = 1;
						resource.manufacturingResources.Add(new ResourceAmount(GetResourceByEnum(resourceToAmount.Key), Mathf.RoundToInt(resourceToAmount.Value)));
					}
				}
			}
		}
	}

	private static readonly Dictionary<ResourceClassEnum, List<Resource>> resourceClassToResources = new Dictionary<ResourceClassEnum, List<Resource>>();

	public static List<Resource> GetResourcesInClass(ResourceClassEnum resourceClass) {
		return resourceClassToResources[resourceClass];
	}

	public static bool IsResourceInClass(ResourceClassEnum resourceClass, Resource resource) {
		return resource.classes.Contains(resourceClass);
	}

	public Dictionary<ResourceGroupEnum, ResourceGroup> resourceGroups = new Dictionary<ResourceGroupEnum, ResourceGroup>();

	public class ResourceGroup {
		public readonly ResourceGroupEnum type;
		public readonly string name;

		public readonly List<Resource> resources;

		public ResourceGroup(
			ResourceGroupEnum type,
			List<Resource> resources
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.resources = resources;
		}
	}

	public ResourceGroup GetResourceGroupByEnum(ResourceGroupEnum resourceGroupEnum) {
		return resourceGroups[resourceGroupEnum];
	}

	public List<ResourceGroup> GetResourceGroups() {
		return resourceGroups.Values.ToList();
	}

	public ResourceGroup GetRandomResourceGroup() {
		List<ResourceGroup> resourcesGroupsWithoutNone = GetResourceGroups().Where(rg => rg.type != ResourceGroupEnum.None).ToList();
		return resourcesGroupsWithoutNone[UnityEngine.Random.Range(0, resourcesGroupsWithoutNone.Count)];
	}

	public Dictionary<ResourceEnum, Resource> resources = new Dictionary<ResourceEnum, Resource>();

	public class Resource {
		public readonly ResourceEnum type;
		public string name; // Can't be readonly

		public readonly ResourceGroupEnum groupType;

		public readonly List<ResourceClassEnum> classes;

		public readonly int weight;
		public readonly int volume;

		public readonly int price;

		public Sprite image; // Can't be readonly

		// Fuel
		public readonly int fuelEnergy;

		// Manufacturing
		public readonly List<ObjectSubGroupEnum> manufacturingObjectSubGroups;
		public readonly List<ObjectEnum> manufacturingObjects;
		public readonly int manufacturingEnergy;
		public readonly List<ResourceAmount> manufacturingResources = new List<ResourceAmount>(); // Filled in CreateResources() after all resources created
		public int amountCreated = 1; // Can't be readonly

		// Desired Amount
		private int desiredAmount = 0;

		// World Amounts
		private int worldTotalAmount;
		private int colonistsTotalAmount;
		private int containerTotalAmount;
		private int unreservedContainerTotalAmount;
		private int unreservedTradingPostTotalAmount;
		private int availableAmount;

		// UI
		public UIManager.ResourceElement resourceListElement; // REMOVE THIS (replace with a UIManager Dictionary<ResourceEnum, ResourceElement> and access like that

		public Resource(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			List<ObjectSubGroupEnum> manufacturingObjectSubGroups,
			List<ObjectEnum> manufacturingObjects,
			int manufacturingEnergy
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.classes = classes;

			this.weight = weight;
			this.volume = volume;

			this.price = price;

			image = Resources.Load<Sprite>(@"Sprites/Resources/" + name + "/" + name.Replace(' ', '-') + "-base");

			// Fuel
			this.fuelEnergy = fuelEnergy;

			// Manufacturing
			this.manufacturingObjectSubGroups = manufacturingObjectSubGroups;
			this.manufacturingObjects = manufacturingObjects;
			this.manufacturingEnergy = manufacturingEnergy;
		}

		public int GetDesiredAmount() {
			return desiredAmount;
		}
	
		public void SetDesiredAmount(int desiredAmount) {
			this.desiredAmount = desiredAmount;
			UpdateDesiredAmountText();
		}

		public void UpdateDesiredAmountText() {
			if (GameManager.uiM.selectedMTO != null && GameManager.uiM.selectedMTO.createResource == this) {
				GameManager.uiM.selectedMTOPanel.obj.transform.Find("ResourceTargetAmount-Panel/TargetAmount-Input").GetComponent<InputField>().text = desiredAmount.ToString();
			}
			resourceListElement.desiredAmountInput.text = desiredAmount.ToString();
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

		public int GetUnreservedTradingPostTotalAmount() {
			return unreservedTradingPostTotalAmount;
		}

		public void SetUnreservedTradingPostTotalAmount(int unreservedTradingPostTotalAmount) {
			this.unreservedTradingPostTotalAmount = unreservedTradingPostTotalAmount;
		}

		public void AddToUnreservedTradingPostTotalAmount(int amount) {
			SetUnreservedTradingPostTotalAmount(GetUnreservedTradingPostTotalAmount() + amount);
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

	public Resource GetResourceByString(string resourceString) {
		return GetResourceByEnum((ResourceEnum)Enum.Parse(typeof(ResourceEnum), resourceString));
	}

	public Resource GetResourceByEnum(ResourceEnum resourceEnum) {
		return resources[resourceEnum];
	}

	public List<Resource> GetResources() {
		return resources.Values.ToList();
	}

	public class Food : Resource {

		public int nutrition = 0;

		public Food(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			List<ObjectSubGroupEnum> manufacturingObjectSubGroups,
			List<ObjectEnum> manufacturingObjects,
			int manufacturingEnergy,
			int nutrition
		) : base(
			type,
			groupType,
			classes,
			weight,
			volume,
			price,
			fuelEnergy,
			manufacturingObjectSubGroups,
			manufacturingObjects,
			manufacturingEnergy
		) {
			this.nutrition = nutrition;
		}
	}

	public List<ClothingPrefab> clothingPrefabs = new List<ClothingPrefab>();

	public class ClothingPrefab {
		public HumanManager.Human.Appearance appearance;
		public ClothingEnum clothingType;
		public int insulation;
		public int waterResistance;
		public int weightCapacity;
		public int volumeCapacity;

		public List<string> colours;

		public List<List<Sprite>> moveSprites = new List<List<Sprite>>();

		public List<Clothing> clothes = new List<Clothing>();

		public ClothingPrefab(
			HumanManager.Human.Appearance appearance, 
			ClothingEnum clothingType, 
			int insulation, 
			int waterResistance, 
			int weightCapacity, 
			int volumeCapacity, 
			List<string> colours, 
			int typeIndex
		) {
			this.appearance = appearance;
			this.clothingType = clothingType;
			this.insulation = insulation;
			this.waterResistance = waterResistance;
			this.weightCapacity = weightCapacity;
			this.volumeCapacity = volumeCapacity;
			this.colours = colours;

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(Resources.LoadAll<Sprite>(@"Sprites/Clothes/" + appearance + "/clothes-" + appearance.ToString().ToLower() + "-" + i).Skip(typeIndex).Take(colours.Count).ToList());
			}
		}
	}

	public List<ClothingPrefab> GetClothingPrefabsByAppearance(HumanManager.Human.Appearance appearance) {
		return clothingPrefabs.FindAll(c => c.appearance == appearance);
	}

	public class Clothing : Resource {
		public ClothingPrefab prefab;
		public string colour;

		public List<Sprite> moveSprites = new List<Sprite>();

		public Clothing(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			List<ObjectSubGroupEnum> manufacturingObjectSubGroups,
			List<ObjectEnum> manufacturingObjects,
			int manufacturingEnergy,
			ClothingPrefab prefab,
			string colour
		) : base(
			type,
			groupType,
			classes,
			weight,
			volume,
			price,
			fuelEnergy,
			manufacturingObjectSubGroups,
			manufacturingObjects,
			manufacturingEnergy
		) {
			this.prefab = prefab;
			this.colour = colour;

			name = UIManager.SplitByCapitals(colour + prefab.clothingType);

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(prefab.moveSprites[i][prefab.colours.IndexOf(colour)]);
			}

			image = moveSprites[0];
		}
	}

	public List<Clothing> GetClothesByAppearance(HumanManager.Human.Appearance appearance) {
		return GetResourcesInClass(ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.appearance == appearance).ToList();
	}

	public class ResourceAmount {
		public Resource resource;
		public int amount;

		public ResourceAmount(Resource resource, int amount) {
			this.resource = resource;
			this.amount = amount;
		}
	}

	public class ResourceRange {
		public Resource resource;
		public int min;
		public int max;

		public ResourceRange(Resource resource, int min, int max) {
			this.resource = resource;
			this.min = min;
			this.max = max;
		}
	}

	public class ReservedResources {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public HumanManager.Human human;

		public ReservedResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
			resources.AddRange(resourcesToReserve);
			human = humanReservingResources;
		}
	}

	public class TradeResourceAmount {
		public Resource resource;

		public int caravanAmount;
		public int colonyAmount;

		private int oldTradeAmount = 0;
		private int tradeAmount;

		public int caravanResourcePrice;

		public CaravanManager.Caravan caravan;

		public TradeResourceAmount(Resource resource, int caravanAmount, CaravanManager.Caravan caravan) {
			this.resource = resource;

			this.caravanAmount = caravanAmount;

			caravanResourcePrice = caravan.DeterminePriceForResource(resource);

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

		public void SetColonyAmount(int colonyAmount) {
			this.colonyAmount = colonyAmount;
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

	public class ConfirmedTradeResourceAmount {
		public Resource resource;
		public int tradeAmount;
		public int amountRemaining;

		public ConfirmedTradeResourceAmount(Resource resource, int tradeAmount) {
			this.resource = resource;
			this.tradeAmount = tradeAmount;
			amountRemaining = tradeAmount;
		}
	}

	public void CreateResource(Resource resource, int amount, ManufacturingObject manufacturingTileObject) {
		for (int i = 0; i < amount; i++) {
			JobManager.Job job = new JobManager.Job(manufacturingTileObject.tile, GetObjectPrefabByEnum(ObjectEnum.CreateResource), 0);
			job.SetCreateResourceData(resource, manufacturingTileObject);
			GameManager.jobM.CreateJob(job);
			manufacturingTileObject.jobBacklog.Add(job);
		}
	}

	public class Inventory {

		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public List<ReservedResources> reservedResources = new List<ReservedResources>();

		public HumanManager.Human human;
		public Container container;

		public int maxAmount;

		public Inventory(HumanManager.Human human, Container container, int maxAmount) {
			this.human = human;
			this.container = container;
			this.maxAmount = maxAmount;
		}

		public int CountResources() {
			return (resources.Sum(resource => resource.amount) + reservedResources.Sum(reservedResource => reservedResource.resources.Sum(rr => rr.amount)));
		}

		public int ChangeResourceAmount(Resource resource, int amount, bool limitToMaxAmount) {
			int remainingAmount = 0;
			if (limitToMaxAmount && amount > 0) {
				remainingAmount = maxAmount - (CountResources() + amount);
				if (remainingAmount < 0) {
					remainingAmount = Mathf.Abs(remainingAmount);
					amount -= remainingAmount;
				} else {
					remainingAmount = 0;
				}
			}

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

			GameManager.resourceM.CalculateResourceTotals();
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.SetSelectedTradingPostInfo();
			GameManager.jobM.UpdateColonistJobs();

			return remainingAmount;
		}

		public bool ReserveResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
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
					ChangeResourceAmount(raInventory.resource, -raReserve.amount, false);
				}
				reservedResources.Add(new ReservedResources(resourcesToReserve, humanReservingResources));
			}
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.SetSelectedTradingPostInfo();
			return allResourcesFound;
		}

		public List<ReservedResources> TakeReservedResources(HumanManager.Human humanReservingResources) {
			List<ReservedResources> reservedResourcesByHuman = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.human == humanReservingResources) {
					reservedResourcesByHuman.Add(rr);
				}
			}
			foreach (ReservedResources rr in reservedResourcesByHuman) {
				reservedResources.Remove(rr);
			}
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.SetSelectedTradingPostInfo();
			return reservedResourcesByHuman;
		}

		public void ReleaseReservedResources(HumanManager.Human human) {
			List<ReservedResources> reservedResourcesToRemove = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.human == human) {
					foreach (ResourceAmount ra in rr.resources) {
						ChangeResourceAmount(ra.resource, ra.amount, false);
					}
					reservedResourcesToRemove.Add(rr);
				}
			}
			foreach (ReservedResources rrRemove in reservedResourcesToRemove) {
				reservedResources.Remove(rrRemove);
			}
			reservedResourcesToRemove.Clear();
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.SetSelectedTradingPostInfo();
		}

		public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, ResourceAmount resourceAmount, bool limitToMaxAmount) {
			Resource resource = resourceAmount.resource;
			int amount = resourceAmount.amount;
			
			fromInventory.ChangeResourceAmount(resource, -amount, false);
			int remainingAmount = toInventory.ChangeResourceAmount(resource, amount, limitToMaxAmount);
			if (remainingAmount > 0) {
				fromInventory.ChangeResourceAmount(resource, remainingAmount, false);
			}
		}

		public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, List<ResourceAmount> resourceAmounts, bool limitToMaxAmount) {
			foreach (ResourceAmount resourceAmount in resourceAmounts.ToList()) {
				TransferResourcesBetweenInventories(fromInventory, toInventory, resourceAmount, limitToMaxAmount);
			}
		}
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in GetResources()) {
			resource.SetWorldTotalAmount(0);
			resource.SetColonistsTotalAmount(0);
			resource.SetContainerTotalAmount(0);
			resource.SetUnreservedContainerTotalAmount(0);
			resource.SetUnreservedTradingPostTotalAmount(0);
			resource.SetAvailableAmount(0);
		}

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
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
		foreach (TradingPost tradingPost in tradingPosts) {
			foreach (ResourceAmount resourceAmount in tradingPost.inventory.resources) {
				resourceAmount.resource.AddToUnreservedTradingPostTotalAmount(resourceAmount.amount);
			}
		}

		foreach (Resource resource in GetResources()) {
			resource.CalculateAvailableAmount();
		}
	}

	public List<ResourceAmount> GetFilteredResources(bool colonistInventory, bool colonistReserved, bool containerInventory, bool containerReserved) {
		List<ResourceAmount> returnResources = new List<ResourceAmount>();
		if (colonistInventory || colonistReserved) {
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
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

	

	

	



	public enum ObjectGroupPropertyEnum {
		Group,
		Type,
		SubGroups
	}

	public enum ObjectGroupEnum {
		Structure, Furniture, Industrial,
		Command,
		Farm,
		None,
	}

	public enum ObjectSubGroupPropertyEnum {
		SubGroup,
		Type,
		Objects
	}

	public enum ObjectSubGroupEnum {
		Walls, Fences, Doors, Floors, Foundations,
		Containers, Beds, Chairs, Tables, Lights, TradingPosts,
		Furnaces, Processing,
		Plants, Terrain, Remove, Cancel, Priority,
		PlantFarm, HarvestFarm,
		None
	}

	public enum ObjectPropertyEnum {
		Object,
		Type,
		InstanceType,
		Layer,
		Bitmasking,
		BlocksLight,
		MultiTilePositions,
		Integrity,
		Walkable,
		WalkSpeed,
		Buildable,
		Flammability,
		MaxInventoryVolume,
		MaxInventoryWeight,
		MaxLightDistance,
		LightColour,
		RestComfortAmount,
		UsesFuel,
		GrowthTimeDays,
		Seed,
		HarvestResource,
		Plants,
		TimeToBuild,
		ResourcesToBuild,
		SelectionModifiers,
		JobType,
		AddToTileWhenBuilt
	}

	public enum ObjectEnum {
		WoodenWall, GraniteWall, BrickWall,
		WoodenFence,
		WoodenDoor,
		WoodenFloor, BrickFloor,
		WoodenDock,
		Basket, WoodenChest, WoodenDrawers,
		WoodenBed,
		WoodenChair,
		WoodenTable,
		Torch, WoodenLamp,
		TradingPost,
		StoneFurnace,
		CottonGin, SplittingBlock, SplittingLog, Anvil, Loom, SowingTable,
		ChopPlant, PlantPlant, PlantAppleTree, PlantBlueberryBush,
		Mine, Dig,
		RemoveFloor, RemoveObject, RemoveAll,
		Cancel,
		IncreasePriority, DecreasePriority,
		WheatFarm, PotatoFarm, CottonFarm,
		HarvestFarm,
		CreateResource, PickupResources, TransferResources, CollectResources, EmptyInventory, Sleep, CollectWater, Drink, CollectFood, Eat,
		PlantTree, PlantBush, PlantCactus
	}

	public enum ObjectInstanceType {
		Normal,
		Container,
		TradingPost,
		SleepSpot,
		LightSource,
		ManufacturingObject,
		Farm
	}

	public void CreateObjectPrefabs() {
		List<KeyValuePair<string, object>> objectGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/objects").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> objectGroupProperty in objectGroupProperties) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupProperty.Key)) {
				case ObjectGroupPropertyEnum.Group:

					ObjectGroupEnum? groupType = null;
					List<ObjectPrefabSubGroup> subGroups = new List<ObjectPrefabSubGroup>();

					foreach (KeyValuePair<string, object> objectGroupSubProperty in (List<KeyValuePair<string, object>>)objectGroupProperty.Value) {
						switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupSubProperty.Key)) {
							case ObjectGroupPropertyEnum.Type:
								groupType = (ObjectGroupEnum)Enum.Parse(typeof(ObjectGroupEnum), (string)objectGroupSubProperty.Value);
								break;
							case ObjectGroupPropertyEnum.SubGroups:
								foreach (KeyValuePair<string, object> objectSubGroupProperty in (List<KeyValuePair<string, object>>)objectGroupSubProperty.Value) {
									switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupProperty.Key)) {
										case ObjectSubGroupPropertyEnum.SubGroup:

											ObjectSubGroupEnum? subGroupType = null;
											List<ObjectPrefab> prefabs = new List<ObjectPrefab>();

											foreach (KeyValuePair<string, object> objectSubGroupSubProperty in (List<KeyValuePair<string, object>>)objectSubGroupProperty.Value) {
												switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupSubProperty.Key)) {
													case ObjectSubGroupPropertyEnum.Type:
														subGroupType = (ObjectSubGroupEnum)Enum.Parse(typeof(ObjectSubGroupEnum), (string)objectSubGroupSubProperty.Value);
														break;
													case ObjectSubGroupPropertyEnum.Objects:
														foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)objectSubGroupSubProperty.Value) {
															switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectProperty.Key)) {
																case ObjectPropertyEnum.Object:

																	ObjectEnum? type = null;
																	ObjectInstanceType instanceType = ObjectInstanceType.Normal;
																	int? layer = null;
																	bool? bitmasking = false;
																	bool? blocksLight = false;
																	int? integrity = 0;
																	List<Vector2> multiTilePositions = new List<Vector2>();

																	float? walkSpeed = 1;
																	bool? walkable = true;
																	bool? buildable = true;
																	float? flammability = 0;

																	// Container
																	int? maxInventoryVolume = null;
																	int? maxInventoryWeight = null;

																	// Light
																	int? maxLightDistance = null;
																	Color? lightColour = null;

																	// Sleep Spot
																	float? restComfortAmount = null;

																	// Manufacturing Object
																	bool? usesFuel = null;

																	// Farm
																	int? growthTimeDays = null;
																	Resource seedResource = null;
																	Resource harvestResource = null;

																	// Plants
																	Dictionary<PlantPrefab, Resource> plants = new Dictionary<PlantPrefab, Resource>();

																	// Job
																	int? timeToBuild = null;
																	List<ResourceAmount> resourcesToBuild = new List<ResourceAmount>();
																	List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
																	JobManager.JobTypesEnum? jobType = null;
																	bool? addToTileWhenBuilt = true;

																	foreach (KeyValuePair<string, object> objectSubProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
																		switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectSubProperty.Key)) {
																			case ObjectPropertyEnum.Type:
																				type = (ObjectEnum)Enum.Parse(typeof(ObjectEnum), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.InstanceType:
																				instanceType = (ObjectInstanceType)Enum.Parse(typeof(ObjectInstanceType), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Layer:
																				layer = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Bitmasking:
																				bitmasking = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.BlocksLight:
																				blocksLight = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Integrity:
																				integrity = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.MultiTilePositions:
																				foreach (string vector2String in ((string)objectSubProperty.Value).Split(';')) {
																					multiTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
																				}
																				break;
																			case ObjectPropertyEnum.WalkSpeed:
																				walkSpeed = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Walkable:
																				walkable = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Buildable:
																				buildable = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Flammability:
																				flammability = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.MaxInventoryVolume:
																				if (((string)objectSubProperty.Value).Contains("Max")) {
																					maxInventoryVolume = int.MaxValue;
																				} else {
																					maxInventoryVolume = int.Parse((string)objectSubProperty.Value);
																				}
																				break;
																			case ObjectPropertyEnum.MaxInventoryWeight:
																				if (((string)objectSubProperty.Value).Contains("Max")) {
																					maxInventoryWeight = int.MaxValue;
																				} else {
																					maxInventoryWeight = int.Parse((string)objectSubProperty.Value);
																				}
																				break;
																			case ObjectPropertyEnum.MaxLightDistance:
																				maxLightDistance = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.LightColour:
																				lightColour = UIManager.HexToColor((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.RestComfortAmount:
																				restComfortAmount = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.UsesFuel:
																				usesFuel = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.GrowthTimeDays:
																				growthTimeDays = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Seed:
																				seedResource = GetResourceByString((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.HarvestResource:
																				harvestResource = GetResourceByString((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Plants:
																				foreach (string plantToHarvestResourceString in ((string)objectSubProperty.Value).Split(',')) {
																					string harvestResourceString = plantToHarvestResourceString.Split(':')[1];
																					plants.Add(
																						GetPlantPrefabByString(plantToHarvestResourceString.Split(':')[0]), 
																						harvestResourceString.Contains("None") ? null : GetResourceByString(harvestResourceString)
																					);
																				}
																				break;
																			case ObjectPropertyEnum.TimeToBuild:
																				timeToBuild = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.ResourcesToBuild:
																				foreach (string resourceAmountString in ((string)objectSubProperty.Value).Split(',')) {
																					resourcesToBuild.Add(new ResourceAmount(GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
																				}
																				break;
																			case ObjectPropertyEnum.SelectionModifiers:
																				foreach (string selectionModifierString in ((string)objectSubProperty.Value).Split(',')) {
																					selectionModifiers.Add((JobManager.SelectionModifiersEnum)Enum.Parse(typeof(JobManager.SelectionModifiersEnum), selectionModifierString));
																				}
																				break;
																			case ObjectPropertyEnum.JobType:
																				jobType = (JobManager.JobTypesEnum)Enum.Parse(typeof(JobManager.JobTypesEnum), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.AddToTileWhenBuilt:
																				addToTileWhenBuilt = bool.Parse((string)objectSubProperty.Value);
																				break;
																			default:
																				Debug.LogError("Unknown object sub property: " + objectSubProperty.Key + " " + objectSubProperty.Value);
																				break;
																		}
																	}

																	if (instanceType != ObjectInstanceType.Container && instanceType != ObjectInstanceType.TradingPost) {
																		if (!maxInventoryVolume.HasValue) {
																			maxInventoryVolume = 0;
																		}
																		if (!maxInventoryWeight.HasValue) {
																			maxInventoryWeight = 0;
																		}
																	}

																	if (instanceType != ObjectInstanceType.LightSource) {
																		if (!maxLightDistance.HasValue) {
																			maxLightDistance = 0;
																		}
																		if (!lightColour.HasValue) {
																			lightColour = Color.white;
																		}
																	}

																	if (instanceType != ObjectInstanceType.SleepSpot) {
																		if (!restComfortAmount.HasValue) {
																			restComfortAmount = 0;
																		}
																	}

																	if (instanceType != ObjectInstanceType.ManufacturingObject) {
																		if (!usesFuel.HasValue) {
																			usesFuel = false;
																		}
																	}

																	if (instanceType != ObjectInstanceType.Farm) {
																		if (!growthTimeDays.HasValue) {
																			growthTimeDays = 0;
																		}
																	}

																	ObjectPrefab objectPrefab = new ObjectPrefab(
																		type.Value,
																		groupType.Value,
																		subGroupType.Value,
																		instanceType,
																		layer.Value,
																		bitmasking.Value,
																		blocksLight.Value,
																		integrity.Value,
																		multiTilePositions,
																		walkSpeed.Value,
																		walkable.Value,
																		buildable.Value,
																		flammability.Value,
																		maxInventoryVolume.Value,
																		maxInventoryWeight.Value,
																		maxLightDistance.Value,
																		lightColour.Value,
																		restComfortAmount.Value,
																		usesFuel.Value,
																		growthTimeDays.Value,
																		seedResource,
																		harvestResource,
																		plants,
																		timeToBuild.Value,
																		resourcesToBuild,
																		selectionModifiers,
																		jobType.Value,
																		addToTileWhenBuilt.Value
																	);
																	prefabs.Add(objectPrefab);
																	objectPrefabs.Add(type.Value, objectPrefab);

																	break;
																default:
																	Debug.LogError("Unknown object property: " + objectProperty.Key + " " + objectProperty.Value);
																	break;
															}
														}
														break;
													default:
														Debug.LogError("Unknown object sub group sub property: " + objectSubGroupSubProperty.Key + " " + objectSubGroupSubProperty.Value);
														break;
												}
											}

											ObjectPrefabSubGroup objectPrefabSubGroup = new ObjectPrefabSubGroup(
												subGroupType.Value,
												groupType.Value,
												prefabs
											);
											subGroups.Add(objectPrefabSubGroup);
											objectPrefabSubGroups.Add(subGroupType.Value, objectPrefabSubGroup);

											break;
										default:
											Debug.LogError("Unknown object sub group property: " + objectSubGroupProperty.Key + " " + objectSubGroupProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown object group sub property: " + objectGroupSubProperty.Key + " " + objectGroupSubProperty.Value);
								break;
						}
					}

					ObjectPrefabGroup objectPrefabGroup = new ObjectPrefabGroup(
						groupType.Value,
						subGroups
					);
					objectPrefabGroups.Add(groupType.Value, objectPrefabGroup);

					break;
				default:
					Debug.LogError("Unknown object group property: " + objectGroupProperty.Key + " " + objectGroupProperty.Value);
					break;
			}
		}
	}

	private Dictionary<ObjectGroupEnum, ObjectPrefabGroup> objectPrefabGroups = new Dictionary<ObjectGroupEnum, ObjectPrefabGroup>();

	public class ObjectPrefabGroup {
		public readonly ObjectGroupEnum type;
		public readonly string name;

		public readonly List<ObjectPrefabSubGroup> subGroups = new List<ObjectPrefabSubGroup>();

		public ObjectPrefabGroup(ObjectGroupEnum type, List<ObjectPrefabSubGroup> subGroups) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.subGroups = subGroups;
		}
	}

	public ObjectPrefabGroup GetObjectPrefabGroupByString(string objectPrefabGroupString) {
		return GetObjectPrefabGroupByEnum((ObjectGroupEnum)Enum.Parse(typeof(ObjectGroupEnum), objectPrefabGroupString));
	}

	public ObjectPrefabGroup GetObjectPrefabGroupByEnum(ObjectGroupEnum objectGroupEnum) {
		return objectPrefabGroups[objectGroupEnum];
	}

	public List<ObjectPrefabGroup> GetObjectPrefabGroups() {
		return objectPrefabGroups.Values.ToList();
	}

	public Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup> objectPrefabSubGroups = new Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup>();

	public class ObjectPrefabSubGroup {
		public readonly ObjectSubGroupEnum type;
		public readonly string name;

		public readonly ObjectGroupEnum groupType;
		public readonly List<ObjectPrefab> prefabs;

		public ObjectPrefabSubGroup(ObjectSubGroupEnum type, ObjectGroupEnum groupType, List<ObjectPrefab> prefabs) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.prefabs = prefabs;
		}
	}

	public ObjectPrefabSubGroup GetTileObjectPrefabSubGroupByEnum(ObjectSubGroupEnum objectSubGroupEnum) {
		return objectPrefabSubGroups[objectSubGroupEnum];
	}

	public Dictionary<ObjectEnum, ObjectPrefab> objectPrefabs = new Dictionary<ObjectEnum, ObjectPrefab>();

	public class ObjectPrefab {
		public readonly ObjectEnum type;
		public readonly string name;

		public readonly ObjectGroupEnum groupType;
		public readonly ObjectSubGroupEnum subGroupType;

		public readonly ObjectInstanceType instanceType;

		public readonly int layer;

		public readonly bool bitmasking;
		public readonly bool canRotate;

		public readonly bool blocksLight;

		public readonly int integrity;

		public readonly Dictionary<int, List<Vector2>> multiTilePositions = new Dictionary<int, List<Vector2>>();
		public readonly Dictionary<int, Vector2> anchorPositionOffset = new Dictionary<int, Vector2>();
		public readonly Dictionary<int, Vector2> dimensions = new Dictionary<int, Vector2>();

		public readonly float walkSpeed;
		public readonly bool walkable;

		public readonly bool buildable;

		public readonly float flammability;

		// Container
		public readonly int maxInventoryAmount;

		// Light Source
		public readonly int maxLightDistance;
		public readonly Color lightColour;

		// Sleep Spot
		public readonly float restComfortAmount;

		// Manufacturing Tile Object
		public readonly bool usesFuel;

		// Farm
		public readonly int growthTimeDays;
		public readonly Resource seedResource;
		public readonly Resource harvestResource;

		// Plants
		public readonly Dictionary<PlantPrefab, Resource> plants;

		// Job
		public readonly int timeToBuild;
		public readonly List<ResourceAmount> resourcesToBuild = new List<ResourceAmount>();
		public readonly List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
		public readonly JobManager.JobTypesEnum jobType;
		public readonly bool addToTileWhenBuilt;

		// Sprites
		public readonly Sprite baseSprite;
		public readonly List<Sprite> bitmaskSprites = new List<Sprite>();
		public readonly List<Sprite> activeSprites = new List<Sprite>();

		public ObjectPrefab(
			ObjectEnum type,
			ObjectGroupEnum groupType,
			ObjectSubGroupEnum subGroupType,
			ObjectInstanceType instanceType,
			int layer,
			bool bitmasking,
			bool blocksLight,
			int integrity,
			List<Vector2> multiTilePositions,
			float walkSpeed,
			bool walkable,
			bool buildable,
			float flammability,
			int maxInventoryVolume,
			int maxInventoryWeight,
			int maxLightDistance,
			Color lightColour,
			float restComfortAmount,
			bool usesFuel,
			int growthTimeDays,
			Resource seedResource,
			Resource harvestResource,
			Dictionary<PlantPrefab, Resource> plants,
			int timeToBuild,
			List<ResourceAmount> resourcesToBuild,
			List<JobManager.SelectionModifiersEnum> selectionModifiers,
			JobManager.JobTypesEnum jobType,
			bool addToTileWhenBuilt
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.subGroupType = subGroupType;

			this.instanceType = instanceType;

			this.layer = layer;

			this.bitmasking = bitmasking;

			this.blocksLight = blocksLight;

			this.integrity = integrity;

			this.walkable = walkable;
			this.walkSpeed = walkSpeed;

			this.buildable = buildable;

			this.flammability = flammability;

			// Container
			this.maxInventoryAmount = maxInventoryVolume;

			// Light
			this.maxLightDistance = maxLightDistance;
			this.lightColour = lightColour;

			// Sleep Spot
			this.restComfortAmount = restComfortAmount;

			// Manufacturing Object
			this.usesFuel = usesFuel;

			// Farm
			this.growthTimeDays = growthTimeDays;
			this.seedResource = seedResource;
			this.harvestResource = harvestResource;

			// Plants
			this.plants = plants;

			// Job
			this.timeToBuild = timeToBuild;
			this.resourcesToBuild = resourcesToBuild;
			this.selectionModifiers = selectionModifiers;
			this.jobType = jobType;
			this.addToTileWhenBuilt = addToTileWhenBuilt;

			// Sprites
			baseSprite = Resources.Load<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-base");
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-bitmask").ToList();
			activeSprites = Resources.LoadAll<Sprite>(@"Sprites/TileObjects/" + name + "/" + name.Replace(' ', '-') + "-active").ToList();

			if (baseSprite == null && bitmaskSprites.Count > 0) {
				baseSprite = bitmaskSprites[0];
			}
			if (jobType == JobManager.JobTypesEnum.PlantFarm) {
				baseSprite = bitmaskSprites[bitmaskSprites.Count - 1];
			}

			// Set Rotation
			canRotate = (!bitmasking && bitmaskSprites.Count > 0);

			// Multi Tile Positions
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
						foreach (Vector2 oldMultiTilePosition in this.multiTilePositions[i - 1]) {
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

	public ObjectPrefab GetObjectPrefabByString(string objectString) {
		return GetObjectPrefabByEnum((ObjectEnum)Enum.Parse(typeof(ObjectEnum), objectString));
	}

	public ObjectPrefab GetObjectPrefabByEnum(ObjectEnum objectEnum) {
		return objectPrefabs[objectEnum];
	}

	public List<ObjectPrefab> GetObjectPrefabs() {
		return objectPrefabs.Values.ToList();
	}

	public Dictionary<ObjectPrefab, List<ObjectInstance>> objectInstances = new Dictionary<ObjectPrefab, List<ObjectInstance>>();

	public List<ObjectInstance> GetObjectInstancesByPrefab(ObjectPrefab prefab) {
		if (objectInstances.ContainsKey(prefab)) {
			return objectInstances[prefab];
		}
		Debug.LogWarning("Tried accessing a tile object instance which isn't already in the list.");
		return null;
	}

	public void AddObjectInstance(ObjectInstance objectInstance) {
		if (objectInstances.ContainsKey(objectInstance.prefab)) {
			objectInstances[objectInstance.prefab].Add(objectInstance);
			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Update, objectInstance.prefab);
		} else {
			objectInstances.Add(objectInstance.prefab, new List<ObjectInstance>() { objectInstance });
			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Add, objectInstance.prefab);
		}
	}

	public void RemoveTileObjectInstance(ObjectInstance instance) {
		switch (instance.prefab.instanceType) {
			case ObjectInstanceType.Normal:
				break;
			case ObjectInstanceType.Container:
				Container container = (Container)instance;

				if (GameManager.uiM.selectedContainer == container) {
					GameManager.uiM.SetSelectedContainer(null);
				}

				containers.Remove(container);
				break;
			case ObjectInstanceType.TradingPost:
				TradingPost tradingPost = (TradingPost)instance;

				if (GameManager.uiM.selectedTradingPost == tradingPost) {
					GameManager.uiM.SetSelectedTradingPost(null);
				}

				tradingPosts.Remove(tradingPost);
				break;
			case ObjectInstanceType.SleepSpot:
				SleepSpot sleepSpot = (SleepSpot)instance;

				sleepSpots.Remove(sleepSpot);
				break;
			case ObjectInstanceType.LightSource:
				LightSource lightSource = (LightSource)instance;

				lightSource.RemoveTileBrightnesses();

				lightSources.Remove(lightSource);
				break;
			case ObjectInstanceType.ManufacturingObject:
				ManufacturingObject manufacturingTileObject = (ManufacturingObject)instance;

				if (GameManager.uiM.selectedMTO == manufacturingTileObject) {
					GameManager.uiM.SetSelectedManufacturingTileObject(null);
				}

				manufacturingObjectInstances.Remove(manufacturingTileObject);
				break;
			case ObjectInstanceType.Farm:
				Farm farm = (Farm)instance;

				farm.tile.farm = null;

				farms.Remove(farm);
				break;
			default:
				Debug.LogWarning("No removal case for removing " + instance.prefab.name);
				break;
		}


		if (objectInstances.ContainsKey(instance.prefab)) {
			objectInstances[instance.prefab].Remove(instance);

			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Update, instance.prefab);
		} else {
			Debug.LogWarning("Tried removing a tile object instance which isn't in the list");
		}

		if (objectInstances[instance.prefab].Count <= 0) {
			objectInstances.Remove(instance.prefab);

			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Remove, instance.prefab);
		}
	}

	public ObjectInstance CreateTileObjectInstance(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex, bool addToList) {
		ObjectInstance instance = null;
		switch (prefab.instanceType) {
			case ObjectInstanceType.Normal:
				instance = new ObjectInstance(prefab, tile, rotationIndex);
				break;
			case ObjectInstanceType.Container:
				instance = new Container(prefab, tile, rotationIndex);
				containers.Add((Container)instance);
				break;
			case ObjectInstanceType.TradingPost:
				instance = new TradingPost(prefab, tile, rotationIndex);
				tradingPosts.Add((TradingPost)instance);
				break;
			case ObjectInstanceType.SleepSpot:
				instance = new SleepSpot(prefab, tile, rotationIndex);
				sleepSpots.Add((SleepSpot)instance);
				break;
			case ObjectInstanceType.LightSource:
				instance = new LightSource(prefab, tile, rotationIndex);
				lightSources.Add((LightSource)instance);
				break;
			case ObjectInstanceType.ManufacturingObject:
				instance = new ManufacturingObject(prefab, tile, rotationIndex);
				manufacturingObjectInstances.Add((ManufacturingObject)instance);
				break;
			case ObjectInstanceType.Farm:
				instance = new Farm(prefab, tile);
				farms.Add((Farm)instance);
				tile.farm = (Farm)instance;
				break;
		}
		if (instance == null) {
			Debug.LogError("Instance is null for prefab " + (prefab != null ? prefab.name : "null") + " at tile " + tile.obj.transform.position);
		}
		if (addToList) {
			AddObjectInstance(instance);
		}
		return instance;
	}

	public class ObjectInstance {

		public readonly TileManager.Tile tile; // The tile that this object covers that is closest to the zeroPointTile (usually they are the same tile)
		public readonly List<TileManager.Tile> additionalTiles = new List<TileManager.Tile>();
		public readonly TileManager.Tile zeroPointTile; // The tile representing the (0,0) position of the object even if the object doesn't cover it

		public readonly ObjectPrefab prefab;
		public readonly GameObject obj;

		public readonly int rotationIndex;

		public bool active;

		public float integrity;

		public ObjectInstance(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) {
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

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, zeroPointTile.obj.transform, false);
			obj.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			obj.name = "Tile Object Instance: " + prefab.name;
			obj.GetComponent<SpriteRenderer>().sortingOrder = 1 + prefab.layer; // Tile Object Sprite
			obj.GetComponent<SpriteRenderer>().sprite = prefab.baseSprite;

			if (prefab.blocksLight) {
				foreach (LightSource lightSource in GameManager.resourceM.lightSources) {
					foreach (TileManager.Tile objectTile in additionalTiles) {
						if (lightSource.litTiles.Contains(objectTile)) {
							lightSource.RemoveTileBrightnesses();
							lightSource.SetTileBrightnesses();
						}
					}
				}
			}

			SetColour(tile.sr.color);

			integrity = prefab.integrity;
		}

		public void FinishCreation() {
			List<TileManager.Tile> bitmaskingTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile additionalTile in additionalTiles) {
				bitmaskingTiles.Add(additionalTile);
				bitmaskingTiles.AddRange(additionalTile.surroundingTiles);
			}
			bitmaskingTiles = bitmaskingTiles.Distinct().ToList();
			GameManager.resourceM.Bitmask(bitmaskingTiles);
			foreach (TileManager.Tile tile in additionalTiles) {
				SetColour(tile.sr.color);
			}
		}

		public void SetColour(Color newColour) {
			obj.GetComponent<SpriteRenderer>().color = new Color(newColour.r, newColour.g, newColour.b, 1f);
		}

		public void SetActiveSprite(JobManager.Job job) {
			if (active) {
				if (prefab.activeSprites.Count > 0) {
					if (prefab.type == ObjectEnum.SplittingBlock) {
						int customActiveSpriteIndex = 0;
						if (job.createResource.type == ResourceEnum.Wood) {
							customActiveSpriteIndex = 0;
						} else if (job.createResource.type == ResourceEnum.Firewood) {
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

		public void SetActive(bool active) {
			this.active = active;
		}

		public virtual void Update() {

		}
	}

	public List<Container> containers = new List<Container>();

	public List<Container> GetContainersInRegion(TileManager.Map.Region region) {
		return containers.Where(c => c.tile.region == region).ToList();
	}

	public Container GetContainerOrChildOnTile(TileManager.Tile tile) {
		Container container = null;
		if (container == null) {
			container = containers.Find(c => c.tile == tile);
		}
		if (container == null) {
			container = GameManager.resourceM.tradingPosts.Find(tp => tp.tile == tile);
		}
		return container;
	}

	public class Container : ObjectInstance {

		public Inventory inventory;

		public Container(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) : base(prefab, tile, rotationIndex) {
			inventory = new Inventory(null, this, prefab.maxInventoryAmount);
		}
	}

	public List<TradingPost> tradingPosts = new List<TradingPost>();

	public List<TradingPost> GetTradingPostsInRegion(TileManager.Map.Region region) {
		return tradingPosts.Where(tp => tp.tile.region == region).ToList();
	}

	public List<ResourceAmount> GetAvailableResourcesInTradingPostsInRegion(TileManager.Map.Region region) {
		List<ResourceAmount> availableResources = new List<ResourceAmount>();
		foreach (TradingPost tradingPost in GetTradingPostsInRegion(region)) {
			foreach (ResourceAmount resourceAmount in tradingPost.inventory.resources) {
				ResourceAmount accessibleResource = availableResources.Find(ra => ra.resource == resourceAmount.resource);
				if (accessibleResource != null) {
					accessibleResource.amount += resourceAmount.amount;
				} else {
					availableResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				}
			}
		}
		return availableResources;
	}

	public class TradingPost : Container {

		public List<ResourceAmount> targetResourceAmounts = new List<ResourceAmount>();

		public TradingPost(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) : base(prefab, tile, rotationIndex) {

		}
	}

	public List<SleepSpot> sleepSpots = new List<SleepSpot>();

	public class SleepSpot : ObjectInstance {

		public ColonistManager.Colonist occupyingColonist;

		public SleepSpot(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) : base(prefab, tile, rotationIndex) {

		}

		public void StartSleeping(ColonistManager.Colonist colonist) {
			occupyingColonist = colonist;
		}

		public void StopSleeping() {
			occupyingColonist = null;
		}
	}

	public List<ManufacturingObject> manufacturingObjectInstances = new List<ManufacturingObject>();

	public class ManufacturingObject : ObjectInstance {

		public Resource createResource;
		public bool hasEnoughRequiredResources;

		public Resource fuelResource;
		public bool hasEnoughFuel;
		public int fuelResourcesRequired = 0;

		public bool canActivate;

		public List<JobManager.Job> jobBacklog = new List<JobManager.Job>();

		public ManufacturingObject(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) : base(prefab, tile, rotationIndex) {

		}

		public override void Update() {
			base.Update();

			hasEnoughRequiredResources = createResource != null;
			if (createResource != null) {
				foreach (ResourceAmount resourceAmount in createResource.manufacturingResources) {
					if (resourceAmount.resource.GetWorldTotalAmount() < resourceAmount.amount) {
						hasEnoughRequiredResources = false;
					}
				}
			}
			if (createResource != null) {
				if (createResource.manufacturingEnergy != 0) {
					hasEnoughFuel = fuelResource != null;
					if (fuelResource != null && createResource != null) {
						fuelResourcesRequired = Mathf.CeilToInt((createResource.manufacturingEnergy) / ((float)fuelResource.fuelEnergy));
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
				if (canActivate && createResource.GetDesiredAmount() > createResource.GetWorldTotalAmount() && jobBacklog.Count < 1) {
					GameManager.resourceM.CreateResource(createResource, 1, this);
				}
			}
		}
	}

	public List<LightSource> lightSources = new List<LightSource>();

	public class LightSource : ObjectInstance {

		public List<TileManager.Tile> litTiles = new List<TileManager.Tile>();

		public LightSource(ObjectPrefab prefab, TileManager.Tile tile, int rotationIndex) : base(prefab, tile, rotationIndex) {
			SetTileBrightnesses();
		}

		public void SetTileBrightnesses() {
			List<TileManager.Tile> newLitTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
				float distance = Vector2.Distance(tile.obj.transform.position, this.tile.obj.transform.position);
				if (distance <= prefab.maxLightDistance) {
					float intensityAtTile = Mathf.Clamp(prefab.maxLightDistance * (1f / Mathf.Pow(distance, 2f)), 0f, 1f);
					if (tile != this.tile) {
						bool lightTile = true;
						Vector3 lightVector = obj.transform.position;
						while ((obj.transform.position - lightVector).magnitude <= distance) {
							TileManager.Tile lightVectorTile = GameManager.colonyM.colony.map.GetTileFromPosition(lightVector);
							if (lightVectorTile != this.tile) {
								if (GameManager.colonyM.colony.map.TileBlocksLight(lightVectorTile)) {
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
							lightVector += (tile.obj.transform.position - obj.transform.position).normalized * 0.1f;
						}
						if (lightTile) {
							tile.AddLightSourceBrightness(this, intensityAtTile);
							newLitTiles.Add(tile);
						}
					} else {
						this.tile.AddLightSourceBrightness(this, intensityAtTile);
					}
				}
			}
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime);
			litTiles.AddRange(newLitTiles);
		}

		public void RemoveTileBrightnesses() {
			foreach (TileManager.Tile tile in litTiles) {
				tile.RemoveLightSourceBrightness(this);
			}
			litTiles.Clear();
			tile.RemoveLightSourceBrightness(this);
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime);
		}
	}

	public enum PlantGroupPropertyEnum {
		PlantGroup,
		Type,
		Plants
	}

	public enum PlantPropertyEnum {
		Plant,
		Type,
		Living,
		Integrity,
		Seed,
		PlantJob,
		ReturnResources,
		HarvestResources
	}

	public enum PlantGroupEnum {
		None,
		Cactus,
		Tree,
		Bush
	};

	public enum PlantEnum {
		None,
		Cactus,
		SnowTree,
		ThinTree,
		WideTree,
		DeadTree,
		Bush
	};

	public static readonly Dictionary<ResourceEnum, ResourceEnum> seedToHarvestResource = new Dictionary<ResourceEnum, ResourceEnum>() {
		{ ResourceEnum.AppleSeed, ResourceEnum.Apple },
		{ ResourceEnum.Blueberry, ResourceEnum.Blueberry }
	};

	public void CreatePlantPrefabs() {
		List<KeyValuePair<string, object>> plantGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/plants").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> plantGroupProperty in plantGroupProperties) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupProperty.Key)) {
				case PlantGroupPropertyEnum.PlantGroup:

					PlantGroupEnum? groupType = null;
					List<PlantPrefab> prefabs = new List<PlantPrefab>();

					foreach (KeyValuePair<string, object> plantGroupSubProperty in (List<KeyValuePair<string, object>>)plantGroupProperty.Value) {
						switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupSubProperty.Key)) {
							case PlantGroupPropertyEnum.Type:
								groupType = (PlantGroupEnum)Enum.Parse(typeof(PlantGroupEnum), (string)plantGroupSubProperty.Value);
								break;
							case PlantGroupPropertyEnum.Plants:
								foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)plantGroupSubProperty.Value) {
									switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantProperty.Key)) {
										case PlantPropertyEnum.Plant:

											PlantEnum? type = null;
											bool? living = null;
											int? integrity = null;
											Resource seed = null;
											List<ResourceRange> returnResources = new List<ResourceRange>();
											List<ResourceRange> harvestResources = new List<ResourceRange>();

											foreach (KeyValuePair<string, object> plantSubProperty in (List<KeyValuePair<string, object>>)plantProperty.Value) {
												switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantSubProperty.Key)) {
													case PlantPropertyEnum.Type:
														type = (PlantEnum)Enum.Parse(typeof(PlantEnum), (string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Living:
														living = bool.Parse((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Integrity:
														integrity = int.Parse((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Seed:
														seed = GetResourceByString((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.ReturnResources:
														foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
															Resource resource = GameManager.resourceM.GetResourceByString(resourceRangeString.Split(':')[0]);
															int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
															int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
															returnResources.Add(new ResourceRange(resource, min, max));
														}
														break;
													case PlantPropertyEnum.HarvestResources:
														foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
															Resource resource = GameManager.resourceM.GetResourceByString(resourceRangeString.Split(':')[0]);
															int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
															int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
															harvestResources.Add(new ResourceRange(resource, min, max));
														}
														break;
													default:
														Debug.LogError("Unknown plant sub property: " + plantSubProperty.Key + " " + plantSubProperty.Value);
														break;
												}
											}

											PlantPrefab plantPrefab = new PlantPrefab(
												type.Value,
												groupType.Value,
												living.Value,
												integrity.Value,
												seed,
												returnResources,
												harvestResources
											);
											prefabs.Add(plantPrefab);
											plantPrefabs.Add(type.Value, plantPrefab);

											break;
										default:
											Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown plant group sub property: " + plantGroupSubProperty.Key + " " + plantGroupSubProperty.Value);
								break;
						}

					}

					PlantGroup plantGroup = new PlantGroup(
						groupType.Value,
						prefabs
					);
					plantGroups.Add(groupType.Value, plantGroup);

					break;
				default:
					Debug.LogError("Unknown plant group property: " + plantGroupProperty.Key + " " + plantGroupProperty.Value);
					break;

			}
		}
	}

	public Dictionary<PlantGroupEnum, PlantGroup> plantGroups = new Dictionary<PlantGroupEnum, PlantGroup>();

	public class PlantGroup {
		public readonly PlantGroupEnum type;
		public readonly string name;

		public readonly List<PlantPrefab> prefabs;

		public PlantGroup(
			PlantGroupEnum type,
			List<PlantPrefab> prefabs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.prefabs = prefabs;
		}
	}

	public PlantGroup GetPlantGroupByEnum(PlantGroupEnum plantGroupEnum) {
		return plantGroups[plantGroupEnum];
	}

	public Dictionary<PlantEnum, PlantPrefab> plantPrefabs = new Dictionary<PlantEnum, PlantPrefab>();

	public class PlantPrefab {
		public readonly PlantEnum type;
		public readonly string name;

		public readonly PlantGroupEnum groupType;

		public readonly Resource seed;

		public readonly bool living;

		public readonly int integrity;

		public readonly List<ResourceRange> returnResources;

		public readonly List<ResourceRange> harvestResources;

		public readonly List<Sprite> smallSprites;
		public readonly List<Sprite> fullSprites;
		public readonly Dictionary<Resource, Dictionary<bool, List<Sprite>>> harvestResourceSprites = new Dictionary<Resource, Dictionary<bool, List<Sprite>>>();

		public PlantPrefab(
			PlantEnum type,
			PlantGroupEnum groupType,
			bool living,
			int integrity,
			Resource seed,
			List<ResourceRange> returnResources,
			List<ResourceRange> harvestResources
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.living = living;

			this.integrity = integrity;

			this.seed = seed;

			this.returnResources = returnResources;

			this.harvestResources = harvestResources;

			smallSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-small").ToList();
			fullSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-full").ToList();

			foreach (Resource harvestResource in harvestResources.Select(rr => rr.resource)) {
				Dictionary<bool, List<Sprite>> foundSpriteSizes = new Dictionary<bool, List<Sprite>>();

				List<Sprite> smallResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-small-" + harvestResource.type.ToString().ToLower()).ToList();
				if (smallResourceSprites != null && smallResourceSprites.Count > 0) {
					foundSpriteSizes.Add(true, smallResourceSprites);
				}

				List<Sprite> fullResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-full-" + harvestResource.type.ToString().ToLower()).ToList();
				if (fullResourceSprites != null && fullResourceSprites.Count > 0) {
					foundSpriteSizes.Add(false, fullResourceSprites);
				}

				if (foundSpriteSizes.Count > 0) {
					harvestResourceSprites.Add(harvestResource, foundSpriteSizes);
				}
			}
		}
	}

	public PlantPrefab GetPlantPrefabByString(string plantString) {
		return GetPlantPrefabByEnum((PlantEnum)Enum.Parse(typeof(PlantEnum), plantString));
	}

	public PlantPrefab GetPlantPrefabByEnum(PlantEnum plantEnum) {
		return plantPrefabs[plantEnum];
	}

	public List<PlantPrefab> GetPlantPrefabs() {
		return plantPrefabs.Values.ToList();
	}

	public PlantPrefab GetPlantPrefabByBiome(TileManager.Biome biome, bool guaranteedTree) {
		if (guaranteedTree) {
			List<PlantEnum> biomePlantEnums = biome.plantChances.Keys.Where(plantEnum => plantEnum != PlantEnum.DeadTree).ToList();
			if (biomePlantEnums.Count > 0) {
				return GetPlantPrefabByEnum(biomePlantEnums[UnityEngine.Random.Range(0, biomePlantEnums.Count)]);
			} else {
				return null;
			}
		} else {
			foreach (KeyValuePair<PlantEnum, float> plantChancesKVP in biome.plantChances) {
				if (UnityEngine.Random.Range(0f, 1f) < biome.plantChances[plantChancesKVP.Key]) {
					return GetPlantPrefabByEnum(plantChancesKVP.Key);
				}
			}
		}
		return null;
	}

	public List<Plant> smallPlants = new List<Plant>();

	public class Plant {
		public readonly string name;

		public readonly PlantPrefab prefab;
		public readonly TileManager.Tile tile;

		public readonly GameObject obj;

		public float integrity;

		public bool small;

		public float growthProgress = 0;

		public readonly Resource harvestResource;

		public Plant(PlantPrefab prefab, TileManager.Tile tile, bool? small, bool randomHarvestResource, Resource specificHarvestResource) {
			this.prefab = prefab;
			this.tile = tile;

			integrity = prefab.integrity;

			this.small = small ?? UnityEngine.Random.Range(0, 100) <= 10;
			if (this.small) {
				GameManager.resourceM.smallPlants.Add(this);
			}

			harvestResource = null;
			if (randomHarvestResource && prefab.harvestResources.Count > 0) {
				if (UnityEngine.Random.Range(0, 100) <= 5) {
					List<ResourceAmount> resourceChances = new List<ResourceAmount>();
					foreach (Resource harvestResource in prefab.harvestResources.Select(rr => rr.resource)) {
						resourceChances.Add(new ResourceAmount(harvestResource, UnityEngine.Random.Range(0, 100)));
					}
					harvestResource = resourceChances.OrderByDescending(ra => ra.amount).First().resource;
				}
			}
			if (specificHarvestResource != null) {
				harvestResource = specificHarvestResource;
			}

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, tile.obj.transform.position, Quaternion.identity);
			SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
			if (harvestResource != null) {
				name = harvestResource.name + " " + prefab.name;
				sr.sprite = prefab.harvestResourceSprites[harvestResource][this.small][UnityEngine.Random.Range(0, prefab.harvestResourceSprites[harvestResource][this.small].Count)];
			} else {
				name = prefab.name;
				sr.sprite = this.small ? prefab.smallSprites[UnityEngine.Random.Range(0, prefab.smallSprites.Count)] : prefab.fullSprites[UnityEngine.Random.Range(0, prefab.fullSprites.Count)];
			}
			sr.sortingOrder = 1; // Plant Sprite

			obj.name = "Plant: " + prefab.name + " " + sr.sprite.name;
			obj.transform.parent = tile.obj.transform;
		}

		public void Grow() {
			small = false;
			if (harvestResource != null) {
				obj.GetComponent<SpriteRenderer>().sprite = prefab.harvestResourceSprites[harvestResource][small][UnityEngine.Random.Range(0, prefab.harvestResourceSprites[harvestResource][small].Count)];
			} else {
				obj.GetComponent<SpriteRenderer>().sprite = prefab.fullSprites[UnityEngine.Random.Range(0, prefab.fullSprites.Count)];
			}
			GameManager.resourceM.smallPlants.Remove(this);
		}

		public void Remove() {
			MonoBehaviour.Destroy(obj);
			GameManager.resourceM.smallPlants.Remove(this);
		}
	}

	public void GrowPlants() {
		if (!GameManager.timeM.GetPaused() && GameManager.timeM.minuteChanged) {
			List<Plant> growPlants = new List<Plant>();
			foreach (Plant plant in smallPlants) {
				plant.growthProgress += 1;
				if (plant.growthProgress > TimeManager.dayLengthSeconds * 4) {
					if (UnityEngine.Random.Range(0, 100) < (0.01f * (plant.growthProgress / TimeManager.dayLengthSeconds * 4))) {
						growPlants.Add(plant);
					}
				}
			}
			foreach (Plant plant in growPlants) {
				plant.Grow();
			}
		}
	}

	public static readonly Dictionary<ResourceEnum, ResourceEnum> farmSeedReturnResource = new Dictionary<ResourceEnum, ResourceEnum>() {
		{ ResourceEnum.WheatSeed, ResourceEnum.Wheat },
		{ ResourceEnum.Potato, ResourceEnum.Potato },
		{ ResourceEnum.CottonSeed, ResourceEnum.Cotton }
	};
	public static readonly Dictionary<ResourceEnum, ObjectEnum> farmSeedsTileObject = new Dictionary<ResourceEnum, ObjectEnum>() {
		{ ResourceEnum.WheatSeed, ObjectEnum.WheatFarm },
		{ ResourceEnum.Potato, ObjectEnum.PotatoFarm },
		{ ResourceEnum.CottonSeed, ObjectEnum.CottonFarm }
	};

	public List<Farm> farms = new List<Farm>();

	public class Farm : ObjectInstance {

		public readonly string name;

		public float growTimer = 0;

		public int growProgressSpriteIndex = -1;
		public readonly List<Sprite> growProgressSprites = new List<Sprite>();
		public readonly int maxSpriteIndex = 0;

		private readonly float precipitationGrowthMultiplier;
		private readonly float temperatureGrowthMultipler;

		public Farm(ObjectPrefab prefab, TileManager.Tile tile) : base(prefab, tile, 0) {
			name = prefab.harvestResource.name + " Farm";

			growProgressSprites = prefab.bitmaskSprites;
			maxSpriteIndex = growProgressSprites.Count - 1;

			precipitationGrowthMultiplier = CalculatePrecipitationGrowthMultiplierForTile(tile);
			temperatureGrowthMultipler = CalculateTemperatureGrowthMultiplierForTile(tile);

			Update();
		}

		public override void Update() {
			base.Update();

			if (growTimer >= prefab.growthTimeDays) {
				if (!GameManager.jobM.JobOfTypeExistsAtTile(JobManager.JobTypesEnum.HarvestFarm, tile)) {
					GameManager.jobM.CreateJob(new JobManager.Job(tile, GameManager.resourceM.GetObjectPrefabByEnum(ObjectEnum.HarvestFarm), 0));
				}
			} else {
				growTimer += CalculateGrowthRate();

				int newGrowProgressSpriteIndex = Mathf.FloorToInt((growTimer / (prefab.growthTimeDays + 10)) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[Mathf.Clamp(growProgressSpriteIndex, 0, maxSpriteIndex)];
				}
			}
		}

		public float CalculateGrowthRate() {
			float growthRate = GameManager.timeM.deltaTime;
			growthRate *= Mathf.Max(GameManager.colonyM.colony.map.CalculateBrightnessLevelAtHour(GameManager.timeM.tileBrightnessTime), tile.lightSourceBrightness);
			growthRate *= precipitationGrowthMultiplier;
			growthRate *= temperatureGrowthMultipler;
			growthRate = Mathf.Clamp(growthRate, 0, 1);
			return growthRate;
		}

		public static float CalculatePrecipitationGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Min((-2 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 2) + 1), (-30 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 3) + 1));
		}

		public static float CalculateTemperatureGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Clamp(Mathf.Min(((tile.temperature - 10) / 15f + 1), (-((tile.temperature - 50) / 20f))), 0, 1);
		}
	}

	public List<string> locationNames = new List<string>();

	public void LoadLocationNames() {
		locationNames = Resources.Load<TextAsset>(@"Data/locationNames").text.Split('\n').Select(s => UIManager.RemoveNonAlphanumericChars(s)).ToList();
	}

	public string GetRandomLocationName() {
		List<string> filteredLocationNames = locationNames.Where(ln =>
				(GameManager.universeM.universe == null || ln != GameManager.universeM.universe.name)
			&&	(GameManager.planetM.planet == null || ln != GameManager.planetM.planet.name)
			&&	(GameManager.colonyM.colony == null || ln != GameManager.colonyM.colony.name)
			&&	GameManager.caravanM.caravans.Find(c => c.location.name == ln) == null
		).ToList();
		
		return filteredLocationNames[UnityEngine.Random.Range(0, filteredLocationNames.Count)];
	}

	private int BitSumTileObjects(List<ObjectEnum> compareTileObjectTypes, List<TileManager.Tile> tileSurroundingTiles) {
		List<int> layers = new List<int>();
		foreach (TileManager.Tile tile in tileSurroundingTiles) {
			if (tile != null) {
				foreach (KeyValuePair<int, ObjectInstance> kvp in tile.objectInstances) {
					if (!layers.Contains(kvp.Key)) {
						layers.Add(kvp.Key);
					}
				}
			}
		}
		layers.Sort();

		Dictionary<int, List<int>> layersSumTiles = new Dictionary<int, List<int>>();
		foreach (int layer in layers) {
			List<int> layerSumTiles = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0 };
			for (int i = 0; i < tileSurroundingTiles.Count; i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareTileObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && TileManager.Map.diagonalCheckMap.ContainsKey(i)) {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
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
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareTileObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer, layerSumTiles);
		}

		List<bool> sumTiles = new List<bool>() { false, false, false, false, false, false, false, false };

		foreach (KeyValuePair<int, List<int>> layerSumTiles in layersSumTiles) {
			foreach (ObjectEnum topEnum in compareTileObjectTypes) {
				ObjectPrefab top = GetObjectPrefabByEnum(topEnum);
				if (top.layer == layerSumTiles.Key) {
					foreach (TileManager.Tile tile in tileSurroundingTiles) {
						if (tile != null) {
							ObjectInstance topInstance = tile.GetAllObjectInstances().Find(instances => instances.prefab == top);
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

		for (int i = 0; i < sumTiles.Count; i++) {
			if (sumTiles[i]) {
				sum += Mathf.RoundToInt(Mathf.Pow(2, i));
			}
		}

		return sum;
	}

	private void BitmaskTileObjects(ObjectInstance objectInstance, bool includeDiagonalSurroundingTiles, bool customBitSumInputs, bool compareEquivalentTileObjects, List<ObjectEnum> customCompareTileObjectTypes) {
		int sum = 0;
		if (customBitSumInputs) {
			sum = BitSumTileObjects(customCompareTileObjectTypes, (includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
		} else {
			if (compareEquivalentTileObjects) {
				if (objectInstance.prefab.subGroupType == ObjectSubGroupEnum.Floors) {
					sum = BitSumTileObjects(objectPrefabSubGroups[ObjectSubGroupEnum.Floors].prefabs.Select(prefab => prefab.type).ToList(), (includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				} else if (objectInstance.prefab.subGroupType == ObjectSubGroupEnum.Walls) {
					sum = BitSumTileObjects(objectPrefabSubGroups[ObjectSubGroupEnum.Walls].prefabs.Select(top => top.type).ToList(), (includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				} else {
					sum = BitSumTileObjects(new List<ObjectEnum>() { objectInstance.prefab.type }, (includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
				}
			} else {
				sum = BitSumTileObjects(new List<ObjectEnum>() { objectInstance.prefab.type }, (includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles));
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
				foreach (ObjectInstance tileObjectInstance in tile.GetAllObjectInstances()) {
					if (tileObjectInstance.prefab.bitmasking) {
						BitmaskTileObjects(tileObjectInstance, true, false, false, null);
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