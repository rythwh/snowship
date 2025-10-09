using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Snowship;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NMap.Models.Geography;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;
using Random = UnityEngine.Random;

public class ResourceManager : IAsyncStartable, IPostStartable, ITickable, IDisposable
{
	private readonly TimeManager timeM;
	private readonly JobManager jobM;
	private readonly IColonistQuery colonistQuery;
	private readonly UniverseManager universeM;
	private readonly PlanetManager planetM;
	private readonly ColonyManager colonyM;
	private readonly CaravanManager caravanM;
	private readonly ResourceLoader resourceLoader;
	private readonly IResourceQuery resourceQuery;

	public GameObject objectPrefab { get; private set; }

	public Sprite selectionCornersSprite;
	public Sprite whiteSquareSprite;
	public Sprite clearSquareSprite;
	public GameObject colonyObj;
	public GameObject tileImage;
	public GameObject objectDataPanel;

	public event Action OnResourceTotalsUpdated;

	private static readonly List<string> locationNames = new();

	public ResourceManager(
		TimeManager timeM,
		JobManager jobM,
		IColonistQuery colonistQuery,
		UniverseManager universeM,
		PlanetManager planetM,
		ColonyManager colonyM,
		CaravanManager caravanM,
		ResourceLoader resourceLoader,
		IResourceQuery resourceQuery
	) {
		this.timeM = timeM;
		this.jobM = jobM;
		this.colonistQuery = colonistQuery;
		this.universeM = universeM;
		this.planetM = planetM;
		this.colonyM = colonyM;
		this.caravanM = caravanM;
		this.resourceLoader = resourceLoader;
		this.resourceQuery = resourceQuery;
	}

