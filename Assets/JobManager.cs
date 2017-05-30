using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JobManager : MonoBehaviour {

	private TileManager tileM;
	private ColonistManager colonistM;
	private CameraManager cameraM;

	void Awake() {
		tileM = GetComponent<TileManager>();
		colonistM = GetComponent<ColonistManager>();
		cameraM = GetComponent<CameraManager>();
	}

	public List<Job> jobs = new List<Job>();

	public class Job {
		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public GameObject jobPreview;

		public bool accessible;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab, ColonistManager colonistM) {
			this.tile = tile;
			this.prefab = prefab;

			jobPreview = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform.position,Quaternion.identity);
			jobPreview.name = "JobPreview: " + prefab.name + " at " + tile.obj.transform.position;
			jobPreview.transform.SetParent(tile.obj.transform);

			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
				jPSR.sortingOrder = 2;
			}
			jPSR.color = new Color(1f,1f,1f,0.25f);

			accessible = false;
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.overTile.region == tile.region) {
					accessible = true;
					break;
				}
			}
		}

		public void SetColonist(ColonistManager.Colonist colonist) {
			this.colonist = colonist;
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

	void Update() {
		GetJobSelectionArea();
		GiveJobsToColonists();
	}

	public enum SelectionTypesEnum { All, AllBuildable, Outline, OnlyStoneTypes, OnlyAllWaterTypes, OnlyLiquidWaterTypes, OmitAnythingOnTile, OnlyPlants, OmitPlants, OnlyObjects, OmitObjects, OnlyWalkable, OmitWalkable, OnlyFloors, OmitFloors };

	private List<GameObject> selectionIndicators = new List<GameObject>();

	public TileManager.Tile firstTile;
	private bool stopSelection;

	public void StopSelection() {
		stopSelection = true;
	}

	public void GetJobSelectionArea() {

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
					float smallerY = Mathf.Min(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					for (float y = smallerY; y < ((largerY - smallerY) + smallerY + 1); y++) {
						for (float x = smallerX; x < ((largerX - smallerX) + smallerX + 1); x++) {
							TileManager.Tile tile = tileM.GetTileFromPosition(new Vector2(x,y));
							if (selectedPrefab.selectionType == SelectionTypesEnum.All) {
								selectionArea.Add(tile);
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.AllBuildable) {
								if (tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.Outline) {
								if (x == smallerX || y == smallerY || x == ((largerX - smallerX) + smallerX) || y == ((largerY - smallerY) + smallerY)) {
									if (tile.tileType.buildable) {
										selectionArea.Add(tile);
									}
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyStoneTypes) {
								if (tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyAllWaterTypes) {
								if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyLiquidWaterTypes) {
								if (tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OmitAnythingOnTile) {
								if (tile.plant == null && tile.objectInstance == null && tile.floorInstance == null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyPlants) {
								if (tile.plant != null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OmitPlants) {
								if (tile.plant == null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyObjects) {
								if (tile.objectInstance != null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OmitObjects) {
								if (tile.objectInstance == null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyWalkable) {
								if (tile.walkable && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OmitWalkable) {
								if (!tile.walkable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OnlyFloors) {
								if (tile.floorInstance != null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							} else if (selectedPrefab.selectionType == SelectionTypesEnum.OmitFloors) {
								if (tile.floorInstance == null && tile.tileType.buildable) {
									selectionArea.Add(tile);
								}
							}
						}
					}

					foreach (TileManager.Tile tile in selectionArea) {
						GameObject selectionIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
						SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
						sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
						sISR.sortingOrder = 2;
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

	public void CreateJobsInSelectionArea(ResourceManager.TileObjectPrefab prefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			CreateJob(new Job(tile,prefab,colonistM));
		}
	}

	public void CreateJob(Job newJob) {
		jobs.Add(newJob);
	}

	public void AddExistingJob(Job existingJob) {
		jobs.Add(existingJob);
	}

	public void GiveJobsToColonists() {
		List<Job> jobsToRemove = new List<Job>();
		foreach (Job job in jobs) {
			bool gaveJob = false;
			if (job.accessible) {
				List<ColonistManager.Colonist> sortedColonists = colonistM.colonists.Where(c => c.job == null).OrderBy(c => Vector2.Distance(c.overTile.obj.transform.position,job.tile.obj.transform.position)).ToList();
				foreach (ColonistManager.Colonist colonist in sortedColonists) {
					if (colonist.path.Count <= 0) {
						colonist.SetJob(job);
						jobsToRemove.Add(job);
						gaveJob = true;
						break;
					}
				}
			}
			if (gaveJob) {
				continue;
			}
		}
		foreach (Job job in jobsToRemove) {
			jobs.Remove(job);
		}
		jobsToRemove.Clear();
	}
}
