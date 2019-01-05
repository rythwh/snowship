using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobManager : BaseManager {

	public override void Awake() {
		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	private bool changedJobList = false;
	private int rotationIndex = 0;

	public override void Update() {
		if (changedJobList) {
			UpdateColonistJobs();
			GameManager.uiM.SetJobElements();
			changedJobList = false;
		}
		GetJobSelectionArea();
		UpdateSelectedPrefabInfo();
	}

	public List<Job> jobs = new List<Job>();

	public class Job {

		public TileManager.Tile tile;
		public ResourceManager.ObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;
		public GameObject priorityIndicator;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public UIManager.JobElement jobUIElement;

		public ResourceManager.Resource createResource;
		public ResourceManager.ObjectInstance activeTileObject;

		public int priority;

		public List<ResourceManager.ResourceAmount> transferResources;

		public Job(TileManager.Tile tile, ResourceManager.ObjectPrefab prefab, int rotationIndex) {

			this.tile = tile;
			this.prefab = prefab;

			resourcesToBuild.AddRange(prefab.resourcesToBuild);

			this.rotationIndex = rotationIndex;

			jobPreview = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, tile.obj.transform, false);
			jobPreview.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			jobPreview.name = "JobPreview: " + prefab.name + " at " + jobPreview.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
			}
			if (!prefab.bitmasking && prefab.bitmaskSprites.Count > 0) {
				jPSR.sprite = prefab.bitmaskSprites[rotationIndex];
			}
			jPSR.sortingOrder = 5 + prefab.layer; // Job Preview Sprite
			jPSR.color = new Color(1f, 1f, 1f, 0.25f);

			jobProgress = prefab.timeToBuild;
			colonistBuildTime = prefab.timeToBuild;
		}

		public void SetCreateResourceData(ResourceManager.Resource createResource, ResourceManager.ManufacturingObject manufacturingTileObject) {
			this.createResource = createResource;
			resourcesToBuild.AddRange(createResource.manufacturingResources);
			if (manufacturingTileObject.fuelResource != null) {
				resourcesToBuild.Add(new ResourceManager.ResourceAmount(manufacturingTileObject.fuelResource, manufacturingTileObject.fuelResourcesRequired));
			}
			activeTileObject = manufacturingTileObject;
		}

		public void SetColonist(ColonistManager.Colonist colonist) {
			this.colonist = colonist;
			if (prefab.jobType != JobTypesEnum.PickupResources && containerPickups != null && containerPickups.Count > 0) {
				colonist.storedJob = this;
				colonist.SetJob(new ColonistJob(colonist, new Job(containerPickups[0].container.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.PickupResources), 0), null, null));
			}
		}

		public void ChangePriority(int amount) {
			priority += amount;
			if (priorityIndicator == null && jobPreview != null) {
				priorityIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, jobPreview.transform, false);
				priorityIndicator.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(@"UI/priorityIndicator");
				priorityIndicator.GetComponent<SpriteRenderer>().sortingOrder = jobPreview.GetComponent<SpriteRenderer>().sortingOrder + 1; // Priority Indicator Sprite
				if (priority == 1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = UIManager.GetColour(UIManager.Colours.LightYellow);
				} else if (priority == -1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = UIManager.GetColour(UIManager.Colours.LightRed);
				}
			}
			if (priority == 0) {
				MonoBehaviour.Destroy(priorityIndicator);
			}
		}

		public void Remove() {
			MonoBehaviour.Destroy(jobPreview);
		}
	}

	public enum JobTypesEnum {
		Build, Remove,
		ChopPlant, PlantPlant, Mine, Dig, PlantFarm, HarvestFarm,
		CreateResource, PickupResources, TransferResources, CollectResources, EmptyInventory, Cancel, IncreasePriority, DecreasePriority,
		CollectFood, Eat, CollectWater, Drink, Sleep
	};

	public static readonly List<JobTypesEnum> nonReturnableJobs = new List<JobTypesEnum>() {
		JobTypesEnum.PickupResources, JobTypesEnum.EmptyInventory, JobTypesEnum.Cancel, JobTypesEnum.IncreasePriority, JobTypesEnum.DecreasePriority,
		JobTypesEnum.CollectFood, JobTypesEnum.CollectWater, JobTypesEnum.Drink, JobTypesEnum.Sleep
	};

	private static readonly Dictionary<JobTypesEnum, Func<Job, string>> jobDescriptionFunctions = new Dictionary<JobTypesEnum, Func<Job, string>>() {
		{ JobTypesEnum.Build, delegate (Job job) {
			return "Building a " + job.prefab.name + ".";
		} },
		{ JobTypesEnum.Remove, delegate (Job job) {
			return "Removing a " + job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.name + ".";
		} },
		{ JobTypesEnum.ChopPlant, delegate (Job job) {
			return "Chopping down a " + job.tile.plant.prefab.name + ".";
		} },
		{ JobTypesEnum.PlantPlant, delegate (Job job) {
			return "Planting a plant.";
		} },
		{ JobTypesEnum.Mine, delegate (Job job) {
			return "Mining " + job.tile.tileType.name + ".";
		} },
		{ JobTypesEnum.Dig, delegate (Job job) {
			return "Digging " + string.Join(" and ", job.tile.tileType.resourceRanges.Select(rr => rr.resource.name).ToArray());
		} },
		{ JobTypesEnum.PlantFarm, delegate (Job job) {
			return "Planting a " + job.prefab.name + ".";
		} },
		{ JobTypesEnum.HarvestFarm, delegate (Job job) {
			return "Harvesting a farm of " + job.tile.farm.name + ".";
		} },
		{ JobTypesEnum.CreateResource, delegate (Job job) {
			return "Creating " + job.createResource.name + ".";
		} },
		{ JobTypesEnum.PickupResources, delegate (Job job) {
			return "Picking up some resources.";
		} },
		{ JobTypesEnum.TransferResources, delegate (Job job) {
			return "Transferring resources.";
		} },
		{ JobTypesEnum.CollectResources, delegate (Job job) {
			return "Collecting resources.";
		} },
		{ JobTypesEnum.EmptyInventory, delegate (Job job) {
			return "Emptying their inventory.";
		} },
		{ JobTypesEnum.CollectFood, delegate (Job job) {
			return "Finding some food to eat.";
		} },
		{ JobTypesEnum.Eat, delegate (Job job) {
			return "Eating.";
		} },
		{ JobTypesEnum.CollectWater, delegate (Job job) {
			return "Finding something to drink.";
		} },
		{ JobTypesEnum.Drink, delegate (Job job) {
			return "Drinking.";
		} },
		{ JobTypesEnum.Sleep, delegate (Job job) {
			return "Sleeping.";
		} }
	};

	public string GetJobDescription(Job job) {
		if (job != null) {
			if (jobDescriptionFunctions.ContainsKey(job.prefab.jobType)) {
				return jobDescriptionFunctions[job.prefab.jobType](job);
			} else {
				return "Doing something.";
			}
		} else {
			return "Wandering around.";
		}
	}

	public static readonly Dictionary<JobTypesEnum, Action<ColonistManager.Colonist, Job>> finishJobFunctions = new Dictionary<JobTypesEnum, Action<ColonistManager.Colonist, Job>>() {
		{ JobTypesEnum.Build, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
			}
		} },
		{ JobTypesEnum.Remove, delegate (ColonistManager.Colonist colonist, Job job) {
			bool previousWalkability = job.tile.walkable;
			ResourceManager.ObjectInstance instance = job.tile.GetObjectInstanceAtLayer(job.prefab.layer);
			if (instance == null) {
				Debug.LogError("Instance being removed at layer " + job.prefab.layer + " is null.");
			}
			foreach (ResourceManager.ResourceAmount resourceAmount in instance.prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource, Mathf.RoundToInt(resourceAmount.amount / 2f), false);
			}
			if (instance is ResourceManager.Farm) {
				ResourceManager.Farm farm = (ResourceManager.Farm)instance;
				if (farm.growProgressSpriteIndex == 0) {
					job.colonist.inventory.ChangeResourceAmount(farm.prefab.seedResource, 1, false);
				}
			} else if (instance is ResourceManager.Container) {
				ResourceManager.Container container = (ResourceManager.Container)instance;
				List<ResourceManager.ResourceAmount> nonReservedResourcesToRemove = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources) {
					nonReservedResourcesToRemove.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
					colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
				}
				foreach (ResourceManager.ResourceAmount resourceAmount in nonReservedResourcesToRemove) {
					container.inventory.ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
				}
				List<ResourceManager.ReservedResources> reservedResourcesToRemove = new List<ResourceManager.ReservedResources>();
				foreach (ResourceManager.ReservedResources reservedResources in container.inventory.reservedResources) {
					foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
						colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
					}
					reservedResourcesToRemove.Add(reservedResources);
					if (reservedResources.human is ColonistManager.Colonist) {
						((ColonistManager.Colonist)reservedResources.human).ReturnJob();
					}
				}
				foreach (ResourceManager.ReservedResources reservedResourceToRemove in reservedResourcesToRemove) {
					container.inventory.reservedResources.Remove(reservedResourceToRemove);
				}
				GameManager.uiM.SetSelectedColonistInformation(true);
				GameManager.uiM.SetSelectedContainerInfo();
				GameManager.uiM.SetSelectedTradingPostInfo();
			} else if (instance is ResourceManager.ManufacturingObject) {
				ResourceManager.ManufacturingObject manufacturingTileObject = (ResourceManager.ManufacturingObject)instance;
				foreach (Job removeJob in manufacturingTileObject.jobBacklog) {
					job.jobUIElement.Remove();
					job.Remove();
					GameManager.jobM.jobs.Remove(job);
				}
				manufacturingTileObject.jobBacklog.Clear();
			} else if (instance is ResourceManager.SleepSpot) {
				ResourceManager.SleepSpot sleepSpot = (ResourceManager.SleepSpot)instance;
				if (sleepSpot.occupyingColonist != null) {
					sleepSpot.occupyingColonist.ReturnJob();
				}
			}
			GameManager.resourceM.RemoveTileObjectInstance(instance);
			job.tile.RemoveTileObjectAtLayer(instance.prefab.layer);
			GameManager.resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
			if (job.tile.walkable && !previousWalkability) {
				GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(job.tile);
			}
		} },
		{ JobTypesEnum.PlantFarm, delegate (ColonistManager.Colonist colonist, Job job) {
			JobManager.finishJobFunctions[JobTypesEnum.Build](colonist, job);
			if (job.tile.tileType.classes[TileManager.TileTypeClassEnum.Dirt]) {
				job.tile.SetTileType(GameManager.tileM.GetTileTypeByEnum(TileManager.TileTypeEnum.Mud), true, false, false, false);
			}
		} },
		{ JobTypesEnum.HarvestFarm, delegate (ColonistManager.Colonist colonist, Job job) {
			if (job.tile.farm != null) {
				colonist.inventory.ChangeResourceAmount(job.tile.farm.prefab.seedResource, UnityEngine.Random.Range(1, 3), false);
				colonist.inventory.ChangeResourceAmount(job.tile.farm.prefab.harvestResource, UnityEngine.Random.Range(1, 6), false);

				GameManager.jobM.CreateJob(new Job(job.tile, job.tile.farm.prefab, 0));

				GameManager.resourceM.RemoveTileObjectInstance(job.tile.farm);
				job.tile.RemoveTileObjectAtLayer(job.tile.farm.prefab.layer);
			}
			GameManager.resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
		} },
		{ JobTypesEnum.ChopPlant, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceRange resourceRange in job.tile.plant.prefab.returnResources) {
				colonist.inventory.ChangeResourceAmount(resourceRange.resource, Mathf.CeilToInt(UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
			}
			if (job.tile.plant.harvestResource != null) {
				ResourceManager.ResourceRange harvestResourceRange = job.tile.plant.prefab.harvestResources.Find(rr => rr.resource == job.tile.plant.harvestResource);
				colonist.inventory.ChangeResourceAmount(job.tile.plant.harvestResource, Mathf.CeilToInt(UnityEngine.Random.Range(harvestResourceRange.min, harvestResourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
			}
			job.tile.SetPlant(true, null);
		} },
		{ JobTypesEnum.PlantPlant, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount ra in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(ra.resource, -ra.amount, false);
			}
			Dictionary<ResourceManager.PlantPrefab, int> plantChances = new Dictionary<ResourceManager.PlantPrefab, int>();
			foreach (ResourceManager.PlantPrefab plantPrefab in job.prefab.plants.Keys) {
				plantChances.Add(plantPrefab, UnityEngine.Random.Range(0, 100));
			}
			ResourceManager.PlantPrefab chosenPlantPrefab = plantChances.OrderByDescending(kvp => kvp.Value).First().Key;
			job.tile.SetPlant(false, new ResourceManager.Plant(chosenPlantPrefab, job.tile, true, false, job.prefab.plants[chosenPlantPrefab]));
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime);
		} },
		{ JobTypesEnum.Mine, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceRange resourceRange in job.tile.tileType.resourceRanges) {
				colonist.inventory.ChangeResourceAmount(resourceRange.resource, UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			job.tile.SetTileType(GameManager.tileM.GetTileTypeByEnum(TileManager.TileTypeEnum.Dirt), true, true, true, false);
			GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(job.tile);
			foreach (ResourceManager.LightSource lightSource in GameManager.resourceM.lightSources) {
				if (Vector2.Distance(job.tile.obj.transform.position, lightSource.obj.transform.position) <= lightSource.prefab.maxLightDistance) {
					lightSource.RemoveTileBrightnesses();
					lightSource.SetTileBrightnesses();
				}
			}
		} },
		{ JobTypesEnum.Dig, delegate (ColonistManager.Colonist colonist, Job job) {
			job.tile.dugPreviously = true;
			foreach (ResourceManager.ResourceRange resourceRange in job.tile.tileType.resourceRanges) {
				colonist.inventory.ChangeResourceAmount(resourceRange.resource, UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			bool setToWater = job.tile.tileType.groupType == TileManager.TileTypeGroupEnum.Water;
			if (!setToWater) {
				foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroupEnum.Water) {
						setToWater = true;
						break;
					}
				}
			}
			if (setToWater) {
				job.tile.SetTileType(job.tile.biome.tileTypes[TileManager.TileTypeGroupEnum.Water], true, true, true, false);
				foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroupEnum.Hole) {
						List<TileManager.Tile> frontier = new List<TileManager.Tile>() { nTile };
						List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>() { };
						TileManager.Tile currentTile = nTile;
						while (frontier.Count > 0) {
							currentTile = frontier[0];
							frontier.RemoveAt(0);
							checkedTiles.Add(currentTile);
							currentTile.SetTileType(currentTile.biome.tileTypes[TileManager.TileTypeGroupEnum.Water], true, true, true, true);
							foreach (TileManager.Tile nTile2 in currentTile.horizontalSurroundingTiles) {
								if (nTile2 != null && nTile2.tileType.groupType == TileManager.TileTypeGroupEnum.Hole && !checkedTiles.Contains(nTile2)) {
									frontier.Add(nTile2);
								}
							}
						}
					}
				}
			} else {
				job.tile.SetTileType(job.tile.biome.tileTypes[TileManager.TileTypeGroupEnum.Hole], true, true, true, false);
			}
		} },
		{ JobTypesEnum.CreateResource, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
			}
			colonist.inventory.ChangeResourceAmount(job.createResource, job.createResource.amountCreated, false);
			if (job.activeTileObject is ResourceManager.ManufacturingObject) {
				((ResourceManager.ManufacturingObject)job.activeTileObject).jobBacklog.Remove(job);
			}
		} },
		{ JobTypesEnum.PickupResources, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null && colonist.storedJob != null) {
				ContainerPickup containerPickup = colonist.storedJob.containerPickups.Find(pickup => pickup.container == container);
				if (containerPickup != null) {
					foreach (ResourceManager.ReservedResources rr in containerPickup.container.inventory.TakeReservedResources(colonist)) {
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							colonist.inventory.ChangeResourceAmount(ra.resource, ra.amount, false);
						}
					}
					colonist.storedJob.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.storedJob != null) {
				if (colonist.storedJob.containerPickups.Count <= 0) {
					colonist.SetJob(new ColonistJob(colonist, colonist.storedJob, colonist.storedJob.colonistResources, null));
					colonist.storedJob = null;
				} else {
					colonist.SetJob(new ColonistJob(colonist, new Job(colonist.storedJob.containerPickups[0].container.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.PickupResources), 0), colonist.storedJob.colonistResources, colonist.storedJob.containerPickups), false);
				}
			}
		} },
		{ JobTypesEnum.TransferResources, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				ResourceManager.Inventory.TransferResourcesBetweenInventories(colonist.inventory, container.inventory, job.resourcesToBuild, true);
			}
		} },
		{ JobTypesEnum.CollectResources, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				foreach (ResourceManager.ReservedResources rr in container.inventory.TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.inventory.ChangeResourceAmount(ra.resource, ra.amount, false);
					}
				}
			}
		} },
		{ JobTypesEnum.EmptyInventory, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				ResourceManager.Inventory.TransferResourcesBetweenInventories(colonist.inventory, container.inventory, colonist.inventory.resources, true);
			}
		} },
		{ JobTypesEnum.CollectFood, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				foreach (ResourceManager.ReservedResources rr in container.inventory.TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.inventory.ChangeResourceAmount(ra.resource, ra.amount, false);
					}
				}
			}
			colonist.SetJob(new ColonistJob(colonist, new Job(colonist.overTile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Eat), 0), null, null));
		} },
		{ JobTypesEnum.Eat, delegate (ColonistManager.Colonist colonist, Job job) {
			List<ResourceManager.ResourceAmount> resourcesToEat = colonist.inventory.resources.Where(r => r.resource.classes.Contains(ResourceManager.ResourceClassEnum.Food)).OrderBy(r => ((ResourceManager.Food)r.resource).nutrition).ToList();
			ColonistManager.NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ColonistManager.NeedsEnum.Food);
			float startingFoodNeedValue = foodNeed.GetValue();
			foreach (ResourceManager.ResourceAmount ra in resourcesToEat) {
				bool stopEating = false;
				for (int i = 0; i < ra.amount; i++) {
					if (foodNeed.GetValue() <= 0) {
						stopEating = true;
						break;
					}
					foodNeed.ChangeValue(-((ResourceManager.Food)ra.resource).nutrition);
					colonist.inventory.ChangeResourceAmount(ra.resource, -1, false);
					if (ra.resource.type == ResourceManager.ResourceEnum.Apple || ra.resource.type == ResourceManager.ResourceEnum.BakedApple) {
						colonist.inventory.ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.AppleSeed), UnityEngine.Random.Range(1, 5), false);
					}
				}
				if (stopEating) {
					break;
				}
			}
			float amountEaten = startingFoodNeedValue - foodNeed.GetValue();
			if (amountEaten >= 15 && foodNeed.GetValue() <= -10) {
				colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Stuffed);
			} else if (amountEaten >= 15) {
				colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Full);
			}
			if (foodNeed.GetValue() < 0) {
				foodNeed.SetValue(0);
			}
		} },
		{ JobTypesEnum.Sleep, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.SleepSpot targetSleepSpot = GameManager.resourceM.sleepSpots.Find(sleepSpot => sleepSpot.tile == job.tile);
			if (targetSleepSpot != null) {
				targetSleepSpot.StopSleeping();
				if (targetSleepSpot.prefab.restComfortAmount >= 10) {
					colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Rested);
				}
			}
			foreach (ResourceManager.SleepSpot sleepSpot in GameManager.resourceM.sleepSpots) {
				if (sleepSpot.occupyingColonist == colonist) {
					sleepSpot.StopSleeping();
				}
			}
		} }
	};

	private ResourceManager.ObjectPrefab selectedPrefab;

	public void SetSelectedPrefab(ResourceManager.ObjectPrefab newSelectedPrefab) {
		if (newSelectedPrefab != selectedPrefab) {
			if (newSelectedPrefab != null) {
				selectedPrefab = newSelectedPrefab;
				rotationIndex = 0;
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
				}
			} else {
				selectedPrefab = null;
			}
		}
	}

	public ResourceManager.ObjectPrefab GetSelectedPrefab() {
		return selectedPrefab;
	}

	private GameObject selectedPrefabPreview;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
		selectedPrefabPreview.transform.position = tile.obj.transform.position + (Vector3)selectedPrefab.anchorPositionOffset[rotationIndex];
	}

	public void UpdateSelectedPrefabInfo() {
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
					if (selectedPrefab.canRotate) {
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.bitmaskSprites[rotationIndex];
					}
					GameManager.uiM.SelectionSizeCanvasSetActive(false);
				}
				SelectedPrefabPreview();
				if (Input.GetKeyDown(KeyCode.R)) {
					if (selectedPrefab.canRotate) {
						rotationIndex += 1;
						if (rotationIndex >= selectedPrefab.bitmaskSprites.Count) {
							rotationIndex = 0;
						}
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.bitmaskSprites[rotationIndex];
					}
				}
			} else {
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(false);
				}
				GameManager.uiM.SelectionSizeCanvasSetActive(true);
			}
		} else {
			selectedPrefabPreview.SetActive(false);
			GameManager.uiM.SelectionSizeCanvasSetActive(false);
		}
	}

	public enum SelectionModifiersEnum {
		Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms,
		ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, LivingTreeOrBushBiomes, CactusBiomes, OmitObjectInstancesOnAdditionalTiles
	};

	private static readonly Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ResourceManager.ObjectPrefab, bool>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ResourceManager.ObjectPrefab, bool>>() {
		{ SelectionModifiersEnum.Walkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.walkable;
		} },
		{ SelectionModifiersEnum.OmitWalkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return !posTile.walkable;
		} },
		{ SelectionModifiersEnum.Buildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.buildable;
		} },
		{ SelectionModifiersEnum.OmitBuildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return !posTile.buildable;
		} },
		{ SelectionModifiersEnum.StoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType == TileManager.TileTypeGroupEnum.Stone;
		} },
		{ SelectionModifiersEnum.OmitStoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType != TileManager.TileTypeGroupEnum.Stone;
		} },
		{ SelectionModifiersEnum.AllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType == TileManager.TileTypeGroupEnum.Water;
		} },
		{ SelectionModifiersEnum.OmitAllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType != TileManager.TileTypeGroupEnum.Water;
		} },
		{ SelectionModifiersEnum.LiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.classes[TileManager.TileTypeClassEnum.LiquidWater];
		} },
		{ SelectionModifiersEnum.OmitLiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return !posTile.tileType.classes[TileManager.TileTypeClassEnum.LiquidWater];
		} },
		{ SelectionModifiersEnum.OmitNonStoneAndWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType != TileManager.TileTypeGroupEnum.Water && posTile.tileType.groupType != TileManager.TileTypeGroupEnum.Stone;
		} },
		{ SelectionModifiersEnum.Plants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.plant != null;
		} },
		{ SelectionModifiersEnum.OmitPlants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.plant == null;
		} },
		{ SelectionModifiersEnum.OmitSameLayerJobs, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			foreach (Job job in GameManager.jobM.jobs) {
				if (job.prefab.layer == prefab.layer) {
					if (job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in job.prefab.multiTilePositions[job.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(job.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				if (colonist.job != null && colonist.job.prefab.layer == prefab.layer) {
					if (colonist.job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.job.prefab.multiTilePositions[colonist.job.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(colonist.job.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				if (colonist.storedJob != null && colonist.storedJob.prefab.layer == prefab.layer) {
					if (colonist.storedJob.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.storedJob.prefab.multiTilePositions[colonist.storedJob.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(colonist.storedJob.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			return true;
		} },
		{ SelectionModifiersEnum.OmitSameLayerObjectInstances, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return !posTile.objectInstances.ContainsKey(prefab.layer) || posTile.objectInstances[prefab.layer] == null;
		} },
		{ SelectionModifiersEnum.Farms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.farm != null;
		} },
		{ SelectionModifiersEnum.OmitFarms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.farm == null;
		} },
		{ SelectionModifiersEnum.ObjectsAtSameLayer, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.GetObjectInstanceAtLayer(prefab.layer) != null;
		} },
		{ SelectionModifiersEnum.Objects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.GetAllObjectInstances().Count > 0;
		} },
		{ SelectionModifiersEnum.OmitObjects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.GetAllObjectInstances().Count <= 0;
		} },
		{ SelectionModifiersEnum.OmitNonCoastWater, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			if (posTile.tileType.groupType == TileManager.TileTypeGroupEnum.Water) {
				if (!(posTile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileManager.TileTypeGroupEnum.Water) != null)) {
					return false;
				}
			}
			return true;
		} },
		{ SelectionModifiersEnum.OmitHoles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return posTile.tileType.groupType != TileManager.TileTypeGroupEnum.Hole;
		} },
		{ SelectionModifiersEnum.OmitPreviousDig, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			return !posTile.dugPreviously;
		} },
		{ SelectionModifiersEnum.LivingTreeOrBushBiomes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			foreach (ResourceManager.PlantEnum plantEnum in posTile.biome.plantChances.Keys) {
				ResourceManager.PlantPrefab plantPrefab = GameManager.resourceM.GetPlantPrefabByEnum(plantEnum);
				if (plantPrefab.living && plantPrefab.groupType == ResourceManager.PlantGroupEnum.Tree || plantPrefab.groupType == ResourceManager.PlantGroupEnum.Bush) {
					return true;
				}
			}
			return false;
		} },
		{ SelectionModifiersEnum.CactusBiomes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			foreach (ResourceManager.PlantEnum plantEnum in posTile.biome.plantChances.Keys) {
				ResourceManager.PlantPrefab plantPrefab = GameManager.resourceM.GetPlantPrefabByEnum(plantEnum);
				if (plantPrefab.groupType == ResourceManager.PlantGroupEnum.Cactus) {
					return true;
				}
			}
			return false;
		} },
		{ SelectionModifiersEnum.OmitObjectInstancesOnAdditionalTiles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab) {
			ResourceManager.ObjectInstance tileObjectInstance = posTile.GetObjectInstanceAtLayer(prefab.layer);
			if (tileObjectInstance != null && tileObjectInstance.tile != posTile) {
				return false;
			}
			return true;
		} }
	};

	private List<GameObject> selectionIndicators = new List<GameObject>();

	public TileManager.Tile firstTile;
	private bool stopSelection;

	public void StopSelection() {
		stopSelection = true;
	}

	private bool enableSelectionPreview = true;

	public void GetJobSelectionArea() {

		enableSelectionPreview = true;

		foreach (GameObject selectionIndicator in selectionIndicators) {
			MonoBehaviour.Destroy(selectionIndicator);
		}
		selectionIndicators.Clear();

		if (selectedPrefab != null) {
			Vector2 mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			if (Input.GetMouseButtonDown(0) && !GameManager.uiM.IsPointerOverUI()) {
				firstTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
				if (secondTile != null) {

					enableSelectionPreview = false;

					float smallerY = Mathf.Min(firstTile.obj.transform.position.y, secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y, secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x, secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x, secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					float maxY = largerY + 1;
					float maxX = largerX + 1;

					bool addedToSelectionArea = false;
					for (float y = smallerY; y < maxY; y += (addedToSelectionArea ? selectedPrefab.dimensions[rotationIndex].y : 1)) {
						//addedToSelectionArea = false;
						for (float x = smallerX; x < maxX; x += (addedToSelectionArea ? selectedPrefab.dimensions[rotationIndex].x : 1)) {
							addedToSelectionArea = true; // default = false // Try swapping x and y values when the object is rotated vertically (i.e. rotationIndex == 1 || 3).
							TileManager.Tile tile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(x, y));
							bool addTile = true;
							bool addOutlineTile = true;
							if (selectedPrefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
								addOutlineTile = (x == smallerX || y == smallerY || x == largerX || y == largerY);
							}
							foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.selectionModifiers) {
								if (selectionModifier != SelectionModifiersEnum.Outline) {
									foreach (Vector2 multiTilePosition in selectedPrefab.multiTilePositions[rotationIndex]) {
										Vector2 actualMultiTilePosition = tile.obj.transform.position + (Vector3)multiTilePosition;
										if (actualMultiTilePosition.x >= 0 && actualMultiTilePosition.x < GameManager.colonyM.colony.map.mapData.mapSize && actualMultiTilePosition.y >= 0 && actualMultiTilePosition.y < GameManager.colonyM.colony.map.mapData.mapSize) {
											TileManager.Tile posTile = GameManager.colonyM.colony.map.GetTileFromPosition(actualMultiTilePosition);
											addTile = selectionModifierFunctions[selectionModifier](tile, posTile, selectedPrefab);
											if (!addTile) {
												break;
											}
										} else {
											addTile = false;
											break;
										}
									}
									if (!addTile) {
										break;
									}
								}
							}
							if (addTile && addOutlineTile) {
								selectionArea.Add(tile);
								addedToSelectionArea = true;

								GameObject selectionIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, tile.obj.transform, false);
								selectionIndicator.name = "Selection Indicator";
								SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
								sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
								sISR.sortingOrder = 20; // Selection Indicator Sprite
								selectionIndicators.Add(selectionIndicator);
							}
						}
					}

					GameManager.uiM.UpdateSelectionSizePanel(smallerX - maxX, smallerY - maxY, selectionArea.Count, selectedPrefab);

					if (Input.GetMouseButtonUp(0)) {
						if (selectedPrefab.jobType == JobTypesEnum.Cancel) {
							CancelJobsInSelectionArea(selectionArea);
						} else if (selectedPrefab.jobType == JobTypesEnum.IncreasePriority) {
							ChangeJobPriorityInSelectionArea(selectionArea, 1);
						} else if (selectedPrefab.jobType == JobTypesEnum.DecreasePriority) {
							ChangeJobPriorityInSelectionArea(selectionArea, -1);
						} else {
							CreateJobsInSelectionArea(selectedPrefab, selectionArea);
						}
						firstTile = null;
					}
				}
			}
		}
	}

	public void CancelJobsInSelectionArea(List<TileManager.Tile> selectionArea) {
		List<Job> removeJobs = new List<Job>();
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				removeJobs.Add(job);
			}
		}
		foreach (Job job in removeJobs) {
			if (job.prefab.jobType == JobTypesEnum.CreateResource) {
				((ResourceManager.ManufacturingObject)job.activeTileObject).jobBacklog.Remove(job);
			}
			job.jobUIElement.Remove();
			job.Remove();
			jobs.Remove(job);
		}
		removeJobs.Clear();

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (!((colonist.job != null && selectionArea.Contains(colonist.job.tile)) || (colonist.storedJob != null && selectionArea.Contains(colonist.storedJob.tile)))) {
				continue;
			}

			if (colonist.storedJob != null) {
				if (colonist.storedJob.containerPickups != null) {
					foreach (ContainerPickup containerPickup in colonist.storedJob.containerPickups) {
						containerPickup.container.inventory.ReleaseReservedResources(colonist);
					}
				}
				if (colonist.storedJob.prefab.jobType == JobTypesEnum.CreateResource) {
					((ResourceManager.ManufacturingObject)colonist.storedJob.activeTileObject).jobBacklog.Remove(colonist.storedJob);
				}
				if (colonist.storedJob.jobUIElement != null) {
					colonist.storedJob.jobUIElement.Remove();
				}
				colonist.storedJob.Remove();
				colonist.storedJob = null;
			}

			if (colonist.job != null) {
				if (colonist.job.containerPickups != null) {
					foreach (ContainerPickup containerPickup in colonist.job.containerPickups) {
						containerPickup.container.inventory.ReleaseReservedResources(colonist);
					}
				}
				if (colonist.job.prefab.jobType == JobTypesEnum.CreateResource) {
					((ResourceManager.ManufacturingObject)colonist.job.activeTileObject).jobBacklog.Remove(colonist.job);
				}
				if (colonist.job.jobUIElement != null) {
					colonist.job.jobUIElement.Remove();
				}
				colonist.job.Remove();
				colonist.job = null;
				colonist.path.Clear();
				colonist.MoveToClosestWalkableTile(false);
			}
		}

		UpdateColonistJobs();
		GameManager.uiM.SetJobElements();
	}

	public void ChangeJobPriorityInSelectionArea(List<TileManager.Tile> selectionArea, int amount) {
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				job.ChangePriority(amount);
			}
		}
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (colonist.job != null) {
				if (selectionArea.Contains(colonist.job.tile)) {
					colonist.job.ChangePriority(amount);
				}
			}
			if (colonist.storedJob != null) {
				if (selectionArea.Contains(colonist.storedJob.tile)) {
					colonist.job.ChangePriority(amount);
				}
			}
		}
		UpdateColonistJobs();
		UpdateAllColonistJobCosts();
		GameManager.uiM.SetJobElements();
	}

	private static readonly Dictionary<int, ResourceManager.ObjectEnum> removeLayerMap = new Dictionary<int, ResourceManager.ObjectEnum>() {
		{ 1, ResourceManager.ObjectEnum.RemoveFloor },
		{ 2, ResourceManager.ObjectEnum.RemoveObject }
	};

	public void CreateJobsInSelectionArea(ResourceManager.ObjectPrefab prefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			if (selectedPrefab.type == ResourceManager.ObjectEnum.RemoveAll) {
				foreach (ResourceManager.ObjectInstance instance in tile.GetAllObjectInstances()) {
					if (removeLayerMap.ContainsKey(instance.prefab.layer) && !JobOfPrefabTypeExistsAtTile(removeLayerMap[instance.prefab.layer], instance.tile)) {
						ResourceManager.ObjectPrefab selectedRemovePrefab = GameManager.resourceM.GetObjectPrefabByEnum(removeLayerMap[instance.prefab.layer]);
						bool createJobAtTile = true;
						foreach (SelectionModifiersEnum selectionModifier in selectedRemovePrefab.selectionModifiers) {
							if (selectionModifier != SelectionModifiersEnum.Outline) {
								createJobAtTile = selectionModifierFunctions[selectionModifier](instance.tile, instance.tile, selectedRemovePrefab);
								if (!createJobAtTile) {
									break;
								}
							}
						}
						if (createJobAtTile) {
							CreateJob(new Job(instance.tile, selectedRemovePrefab, rotationIndex));
						}
					}
				}
			} else {
				CreateJob(new Job(tile, prefab, rotationIndex));
			}
		}
	}

	public void CreateJob(Job newJob) {
		jobs.Add(newJob);
		changedJobList = true;
	}

	public void AddExistingJob(Job existingJob) {
		jobs.Add(existingJob);
		changedJobList = true;
	}

	public class ContainerPickup {
		public ResourceManager.Container container;
		public List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();

		public ContainerPickup(ResourceManager.Container container, List<ResourceManager.ResourceAmount> resourcesToPickup) {
			this.container = container;
			this.resourcesToPickup = resourcesToPickup;
		}
	}

	public List<ContainerPickup> CalculateColonistPickupContainers(ColonistManager.Colonist colonist, Job job, List<ResourceManager.ResourceAmount> resourcesToPickup) {
		List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
		List<ResourceManager.Container> sortedContainersByDistance = GameManager.resourceM.GetContainersInRegion(colonist.overTile.region).OrderBy(container => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, container.tile.regionBlock, true, true, false)).ToList();
		if (sortedContainersByDistance.Count > 0) {
			foreach (ResourceManager.Container container in sortedContainersByDistance) {
				List<ResourceManager.ResourceAmount> resourcesToPickupAtContainer = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.resource == ra.resource) != null)) {
					ResourceManager.ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.resource == resourceAmount.resource);
					if (resourceAmount.amount >= pickupResource.amount) {
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource, pickupResource.amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.amount > 0 && resourceAmount.amount < pickupResource.amount) {
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource, resourceAmount.amount));
						pickupResource.amount -= resourceAmount.amount;
						if (pickupResource.amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					} else {
					}
				}
				if (resourcesToPickupAtContainer.Count > 0) {
					containersToPickupFrom.Add(new ContainerPickup(container, resourcesToPickupAtContainer));
				}
			}
			if (containersToPickupFrom.Count > 0) {
				if (resourcesToPickup.Count <= 0) {
					return containersToPickupFrom;
				} else {
					return null;
				}
			} else {
				return null;
			}
		} else {
			return null;
		}
	}

	public KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> CalculateColonistResourcesToPickup(ColonistManager.Colonist colonist, List<ResourceManager.ResourceAmount> resourcesToFind) {
		bool colonistHasAllResources = false;
		List<ResourceManager.ResourceAmount> resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
		List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
		foreach (ResourceManager.ResourceAmount resourceAmount in resourcesToFind) {
			ResourceManager.ResourceAmount colonistResourceAmount = colonist.inventory.resources.Find(resource => resource.resource == resourceAmount.resource);
			if (colonistResourceAmount != null) {
				if (colonistResourceAmount.amount >= resourceAmount.amount) {
					colonistHasAllResources = true;
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				} else if (colonistResourceAmount.amount > 0 && colonistResourceAmount.amount < resourceAmount.amount) {
					colonistHasAllResources = false;
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, colonistResourceAmount.amount));
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount - colonistResourceAmount.amount));
				} else {
					colonistHasAllResources = false;
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				}
			} else {
				colonistHasAllResources = false;
				resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
			}
		}
		return new KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceManager.ResourceAmount>>() { (resourcesToPickup.Count > 0 ? resourcesToPickup : null), (resourcesColonistHas.Count > 0 ? resourcesColonistHas : null) });
	}

	public float CalculateJobCost(ColonistManager.Colonist colonist, Job job, List<ContainerPickup> containerPickups) {
		/* The cost of a job is determined using:
				- the amount of resources the colonist has
					- sometimes the amount of resources containers have
				- the distance of the colonist to the job
					- sometimes the distance between the colonist position and the job following the path of pickups
				- the skill of the colonist
			The cost should be updated whenever:
				- an inventory is changed:
					- all colonists if any container's inventory is changed
					- single colonist is the colonist's inventory is changed
				- the colonist moves
				- the skill of the colonist changes
		*/
		float cost = 0;
		if (containerPickups != null) {
			for (int i = 0; i < containerPickups.Count; i++) {
				if (i == 0) {
					cost += PathManager.RegionBlockDistance(colonist.overTile.regionBlock, containerPickups[i].container.tile.regionBlock, true, true, true);
				} else {
					cost += PathManager.RegionBlockDistance(containerPickups[i - 1].container.tile.regionBlock, containerPickups[i].container.tile.regionBlock, true, true, true);
				}
			}
			cost += PathManager.RegionBlockDistance(job.tile.regionBlock, containerPickups[containerPickups.Count - 1].container.tile.regionBlock, true, true, true);
		} else {
			cost += PathManager.RegionBlockDistance(job.tile.regionBlock, colonist.overTile.regionBlock, true, true, true);
		}
		ColonistManager.SkillInstance skill = colonist.GetSkillFromJobType(job.prefab.jobType);
		if (skill != null) {
			ColonistManager.Profession jobTypeProfession = GameManager.colonistM.professions.Find(profession => profession.primarySkill == skill.prefab);
			if (jobTypeProfession != null && jobTypeProfession == colonist.profession) {
				cost -= GameManager.colonyM.colony.map.mapData.mapSize + (skill.level * 5f);
			} else {
				cost -= skill.level * 5f;
			}
		}
		if (colonist.profession.type == ColonistManager.ProfessionTypeEnum.Builder && job.prefab.instanceType == ResourceManager.ObjectInstanceType.Container) {
			cost -= GameManager.colonyM.colony.map.mapData.mapSize;
		}
		return cost;
	}

	public class ColonistJob {
		public ColonistManager.Colonist colonist;
		public Job job;

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public float cost;

		public ColonistJob(ColonistManager.Colonist colonist, Job job, List<ResourceManager.ResourceAmount> colonistResources, List<ContainerPickup> containerPickups) {
			this.colonist = colonist;
			this.job = job;
			this.colonistResources = colonistResources;
			this.containerPickups = containerPickups;

			CalculateCost();
		}

		public void CalculateCost() {
			cost = GameManager.jobM.CalculateJobCost(colonist, job, containerPickups);
		}

		public void RecalculatePickupResources() {
			KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> returnKVP = GameManager.jobM.CalculateColonistResourcesToPickup(colonist, job.resourcesToBuild);
			List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
			colonistResources = returnKVP.Value[1];
			if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
				containerPickups = GameManager.jobM.CalculateColonistPickupContainers(colonist, job, resourcesToPickup);
			} else {
				containerPickups = null;
			}
		}
	}

	public void UpdateColonistJobCosts(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			foreach (ColonistJob colonistJob in colonistJobs[colonist]) {
				colonistJob.CalculateCost();
			}
		}
	}

	public void UpdateAllColonistJobCosts() {
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			UpdateColonistJobCosts(colonist);
		}
	}

	public void UpdateSingleColonistJobs(ColonistManager.Colonist colonist) {
		List<Job> sortedJobs = jobs.Where(job => (job.tile.region == colonist.overTile.region) || (job.tile.region != colonist.overTile.region && job.tile.horizontalSurroundingTiles.Find(nTile => nTile != null && nTile.region == colonist.overTile.region) != null)).OrderBy(job => CalculateJobCost(colonist, job, null)).ToList();
		List<ColonistJob> validJobs = new List<ColonistJob>();
		foreach (Job job in sortedJobs) {
			if (job.resourcesToBuild.Count > 0) {
				KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> returnKVP = CalculateColonistResourcesToPickup(colonist, job.resourcesToBuild);
				bool colonistHasAllResources = returnKVP.Key;
				List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
				List<ResourceManager.ResourceAmount> resourcesColonistHas = returnKVP.Value[1];
				if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
					List<ContainerPickup> containerPickups = CalculateColonistPickupContainers(colonist, job, resourcesToPickup);
					if (containerPickups != null) { // If all resources were found in containers
						validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, containerPickups));
					} else {
						continue;
					}
				} else if (colonistHasAllResources) { // If the colonist has all resources
					validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, null));
				} else {
					continue;
				}
			} else {
				validJobs.Add(new ColonistJob(colonist, job, null, null));
			}
		}
		if (validJobs.Count > 0) {
			validJobs = validJobs.OrderByDescending(job => job.job.priority).ThenBy(job => job.cost).ToList();
			if (colonistJobs.ContainsKey(colonist)) {
				colonistJobs[colonist] = validJobs;
			} else {
				colonistJobs.Add(colonist, validJobs);
			}
		}
	}

	private Dictionary<ColonistManager.Colonist, List<ColonistJob>> colonistJobs = new Dictionary<ColonistManager.Colonist, List<ColonistJob>>();
	public void UpdateColonistJobs() {
		colonistJobs.Clear();
		List<ColonistManager.Colonist> availableColonists = GameManager.colonistM.colonists.Where(colonist => colonist.job == null && colonist.overTile.walkable).ToList();
		foreach (ColonistManager.Colonist colonist in availableColonists) {
			UpdateSingleColonistJobs(colonist);
		}
	}

	public int GetColonistJobsCountForColonist(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			return colonistJobs[colonist].Count;
		}
		return 0;
	}

	public void GiveJobsToColonists() {
		bool gaveJob = false;
		Dictionary<ColonistManager.Colonist, ColonistJob> jobsGiven = new Dictionary<ColonistManager.Colonist, ColonistJob>();
		foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null && !colonist.playerMoved) {
				for (int i = 0; i < colonistJobsList.Count; i++) {
					ColonistJob colonistJob = colonistJobsList[i];
					bool bestColonistForJob = true;
					foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> otherColonistKVP in colonistJobs) {
						ColonistManager.Colonist otherColonist = otherColonistKVP.Key;
						if (colonist != otherColonist && otherColonist.job == null) {
							ColonistJob otherColonistJob = otherColonistKVP.Value.Find(job => job.job == colonistJob.job);
							if (otherColonistJob != null && otherColonistJob.cost < colonistJob.cost) {
								bestColonistForJob = false;
								break;
							}
						}
					}
					if (bestColonistForJob) {
						gaveJob = true;
						jobsGiven.Add(colonist, colonistJob);
						jobs.Remove(colonistJob.job);
						foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> removeKVP in colonistJobs) {
							ColonistJob jobToRemove = removeKVP.Value.Find(cJob => cJob.job == colonistJob.job);
							if (jobToRemove != null) {
								removeKVP.Value.Remove(jobToRemove);
							}
						}
						i -= 1;
						break;
					}
				}
			}
		}
		foreach (KeyValuePair<ColonistManager.Colonist, ColonistJob> jobGiven in jobsGiven) {
			jobGiven.Key.SetJob(jobGiven.Value);
		}
		if (gaveJob) {
			GameManager.uiM.SetJobElements();
			UpdateColonistJobs();
		}
	}

	public bool JobOfTypeExistsAtTile(JobTypesEnum jobType, TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.jobType == jobType && job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.jobType == jobType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.jobType == jobType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}

	public bool JobOfPrefabTypeExistsAtTile(ResourceManager.ObjectEnum prefabType, TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.type == prefabType && job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.type == prefabType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.type == prefabType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}
}