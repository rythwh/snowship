using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JobManager:MonoBehaviour {

	private TileManager tileM;
	private ColonistManager colonistM;
	private CameraManager cameraM;
	private TimeManager timeM;
	private UIManager uiM;
	private PathManager pathM;
	private ResourceManager resourceM;

	void Awake() {
		tileM = GetComponent<TileManager>();
		colonistM = GetComponent<ColonistManager>();
		cameraM = GetComponent<CameraManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
		pathM = GetComponent<PathManager>();
		resourceM = GetComponent<ResourceManager>();

		InitializeSelectionModifierFunctions();
		InitializeFinishJobFunctions();
		InitializeJobDescriptionFunctions();

		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	public List<Job> jobs = new List<Job>();

	public class Job {

		private ColonistManager colonistM;
		private ResourceManager resourceM;
		private TileManager tileM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			colonistM = GM.GetComponent<ColonistManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			tileM = GM.GetComponent<TileManager>();
		}

		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public UIManager.JobElement jobUIElement;

		public TileManager.Plant plant;

		public ResourceManager.Resource createResource;
		public ResourceManager.TileObjectInstance activeTileObject;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab,int rotationIndex) {

			GetScriptReferences();

			this.tile = tile;
			this.prefab = prefab;

			resourcesToBuild.AddRange(prefab.resourcesToBuild);

			if (prefab.jobType == JobTypesEnum.PlantPlant) {
				TileManager.PlantGroup plantGroup = tileM.GetPlantGroupByBiome(tile.biome, true);
				if (plantGroup != null) {
					plant = new TileManager.Plant(plantGroup, tile, false, true);
					plant.obj.SetActive(false);
					resourcesToBuild.AddRange(tileM.GetPlantResources()[plant.group.type]);
				}
			}

			this.rotationIndex = rotationIndex;

			jobPreview = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
			jobPreview.name = "JobPreview: " + prefab.name + " at " + tile.obj.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
			}
			if (!resourceM.GetBitmaskingTileObjects().Contains(prefab.type) && prefab.bitmaskSprites.Count > 0) {
				jPSR.sprite = prefab.bitmaskSprites[rotationIndex];
			}
			jPSR.sortingOrder = 2 + prefab.layer; // Job Preview Sprite
			jPSR.color = new Color(1f,1f,1f,0.25f);

			jobProgress = prefab.timeToBuild;
			colonistBuildTime = prefab.timeToBuild;
		}

		public void SetCreateResourceData(ResourceManager.Resource createResource, ResourceManager.TileObjectInstance manufacturingTileObject) {
			this.createResource = createResource;
			/*
			foreach (ResourceManager.ResourceAmount resourceAmount in createResource.requiredResources) {
				resourcesToBuild.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
			}
			*/
			resourcesToBuild.AddRange(createResource.requiredResources);
			if (manufacturingTileObject.mto.fuelResource != null) {
				resourcesToBuild.Add(new ResourceManager.ResourceAmount(manufacturingTileObject.mto.fuelResource, manufacturingTileObject.mto.fuelResourcesRequired));
			}
			activeTileObject = manufacturingTileObject;
		}

		public void SetColonist(ColonistManager.Colonist colonist, ResourceManager resourceM, ColonistManager colonistM, JobManager jobM, PathManager pathM) {
			this.colonist = colonist;
			if (prefab.jobType != JobTypesEnum.PickupResources && containerPickups != null && containerPickups.Count > 0) {
				print(containerPickups[0].resourcesToPickup.Count);
				colonist.storedJob = this;
				colonist.SetJob(new ColonistJob(colonist,new Job(containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),0),null,null,jobM,pathM));
			}
		}

		public void Remove() {
			Destroy(jobPreview);
		}
	}

	public enum JobTypesEnum {
		Build, Remove,
		ChopPlant, PlantPlant, Mine, Dig, PlantFarm, HarvestFarm,
		CreateResource, PickupResources, EmptyInventory, Cancel, CollectFood, Eat, Sleep
	};

	public Dictionary<JobTypesEnum,System.Func<Job,string>> jobDescriptionFunctions = new Dictionary<JobTypesEnum,System.Func<Job,string>>();

	void InitializeJobDescriptionFunctions() {
		jobDescriptionFunctions.Add(JobTypesEnum.Build,delegate (Job job) {
			return "Building a " + job.prefab.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Remove,delegate (Job job) {
			return "Removing a " + job.tile.GetObjectInstanceAtLayer(job.prefab.layer) + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.ChopPlant,delegate (Job job) {
			return "Chopping down a " + job.tile.plant.group.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PlantPlant,delegate (Job job) {
			return "Planting a " + job.plant.group.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Mine,delegate (Job job) {
			return "Mining " + job.tile.tileType.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Dig,delegate (Job job) {
			if (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type)) {
				if (tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) {
					if (tileM.GetWaterToGroundResourceMap().ContainsKey(job.tile.tileType.type)) {
						return "Digging " + tileM.GetTileTypeByEnum(tileM.GetWaterToGroundResourceMap()[job.tile.tileType.type]).name + ".";
					} else {
						return "Digging something.";
					}
				} else {
					return "Digging " + tileM.GetTileTypeByEnum(job.tile.tileType.type).name + ".";
				}
			} else {
				return "Digging " + job.tile.biome.groundResource.name + ".";
			}
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PlantFarm,delegate (Job job) {
			return "Planting a " + job.prefab.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.HarvestFarm,delegate (Job job) {
			return "Harvesting a farm of " + job.tile.farm.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.CreateResource,delegate (Job job) {
			return "Creating " + job.createResource.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PickupResources,delegate (Job job) {
			return "Picking up some resources.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.EmptyInventory,delegate (Job job) {
			return "Emptying their inventory.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.CollectFood,delegate (Job job) {
			return "Finding some food to eat.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Eat,delegate (Job job) {
			return "Eating.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Sleep,delegate (Job job) {
			return "Sleeping.";
		});
	}

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

	public Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>> finishJobFunctions = new Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>>();

	void InitializeFinishJobFunctions() {
		finishJobFunctions.Add(JobTypesEnum.Build,delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,-resourceAmount.amount);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.Remove,delegate (ColonistManager.Colonist colonist,Job job) {
			bool previousWalkability = job.tile.walkable;
			ResourceManager.TileObjectInstance instance = job.tile.GetObjectInstanceAtLayer(job.prefab.layer);
			foreach (ResourceManager.ResourceAmount resourceAmount in instance.prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,Mathf.RoundToInt(resourceAmount.amount / 2f));
			}
			if (instance.prefab.jobType == JobTypesEnum.PlantFarm) {
				if (instance.tile.farm.growProgressSpriteIndex == 0) {
					job.colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(instance.tile.farm.seedType),1);
				}
				instance.tile.SetFarm(null);
			}
			if (instance.prefab.tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.Containers) {
				ResourceManager.Container targetContainer = resourceM.containers.Find(container => container.parentObject == instance);
				if (targetContainer != null) {
					foreach (ResourceManager.ResourceAmount resourceAmount in targetContainer.inventory.resources) {
						targetContainer.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
						colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
					}
					List<ResourceManager.ReservedResources> reservedResourcesToRemove = new List<ResourceManager.ReservedResources>();
					foreach (ResourceManager.ReservedResources reservedResources in targetContainer.inventory.reservedResources) {
						foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
							colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
						}
						reservedResourcesToRemove.Add(reservedResources);
						reservedResources.colonist.ReturnJob();
					}
					foreach (ResourceManager.ReservedResources reservedResourceToRemove in reservedResourcesToRemove) {
						targetContainer.inventory.reservedResources.Remove(reservedResourceToRemove);
					}
					uiM.SetSelectedColonistInformation();
					uiM.SetSelectedContainerInfo();
				} else {
					print("Target container is null but it shouldn't be...");
				}
			}
			colonist.resourceM.RemoveTileObjectInstance(instance);
			job.tile.RemoveTileObjectAtLayer(instance.prefab.layer);
			resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
			if (job.tile.walkable && !previousWalkability) {
				tileM.map.RemoveTileBrightnessEffect(job.tile);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.PlantFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			finishJobFunctions[JobTypesEnum.Build](colonist,job);
		});
		finishJobFunctions.Add(JobTypesEnum.HarvestFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			if (job.tile.farm != null) {
				colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(job.tile.farm.seedType), Random.Range(1, 3));
				colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(resourceM.GetFarmSeedReturnResource()[job.tile.farm.seedType]), Random.Range(1, 6));

				CreateJob(new Job(job.tile, resourceM.GetTileObjectPrefabByEnum(resourceM.GetFarmSeedsTileObject()[job.tile.farm.seedType]), 0));

				colonist.resourceM.RemoveTileObjectInstance(job.tile.farm);
				job.tile.RemoveTileObjectAtLayer(job.tile.farm.prefab.layer);
			}
			job.tile.SetFarm(null);
			resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
		});
		finishJobFunctions.Add(JobTypesEnum.ChopPlant,delegate (ColonistManager.Colonist colonist,Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.tile.plant.GetResources()) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,resourceAmount.amount);
			}
			job.tile.SetPlant(true,null);
		});
		finishJobFunctions.Add(JobTypesEnum.PlantPlant,delegate (ColonistManager.Colonist colonist,Job job) {
			job.plant.obj.SetActive(true);
			job.tile.SetPlant(false,job.plant);
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(tileM.GetPlantSeeds()[job.tile.plant.group.type]),-1);
			tileM.map.SetTileBrightness(timeM.GetTileBrightnessTime());
		});
		finishJobFunctions.Add(JobTypesEnum.Mine,delegate (ColonistManager.Colonist colonist,Job job) {
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),job.tile.tileType.type.ToString())),Random.Range(4,7));
			job.tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Dirt),true,true,true,false);
			tileM.map.RemoveTileBrightnessEffect(job.tile);
		});
		finishJobFunctions.Add(JobTypesEnum.Dig, delegate (ColonistManager.Colonist colonist, Job job) {
			job.tile.dugPreviously = true;
			if (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type)) {
				if (tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) {
					if (tileM.GetWaterToGroundResourceMap().ContainsKey(job.tile.tileType.type)) {
						colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),tileM.GetTileTypeByEnum(tileM.GetWaterToGroundResourceMap()[job.tile.tileType.type]).type.ToString())),Random.Range(4,7));
					}
				} else {
					colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),job.tile.tileType.type.ToString())),Random.Range(4,7));
				}
			} else {
				colonist.inventory.ChangeResourceAmount(job.tile.biome.groundResource, Random.Range(4, 7));
			}
			bool setToWater = false;
			if ((!tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) || (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type))) {
				foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
					if (nTile != null && tileM.GetWaterEquivalentTileTypes().Contains(nTile.tileType.type)) {
						job.tile.SetTileType(nTile.tileType, true, true, true, true);
						setToWater = true;
						break;
					}
				}
				if (setToWater) {
					foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
						if (nTile != null && tileM.GetHoleTileTypes().Contains(nTile.tileType.type)) {
							List<TileManager.Tile> frontier = new List<TileManager.Tile>() { nTile };
							List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>() { };
							TileManager.Tile currentTile = nTile;

							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								checkedTiles.Add(currentTile);
								currentTile.SetTileType(job.tile.tileType, true, true, true, true);
								foreach (TileManager.Tile nTile2 in currentTile.horizontalSurroundingTiles) {
									if (nTile2 != null && tileM.GetHoleTileTypes().Contains(nTile2.tileType.type) && !checkedTiles.Contains(nTile2)) {
										frontier.Add(nTile2);
									}
								}
							}
						}
					}
				}
			}
			if (!setToWater) {
				job.tile.SetTileType(job.tile.biome.holeType, true, true, true, false);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.PickupResources,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			//print(containerOnTile + " " + colonist.storedJob.prefab.type.ToString() + " " + colonist.storedJob.containerPickups + " " + colonist.storedJob.containerPickups.Count);
			if (containerOnTile != null && colonist.storedJob != null) {
				ContainerPickup containerPickup = colonist.storedJob.containerPickups.Find(pickup => pickup.container == containerOnTile);
				//print(containerPickup);
				if (containerPickup != null) {
					foreach (ResourceManager.ReservedResources rr in containerPickup.container.inventory.TakeReservedResources(colonist)) {
						//print(name + " " + rr.colonist.name + " " + rr.resources.Count);
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							colonist.inventory.ChangeResourceAmount(ra.resource,ra.amount);
							//print(name + " " + ra.resource.name + " " + ra.amount);
						}
					}
					colonist.storedJob.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.storedJob != null) {
				if (colonist.storedJob.containerPickups.Count <= 0) {
					//print("Setting stored job on " + name);
					colonist.SetJob(new ColonistJob(colonist, colonist.storedJob, colonist.storedJob.colonistResources, null, colonist.jobM, pathM));
					colonist.storedJob = null;
				} else {
					//print("Setting next pickup resources job on " + name + " -- " + colonist.storedJob.containerPickups.Count + " more left");
					colonist.SetJob(new ColonistJob(colonist, new Job(colonist.storedJob.containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),0), colonist.storedJob.colonistResources, colonist.storedJob.containerPickups, colonist.jobM, pathM),false);
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.CreateResource,delegate (ColonistManager.Colonist colonist,Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,-resourceAmount.amount);
			}
			colonist.inventory.ChangeResourceAmount(job.createResource,job.createResource.amountCreated);
			job.activeTileObject.mto.jobBacklog.Remove(job);
		});
		finishJobFunctions.Add(JobTypesEnum.EmptyInventory,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			if (containerOnTile != null) {
				List<ResourceManager.ResourceAmount> removeResourceAmounts = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount inventoryResourceAmount in colonist.inventory.resources) {
					if (inventoryResourceAmount.amount <= containerOnTile.maxAmount - containerOnTile.inventory.CountResources()) {
						containerOnTile.inventory.ChangeResourceAmount(inventoryResourceAmount.resource,inventoryResourceAmount.amount);
						removeResourceAmounts.Add(new ResourceManager.ResourceAmount(inventoryResourceAmount.resource,inventoryResourceAmount.amount));
					} else if (containerOnTile.inventory.CountResources() < containerOnTile.maxAmount) {
						int amount = containerOnTile.maxAmount - containerOnTile.inventory.CountResources();
						containerOnTile.inventory.ChangeResourceAmount(inventoryResourceAmount.resource,amount);
						removeResourceAmounts.Add(new ResourceManager.ResourceAmount(inventoryResourceAmount.resource,amount));
					} else {
						print("No space left in container");
					}
				}
				foreach (ResourceManager.ResourceAmount removeResourceAmount in removeResourceAmounts) {
					colonist.inventory.ChangeResourceAmount(removeResourceAmount.resource,-removeResourceAmount.amount);
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.CollectFood,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			if (containerOnTile != null) {
				foreach (ResourceManager.ReservedResources rr in containerOnTile.inventory.TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.inventory.ChangeResourceAmount(ra.resource,ra.amount);
					}
				}
			}
			colonist.SetJob(new ColonistJob(colonist,new Job(colonist.overTile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Eat),0),null,null,this,pathM));
		});
		finishJobFunctions.Add(JobTypesEnum.Eat,delegate (ColonistManager.Colonist colonist,Job job) {
			List<ResourceManager.ResourceAmount> resourcesToEat = colonist.inventory.resources.Where(r => r.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods).OrderBy(r => r.resource.nutrition).ToList();
			ColonistManager.NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ColonistManager.NeedsEnum.Food);
			/*
			if (resourcesToEat.Count > 0) {
				ResourceManager.ResourceAmount resourceToEat = resourcesToEat[0];
				colonist.inventory.ChangeResourceAmount(resourceToEat.resource,-1);
				foodNeed.value -= resourceToEat.resource.nutrition;
			}
			*/
			foreach (ResourceManager.ResourceAmount ra in resourcesToEat) {
				bool stopEating = false;
				for (int i = 0; i < ra.amount; i++) {
					if (foodNeed.value - ra.resource.nutrition < 0) {
						stopEating = true;
						break;
					}
					foodNeed.value -= ra.resource.nutrition;
					colonist.inventory.ChangeResourceAmount(ra.resource, -1);
				}
				if (stopEating) {
					break;
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.Sleep,delegate (ColonistManager.Colonist colonist,Job job) {
			print("Sleeping");
		});
	}

	private ResourceManager.TileObjectPrefab selectedPrefab;

	public void SetSelectedPrefab(ResourceManager.TileObjectPrefab newSelectedPrefab) {
		if (newSelectedPrefab != selectedPrefab) {
			if (newSelectedPrefab != null) {
				selectedPrefab = newSelectedPrefab;
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
				}
			} else {
				selectedPrefab = null;
			}
		}
	}

	public ResourceManager.TileObjectPrefab GetSelectedPrefab() {
		return selectedPrefab;
	}

	private GameObject selectedPrefabPreview;
	private GameObject selectionSizePanel;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = tileM.map.GetTileFromPosition(mousePosition);
		selectedPrefabPreview.transform.position = tile.obj.transform.position;
	}

	public void UpdateSelectedPrefabInfo() {
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
					uiM.SelectionSizeCanvasSetActive(false);
				}
				SelectedPrefabPreview();
				if (Input.GetKeyDown(KeyCode.R)) {
					if (!resourceM.GetBitmaskingTileObjects().Contains(selectedPrefab.type) && selectedPrefab.bitmaskSprites.Count > 0) {
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
				uiM.SelectionSizeCanvasSetActive(true);
			}
		} else {
			selectedPrefabPreview.SetActive(false);
			uiM.SelectionSizeCanvasSetActive(false);
		}
	}

	private bool changedJobList = false;
	private int rotationIndex = 0;
	void Update() {
		if (changedJobList) {
			UpdateColonistJobs();
			uiM.SetJobElements();
			changedJobList = false;
		}
		GetJobSelectionArea();
		UpdateSelectedPrefabInfo();
	}

	public enum SelectionModifiersEnum { Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms, ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, OmitNoTreeOrDeadTreeBiomes
	};
	Dictionary<SelectionModifiersEnum,System.Action<TileManager.Tile,List<TileManager.Tile>>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum,System.Action<TileManager.Tile,List<TileManager.Tile>>>();

	void InitializeSelectionModifierFunctions() {
		selectionModifierFunctions.Add(SelectionModifiersEnum.Walkable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tile.walkable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitWalkable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.walkable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Buildable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tile.tileType.buildable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitBuildable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.tileType.buildable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.StoneTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitStoneTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.AllWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitAllWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.LiquidWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitLiquidWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonStoneAndWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type) || tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Plants,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.plant == null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitPlants,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.plant != null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerJobs,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (jobs.Find(job => job.prefab.layer == selectedPrefab.layer && job.tile == tile) != null) {
				removeTiles.Add(tile);
				return;
			}
			if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.layer == selectedPrefab.layer && colonist.job.tile == tile) != null) {
				removeTiles.Add(tile);
				return;
			}
			if (colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.layer == selectedPrefab.layer && colonist.storedJob.tile == tile) != null) {
				removeTiles.Add(tile);
				return;
			}
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerObjectInstances,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.objectInstances.ContainsKey(selectedPrefab.layer) && tile.objectInstances[selectedPrefab.layer] != null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Farms,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.farm == null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitFarms,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.farm != null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.ObjectsAtSameLayer,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.GetObjectInstanceAtLayer(selectedPrefab.layer) == null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Objects,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.GetAllObjectInstances().Count <= 0) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitObjects,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.GetAllObjectInstances().Count > 0) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonCoastWater, delegate (TileManager.Tile tile, List<TileManager.Tile> removeTiles) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) {
				if (!(tile.surroundingTiles.Find(t => t != null && !tileM.GetWaterEquivalentTileTypes().Contains(t.tileType.type)) != null)) {
					removeTiles.Add(tile);
				}
			}
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitHoles, delegate (TileManager.Tile tile, List<TileManager.Tile> removeTiles) {
			if (tileM.GetHoleTileTypes().Contains(tile.tileType.type)) {
					removeTiles.Add(tile);
			}
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitPreviousDig, delegate (TileManager.Tile tile, List<TileManager.Tile> removeTiles) {
			if (tile.dugPreviously) {
				removeTiles.Add(tile);
			}
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNoTreeOrDeadTreeBiomes, delegate (TileManager.Tile tile, List<TileManager.Tile> removeTiles) {
			if (tile.biome.vegetationChances.Keys.Where(group => group != TileManager.PlantGroupsEnum.DeadTree).ToList().Count <= 0) {
				removeTiles.Add(tile);
			}
		});
	}

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
			Destroy(selectionIndicator);
		}

		if (selectedPrefab != null) {
			Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				firstTile = tileM.map.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = tileM.map.GetTileFromPosition(mousePosition);
				if (secondTile != null) {

					enableSelectionPreview = false;

					float smallerY = Mathf.Min(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					float maxY = ((largerY - smallerY) + smallerY + 1);
					float maxX = ((largerX - smallerX) + smallerX + 1);

					for (float y = smallerY; y < maxY; y++) {
						for (float x = smallerX; x < maxX; x++) {
							TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(x,y));
							if (selectedPrefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
								if (x == smallerX || y == smallerY || x == ((largerX - smallerX) + smallerX) || y == ((largerY - smallerY) + smallerY)) {
									selectionArea.Add(tile);
								}
							} else {
								selectionArea.Add(tile);
							}
						}
					}

					foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.selectionModifiers) {
						List<TileManager.Tile> removeTiles = new List<TileManager.Tile>();
						if (selectionModifier == SelectionModifiersEnum.Outline) {
							continue;
						} else {
							foreach (TileManager.Tile tile in selectionArea) {
								selectionModifierFunctions[selectionModifier](tile,removeTiles);
							}
						}
						RemoveTilesFromList(selectionArea,removeTiles);
						removeTiles.Clear();
					}

					uiM.UpdateSelectionSizePanel(smallerX - maxX,smallerY - maxY,selectionArea.Count,selectedPrefab);

					foreach (TileManager.Tile tile in selectionArea) {
						GameObject selectionIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
						selectionIndicator.name = "Selection Indicator";
						SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
						sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
						//sISR.color = new Color(241f,196f,15f,255f) / 255f; // Yellow
						//sISR.color = new Color(231f,76f,60f,255f) / 255f; // Red
						sISR.sortingOrder = 20; // Selection Indicator Sprite
						selectionIndicators.Add(selectionIndicator);
					}

					if (Input.GetMouseButtonUp(0)) {
						if (selectedPrefab.jobType != JobTypesEnum.Cancel) {
							CreateJobsInSelectionArea(selectedPrefab,selectionArea);
						} else {
							CancelJobsInSelectionArea(selectionArea);
						}
						firstTile = null;
						rotationIndex = 0;
					}
				}
			}
		}
	}

	public void RemoveTilesFromList(List<TileManager.Tile> listToModify,List<TileManager.Tile> removeList) {
		foreach (TileManager.Tile tile in removeList) {
			listToModify.Remove(tile);
		}
		removeList.Clear();
	}

	public void CancelJobsInSelectionArea(List<TileManager.Tile> selectionArea) {
		List<Job> removeJobs = new List<Job>();
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				removeJobs.Add(job);
			}
		}
		foreach (Job job in removeJobs) {
			//print(job.prefab.jobType + " " + job.tile.obj.transform.position);
			if (job.prefab.jobType == JobTypesEnum.CreateResource) {
				job.activeTileObject.mto.jobBacklog.Remove(job);
			}
			job.jobUIElement.Remove(uiM);
			job.Remove();
			jobs.Remove(job);
		}
		removeJobs.Clear();

		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {

			bool removeJob = false;
			bool removeStoredJob = false;

			if (colonist.job != null && selectionArea.Contains(colonist.job.tile)) {
				removeJob = true;

				if (colonist.storedJob != null && !selectionArea.Contains(colonist.storedJob.tile)) {
					removeStoredJob = true;
				}
			}

			if (removeStoredJob || (colonist.storedJob != null && selectionArea.Contains(colonist.storedJob.tile))) {
				if (colonist.storedJob.prefab.jobType == JobTypesEnum.CreateResource) {
					colonist.storedJob.activeTileObject.mto.jobBacklog.Remove(colonist.storedJob);
				}
				if (colonist.storedJob.jobUIElement != null) {
					colonist.storedJob.jobUIElement.Remove(uiM);
				} else {
					Debug.LogWarning("storedJob on Colonist " + colonist.name + " jobUIElement is null for job " + colonist.storedJob.prefab.type);
				}
				colonist.storedJob.Remove();
				colonist.storedJob = null;

				if (colonist.job != null) {
					removeJob = true;
				}
			}

			if (removeJob) {
				if (colonist.job.prefab.jobType == JobTypesEnum.CreateResource) {
					colonist.job.activeTileObject.mto.jobBacklog.Remove(colonist.job);
				}
				if (colonist.job.jobUIElement != null) {
					colonist.job.jobUIElement.Remove(uiM);
				}
				colonist.job.Remove();
				colonist.job = null;
				colonist.path.Clear();
				colonist.MoveToClosestWalkableTile(false);
			}

			/*
			if (colonist.storedJob == null) {
				if (colonist.job != null && selectionArea.Contains(colonist.job.tile)) {
					if (colonist.job.prefab.jobType == JobTypesEnum.CreateResource) {
						colonist.job.activeTileObject.mto.jobBacklog.Remove(colonist.job);
					}
					colonist.job.jobUIElement.Remove(uiM);
					colonist.job.Remove();
					colonist.job = null;
					colonist.path.Clear();
					colonist.MoveToClosestWalkableTile(false);
				}
			} else {
				if ((selectionArea.Contains(colonist.storedJob.tile)) || (!selectionArea.Contains(colonist.storedJob.tile) && colonist.job != null && selectionArea.Contains(colonist.job.tile))) {
					//print(colonist.storedJob.prefab.jobType + " " + colonist.storedJob.tile.obj.transform.position);
					if (colonist.storedJob.prefab.jobType == JobTypesEnum.CreateResource) {
						colonist.storedJob.activeTileObject.mto.jobBacklog.Remove(colonist.storedJob);
					}
					if (colonist.storedJob.jobUIElement != null) {
						colonist.storedJob.jobUIElement.Remove(uiM);
					} else {
						Debug.LogWarning("storedJob on Colonist " + colonist.name + " jobUIElement is null for job " + colonist.storedJob.prefab.type);
					}
					colonist.storedJob.Remove();
					colonist.storedJob = null;
					//print(colonist.job.prefab.jobType + " " + colonist.job.tile.obj.transform.position);
					if (colonist.job.prefab.jobType == JobTypesEnum.CreateResource) {
						colonist.job.activeTileObject.mto.jobBacklog.Remove(colonist.job);
					}
					colonist.job.jobUIElement.Remove(uiM);
					colonist.job.Remove();
					colonist.job = null;
					colonist.path.Clear();
					colonist.MoveToClosestWalkableTile(false);
				}
			}
			*/
		}

		UpdateColonistJobs();
	}

	Dictionary<int,ResourceManager.TileObjectPrefabsEnum> RemoveLayerMap = new Dictionary<int,ResourceManager.TileObjectPrefabsEnum>() {
		{1,ResourceManager.TileObjectPrefabsEnum.RemoveLayer1 },{2,ResourceManager.TileObjectPrefabsEnum.RemoveLayer2 }
	};

	public void CreateJobsInSelectionArea(ResourceManager.TileObjectPrefab prefab,List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			if (selectedPrefab.type == ResourceManager.TileObjectPrefabsEnum.RemoveAll) {
				foreach (ResourceManager.TileObjectInstance instance in tile.GetAllObjectInstances()) {
					if (RemoveLayerMap.ContainsKey(instance.prefab.layer) && !JobOfPrefabTypeExistsAtTile(RemoveLayerMap[instance.prefab.layer],tile)) {
						CreateJob(new Job(tile,resourceM.GetTileObjectPrefabByEnum(RemoveLayerMap[instance.prefab.layer]),rotationIndex));
					}
				}
			} else {
				CreateJob(new Job(tile,prefab,rotationIndex));
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

		public ContainerPickup(ResourceManager.Container container,List<ResourceManager.ResourceAmount> resourcesToPickup) {
			this.container = container;
			this.resourcesToPickup = resourcesToPickup;
		}
	}

	public List<ContainerPickup> CalculateColonistPickupContainers(ColonistManager.Colonist colonist,Job job,List<ResourceManager.ResourceAmount> resourcesToPickup) {
		List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
		List<ResourceManager.Container> sortedContainersByDistance = resourceM.containers.Where(container => container.parentObject.tile.region == colonist.overTile.region).OrderBy(container => pathM.RegionBlockDistance(colonist.overTile.regionBlock,container.parentObject.tile.regionBlock,true,true,false)).ToList();
		if (sortedContainersByDistance.Count > 0) {
			foreach (ResourceManager.Container container in sortedContainersByDistance) {
				List<ResourceManager.ResourceAmount> resourcesToPickupAtContainer = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.resource == ra.resource) != null)) {
					ResourceManager.ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.resource == resourceAmount.resource);
					if (resourceAmount.amount >= pickupResource.amount) {
						//print("Found all of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,pickupResource.amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.amount > 0 && resourceAmount.amount < pickupResource.amount) {
						//print("Found some of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,resourceAmount.amount));
						pickupResource.amount -= resourceAmount.amount;
						if (pickupResource.amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					} else {
						//print("Found none of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
					}
				}
				if (resourcesToPickupAtContainer.Count > 0) {
					containersToPickupFrom.Add(new ContainerPickup(container,resourcesToPickupAtContainer));
				}
			}
			if (containersToPickupFrom.Count > 0) {
				if (resourcesToPickup.Count <= 0) {
					return containersToPickupFrom;
				} else {
					//print("Didn't find all resources in containers. Missed " + resourcesToPickup.Count + " resources");
					return null;
				}
			} else {
				//print("Didn't find any containers which contain the resources the colonist needs");
				return null;
			}
		} else {
			//print("Didn't find any valid containers");
			return null;
		}
	}

	public KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> CalculateColonistResourcesToPickup(ColonistManager.Colonist colonist, List<ResourceManager.ResourceAmount> resourcesToFind) {
		bool colonistHasAllResources = false;
		List<ResourceManager.ResourceAmount> resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
		List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
		foreach (ResourceManager.ResourceAmount resourceAmount in resourcesToFind) {
			ResourceManager.ResourceAmount colonistResourceAmount = colonist.inventory.resources.Find(resource => resource.resource == resourceAmount.resource);
			if (colonistResourceAmount != null) {
				if (colonistResourceAmount.amount >= resourceAmount.amount) {
					colonistHasAllResources = true;
					//print("Found all of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				} else if (colonistResourceAmount.amount > 0 && colonistResourceAmount.amount < resourceAmount.amount) {
					colonistHasAllResources = false;
					//print("Found some of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,colonistResourceAmount.amount));
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount - colonistResourceAmount.amount));
				} else {
					colonistHasAllResources = false;
					//print("Found none of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				}
			} else {
				colonistHasAllResources = false;
				resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
			}
		}
		/*
		if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count > 0) {
			print("Found " + resourcesToPickup.Count + " that " + colonist.name + " needs to pickup");
		} else if (resourcesToPickup.Count <= 0 && resourcesColonistHas.Count > 0) {
			print("Found all resources in " + colonist.name);
		} else if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count <= 0) {
			print("Found no resources in " + colonist.name);
		}
		*/
		return new KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceManager.ResourceAmount>>() { (resourcesToPickup.Count > 0 ? resourcesToPickup : null),(resourcesColonistHas.Count > 0 ? resourcesColonistHas : null) });
	}

	public float CalculateJobCost(ColonistManager.Colonist colonist,Job job, List<ContainerPickup> containerPickups) {
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
			for (int i = 0;i < containerPickups.Count;i++) {
				if (i == 0) {
					cost += pathM.RegionBlockDistance(colonist.overTile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true,true);
				} else {
					cost += pathM.RegionBlockDistance(containerPickups[i - 1].container.parentObject.tile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true,true);
				}
			}
			cost += pathM.RegionBlockDistance(job.tile.regionBlock,containerPickups[containerPickups.Count-1].container.parentObject.tile.regionBlock,true,true,true);
		} else {
			cost += pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true,true);
		}
		if (job.prefab.tileObjectPrefabSubGroup.tileObjectPrefabGroup.type != ResourceManager.TileObjectPrefabGroupsEnum.None) {
			ColonistManager.Profession jobTypeProfession = colonistM.professions.Find(profession => profession.primarySkill == colonist.GetSkillFromJobType(job.prefab.jobType).prefab);
			if (jobTypeProfession != null && jobTypeProfession == colonist.profession) {
				cost -= 30 + (colonist.GetSkillFromJobType(job.prefab.jobType).level * 5f);
			} else {
				cost -= colonist.GetSkillFromJobType(job.prefab.jobType).level * 5f;
			}
		}
		return cost;
	}

	public class ColonistJob {
		public ColonistManager.Colonist colonist;
		public Job job;

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public float cost;

		public ColonistJob(ColonistManager.Colonist colonist,Job job,List<ResourceManager.ResourceAmount> colonistResources,List<ContainerPickup> containerPickups, JobManager jobM, PathManager pathM) {
			this.colonist = colonist;
			this.job = job;
			this.colonistResources = colonistResources;
			this.containerPickups = containerPickups;

			CalculateCost(jobM);
		}

		public void CalculateCost(JobManager jobM) {
			cost = jobM.CalculateJobCost(colonist,job,containerPickups);
		}

		public void RecalculatePickupResources(JobManager jobM) {
			KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = jobM.CalculateColonistResourcesToPickup(colonist,job.resourcesToBuild);
			List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
			colonistResources = returnKVP.Value[1];
			if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
				containerPickups = jobM.CalculateColonistPickupContainers(colonist,job,resourcesToPickup);
			} else {
				containerPickups = null;
			}
		}
	}

	public void UpdateColonistJobCosts(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			foreach (ColonistJob colonistJob in colonistJobs[colonist]) {
				colonistJob.CalculateCost(this);
			}
		}
	}

	public void UpdateAllColonistJobCosts() {
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			UpdateColonistJobCosts(colonist);
		}
	}

	public void UpdateSingleColonistJobs(ColonistManager.Colonist colonist) {
		List<Job> sortedJobs = jobs.Where(job => (job.tile.region == colonist.overTile.region) || (job.prefab.jobType == JobTypesEnum.Mine && job.tile.horizontalSurroundingTiles.Find(nTile => nTile != null && nTile.region == colonist.overTile.region) != null)).OrderBy(job => CalculateJobCost(colonist,job,null)).ToList();
		List<ColonistJob> validJobs = new List<ColonistJob>();
		foreach (Job job in sortedJobs) {
			if (job.resourcesToBuild.Count > 0) {
				KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = CalculateColonistResourcesToPickup(colonist,job.resourcesToBuild);
				bool colonistHasAllResources = returnKVP.Key;
				List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
				List<ResourceManager.ResourceAmount> resourcesColonistHas = returnKVP.Value[1];
				if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
					List<ContainerPickup> containerPickups = CalculateColonistPickupContainers(colonist,job,resourcesToPickup);
					if (containerPickups != null) { // If all resources were found in containers
						validJobs.Add(new ColonistJob(colonist,job,resourcesColonistHas,containerPickups,this,pathM));
					} else {
						continue;
					}
				} else if (colonistHasAllResources) { // If the colonist has all resources
					validJobs.Add(new ColonistJob(colonist,job,resourcesColonistHas,null,this,pathM));
				} else {
					continue;
				}
			} else {
				validJobs.Add(new ColonistJob(colonist,job,null,null,this,pathM));
			}
		}
		if (validJobs.Count > 0) {
			validJobs = validJobs.OrderBy(job => job.cost).ToList();
			if (colonistJobs.ContainsKey(colonist)) {
				colonistJobs[colonist] = validJobs;
			} else {
				colonistJobs.Add(colonist,validJobs);
			}
		}
	}

	private Dictionary<ColonistManager.Colonist,List<ColonistJob>> colonistJobs = new Dictionary<ColonistManager.Colonist,List<ColonistJob>>();
	public void UpdateColonistJobs() {
		colonistJobs.Clear();
		List<ColonistManager.Colonist> availableColonists = colonistM.colonists.Where(colonist => colonist.job == null && colonist.overTile.walkable).ToList();
		foreach (ColonistManager.Colonist colonist in availableColonists) {
			UpdateSingleColonistJobs(colonist);
		}
	}

	public void GiveJobsToColonists() {
		bool gaveJob = false;
		Dictionary<ColonistManager.Colonist,ColonistJob> jobsGiven = new Dictionary<ColonistManager.Colonist,ColonistJob>();
		foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null && !colonist.playerMoved) {
				for (int i = 0; i < colonistJobsList.Count; i++) {
					ColonistJob colonistJob = colonistJobsList[i];
					bool bestColonistForJob = true;
					foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> otherColonistKVP in colonistJobs) {
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
						jobsGiven.Add(colonist,colonistJob);
						jobs.Remove(colonistJob.job);
						foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> removeKVP in colonistJobs) {
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
		foreach (KeyValuePair<ColonistManager.Colonist,ColonistJob> jobGiven in jobsGiven) {
			jobGiven.Key.SetJob(jobGiven.Value);
		}
		if (gaveJob) {
			uiM.SetJobElements();
			UpdateColonistJobs();
		}
	}

	public bool JobOfTypeExistsAtTile(JobTypesEnum jobType,TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.jobType == jobType && job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.jobType == jobType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.jobType == jobType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}

	public bool JobOfPrefabTypeExistsAtTile(ResourceManager.TileObjectPrefabsEnum prefabType,TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.type == prefabType && job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.type == prefabType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.type == prefabType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}
}