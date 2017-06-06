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

	private GameObject mainMenu;

	private TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	public int mapSize;

	void Awake() {
		tileM = GetComponent<TileManager>();
		jobM = GetComponent<JobManager>();
		resourceM = GetComponent<ResourceManager>();
		cameraM = GetComponent<CameraManager>();

		GameObject.Find("PlayButton").GetComponent<Button>().onClick.AddListener(delegate { PlayButton(); });

		GameObject.Find("MapSize-Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMapSizeText(); });
		GameObject.Find("MapSize-Slider").GetComponent<Slider>().value = 2;

		mainMenu = GameObject.Find("MainMenu");

		tileInformation = GameObject.Find("TileInformation-Panel");
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
				tileLayerSpriteObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
				tileLayerSpriteObject.GetComponent<Image>().sprite = mouseOverTile.plant.GetComponent<SpriteRenderer>().sprite;
				tileObjects.Add(tileLayerSpriteObject);
			}
			if (mouseOverTile.GetAllObjectInstances().Count > 0) {
				foreach (ResourceManager.TileObjectInstance tileObject in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
					GameObject tileLayerSpriteObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-TileImage"),tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-TileImage"),false);
					tileLayerSpriteObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
					tileLayerSpriteObject.GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;
					tileObjects.Add(tileLayerSpriteObject);

					GameObject tileObjectDataObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-ObjectData-Panel"),tileInformation.transform.Find("TileInformation-ObjectDataLayoutGroup-Panel"),false);
					tileObjectDataObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
					tileObjectDataObject.transform.Find("TileInformation-ObjectData-Label").GetComponent<Text>().text = "L" + tileObject.prefab.layer;
					tileObjectDataObject.transform.Find("TileInformation-ObjectData-Value").GetComponent<Text>().text = tileObject.prefab.name;
					tileObjects.Add(tileObjectDataObject);
				}
			} else {
				GameObject tileObjectDataObject = Instantiate(Resources.Load<GameObject>(@"UI/TileInformation-ObjectData-Panel"),tileInformation.transform.Find("TileInformation-ObjectDataLayoutGroup-Panel"),false);
				tileObjectDataObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
				tileObjectDataObject.transform.Find("TileInformation-ObjectData-Label").GetComponent<Text>().text = "";
				tileObjectDataObject.transform.Find("TileInformation-ObjectData-Value").GetComponent<Text>().text = "";
				tileObjects.Add(tileObjectDataObject);
			}
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "<b>Position</b>\n(" + Mathf.FloorToInt(mouseOverTile.obj.transform.position.x) + ", " + Mathf.FloorToInt(mouseOverTile.obj.transform.position.y) + ")";
		} else {
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "<b>Position</b>\n";
		}
	}
}
