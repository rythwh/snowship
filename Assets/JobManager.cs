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

		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	public List<Job> jobs = new List<Job>();

	public class Job {
		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public GameObject jobPreview;

		public bool accessible;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab,ColonistManager colonistM) {
			this.tile = tile;
			this.prefab = prefab;

			jobPreview = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
			jobPreview.name = "JobPreview: " + prefab.name + " at " + tile.obj.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
				jPSR.sortingOrder = 2 + prefab.layer; // Job Preview Sprite
			}
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
			if (prefab.jobType != JobTypesEnum.PickupResources && containerPickups.Count > 0) {
				colonist.storedJob = this;
				colonist.SetJob(new ColonistJob(colonist,new Job(containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),colonistM),null,null,jobM,pathM));
			}
		}
	}

	ResourceManager.TileObjectPrefab selectedPrefab;

	public void SetSelectedPrefab(ResourceManager.TileObjectPrefab newSelectedPrefab) {
		if (newSelectedPrefab != selectedPrefab) {
			if (newSelectedPrefab != null) {
				selectedPrefab = newSelectedPrefab;
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

	void Update() {
		GetJobSelectionArea();
		if (timeM.timeModifier > 0) {
			GiveJobsToColonists();
		}
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
					uiM.SelectionSizeCanvasSetActive(false);
				}
				SelectedPrefabPreview();
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
		/*
		if (enableSelectionPreview && selectedPrefab != null) {
			print("Not selecting area");
			if (!selectedPrefabPreview.activeSelf) {
				selectedPrefabPreview.SetActive(false);
				selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
				selectionSizePanel.SetActive(true);
			}
			SelectedPrefabPreview();
		} else if (selectedPrefabPreview.activeSelf) {
			print("Selecting area");
			selectedPrefabPreview.SetActive(true);
			selectionSizePanel.SetActive(false);
		} else {

		}
		*/
	}

	public enum JobTypesEnum { Build, Remove, Mine, PlantFarm, HarvestFarm, PickupResources };

	public enum SelectionModifiersEnum { Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances
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
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerObjectInstances,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.objectInstances.ContainsKey(selectedPrefab.layer) && tile.objectInstances[selectedPrefab.layer] != null) { removeTiles.Add(tile); }
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
			CreateJob(new Job(tile,prefab,colonistM));
		}
	}

	public void CreateJob(Job newJob) {
		jobs.Add(newJob);
		uiM.SetJobElements();
	}

	public void AddExistingJob(Job existingJob) {
		jobs.Add(existingJob);
		uiM.SetJobElements();
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
		List<ResourceManager.Container> sortedContainersByDistance = resourceM.containers.Where(container => container.parentObject.tile.region == colonist.overTile.region).OrderBy(container => pathM.RegionBlockDistance(colonist.overTile.regionBlock,container.parentObject.tile.regionBlock,true,true)).ToList();
		if (sortedContainersByDistance.Count > 0) {
			foreach (ResourceManager.Container container in sortedContainersByDistance) {
				List<ResourceManager.ResourceAmount> resourcesToPickupAtContainer = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.resource == ra.resource) != null)) {
					ResourceManager.ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.resource == resourceAmount.resource);
					if (resourceAmount.amount >= pickupResource.amount) {
						print("Found all of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,pickupResource.amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.amount > 0 && resourceAmount.amount < pickupResource.amount) {
						print("Found some of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,resourceAmount.amount));
						pickupResource.amount -= resourceAmount.amount;
						if (pickupResource.amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					} else {
						print("Found none of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
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
					print("Didn't find all resources in containers. Missed " + resourcesToPickup.Count + " resources");
					return null;
				}
			} else {
				print("Didn't find any containers which contain the resources the colonist needs");
				return null;
			}
		} else {
			print("Didn't find any valid containers");
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
					print("Found all of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				} else if (colonistResourceAmount.amount > 0 && colonistResourceAmount.amount < resourceAmount.amount) {
					colonistHasAllResources = false;
					print("Found some of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,colonistResourceAmount.amount));
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount - colonistResourceAmount.amount));
				} else {
					colonistHasAllResources = false;
					print("Found none of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				}
			} else {
				colonistHasAllResources = false;
				resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
			}
		}
		if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count > 0) {
			print("Found " + resourcesToPickup.Count + " that " + colonist.name + " needs to pickup");
		} else if (resourcesToPickup.Count <= 0 && resourcesColonistHas.Count > 0) {
			print("Found all resources in " + colonist.name);
		} else if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count <= 0) {
			print("Found no resources in " + colonist.name);
		}
		return new KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceManager.ResourceAmount>>() { (resourcesToPickup.Count > 0 ? resourcesToPickup : null),(resourcesColonistHas.Count > 0 ? resourcesColonistHas : null) });
	}

	public float CalculateJobCost(ColonistManager.Colonist colonist,Job job) {
		float cost = 0;
		cost += pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true);
		cost -= colonist.GetSkillFromJobType(job.prefab.jobType).level * 10f;
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

			cost = jobM.CalculateJobCost(colonist,job);
			if (containerPickups != null) {
				for (int i = 0; i < containerPickups.Count; i++) {
					if (i == 0) {
						cost += pathM.RegionBlockDistance(colonist.overTile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true);
					} else {
						cost += pathM.RegionBlockDistance(containerPickups[i - 1].container.parentObject.tile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true);
					}
				}
			}
		}
	}

	public void GiveJobsToColonists() {
		Dictionary<ColonistManager.Colonist,List<ColonistJob>> colonistJobs = new Dictionary<ColonistManager.Colonist,List<ColonistJob>>();
		List<ColonistManager.Colonist> availableColonists = colonistM.colonists.Where(colonist => colonist.job == null).ToList();
		foreach (ColonistManager.Colonist colonist in availableColonists) {
			List<Job> sortedJobs = jobs.Where(job => job.tile.region == colonist.overTile.region).OrderBy(job => CalculateJobCost(colonist,job)).ToList();
			List<ColonistJob> validJobs = new List<ColonistJob>();
			foreach (Job job in sortedJobs) {
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
			}
			if (validJobs.Count > 0) {
				colonistJobs.Add(colonist,validJobs.OrderBy(job => job.cost).ToList());
			}
		}

		// IMPLEMENT ACTUAL DISTRIBUTION OF JOBS NOW
		foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null) {
				foreach (ColonistJob colonistJob in colonistJobsList) {
					bool bestColonistForJob = true;
					foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> otherColonistKVP in colonistJobs) {
						if (colonistKVP.Key != otherColonistKVP.Key && otherColonistKVP.Key.job == null) {
							ColonistJob otherColonistJob = otherColonistKVP.Value.Find(job => job == colonistJob);
							if (otherColonistJob != null && otherColonistJob.cost < colonistJob.cost) {
								bestColonistForJob = false;
								break;
							}
						}
					}
					if (bestColonistForJob) {
						colonist.SetJob(colonistJob);
					}
				}
			}
		}

		/*
		if (jobs.Count > 0) {
			bool updateJobListUI = false;
			for (int i = 0; i < jobs.Count; i++) {
				Job job = jobs[i];
				List<ColonistManager.Colonist> availableColonists = colonistM.colonists.Where(colonist => colonist.job == null && job.tile.region == colonist.overTile.region).ToList();
				if (availableColonists.Count > 0) {
					//List<ColonistManager.Colonist> sortedColonists = availableColonists.OrderBy(colonist => pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true)).ToList();
					List<ColonistManager.Colonist> sortedColonists = availableColonists.OrderBy(colonist => CalculateJobCost(colonist,job)).ToList();
					foreach (ColonistManager.Colonist colonist in sortedColonists) {
						colonist.SetJob(job);
						jobs.RemoveAt(i);
						i -= 1;
						updateJobListUI = true;
						break;
					}
				}
			}
			if (updateJobListUI) {
				uiM.SetJobElements();
			}
		}
		*/
		/*
		if (availableColonists.Count > 0) {
			bool gaveJob = false;
			foreach (ColonistManager.Colonist colonist in availableColonists) {
				List<Job> sortedJobs = jobs.Where(job => (job.tile.surroundingTiles.Find(tile => tile != null && tile.region == colonist.overTile.region) != null) || (job.tile.region == colonist.overTile.region)).OrderBy(job => pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true)).ToList();
				if (sortedJobs.Count > 0) {
					if (availableColonists.OrderBy(c => pathM.RegionBlockDistance(c.overTile.regionBlock,sortedJobs[0].tile.regionBlock,true,true)).ToList()[0] == colonist) {
						colonist.SetJob(sortedJobs[0]);
						jobs.Remove(sortedJobs[0]);
						gaveJob = true;
					} else {
						continue;
					}
				}
			}
			if (gaveJob) {
				uiM.SetJobList();
			}
		}
		*/
	}
}
