# Architecture Overview

Snowship is a 2D colony-survival simulation built in Unity 6 with URP 2D. The player:
1. generates a **planet**
2. picks a tile on it and founds a **colony**
3. generates a local **map** from that tile's climate
4. directs **colonists** through **jobs** (build, mine, farm, craft, haul) while they manage their needs

Caravans of traders visit. Everything runs in a single scene: `Assets/Scenes/Game.unity`.

## Tech stack

| Concern | Technology |
|---|---|
| Engine | Unity 6000.6.3f1 (`ProjectSettings/ProjectVersion.txt`); the upgrade is uncommitted, see GIT-1 in [issues.md](issues.md) |
| Rendering | URP 2D (`Assets/URP-2D*.asset`); tiles and objects are `SpriteRenderer`s on individual GameObjects (no Tilemap) |
| DI / composition root | **VContainer** (`jp.hadashikick.vcontainer`, git) |
| Async | **UniTask** (`com.cysharp.unitask`, git) |
| Tweening | **LitMotion** (`com.annulusgames.lit-motion`, git); used for the camera |
| Input | Input System, with generated wrapper `Assets/Code/Input/InputSystemActions.cs` from `Assets/Input/InputSystemActions.inputactions`; legacy `Input` is still used in places (INPUT-1) |
| Asset loading | Addressables (UI prefabs, game prefabs, job icons) + `Resources.Load` (sprites, legacy data) + `StreamingAssets` (new JSON data) |
| JSON | Newtonsoft.Json (entities, materials, persistence) |
| UI | uGUI + TextMeshPro, with a custom MVP framework (see [systems/ui.md](systems/ui.md)) |
| Other | `com.rywh.ryui` (the author's UI package, a dependency but currently unused in code), Localization, Burst/Collections/Mathematics (`Unity.Mathematics.Random` drives `Snowship.Random`) |

Everything under `Assets/Code` lives in **one assembly**, `Assets/Code/Snowship.asmdef` (root namespace
`Snowship`). Editor tools in `Assets/Editor` compile into `Assembly-CSharp-Editor`.

## Composition root & service access

- **`Assets/Code/GameLifetimeScope.cs`**: a VContainer `LifetimeScope` in the scene.
  - `Configure()` registers every manager, either directly or through a per-module `*ServiceInstaller.Install(builder)`.
  - Managers implement VContainer entry-point interfaces (`IInitializable`, `IStartable`, `IAsyncStartable`, `IPostStartable`, `ITickable`, `IDisposable`) to get lifecycle callbacks and a per-frame `Tick()`.
- **`Assets/Code/SharedReferences.cs`**: a scene MonoBehaviour holding scene references (camera, canvas, parents for tiles/life/jobs/selection, the job preview renderer, placeholder sprites). It is injected as an instance.
- **`GameManager.Get<T>()`** (`Assets/Code/GameManager.cs`): a *static service locator*.
  - `ServiceLocatorBridge` (an entry point) hands it the resolver.
  - Plain model objects that aren't DI-constructed (Tile, Job, Life, Region, Inventory…) use it to reach managers. This is tech debt (CORE-1).
  - Prefer constructor injection in anything DI-created.
- **Provider pattern** (Map, Camera, Colony, Human, Colonist, Resource): state and events live in a `*Provider` class that implements narrow interfaces:
  - `I<X>Query`: read
  - `I<X>Write`: mutate
  - `I<X>Events`: subscribe
  - The matching `*Manager` holds the behaviour. Consumers depend on the narrowest interface they need.

## Game state machine

`Assets/Code/State/StateManager.cs` owns a dictionary of `State<EState>` objects (`Assets/Code/State/States/`).
Each state declares `ValidNextStates` and `ActionsOnTransition` (async functions that receive the
resolver, usually to open a UI view). `TransitionToState(newState, ETransitionUIAction)` does four things:
1. checks legality
2. hides or closes all UI
3. runs the actions
4. raises `IStateEvents.OnStateChanged`

```
Boot ──► MainMenu ──► LoadToSimulation ──► Simulation ──► PauseMenu ──► Simulation
            ▲                                  │  ├──► Saving ──► PauseMenu/Simulation
            └────────── QuitToMenu ◄───────────┘  └──► QuitToDesktop
```
Many systems gate their `Tick` on `stateQuery.State == EState.Simulation`. Known flow gaps: FLOW-1 to FLOW-3.

## End-to-end flow: new game

1. **Boot.**
   - VContainer builds the container, and each entry point starts.
   - Async loaders fire in parallel: TileManager loads the Tile prefab, ResourceManager loads the Object prefab and then initialises the static tile, biome and vein data, HumanManager loads the Human prefab, MaterialProvider reads `materials.json`, and SelectionManager loads its indicator prefab.
   - `StateManager.Start` → Boot → MainMenu → `UIMainMenu` opens.
2. **Create Planet** (`UICreatePlanetPresenter`).
   - `PlanetManager.CreatePlanet(CreatePlanetData)` runs `MapGenerator` with `actualMap = false`. That produces a small coarse map, turned into `PlanetTile`s whose GameObjects are destroyed.
   - `PlanetViewModule` renders it as UI tiles.
3. **Create Colony** (`UICreateColonyPresenter`).
   - `ColonyManager.CreateColony(CreateColonyData)` builds `MapData` from the chosen `PlanetTile`'s climate and its neighbours' height and river hints.
   - It stores the data via `IMapWrite.SetMapData` and raises `IColonyEvents.OnColonyCreated`.
4. **MapManager.OnColonyCreated.**
   - Transitions to `LoadToSimulation` (loading screen).
   - Creates the `Map`, `IMapWrite.SetMap`, which raises `MapSet`, so the camera initialises.
   - Awaits the full `MapGenerator.Run`, which updates the loading-screen text through `UIEvents`.
   - Hooks camera and time events into the map.
   - Raises `OnMapCreated`, so `ColonyManager` spawns 3 colonists and `LifeManager` hooks lighting.
   - Transitions to `Simulation`, which opens `UISimulation` with UIDateTime and UIActionsPanel.
5. **Simulation loop.** Every `ITickable` ticks each frame.
   - **Time:** `TimeManager` advances in-game minutes and raises `OnTimeChanged` once per game minute. Most simulation logic hangs off this: jobs progress, job assignment, needs, moods, plant growth, caravan timer, lighting every 10 minutes.
   - **Colonists:** `ColonistManager.Tick` updates each colonist (movement along its path, wandering).
   - **Selection:** `SelectionManager` turns drag-selection into job instances. `JobManager.AssignJobs` pairs free colonists with jobs.

**Debug QuickStart:** with the Editor toggle *Snowship → Snowship Debug Settings → Quick Start* on,
`DebugManager.PostStart` generates a planet and colony automatically.

## Module map

| Module (folder under `Assets/Code`) | Namespace | Doc |
|---|---|---|
| Composition root, State, Settings, Time, Input, Camera, Simulation, Universe, Ship | `Snowship`, `NState`, `NSettings`, `NTime`, `NInput`, `NCamera` | [systems/core.md](systems/core.md) |
| Map (generation, tiles, regions, lighting, rendering) | `NMap`, `NMap.NTile`, `NMap.Generation` | [systems/map.md](systems/map.md) |
| Planet, Colony | `NPlanet`, `NColony` | [systems/planet-colony.md](systems/planet-colony.md) |
| Entity (new component-based objects) | `NEntity` | [systems/entity.md](systems/entity.md) |
| Materials (new JSON resource model) | `NMaterial` | [systems/materials.md](systems/materials.md) |
| Resource (legacy resources, objects, plants, inventory) | `NResource` | [systems/resources-objects.md](systems/resources-objects.md) |
| Life, Human, Colonist (needs, moods, skills, professions) | `NLife`, `NHuman`, `NColonist`, `NProfession` | [systems/life-humans.md](systems/life-humans.md) |
| Job | `NJob` | [systems/jobs.md](systems/jobs.md) |
| Selection | `NSelection` | [systems/selection.md](systems/selection.md) |
| UI | `NUI` | [systems/ui.md](systems/ui.md) |
| Caravan (traders, trade) | `NCaravan` | [systems/caravans-trade.md](systems/caravans-trade.md) |
| Persistence | `NPersistence` | [systems/persistence.md](systems/persistence.md) |
| Debug console + Editor tools | global / `NEditor` | [systems/debug-and-tools.md](systems/debug-and-tools.md) |
| Utilities (Random, Path, colours, ids) | `Snowship`, `NUtilities`, `NPath` | [systems/core.md](systems/core.md#utilities) |
| Data files & asset keys | — | [data.md](data.md) |

## Two generations of code

The codebase is mid-migration, and knowing which side a file is on matters:

- **Legacy (2018–2021 era), still load-bearing:**
  - static registries plus text data under `Assets/Resources/Data/*.txt`
  - `Resource`/`ObjectPrefab`/`ObjectInstance`, `TileType`, `Biome`, prefab classes for needs, moods, skills and professions
- **Newer (2024–2026):**
  - VContainer managers and providers
  - the MVP UI framework
  - the attribute-registered job system
  - the component-based `Entity` + JSON `Material` pipeline (not yet connected to gameplay)
  - the `ISaveable` persistence framework (skeleton)
- **Dead code:** files wrapped in `#if OLD_CODE` (`Assets/Code/UI/Old/*`, `Assets/Code/Job/Old_Unused/*`) are excluded from compilation and kept for reference only.

The intended direction is to replace `ObjectPrefab`/`ObjectInstance` with `Entity` and `Resource` with
`Material`. A feature flag controls which system the build menu uses: *Debug Settings → "Use Entity System for Buildings"*, read in `UIActionsPanelPresenter.CreateBuildButtons`.
