using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static readonly CameraManager cameraM = new CameraManager();
	public static readonly CaravanManager caravanM = new CaravanManager();
	public static readonly ColonistManager colonistM = new ColonistManager();
	public static readonly ColonyManager colonyM = new ColonyManager();
	public static readonly DebugManager debugM = new DebugManager();
	public static readonly HumanManager humanM = new HumanManager();
	public static readonly JobManager jobM = new JobManager();
	public static readonly LifeManager lifeM = new LifeManager();
	public static readonly PersistenceManager persistenceM = new PersistenceManager();
	public static readonly PlanetManager planetM = new PlanetManager();
	public static readonly ResourceManager resourceM = new ResourceManager();
	public static readonly TileManager tileM = new TileManager();
	public static readonly TimeManager timeM = new TimeManager();
	public static readonly UIManager uiM = new UIManager();
	public static readonly UniverseManager universeM = new UniverseManager();

	public static readonly List<BaseManager> managers = new List<BaseManager>() {
		cameraM,
		caravanM,
		colonistM,
		debugM,
		humanM,
		jobM,
		lifeM,
		persistenceM,
		planetM,
		resourceM,
		tileM,
		timeM,
		uiM,
		universeM
	};

	public void Awake() {
		tileM.SetStartCoroutineReference(this);
		persistenceM.SetStartCoroutineReference(this);
		uiM.SetStartCoroutineReference(this);

		resourceM.GetResourceReferences();
		resourceM.CreateResources();
		resourceM.CreateTileObjectPrefabs();
		resourceM.CreatePlantGroups();

		tileM.CreateTileTypes();
		tileM.CreateBiomes();
		tileM.CreateBiomeRanges();

		tileM.InitializeResourceVeinValidTileFunctions();

		humanM.CreateNames();
		humanM.CreateHumanSprites();

		colonistM.CreateColonistSkills();
		colonistM.CreateColonistProfessions();
		colonistM.CreateColonistNeeds();
		colonistM.CreateHappinessModifiers();

		colonistM.InitializeNeedsValueFunctions();
		colonistM.InitializeHappinessModifierFunctions();

		jobM.InitializeSelectionModifierFunctions();
		jobM.InitializeFinishJobFunctions();
		jobM.InitializeJobDescriptionFunctions();

		foreach (BaseManager manager in managers) {
			manager.Awake();
		}

		uiM.SetupUI();

		persistenceM.CreateSettingsState();
	}

	public void Start() {
		foreach (BaseManager manager in managers) {
			manager.Start();
		}
	}

	public void Update() {
		foreach (BaseManager manager in managers) {
			manager.Update();
		}
	}
}