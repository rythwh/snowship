using System;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.Selectable;
using Snowship.NTime;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.NTile;
using Snowship;
using Snowship.NCamera;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NInput;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NSettings;
using Snowship.NState;
using Snowship.NUI;
using Snowship.Persistence;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static readonly (int increment, string text) GameVersion = (3, "2025.1");

	public static SharedReferences SharedReferences { get; private set; }

	private static readonly Dictionary<Type, Manager> managersMap = new();
	private static readonly List<Manager> managers = new();

	public void Awake() {

		SharedReferences = GetComponent<SharedReferences>();

		// Awakes
		CreateManagers();

		foreach (Manager manager in managers) {
			manager.OnCreate();
			manager.Created = true;
		}

		SpecialManagerSetups();
	}

	private void CreateManagers() {
		Create<SettingsManager>();
		Create<InputManager>();
		Create<TimeManager>();
		Create<CameraManager>();
		Create<ResourceManager>();
		Create<BuildableManager>();
		Create<LifeManager>();
		Create<HumanManager>();
		Create<ColonistManager>();
		Create<CaravanManager>();
		Create<JobManager>();
		Create<MapManager>();
		Create<TileManager>();
		Create<ColonyManager>();
		Create<PlanetManager>();
		Create<UniverseManager>();
		Create<UIManager>();
		Create<SelectionManager>();
		Create<DebugManager>();
		Create<StateManager>();
		Create<PersistenceManager>();
	}

	private void SpecialManagerSetups() {
		// TODO Move these to be called from their respective files
		TileType.InitializeTileTypes();
		Biome.InitializeBiomes();
		ResourceVein.InitializeResourceVeins();

		Get<HumanManager>().LoadNames();
		Get<HumanManager>().LoadSprites();

		SkillPrefab.CreateColonistSkills(); // TODO (Solution: Use string references which can be converted to the correct Prefab obj when needed) Skills must currently be ahead of professions to determine skill-profession relationship
		ProfessionPrefab.CreateProfessionPrefabs();
		NeedPrefab.CreateColonistNeeds();
		MoodModifierGroup.CreateMoodModifiers();

		// Get<UIManagerOld>().SetupUI();
	}

	public async void Start() {

		await UniTask.WaitUntil(() => managers.TrueForAll(manager => manager.Created));

		foreach (Manager manager in managers) {
			manager.OnGameSetupComplete();
			manager.PostGameSetupCompleted = true;
		}
	}

	public async void Update() {

		await UniTask.WaitUntil(() => managers.TrueForAll(manager => manager.PostGameSetupCompleted));

		foreach (IManager manager in managers) {
			manager.OnUpdate();
		}
	}

	private static void Create<TManager>() where TManager : Manager, new() {
		TManager manager = new();
		managersMap.Add(typeof(TManager), manager);
		managers.Add(manager);
	}

	public static TManager Get<TManager>() where TManager : class, IManager {
		return managersMap[typeof(TManager)] as TManager;
	}
}
