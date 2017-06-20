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

		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	public List<Job> jobs = new List<Job>();

	public class Job {
		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;

		public bool accessible;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab,int rotationIndex,ColonistManager colonistM,ResourceManager resourceM) {
			this.tile = tile;
			this.prefab = prefab;

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

			accessible = false;
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.overTile.region == tile.region) {
					accessible = true;
					break;
				}
			}
		}

		public void SetColonist(ColonistManager.Colonist colonist, ResourceManager resourceM, ColonistManager colonistM, JobManager jobM, PathManager pathM) {
			this.colonist = colonist;
			if (prefab.jobType != JobTypesEnum.PickupResources && containerPickups != null && containerPickups.Count > 0) {
				print(containerPickups[0].resourcesToPickup.Count);
				colonist.storedJob = this;
				colonist.SetJob(new ColonistJob(colonist,new Job(containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),0,colonistM,resourceM),null,null,jobM,pathM));
			}
		}
	}

	public enum JobTypesEnum {
		Build, RemoveObject, RemoveFloor,
		ChopPlant, Mine, PlantFarm, HarvestFarm,
		PickupResources, EmptyInventory
	};

	public Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>> finishJobFunctions = new Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>>();

	void InitializeFinishJobFunctions() {
		finishJobFunctions.Add(JobTypesEnum.Build,delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,-resourceAmount.amount);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.RemoveObject,delegate (ColonistManager.Colonist colonist,Job job) {
			/*
			 * NOT SURE IF THIS WORKS, FIX WHEN IMPLEMENTING REMOVE (MAYBE REMOVE REDUNDANT FINISHJOBFUNCTIONS)
			 * 
			foreach (ResourceManager.ResourceAmount resourceAmount in job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,Mathf.RoundToInt(resourceAmount.amount / 2f));
			}
			colonist.resourceM.RemoveTileObjectInstance(job.tile.GetObjectInstanceAtLayer(job.prefab.layer));
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			*/
		});
		finishJobFunctions.Add(JobTypesEnum.RemoveFloor,delegate (ColonistManager.Colonist colonist,Job job) {
			/*
			 * NOT SURE IF THIS WORKS, FIX WHEN IMPLEMENTING REMOVE (MAYBE REMOVE REDUNDANT FINISHJOBFUNCTIONS)
			 * 
			foreach (ResourceManager.ResourceAmount resourceAmount in job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,Mathf.RoundToInt(resourceAmount.amount/2f));
			}
			colonist.resourceM.RemoveTileObjectInstance(job.tile.GetObjectInstanceAtLayer(job.prefab.layer));
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			*/
		});
		finishJobFunctions.Add(JobTypesEnum.PlantFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			finishJobFunctions[JobTypesEnum.Build](colonist,job);
		});
		finishJobFunctions.Add(JobTypesEnum.HarvestFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(job.tile.farm.seedType),Random.Range(1,3));
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(resourceM.GetFarmSeedReturnResource()[job.tile.farm.seedType]),Random.Range(1,6));
			CreateJob(new Job(job.tile,resourceM.GetTileObjectPrefabByEnum(resourceM.GetFarmSeedsTileObject()[job.tile.farm.seedType]),0,colonistM,resourceM));
			job.tile.RemoveTileObjectAtLayer(job.tile.farm.prefab.layer);
			job.tile.SetFarm(null);
		});
		finishJobFunctions.Add(JobTypesEnum.ChopPlant,delegate (ColonistManager.Colonist colonist,Job job) {
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			foreach (ResourceManager.ResourceAmount resourceAmount in job.tile.plant.GetResources()) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,resourceAmount.amount);
			}
			job.tile.SetPlant(true);
		});
		finishJobFunctions.Add(JobTypesEnum.Mine,delegate (ColonistManager.Colonist colonist,Job job) {
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),job.tile.tileType.type.ToString())),Random.Range(4,7));
			job.tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Dirt),true,true,true,false);
		});
		finishJobFunctions.Add(JobTypesEnum.PickupResources,delegate (ColonistManager.Colonist colonist,Job job) {
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			print(containerOnTile + " " + colonist.storedJob.prefab.type.ToString() + " " + colonist.storedJob.containerPickups + " " + colonist.storedJob.containerPickups.Count);
			if (containerOnTile != null && colonist.storedJob != null) {
				JobManager.ContainerPickup containerPickup = colonist.storedJob.containerPickups.Find(pickup => pickup.container == containerOnTile);
				print(containerPickup);
				if (containerPickup != null) {
					foreach (ResourceManager.ReservedResources rr in containerPickup.container.inventory.TakeReservedResources(colonist)) {
						print(name + " " + rr.colonist.name + " " + rr.resources.Count);
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							colonist.inventory.ChangeResourceAmount(ra.resource,ra.amount);
							print(name + " " + ra.resource.name + " " + ra.amount);
						}
					}
					colonist.storedJob.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.storedJob != null && colonist.storedJob.containerPickups.Count <= 0) {
				print("Setting stored job on " + name);
				colonist.SetJob(new JobManager.ColonistJob(colonist,colonist.storedJob,colonist.storedJob.colonistResources,null,colonist.jobM,pathM));
			}
		});
		finishJobFunctions.Add(JobTypesEnum.EmptyInventory,delegate (ColonistManager.Colonist colonist,Job job) {
			job.tile.RemoveTileObjectAtLayer(job.prefab.layer);
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
	}

	ResourceManager.TileObjectPrefab selectedPrefab;

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

	private GameObject selectedPrefabPreview;
	private GameObject selectionSizePanel;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = tileM.GetTileFromPosition(mousePosition);
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
		if (timeM.timeModifier > 0) {
			GiveJobsToColonists();
		}
		UpdateSelectedPrefabInfo();
	}

	public enum SelectionModifiersEnum { Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms
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
				firstTile = tileM.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = tileM.GetTileFromPosition(mousePosition);
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
							TileManager.Tile tile = tileM.GetTileFromPosition(new Vector2(x,y));
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
								selectionModifierFunctions[selectionModifier].Invoke(tile,removeTiles);
							}
						}
						RemoveTilesFromList(selectionArea,removeTiles);
						removeTiles.Clear();
					}

					uiM.UpdateSelectionSizePanel(smallerX - maxX,smallerY - maxY,selectionArea.Count,selectedPrefab);

					foreach (TileManager.Tile tile in selectionArea) {
						GameObject selectionIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
						SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
						sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
						//sISR.color = new Color(241f,196f,15f,255f) / 255f; // Yellow
						//sISR.color = new Color(231f,76f,60f,255f) / 255f; // Red
						sISR.sortingOrder = 20; // Selection Indicator Sprite
						selectionIndicators.Add(selectionIndicator);
					}

					if (Input.GetMouseButtonUp(0)) {
						CreateJobsInSelectionArea(selectedPrefab,selectionArea);
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

	public void CreateJobsInSelectionArea(ResourceManager.TileObjectPrefab prefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			CreateJob(new Job(tile,prefab,rotationIndex,colonistM,resourceM));
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
			KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = jobM.CalculateColonistResourcesToPickup(colonist,job.prefab.resourcesToBuild);
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
			if (job.prefab.resourcesToBuild.Count > 0) {
				KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = CalculateColonistResourcesToPickup(colonist,job.prefab.resourcesToBuild);
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

	void GiveJobsToColonists() {
		bool gaveJob = false;
		Dictionary<ColonistManager.Colonist,ColonistJob> jobsGiven = new Dictionary<ColonistManager.Colonist,ColonistJob>();
		foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null) {
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
}