	public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken()) {

		await LoadObjectPrefab();

		SetResourceReferences();
		CreatePlantPrefabs();
		CreateObjectPrefabs();
		LoadLocationNames();

		TileType.InitializeTileTypes();
		Biome.InitializeBiomes();
		ResourceVein.InitializeResourceVeins();

		Inventory.AnyInventoryChanged += OnAnyInventoryChanged;
	}

	private async UniTask LoadObjectPrefab() {
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Game/Object");
		handle.ReleaseHandleOnCompletion();
		objectPrefab = await handle;
	}

	public void PostStart() {
		timeM.OnTimeChanged += OnTimeChanged;
	}

	public void Dispose() {
		timeM.OnTimeChanged -= OnTimeChanged;
	}

	private void OnTimeChanged(SimulationDateTime time) {
		GrowPlants();
	}

	private void SetResourceReferences() {
		selectionCornersSprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		whiteSquareSprite = Resources.Load<Sprite>(@"UI/white-square");
		clearSquareSprite = Resources.Load<Sprite>(@"UI/clear-square");
		colonyObj = Resources.Load<GameObject>(@"UI/UIElements/ColonyObj");
		tileImage = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage");
		objectDataPanel = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel");
	}

	public void Tick() {
		foreach (Farm farm in Farm.farms) {
			farm.Update();
		}
		foreach (CraftingObject craftingObject in CraftingObject.craftingObjectInstances) {
			craftingObject.Update();
		}
	}





	private void OnAnyInventoryChanged() {
		CalculateResourceTotals();
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in resourceQuery.GetResources()) {
			resource.SetWorldTotalAmount(0);
			resource.SetColonistsTotalAmount(0);
			resource.SetContainerTotalAmount(0);
			resource.SetUnreservedContainerTotalAmount(0);
			resource.SetUnreservedTradingPostTotalAmount(0);
			resource.SetAvailableAmount(0);
		}

		foreach (Colonist colonist in colonistQuery.Colonists) {
			foreach (ResourceAmount resourceAmount in colonist.Inventory.resources) {
				resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToColonistsTotalAmount(resourceAmount.Amount);
			}
			foreach (ReservedResources reservedResources in colonist.Inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
					resourceAmount.Resource.AddToColonistsTotalAmount(resourceAmount.Amount);
				}
			}
		}
		foreach (Container container in Container.containers) {
			foreach (ResourceAmount resourceAmount in container.Inventory.resources) {
				resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToContainerTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToUnreservedContainerTotalAmount(resourceAmount.Amount);
			}
			foreach (ReservedResources reservedResources in container.Inventory.reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
					resourceAmount.Resource.AddToContainerTotalAmount(resourceAmount.Amount);
				}
			}
		}
		foreach (TradingPost tradingPost in TradingPost.tradingPosts) {
			foreach (ResourceAmount resourceAmount in tradingPost.Inventory.resources) {
				resourceAmount.Resource.AddToUnreservedTradingPostTotalAmount(resourceAmount.Amount);
			}
		}

		foreach (Resource resource in resourceQuery.GetResources()) {
			resource.CalculateAvailableAmount();
		}

		OnResourceTotalsUpdated?.Invoke();
	}

	public List<ResourceAmount> GetFilteredResources(bool colonistInventory, bool colonistReserved, bool containerInventory, bool containerReserved) {
		List<ResourceAmount> returnResources = new();
		if (colonistInventory || colonistReserved) {
			foreach (Colonist colonist in colonistQuery.Colonists) {
				if (colonistInventory) {
					foreach (ResourceAmount resourceAmount in colonist.Inventory.resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
						} else {
							existingResourceAmount.Amount += resourceAmount.Amount;
						}
					}
				}
				if (colonistReserved) {
					foreach (ReservedResources reservedResources in colonist.Inventory.reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							} else {
								existingResourceAmount.Amount += resourceAmount.Amount;
							}
						}
					}
				}
			}
		}
		if (containerInventory || containerReserved) {
			foreach (Container container in Container.containers) {
				if (containerInventory) {
					foreach (ResourceAmount resourceAmount in container.Inventory.resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
						} else {
							existingResourceAmount.Amount += resourceAmount.Amount;
						}
					}
				}
				if (containerReserved) {
					foreach (ReservedResources reservedResources in container.Inventory.reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							} else {
								existingResourceAmount.Amount += resourceAmount.Amount;
							}
						}
					}
				}
			}
		}
		return returnResources;
	}

	private enum ObjectGroupPropertyEnum
	{
		Group,
		Type,
		SubGroups
	}

	private enum ObjectSubGroupPropertyEnum
	{
		SubGroup,
		Type,
		Objects
	}

	private enum ObjectPropertyEnum
	{
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
		HarvestResources,
		TimeToBuild,
		CommonResources,
		Variations,
		VariationNameOrder,
		SelectionModifiers,
		JobType,
		AddToTileWhenBuilt
	}

	private enum VariationPropertyEnum
	{
		Variation,
		Name,
		UniqueResources,
		WalkSpeed,
		Integrity,
		Flammability,
		TimeToBuild,
		Plants
	}

	private void CreateObjectPrefabs() {
		List<KeyValuePair<string, object>> objectGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/objects").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> objectGroupProperty in objectGroupProperties) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupProperty.Key)) {
				case ObjectGroupPropertyEnum.Group:

					ObjectPrefabGroup objectPrefabGroup = ParseObjectPrefabGroup(objectGroupProperty);
					ObjectPrefabGroup.objectPrefabGroups.Add(objectPrefabGroup.type, objectPrefabGroup);

					break;
				default:
					Debug.LogError("Unknown object group property: " + objectGroupProperty.Key + " " + objectGroupProperty.Value);
					break;
			}
		}
	}

	private ObjectPrefabGroup ParseObjectPrefabGroup(KeyValuePair<string, object> objectGroupProperty) {

		ObjectPrefabGroup.ObjectGroupEnum? groupType = null;
		List<IGroupItem> subGroups = new();

		foreach (KeyValuePair<string, object> objectGroupSubProperty in (List<KeyValuePair<string, object>>)objectGroupProperty.Value) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupSubProperty.Key)) {
				case ObjectGroupPropertyEnum.Type:
					groupType = (ObjectPrefabGroup.ObjectGroupEnum)Enum.Parse(typeof(ObjectPrefabGroup.ObjectGroupEnum), (string)objectGroupSubProperty.Value);
					break;
				case ObjectGroupPropertyEnum.SubGroups:
					foreach (KeyValuePair<string, object> objectSubGroupProperty in (List<KeyValuePair<string, object>>)objectGroupSubProperty.Value) {
						switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupProperty.Key)) {
							case ObjectSubGroupPropertyEnum.SubGroup:

								ObjectPrefabSubGroup objectPrefabSubGroup = ParseObjectPrefabSubGroup(groupType, objectSubGroupProperty);
								subGroups.Add(objectPrefabSubGroup);
								ObjectPrefabSubGroup.objectPrefabSubGroups.Add(objectPrefabSubGroup.type, objectPrefabSubGroup);

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

		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

		return new ObjectPrefabGroup(
			groupType.Value,
			subGroups
		);
	}

	private ObjectPrefabSubGroup ParseObjectPrefabSubGroup(ObjectPrefabGroup.ObjectGroupEnum? groupType, KeyValuePair<string, object> objectSubGroupProperty) {

		ObjectPrefabSubGroup.ObjectSubGroupEnum? subGroupType = null;
		List<IGroupItem> prefabs = new();

		foreach (KeyValuePair<string, object> objectSubGroupSubProperty in (List<KeyValuePair<string, object>>)objectSubGroupProperty.Value) {
			switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupSubProperty.Key)) {
				case ObjectSubGroupPropertyEnum.Type:
					subGroupType = (ObjectPrefabSubGroup.ObjectSubGroupEnum)Enum.Parse(typeof(ObjectPrefabSubGroup.ObjectSubGroupEnum), (string)objectSubGroupSubProperty.Value);
					break;
				case ObjectSubGroupPropertyEnum.Objects:
					foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)objectSubGroupSubProperty.Value) {
						switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectProperty.Key)) {
							case ObjectPropertyEnum.Object:

								ObjectPrefab objectPrefab = ParseObjectPrefab(groupType, subGroupType, objectProperty);
								prefabs.Add(objectPrefab);
								ObjectPrefab.objectPrefabs.Add(objectPrefab.type, objectPrefab);

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

		System.Diagnostics.Debug.Assert(subGroupType != null, nameof(subGroupType) + " != null");
		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

		return new ObjectPrefabSubGroup(
			subGroupType.Value,
			prefabs
		);
	}

	private ObjectPrefab ParseObjectPrefab(
		ObjectPrefabGroup.ObjectGroupEnum? groupType,
		ObjectPrefabSubGroup.ObjectSubGroupEnum? subGroupType,
		KeyValuePair<string, object> objectProperty
	) {
		ObjectPrefab.ObjectEnum? type = null;
		ObjectInstance.ObjectInstanceType instanceType = ObjectInstance.ObjectInstanceType.Normal;
		int? layer = null;
		bool? bitmasking = false;
		bool? blocksLight = false;
		int? integrity = 0;
		List<Vector2> multiTilePositions = new();

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

		// Crafting Object
		bool? usesFuel = null;

		// Farm
		int? growthTimeDays = null;
		Resource seedResource = null;
		List<ResourceRange> harvestResources = new();

		// Job
		int? timeToBuild = null;
		List<ResourceAmount> commonResources = new();
		List<Variation> variations = new();
		Variation.VariationNameOrderEnum variationNameOrder = Variation.VariationNameOrderEnum.VariationObject;
		// List<SelectionModifiers.SelectionModifiersEnum> selectionModifiers = new();
		string jobType = null;
		bool? addToTileWhenBuilt = true;

		foreach (KeyValuePair<string, object> objectSubProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
			switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectSubProperty.Key)) {
				case ObjectPropertyEnum.Type:
					type = (ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), (string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.InstanceType:
					instanceType = (ObjectInstance.ObjectInstanceType)Enum.Parse(typeof(ObjectInstance.ObjectInstanceType), (string)objectSubProperty.Value);
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
					maxInventoryVolume = ((string)objectSubProperty.Value).Contains("Max") ? int.MaxValue : int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MaxInventoryWeight:
					maxInventoryWeight = ((string)objectSubProperty.Value).Contains("Max") ? int.MaxValue : int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MaxLightDistance:
					maxLightDistance = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.LightColour:
					lightColour = ColourUtilities.HexToColor((string)objectSubProperty.Value);
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
					seedResource = resourceQuery.GetResourceByString((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.HarvestResources:
					foreach (string resourceRangeString in ((string)objectSubProperty.Value).Split(',')) {
						Resource resource = resourceQuery.GetResourceByString(resourceRangeString.Split(':')[0]);
						int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
						int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
						harvestResources.Add(new ResourceRange(resource, min, max));
					}
					break;
				case ObjectPropertyEnum.TimeToBuild:
					timeToBuild = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.CommonResources:
					foreach (string resourceAmountString in ((string)objectSubProperty.Value).Split(',')) {
						commonResources.Add(new ResourceAmount(resourceQuery.GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
					}
					break;
				case ObjectPropertyEnum.Variations:
					foreach (KeyValuePair<string, object> variationsProperty in (List<KeyValuePair<string, object>>)objectSubProperty.Value) {
						switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationsProperty.Key)) {
							case VariationPropertyEnum.Variation:
								Variation variation = ParseVariation((walkSpeed.Value, integrity.Value, flammability.Value, timeToBuild.Value), variationsProperty);
								variations.Add(variation);
								break;
						}
					}
					break;
				case ObjectPropertyEnum.VariationNameOrder:
					variationNameOrder = (Variation.VariationNameOrderEnum)Enum.Parse(typeof(Variation.VariationNameOrderEnum), (string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.SelectionModifiers:
					// 	foreach (string selectionModifierString in ((string)objectSubProperty.Value).Split(',')) {
					// 		selectionModifiers.Add((SelectionModifiers.SelectionModifiersEnum)Enum.Parse(typeof(SelectionModifiers.SelectionModifiersEnum), selectionModifierString));
					// 	}
					break;
				case ObjectPropertyEnum.JobType:
					jobType = (string)objectSubProperty.Value;
					break;
				case ObjectPropertyEnum.AddToTileWhenBuilt:
					addToTileWhenBuilt = bool.Parse((string)objectSubProperty.Value);
					break;
				default:
					Debug.LogError("Unknown object sub property: " + objectSubProperty.Key + " " + objectSubProperty.Value);
					break;
			}
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.Container && instanceType != ObjectInstance.ObjectInstanceType.TradingPost) {
			maxInventoryVolume ??= 0;
			maxInventoryWeight ??= 0;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.LightSource) {
			maxLightDistance ??= 0;
			lightColour ??= Color.white;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.Bed) {
			restComfortAmount ??= 0;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.CraftingObject) {
			usesFuel ??= false;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.Farm) {
			growthTimeDays ??= 0;
		}

		ObjectPrefab objectPrefab = new(
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
			harvestResources,
			timeToBuild.Value,
			commonResources,
			variations,
			variationNameOrder,
			// selectionModifiers,
			jobType,
			addToTileWhenBuilt.Value
		);
		foreach (Variation variation in objectPrefab.variations) {
			variation.SetPrefab(objectPrefab);
		}
		return objectPrefab;
	}

	private Variation ParseVariation((float walkSpeed, int integrity, float flammability, int timeToBuild) parentProperties, KeyValuePair<string, object> variationsProperty) {

		string variationName = null;
		List<ResourceAmount> variationUniqueResources = new();
		float? variationWalkSpeed = null;
		int? variationIntegrity = null;
		float? variationFlammability = null;
		int? variationTimeToBuild = null;

		// Plants
		Dictionary<PlantPrefab, Resource> variationPlants = new();

		foreach (KeyValuePair<string, object> variationProperty in (List<KeyValuePair<string, object>>)variationsProperty.Value) {
			switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationProperty.Key)) {
				case VariationPropertyEnum.Name:
					variationName = StringUtilities.RemoveNonAlphanumericChars((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.UniqueResources:
					foreach (string resourceAmountString in ((string)variationProperty.Value).Split(',')) {
						variationUniqueResources.Add(new ResourceAmount(resourceQuery.GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
					}
					break;
				case VariationPropertyEnum.WalkSpeed:
					variationWalkSpeed = float.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Integrity:
					variationIntegrity = int.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Flammability:
					variationFlammability = float.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.TimeToBuild:
					variationTimeToBuild = int.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Plants:
					foreach (string plantToHarvestResourceString in ((string)variationProperty.Value).Split(',')) {
						string harvestResourceString = plantToHarvestResourceString.Split(':')[1];
						variationPlants.Add(
							PlantPrefab.GetPlantPrefabByString(plantToHarvestResourceString.Split(':')[0]),
							harvestResourceString.Contains("None") ? null : resourceQuery.GetResourceByString(harvestResourceString)
						);
					}
					break;
				default:
					Debug.LogError("Unknown variation property: " + variationProperty.Key + " " + variationProperty.Value);
					break;
			}
		}

		return new Variation(
			null, // Set after objectPrefab is created
			variationName,
			variationUniqueResources,
			variationWalkSpeed ?? parentProperties.walkSpeed,
			variationIntegrity ?? parentProperties.integrity,
			variationFlammability ?? parentProperties.flammability,
			variationTimeToBuild ?? parentProperties.timeToBuild,
			variationPlants
		);
	}

	private enum PlantGroupPropertyEnum
	{
		PlantGroup,
		Type,
		Plants
	}

	private enum PlantPropertyEnum
	{
		Plant,
		Type,
		Living,
		Integrity,
		Seed,
		//PlantJob, // TODO Use this?
		ReturnResources,
		HarvestResources
	}

	private void CreatePlantPrefabs() {
		List<KeyValuePair<string, object>> plantGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/plants").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> plantGroupProperty in plantGroupProperties) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupProperty.Key)) {
				case PlantGroupPropertyEnum.PlantGroup:

					PlantGroup plantGroup = ParsePlantGroup(plantGroupProperty);
					PlantGroup.plantGroups.Add(plantGroup.type, plantGroup);

					break;
				default:
					Debug.LogError("Unknown plant group property: " + plantGroupProperty.Key + " " + plantGroupProperty.Value);
					break;

			}
		}
	}

	private PlantGroup ParsePlantGroup(KeyValuePair<string, object> plantGroupProperty) {

		PlantGroup.PlantGroupEnum? groupType = null;
		List<PlantPrefab> prefabs = new();

		foreach (KeyValuePair<string, object> plantGroupSubProperty in (List<KeyValuePair<string, object>>)plantGroupProperty.Value) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupSubProperty.Key)) {
				case PlantGroupPropertyEnum.Type:
					groupType = (PlantGroup.PlantGroupEnum)Enum.Parse(typeof(PlantGroup.PlantGroupEnum), (string)plantGroupSubProperty.Value);
					break;
				case PlantGroupPropertyEnum.Plants:
					foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)plantGroupSubProperty.Value) {
						switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantProperty.Key)) {
							case PlantPropertyEnum.Plant:

								PlantPrefab plantPrefab = ParsePlantPrefab(groupType, plantProperty);
								prefabs.Add(plantPrefab);
								PlantPrefab.plantPrefabs.Add(plantPrefab.type, plantPrefab);

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

		return new PlantGroup(
			groupType.GetValueOrDefault(),
			prefabs
		);
	}

	private PlantPrefab ParsePlantPrefab(PlantGroup.PlantGroupEnum? groupType, KeyValuePair<string, object> plantProperty) {

		Plant.PlantEnum? type = null;
		bool? living = null;
		int? integrity = null;
		Resource seed = null;
		List<ResourceRange> returnResources = new();
		List<ResourceRange> harvestResources = new();

		foreach (KeyValuePair<string, object> plantSubProperty in (List<KeyValuePair<string, object>>)plantProperty.Value) {
			switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantSubProperty.Key)) {
				case PlantPropertyEnum.Type:
					type = (Plant.PlantEnum)Enum.Parse(typeof(Plant.PlantEnum), (string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Living:
					living = bool.Parse((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Integrity:
					integrity = int.Parse((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Seed:
					seed = resourceQuery.GetResourceByString((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.ReturnResources:
					foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
						Resource resource = resourceQuery.GetResourceByString(resourceRangeString.Split(':')[0]);
						int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
						int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
						returnResources.Add(new ResourceRange(resource, min, max));
					}
					break;
				case PlantPropertyEnum.HarvestResources:
					foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
						Resource resource = resourceQuery.GetResourceByString(resourceRangeString.Split(':')[0]);
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

		System.Diagnostics.Debug.Assert(type != null, nameof(type) + " != null");
		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");
		System.Diagnostics.Debug.Assert(living != null, nameof(living) + " != null");
		System.Diagnostics.Debug.Assert(integrity != null, nameof(integrity) + " != null");

		return new PlantPrefab(
			type.Value,
			groupType.Value,
			living.Value,
			integrity.Value,
			seed,
			returnResources,
			harvestResources
		);
	}

	private void GrowPlants() {
		List<Plant> growPlants = new();
		foreach (Plant plant in Plant.smallPlants) {
			plant.growthProgress += 1;
			if (plant.growthProgress > SimulationDateTime.DayLengthSeconds * 4) {
				if (Random.Range(0, 100) < 0.01f * (plant.growthProgress / SimulationDateTime.DayLengthSeconds * 4)) {
					growPlants.Add(plant);
				}
			}
		}
		foreach (Plant plant in growPlants) {
			plant.Grow();
		}
	}

	private void LoadLocationNames() {
		locationNames.AddRange(Resources.Load<TextAsset>(@"Data/names-locations").text.Split('\n').Select(StringUtilities.RemoveNonAlphanumericChars).ToList());
	}

	public string GetRandomLocationName() {
		string[] filteredLocationNames = locationNames
			.Where(name => universeM.universe == null || name != universeM.universe.name)
			.Where(name => planetM.planet == null || name != planetM.planet.name)
			.Where(name => colonyM.colony == null || name != colonyM.colony.Name)
			.Where(name => caravanM.caravans.Find(c => c.location.Name == name) == null)
			.ToArray();

		return filteredLocationNames.RandomElement();
	}

	private int BitSumObjects(List<ObjectPrefab.ObjectEnum> compareObjectTypes, List<Tile> tileSurroundingTiles) {
		List<int> layers = new();
		foreach (Tile tile in tileSurroundingTiles) {
			if (tile != null) {
				foreach (KeyValuePair<int, ObjectInstance> kvp in tile.objectInstances) {
					if (!layers.Contains(kvp.Key)) {
						layers.Add(kvp.Key);
					}
				}
			}
		}
		layers.Sort();

		Dictionary<int, List<int>> layersSumTiles = new();
		foreach (int layer in layers) {
			List<int> layerSumTiles = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
			for (int i = 0; i < tileSurroundingTiles.Count; i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && Map.diagonalCheckMap.ContainsKey(i)) {
							List<Tile> surroundingHorizontalTiles = new() { tileSurroundingTiles[Map.diagonalCheckMap[i][0]], tileSurroundingTiles[Map.diagonalCheckMap[i][1]] };
							List<Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							layerSumTiles[i] = 1;
						}
					}
				} else {
					if (tileSurroundingTiles.Find(tile => tile != null && tileSurroundingTiles.IndexOf(tile) <= 3 && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
						layerSumTiles[i] = 1;
					} else {
						if (i <= 3) {
							layerSumTiles[i] = 1;
						} else {
							List<Tile> surroundingHorizontalTiles = new() { tileSurroundingTiles[Map.diagonalCheckMap[i][0]], tileSurroundingTiles[Map.diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer, layerSumTiles);
		}

		List<bool> sumTiles = new() { false, false, false, false, false, false, false, false };

		foreach (KeyValuePair<int, List<int>> layerSumTiles in layersSumTiles) {
			foreach (ObjectPrefab.ObjectEnum objectEnum in compareObjectTypes) {
				ObjectPrefab objectPrefab = ObjectPrefab.GetObjectPrefabByEnum(objectEnum);
				if (objectPrefab.layer == layerSumTiles.Key) {
					foreach (Tile tile in tileSurroundingTiles) {
						if (tile != null) {
							ObjectInstance objectInstance = tile.GetAllObjectInstances().Find(instances => instances.prefab == objectPrefab);
							if (objectInstance != null) {
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

	private void BitmaskObjects(
		ObjectInstance objectInstance,
		bool includeDiagonalSurroundingTiles,
		bool customBitSumInputs,
		bool compareEquivalentObjects,
		List<ObjectPrefab.ObjectEnum> customCompareObjectTypes
	) {
		List<Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles;

		int sum;
		if (customBitSumInputs) {
			sum = BitSumObjects(
				customCompareObjectTypes,
				surroundingTilesToUse
			);
		} else {
			if (compareEquivalentObjects) {
				if (objectInstance.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Walls) {
					sum = BitSumObjects(
						new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
						surroundingTilesToUse
					);
					// TODO Not-fully-working implementation of walls and stone connecting
					//sum += MapM.Map.BitSum(
					//	TileManager.TileTypeGroup.GetTileTypeGroupByEnum(TileManager.TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tileType => tileType.type).ToList(),
					//	new List<ObjectEnum>() { objectInstance.prefab.type },
					//	surroundingTilesToUse,
					//	true
					//);
				} else {
					sum = BitSumObjects(
						new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
						surroundingTilesToUse
					);
				}
			} else {
				sum = BitSumObjects(
					new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
					surroundingTilesToUse
				);
			}
		}

		SpriteRenderer spriteRenderer = objectInstance.obj.GetComponent<SpriteRenderer>();
		List<Sprite> sprites = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation);
		spriteRenderer.sprite = sum >= 16 ? sprites[Map.bitmaskMap[sum]] : sprites[sum];
	}

	public void Bitmask(List<Tile> tilesToBitmask) {
		foreach (Tile tile in tilesToBitmask) {
			if (tile != null && tile.GetAllObjectInstances().Count > 0) {
				foreach (ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
					if (objectInstance.prefab.bitmasking) {
						BitmaskObjects(
							objectInstance,
							true, // includeDiagonalSurroundingTiles -- default: true
							false, // customBitSumInput -- default: false
							true, // compareEquivalentObjects -- default: false
							null // customCompareObjectTypes -- default: null
						);
					} else {
						if (objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation).Count > 0) {
							if (objectInstance.prefab.jobType != "PlantFarm") {
								objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[objectInstance.rotationIndex];
							}
						} else {
							objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBaseSpriteForVariation(objectInstance.variation);
						}
					}
				}
			}
		}
	}
}
