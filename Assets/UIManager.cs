using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class UIManager:MonoBehaviour {

	public string SplitByCapitals(string combinedString) {
		var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
				 RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString," ");
	}

	private TileManager tileM;
	private JobManager jobM;
	private ResourceManager resourceM;
	private CameraManager cameraM;
	private ColonistManager colonistM;
	private TimeManager timeM;

	public int mapSize;


	private GameObject mainMenu;

	private TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	private GameObject colonistListToggleButton;
	private GameObject colonistList;
	private GameObject jobListToggleButton;
	private GameObject jobList;

	private GameObject selectedColonistInformationPanel;

	private GameObject dateTimeInformationPanel;

	void Awake() {
		tileM = GetComponent<TileManager>();
		jobM = GetComponent<JobManager>();
		resourceM = GetComponent<ResourceManager>();
		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		timeM = GetComponent<TimeManager>();

		GameObject.Find("PlayButton").GetComponent<Button>().onClick.AddListener(delegate { PlayButton(); });

		GameObject.Find("MapSize-Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMapSizeText(); });
		GameObject.Find("MapSize-Slider").GetComponent<Slider>().value = 2;

		mainMenu = GameObject.Find("MainMenu");

		tileInformation = GameObject.Find("TileInformation-Panel");

		colonistListToggleButton = GameObject.Find("ColonistListToggle-Button");
		colonistList = GameObject.Find("ColonistList-ScrollPanel");
		colonistListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { colonistList.SetActive(!colonistList.activeSelf); });
		jobListToggleButton = GameObject.Find("JobListToggle-Button");
		jobList = GameObject.Find("JobList-ScrollPanel");
		jobListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { jobList.SetActive(!jobList.activeSelf); });

		selectedColonistInformationPanel = GameObject.Find("SelectedColonistInfo-Panel");

		dateTimeInformationPanel = GameObject.Find("DateTimeInformation-Panel");
	}

	void Update() {
		if (tileM.generated) {
			Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile newMouseOverTile = tileM.GetTileFromPosition(mousePosition);
			if (newMouseOverTile != mouseOverTile) {
				mouseOverTile = newMouseOverTile;
				UpdateTileInformation();
			}
			if (Input.GetMouseButtonDown(1)) {
				if (jobM.firstTile != null) {
					jobM.StopSelection();
				} else {
					jobM.SetSelectedPrefab(null);
				}
			}
			if (colonistM.selectedColonist != null) {
				UpdateSelectedColonistInformation();
			}
		}
	}

	public void PlayButton() {
		string mapSeedString = GameObject.Find("MapSeedInput-Text").GetComponent<Text>().text;
		if (string.IsNullOrEmpty(mapSeedString)) {
			mapSeedString = UnityEngine.Random.Range(0,int.MaxValue).ToString();
		}
		int mapSeed = 0;
		if (!int.TryParse(mapSeedString,out mapSeed)) {
			foreach (char c in mapSeedString) {
				mapSeed += c;
			}
		}
		tileM.Initialize(mapSize,mapSeed);
		mainMenu.SetActive(false);
	}

	public void UpdateMapSizeText() {
		mapSize = Mathf.RoundToInt(GameObject.Find("MapSize-Slider").GetComponent<Slider>().value * 50);
		GameObject.Find("MapSizeValue-Text").GetComponent<Text>().text = mapSize.ToString();
	}

	public Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>> menuStructure = new Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>();

	public void CreateBuildMenuButtons() {
		foreach (ResourceManager.TileObjectPrefabGroup topg in resourceM.tileObjectPrefabGroups) {
			GameObject buildMenuGroupButton = Instantiate(Resources.Load<GameObject>(@"UI/BuildItem-Button-Prefab"),GameObject.Find("BuildMenu-Panel").transform,false);
			buildMenuGroupButton.transform.Find("Text").GetComponent<Text>().text = topg.name;

			GameObject groupPanel = buildMenuGroupButton.transform.Find("Panel").gameObject;

			Dictionary<GameObject,List<GameObject>> subGroupPanels = new Dictionary<GameObject,List<GameObject>>();
			foreach (ResourceManager.TileObjectPrefabSubGroup topsg in topg.tileObjectPrefabSubGroups) {
				buildMenuGroupButton.transform.Find("Panel").GetComponent<GridLayoutGroup>().cellSize = new Vector2(100,21);
				GameObject buildMenuSubGroupButton = Instantiate(Resources.Load<GameObject>(@"UI/BuildItem-Button-Prefab"),buildMenuGroupButton.transform.Find("Panel").transform,false);
				buildMenuSubGroupButton.transform.Find("Text").GetComponent<Text>().text = topsg.name;

				GameObject subGroupPanel = buildMenuSubGroupButton.transform.Find("Panel").gameObject;

				List<GameObject> objectButtons = new List<GameObject>();
				foreach (ResourceManager.TileObjectPrefab top in topsg.tileObjectPrefabs) {
					GameObject buildMenuObjectButton = Instantiate(Resources.Load<GameObject>(@"UI/BuildObject-Button-Prefab"),buildMenuSubGroupButton.transform.Find("Panel").transform,false);
					buildMenuObjectButton.transform.Find("Text").GetComponent<Text>().text = top.name;
					if (top.baseSprite != null) {
						buildMenuObjectButton.transform.Find("Image").GetComponent<Image>().sprite = top.baseSprite;
					}
					objectButtons.Add(buildMenuObjectButton);
					buildMenuObjectButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(top); });
				}
				subGroupPanels.Add(subGroupPanel,objectButtons);

				buildMenuSubGroupButton.GetComponent<Button>().onClick.AddListener(delegate {
					foreach (KeyValuePair<GameObject,List<GameObject>> kvp2 in subGroupPanels) {
						if (kvp2.Key != subGroupPanel) {
							kvp2.Key.SetActive(false);
						}
					}
				});
				buildMenuSubGroupButton.GetComponent<Button>().onClick.AddListener(delegate { subGroupPanel.SetActive(!subGroupPanel.activeSelf); });

				subGroupPanel.SetActive(false);
			}
			menuStructure.Add(groupPanel,subGroupPanels);

			buildMenuGroupButton.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject,Dictionary<GameObject,List<GameObject>>> kvp in menuStructure) {
					if (kvp.Key != groupPanel) {
						kvp.Key.SetActive(false);
					}
					foreach (KeyValuePair<GameObject,List<GameObject>> kvp2 in kvp.Value) {
						kvp2.Key.SetActive(false);
					}
				}
			});
			buildMenuGroupButton.GetComponent<Button>().onClick.AddListener(delegate { groupPanel.SetActive(!groupPanel.activeSelf); });

			groupPanel.SetActive(false);
		}

		GameObject buildMenuButton = GameObject.Find("BuildMenu-Button");
		GameObject buildMenuPanel = GameObject.Find("BuildMenu-Panel");
		buildMenuButton.GetComponent<Button>().onClick.AddListener(delegate {
			foreach (KeyValuePair<GameObject,Dictionary<GameObject,List<GameObject>>> kvp in menuStructure) {
				kvp.Key.SetActive(false);
				foreach (KeyValuePair<GameObject,List<GameObject>> kvp2 in kvp.Value) {
					kvp2.Key.SetActive(false);
				}
			}
		});
		buildMenuButton.GetComponent<Button>().onClick.AddListener(delegate { buildMenuPanel.SetActive(!buildMenuPanel.activeSelf); });
		buildMenuPanel.SetActive(false);
	}

	private List<GameObject> tileObjects = new List<GameObject>();
	public void UpdateTileInformation() {
		if (mouseOverTile != null) {
			foreach (GameObject tileLayerSpriteObject in tileObjects) {
				Destroy(tileLayerSpriteObject);
			}
			tileObjects.Clear();

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-TileImage").GetComponent<Image>().sprite = mouseOverTile.obj.GetComponent<SpriteRenderer>().sprite;

			if (mouseOverTile.plant != null) {
				GameObject tileLayerSpriteObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-TileImage"),tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-TileImage"),false);
				tileLayerSpriteObject.GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;
				tileObjects.Add(tileLayerSpriteObject);

				GameObject tileObjectDataObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-ObjectData-Panel"),tileInformation.transform,false);
				tileObjectDataObject.transform.Find("TileInformation-ObjectData-Label").GetComponent<Text>().text = "Plant";
				tileObjectDataObject.transform.Find("TileInformation-ObjectData-Value").GetComponent<Text>().text = mouseOverTile.plant.group.name;
				tileObjects.Add(tileObjectDataObject);
			}
			if (mouseOverTile.GetAllObjectInstances().Count > 0) {
				foreach (ResourceManager.TileObjectInstance tileObject in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
					GameObject tileLayerSpriteObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-TileImage"),tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-TileImage"),false);
					tileLayerSpriteObject.GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;
					tileObjects.Add(tileLayerSpriteObject);

					GameObject tileObjectDataObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-ObjectData-Panel"),tileInformation.transform,false);
					tileObjectDataObject.transform.Find("TileInformation-ObjectData-Label").GetComponent<Text>().text = "L" + tileObject.prefab.layer;
					tileObjectDataObject.transform.Find("TileInformation-ObjectData-Value").GetComponent<Text>().text = tileObject.prefab.name;
					tileObjects.Add(tileObjectDataObject);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "<b>Position</b>\n(" + Mathf.FloorToInt(mouseOverTile.obj.transform.position.x) + ", " + Mathf.FloorToInt(mouseOverTile.obj.transform.position.y) + ")";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = "<b><size=14>Biome</size></b>\n<size=12>" + mouseOverTile.biome.name + "</size>";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Temperature").GetComponent<Text>().text = "<b>Temperature</b>\n" + Mathf.RoundToInt(mouseOverTile.temperature) + "°C";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Precipitation").GetComponent<Text>().text = "<b>Precipitation</b>\n" + Mathf.RoundToInt(mouseOverTile.precipitation * 100f) + "%";

			if (!tileInformation.activeSelf) {
				tileInformation.SetActive(true);
			}
		} else {
			tileInformation.SetActive(false);
		}
	}

	List<GameObject> skillObjects = new List<GameObject>();

	/* Called from ColonistManager.SetSelectedColonistFromInput(), ColonistManager.SetSelectedColonist(), ColonistManager.DeselectSelectedColonist(), TileManager.Initialize() */
	public void SetSelectedColonistInformation() {
		if (colonistM.selectedColonist != null) {
			selectedColonistInformationPanel.SetActive(true);

			RectTransform rightListPanel = GameObject.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x,310);

			foreach (GameObject skillObject in skillObjects) {
				Destroy(skillObject);
			}
			skillObjects.Clear();

			selectedColonistInformationPanel.transform.Find("SkillsList-Panel/SkillsListTitle-Panel/Profession-Text").GetComponent<Text>().text = colonistM.selectedColonist.profession.name;

			foreach (ColonistManager.SkillInstance skill in colonistM.selectedColonist.skills) {
				GameObject skillObject = Instantiate(Resources.Load<GameObject>(@"UI/SkillInformation-Panel"),selectedColonistInformationPanel.transform.Find("SkillsList-Panel"),false);
				skillObject.transform.Find("SkillName-Text").GetComponent<Text>().text = skill.prefab.name;
				skillObjects.Add(skillObject);
			}
		} else {
			selectedColonistInformationPanel.SetActive(false);

			RectTransform rightListPanel = GameObject.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x,5);

			if (skillObjects.Count > 0) {
				foreach (GameObject skillObject in skillObjects) {
					Destroy(skillObject);
				}
				skillObjects.Clear();
			}
		}
	}

	public void UpdateSelectedColonistInformation() {
		selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().minValue = 0;
		selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().maxValue = colonistM.selectedColonist.inventory.maxAmount;
		selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().value = colonistM.selectedColonist.inventory.CountResources();
		selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventoryValue-Text").GetComponent<Text>().text = colonistM.selectedColonist.inventory.CountResources() + "/ " + colonistM.selectedColonist.inventory.maxAmount;
		int skillIndex = 0;
		foreach (GameObject skillObject in skillObjects) {
			ColonistManager.SkillInstance skill = colonistM.selectedColonist.skills[skillIndex];
			skillObject.transform.Find("SkillLevelValue-Text").GetComponent<Text>().text = "Level " + skill.level + " (+" + Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f) + "%)";
			skillObject.transform.Find("SkillExperience-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f);
			if (skill.level >= 10) {
				skillObject.transform.Find("SkillExperience-Slider/Fill Area/Fill").GetComponent<Image>().color = new Color(192f,57f,43f,255f) / 255f;
				skillObject.transform.Find("SkillExperience-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = new Color(231f,76f,60f,255f) / 255f;
			}
			skillIndex += 1;
		}
	}

	List<GameObject> colonistSpriteObjects = new List<GameObject>();

	/* Called from ColonistManager.SpawnColonists() */
	public void SetColonistList() {
		foreach (GameObject colonistSpriteObject in colonistSpriteObjects) {
			Destroy(colonistSpriteObject);
		}
		colonistSpriteObjects.Clear();
		if (colonistList.activeSelf) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				GameObject colonistBaseSpriteObject = Instantiate(Resources.Load<GameObject>(@"UI/ColonistInformation-Panel"),colonistList.transform.Find("ColonistList-Panel"),false);
				colonistBaseSpriteObject.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = colonist.moveSprites[0];
				colonistBaseSpriteObject.GetComponent<Button>().onClick.AddListener(delegate { colonistM.SetSelectedColonist(colonist); });
				colonistSpriteObjects.Add(colonistBaseSpriteObject);
			}
		}
	}

	List<GameObject> jobElements = new List<GameObject>();

	/* Called from Colonist.FinishJob(), Colonist.CreateJob(), Colonist.AddExistingJob(), JobManager.GiveJobsToColonists(), TileManager.Initialize() */
	public void SetJobList() {
		if (jobM.jobs.Count > 0 || colonistM.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
			jobList.SetActive(true);
			foreach (GameObject jobElement in jobElements) {
				Destroy(jobElement);
			}
			jobElements.Clear();
			foreach (ColonistManager.Colonist jobColonist in colonistM.colonists.Where(colonist => colonist.job != null).ToList()) {
				GameObject jobElement = Instantiate(Resources.Load<GameObject>(@"UI/JobInformation-Panel"),jobList.transform.Find("JobList-Panel"),false);
				jobElement.transform.Find("JobInfo/JobBaseSprite-Image").GetComponent<Image>().sprite = jobColonist.job.prefab.baseSprite;
				jobElement.transform.Find("JobInfo/JobName-Text").GetComponent<Text>().text = jobColonist.job.prefab.name;
				jobElement.GetComponent<Button>().onClick.AddListener(delegate {
					cameraM.SetCameraPosition(jobColonist.job.tile.obj.transform.position);
					cameraM.SetCameraZoom(5);
				});
				jobElement.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
				jobElements.Add(jobElement);

				GameObject colonistJobElement = Instantiate(Resources.Load<GameObject>(@"UI/ColonistInformation-Panel"),jobElement.transform,false);

				colonistJobElement.GetComponent<Outline>().enabled = false;
				jobElement.GetComponent<RectTransform>().sizeDelta = new Vector2(jobElement.GetComponent<RectTransform>().sizeDelta.x,105);

				colonistJobElement.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = jobColonist.moveSprites[0];
				colonistJobElement.GetComponent<Button>().onClick.AddListener(delegate { colonistM.SetSelectedColonist(jobColonist); });
				colonistJobElement.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
				colonistJobElement.GetComponent<RectTransform>().sizeDelta = new Vector2(175,colonistJobElement.GetComponent<RectTransform>().sizeDelta.y);
				jobElements.Add(colonistJobElement);
			}
			foreach (JobManager.Job job in jobM.jobs) {
				GameObject jobElement = Instantiate(Resources.Load<GameObject>(@"UI/JobInformation-Panel"),jobList.transform.Find("JobList-Panel"),false);
				jobElement.transform.Find("JobInfo/JobBaseSprite-Image").GetComponent<Image>().sprite = job.prefab.baseSprite;
				jobElement.transform.Find("JobInfo/JobName-Text").GetComponent<Text>().text = SplitByCapitals(job.prefab.jobType.ToString());
				jobElement.transform.Find("JobInfo/JobName-Text").GetComponent<Text>().text = job.prefab.name;
				jobElement.GetComponent<Button>().onClick.AddListener(delegate {
					cameraM.SetCameraPosition(job.tile.obj.transform.position);
					cameraM.SetCameraZoom(5);
				});
				jobElements.Add(jobElement);
			}
		} else {
			jobList.SetActive(false);
			if (jobElements.Count > 0) {
				foreach (GameObject jobElement in jobElements) {
					Destroy(jobElement);
				}
				jobElements.Clear();
			}
		}
	}

	public void UpdateDateTimeInformation(int minute, int hour, int day, int month, int year, bool isDay) {
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Time-Text").GetComponent<Text>().text = timeM.Get12HourTime() + ":" + (minute < 10 ? ("0" + minute) : minute.ToString()) + (hour < 13 ? "AM" : "PM") + "(" + (isDay ? "D" : "N") + ")";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = (timeM.timeModifier > 0 ? new string('>',timeM.timeModifier) : "-");
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Date-Text").GetComponent<Text>().text = "D" + day + " M" + month + " Y" + year;
	}
}