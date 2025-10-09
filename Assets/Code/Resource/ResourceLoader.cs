using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NPersistence;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class ResourceLoader
	{
		private readonly IResourceQuery resourceQuery;

		public ResourceLoader(IResourceQuery resourceQuery) {
			this.resourceQuery = resourceQuery;

			CreateResources();
		}

		private enum ResourceGroupPropertyEnum
		{
			ResourceGroup,
			Type,
			Resources
		}

		private enum ResourcePropertyEnum
		{
			Resource,
			Type,
			Classes,
			Weight,
			Volume,
			Price,
			Food,
			Fuel,
			Crafting,
			Clothing
		}

		private enum ResourceFoodPropertyEnum
		{
			Nutrition
		}

		private enum ResourceFuelPropertyEnum
		{
			FuelEnergy
		}

		private enum ResourceCraftingPropertyEnum
		{
			Objects,
			CraftingEnergy,
			CraftingTime,
			Resources
		}

		private enum ResourceClothingPropertyEnum
		{
			Appearance,
			ClothingType,
			Insulation,
			WaterResistance,
			WeightCapacity,
			VolumeCapacity,
			Colours
		}

		public void CreateResources() {

			Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp = new();

			List<KeyValuePair<string, object>> resourceGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(UnityEngine.Resources.Load<TextAsset>(@"Data/resources").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> resourceGroupProperty in resourceGroupProperties) {
				switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupProperty.Key)) {
					case ResourceGroupPropertyEnum.ResourceGroup:

						ResourceGroup resourceGroup = ParseResourceGroup(craftingResourcesTemp, resourceGroupProperty);
						ResourceGroup.resourceGroups.Add(resourceGroup.type, resourceGroup);

						break;
					default:
						Debug.LogError("Unknown resource group property: " + resourceGroupProperty.Key + " " + resourceGroupProperty.Value);
						break;

				}
			}

			// Set Resource Classes
			foreach (Resource.ResourceClassEnum resourceClassEnum in Enum.GetValues(typeof(Resource.ResourceClassEnum))) {
				resourceQuery.ResourceClassToResources.Add(resourceClassEnum, new List<Resource>());
			}

			foreach (Resource resource in resourceQuery.GetResources()) {
				foreach (Resource.ResourceClassEnum resourceClassEnum in resource.classes) {
					resourceQuery.ResourceClassToResources[resourceClassEnum].Add(resource);
				}
			}

			// Set Crafting Resources
			foreach (KeyValuePair<EResource, List<(EResource resource, float amount)>> craftingResourceToResourceAmount in craftingResourcesTemp) {
				List<Resource> resourcesToApplyTo = new();
				Resource craftableResource = resourceQuery.GetResourceByEnum(craftingResourceToResourceAmount.Key);
				if (craftableResource.classes.Contains(Resource.ResourceClassEnum.Clothing)) {
					Clothing craftableClothing = (Clothing)craftableResource;
					foreach (Clothing resource in resourceQuery.GetResourcesInClass(Resource.ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.clothingType == craftableClothing.prefab.clothingType)) {
						resourcesToApplyTo.Add(resource);
					}
				} else {
					resourcesToApplyTo.Add(craftableResource);
				}
				foreach (Resource resource in resourcesToApplyTo) {
					foreach ((EResource resource, float amount) resourceAmount in craftingResourceToResourceAmount.Value) {
						float amount = resourceAmount.amount;
						if (amount is < 1 and > 0) {
							resource.amountCreated = Mathf.RoundToInt(1 / amount);
							resource.craftingResources.Add(new ResourceAmount(resourceQuery.GetResourceByEnum(resourceAmount.resource), 1));
						} else {
							resource.amountCreated = 1;
							resource.craftingResources.Add(new ResourceAmount(resourceQuery.GetResourceByEnum(resourceAmount.resource), Mathf.RoundToInt(resourceAmount.amount)));
						}
					}
				}
			}
		}

		private ResourceGroup ParseResourceGroup(
			Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp,
			KeyValuePair<string, object> resourceGroupProperty
		) {
			ResourceGroup.ResourceGroupEnum? groupType = null;
			List<Resource> resources = new();

			foreach (KeyValuePair<string, object> resourceGroupSubProperty in (List<KeyValuePair<string, object>>)resourceGroupProperty.Value) {
				switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupSubProperty.Key)) {
					case ResourceGroupPropertyEnum.Type:
						groupType = (ResourceGroup.ResourceGroupEnum)Enum.Parse(typeof(ResourceGroup.ResourceGroupEnum), (string)resourceGroupSubProperty.Value);
						break;
					case ResourceGroupPropertyEnum.Resources:
						foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)resourceGroupSubProperty.Value) {
							switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceProperty.Key)) {
								case ResourcePropertyEnum.Resource:

									System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

									Resource resource = ParseResource(craftingResourcesTemp, groupType.Value, resourceProperty);
									resources.Add(resource);
									resourceQuery.Resources.Add(resource.type, resource);

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

			System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

			return new ResourceGroup(
				groupType.Value,
				resources
			);
		}

		private Resource ParseResource(
			Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp,
			ResourceGroup.ResourceGroupEnum groupType,
			KeyValuePair<string, object> resourceProperty
		) {
			EResource? type = null;
			HashSet<Resource.ResourceClassEnum> classes = new();
			int? weight = null;
			int? volume = null;
			int? price = null;

			// Food
			int? foodNutrition = null;

			// Fuel
			int? fuelEnergy = 0;

			// Crafting
			Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects = new();
			int? craftingEnergy = 0;
			int? craftingTime = 0;
			// craftingResources -> craftingResourcesTemp

			// Clothing
			BodySection? clothingAppearance = null;
			Clothing.ClothingEnum? clothingType = null;
			int? clothingInsulation = null;
			int? clothingWaterResistance = null;
			int? clothingWeightCapacity = 0;
			int? clothingVolumeCapacity = 0;
			List<string> clothingColours = new();

			foreach (KeyValuePair<string, object> resourceSubProperty in (List<KeyValuePair<string, object>>)resourceProperty.Value) {
				switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceSubProperty.Key)) {
					case ResourcePropertyEnum.Type:
						type = (EResource)Enum.Parse(typeof(EResource), (string)resourceSubProperty.Value);
						break;
					case ResourcePropertyEnum.Classes:
						foreach (string classString in ((string)resourceSubProperty.Value).Split(',')) {
							classes.Add((Resource.ResourceClassEnum)Enum.Parse(typeof(Resource.ResourceClassEnum), classString));
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
					case ResourcePropertyEnum.Crafting:
						foreach (KeyValuePair<string, object> craftingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
							switch ((ResourceCraftingPropertyEnum)Enum.Parse(typeof(ResourceCraftingPropertyEnum), craftingProperty.Key)) {
								case ResourceCraftingPropertyEnum.Objects:
									foreach (string objectSubGroupString in ((string)craftingProperty.Value).Split(';')) {
										ObjectPrefabSubGroup.ObjectSubGroupEnum objectSubGroupEnum = (ObjectPrefabSubGroup.ObjectSubGroupEnum)Enum.Parse(typeof(ObjectPrefabSubGroup.ObjectSubGroupEnum), objectSubGroupString.Split(':')[0]);
										craftingObjects.Add(
											objectSubGroupEnum,
											null
										);
										if (objectSubGroupString.Split(':').Count() > 1) {
											craftingObjects[objectSubGroupEnum] = new List<ObjectPrefab.ObjectEnum>();
											foreach (string objectString in objectSubGroupString.Split(':')[1].Split(',')) {
												craftingObjects[objectSubGroupEnum].Add((ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), objectString));
											}
										}
									}
									break;
								case ResourceCraftingPropertyEnum.CraftingEnergy:
									craftingEnergy = int.Parse((string)craftingProperty.Value);
									break;
								case ResourceCraftingPropertyEnum.CraftingTime:
									craftingTime = int.Parse((string)craftingProperty.Value);
									break;
								case ResourceCraftingPropertyEnum.Resources:
									craftingResourcesTemp.Add(type.Value, new List<(EResource resource, float amount)>());
									foreach (string resourceAmountString in ((string)craftingProperty.Value).Split(',')) {
										float amount = float.Parse(resourceAmountString.Split(':')[1]);
										craftingResourcesTemp[type.Value]
											.Add(((EResource)Enum.Parse(typeof(EResource), resourceAmountString.Split(':')[0]), amount));
									}
									break;
								default:
									Debug.LogError("Unknown resource crafting property: " + craftingProperty.Key + " " + craftingProperty.Value);
									break;
							}
						}
						break;
					case ResourcePropertyEnum.Clothing:
						foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
							switch ((ResourceClothingPropertyEnum)Enum.Parse(typeof(ResourceClothingPropertyEnum), clothingProperty.Key)) {
								case ResourceClothingPropertyEnum.Appearance:
									clothingAppearance = (BodySection)Enum.Parse(typeof(BodySection), (string)clothingProperty.Value);
									break;
								case ResourceClothingPropertyEnum.ClothingType:
									clothingType = (Clothing.ClothingEnum)Enum.Parse(typeof(Clothing.ClothingEnum), (string)clothingProperty.Value);
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
										clothingColours.Add(StringUtilities.RemoveNonAlphanumericChars(colourString));
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

			if (classes.Contains(Resource.ResourceClassEnum.Food)) {
				return new Food(
					type.Value,
					groupType,
					classes,
					weight.Value,
					volume.Value,
					price.Value,
					fuelEnergy.Value,
					craftingObjects,
					craftingEnergy.Value,
					craftingTime.Value,
					foodNutrition.Value
				);
			}
			if (classes.Contains(Resource.ResourceClassEnum.Clothing)) {
				ClothingPrefab clothingPrefab = new(
					clothingAppearance.Value,
					clothingType.Value,
					clothingInsulation.Value,
					clothingWaterResistance.Value,
					clothingWeightCapacity.Value,
					clothingVolumeCapacity.Value,
					clothingColours,
					ClothingPrefab.clothingPrefabs.Where(c => c.BodySection == clothingAppearance.Value).Sum(c => c.colours.Count)
				);
				ClothingPrefab.clothingPrefabs.Add(clothingPrefab);
				return new Clothing( // TODO This will only return a colourless clothing resource, need to move Clothing to a ResourceInstance system for colours
					(EResource)Enum.Parse(typeof(EResource), clothingType.Value.ToString()),
					groupType,
					classes,
					weight.Value,
					volume.Value,
					price.Value,
					fuelEnergy.Value,
					craftingObjects,
					craftingEnergy.Value,
					craftingTime.Value,
					clothingPrefab,
					string.Empty // TODO Fix this too
				);
			}
			Resource resource = new(
				type.Value,
				groupType,
				classes,
				weight.Value,
				volume.Value,
				price.Value,
				fuelEnergy.Value,
				craftingObjects,
				craftingEnergy.Value,
				craftingTime.Value
			);
			return resource;
		}
	}
}
