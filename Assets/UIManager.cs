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


	public int mapSize;


	private GameObject mainMenu;

	private TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	private GameObject colonistListToggleButton;
	private GameObject colonistList;
	private GameObject jobListToggleButton;
	private GameObject jobList;

	void Awake() {
		tileM = GetComponent<TileManager>();
		jobM = GetComponent<JobManager>();
		resourceM = GetComponent<ResourceManager>();
		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();

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

	List<GameObject> colonistSpriteObjects = new List<GameObject>();

	public void UpdateColonistList() {
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
}