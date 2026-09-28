# Known Issues & Unfinished Work

The single source of truth for bugs, half-built features and tech debt. System docs in
[systems/](systems/) link here by ID; details live **only** here.

- **IDs are stable.** Never renumber. When an item is fixed, move it to [Resolved](#resolved) with the
  date and a devlog link, and do not reuse its ID.
- **Confidence tags:**
  - **[C] Confirmed.** Verified by reading the code, and certain to behave as described.
  - **[S] Suspected.** Needs a runtime check before you build a fix on it.
- **Priority:**
  - **P0:** won't compile or run.
  - **P1:** breaks the core loop (menu → planet → colony → simulation → jobs → pause/quit).
  - **P2:** a feature that was started but not finished.
  - **P3:** tech debt, performance, cleanup.
- Cite files and symbols, not line numbers.

Last full review: 2026-09-28 (see [devlog](devlog/2026-09-28-initial-review.md)).

---

## P0: Build blockers

*(none open. The project compiles on Unity 6000.6.3f1 as of 2026-09-28; see Resolved BUILD-1, BUILD-2, TOOL-1, GIT-1.)*

---

## P1: Core loop broken

### Entity system (in-progress work; see [systems/entity.md](systems/entity.md))
- **ENT-1 [C] Components added from JSON are all keyed as `IComponent`.**
  - `Entity.AddComponent<TComponent>` keys the component by `typeof(TComponent)`.
  - `JsonEntityLoader.BuildFromDef` calls it with a variable of static type `IComponent`, so every JSON component gets the same key.
  - As a result, the second component throws `InvalidOperationException`, and `TryGet<BuildingComponent>()` etc. never find JSON components.
  - Fix: key by `component.GetType()`.
  - **Also observed at runtime:** the June 2026 Editor log (6000.4.5) shows `InvalidOperationException: Component IComponent is already registered on entity 1` from `Entity.AddComponent`.
- **ENT-2 [C] There are two `MaterialRegistry` instances.**
  - `ResourceServiceInstaller` registers `RegisterEntryPoint<MaterialRegistry>`, which VContainer constructs from `IEnumerable<Material>` with nothing registered. The June 2026 log has no resolve error, so it is almost certainly an **empty** registry.
  - The real registry is built asynchronously in `MaterialProvider.Registry` (`IMaterialProvider`).
  - `BlueprintBuilder` injects the DI one, so `materialRegistry.Get(...)` throws `KeyNotFoundException`.
  - Fix: depend on `IMaterialProvider` (after `WhenReadyAsync`) and remove the stray registration.
- **ENT-3 [C] Every entity-build placement throws.** `SelectionManager.OnSelectCanceled` hard-codes `materialId = "wood/plank"`, which fails twice:
  - `MaterialRegistry` keys by bare id (`plank`), so the lookup misses.
  - No `MaterialOptions` in `entities.json` allows it.

  So `BlueprintBuilder.CreateBlueprint` throws for every entity-build placement. Material choice needs UI, or should default to `MaterialOptions.DefaultMaterialId`.
- **ENT-4 [C] Entities never get visible views.**
  - `EntityViewManager` is registered with `RegisterComponentOnNewGameObject`, but nothing resolves it. [S] So it is probably never created.
  - Even if it were, `EntityRegistry.Create()` fires `OnEntityCreated` *before* `BuildFromDef` adds components.
  - `EntityView.UpdateView()` only runs in `Initialize`, so views get no sprite or position, and nothing updates them afterwards.
- **ENT-5 [C] Variations are never applied.**
  - `JsonEntity.Variations` are parsed but `JsonEntityLoader` never applies them.
  - Abstract entities are filtered out of the build menu (`UIActionsPanelPresenter.CreateBuildButtons`) and not expanded into their variations: `wall`, `fence`, `furnace`, `splitting_block`, `plant`.
  - So walls and fences can't be built through the entity path.
- **ENT-6 [C] `entities.json` data is inconsistent:**
  - Ingredient ids mix three formats:
    - legacy `EResource` names (`Wood`, `Snow`, `TreeSeed`, `Blueberry`)
    - `class/id` paths (`wood/log`, `refined/mudbrick`)
    - materials that don't exist in `materials.json` (`mudbrick`, `brick`, `compacted_snow`, class `refined`)
  - Every `sprite_path` is empty.
  - `Assets/StreamingAssets/BaseGame/Entities/house.png` can't be loaded with `Resources.Load`, which is what `EntityView` and `JsonEntity.Icon` use.

  Decide on one id scheme first; see MAT-1.
- **ENT-7 [C] `EntityBuildJob` is incomplete:**
  - The Addressables key `ui_icon_job_EntityBuild` doesn't exist; `JobRegistry.LoadIcon` logs an error.
  - `OnJobFinished` only removes `BlueprintMarkerComponent`.
  - Cancelling a job leaves its entity behind.
  - `OccupiedTilesComponent` doesn't change tile walkability, light blocking or regions.
  - `RequiredResourcesComponent` (string ids) isn't bridged to `Job.RequiredResources` (legacy `Resource`), so colonists never haul materials to it.

### Map generation ([systems/map.md](systems/map.md))
- **MAP-1 [C] Seeds are never applied.** `Snowship.Random.Seed(uint)` is never called anywhere. The planet and colony seeds shown in the UI do nothing, and generation isn't reproducible.
- **MAP-2 [C] The seed is passed as the start hour.** `MapGenContext(map, data, startDecimalHour)` receives `mapData.mapSeed` as the start hour from both `PlanetManager.CreatePlanet` and `MapManager.CreateMap`. `StartDecimalHour` is otherwise unused.
- **MAP-3 [C] `MapGenerator` is registered Transient but keeps per-run state in fields:**
  - the fields: `directionPrecipitations`, `windStrengthMapSum`, `primaryWindDirection`
  - `PlanetManager` and `MapManager` each hold one instance and reuse it.
  - On a second planet preview (Refresh/Randomize) or a second colony, `directionPrecipitations` gets 8 more lists, but indices 0–7 still read the first run's data. `windStrengthMapSum` keeps growing.
  - Fix: reset the fields at the start of `Run`, or move them into `MapGenContext`.
- **MAP-4 [C] Shadow directions are cached once per session.** `MapGenerator.CachedShadowDirectionsAtTime` / `ShadowDirectionsCalculated` are static and computed once, using the first map's `equatorOffset`. Later colonies in the same session reuse them, and calling `.Add` again would throw.
- **MAP-5 [C] Resource veins remove the wrong tile.** `PlaceResourceVeins` picks `frontier.RandomElement()` and then calls `frontier.RemoveAt(0)`, which removes a different tile. Vein shapes and sizes come out wrong.
- **MAP-6 [C] River start points aren't spread out.** `CreateRivers` adds `startTile` instead of `otherStartTile` to `removeTiles`, so nearby start candidates are never removed.
- **MAP-7 [C] `SetBiomes` doesn't stop at the first match.** The last matching range wins, and `SetBiome`/`SetPlant` can run several times per tile, creating and destroying plant GameObjects each time.
- **MAP-8 [C] Object layers are never sorted.** In `Tile.AddObjectInstanceToLayer`, the result of `objectInstances.OrderBy(...)` is thrown away. `PostChangeObject` assumes lowest-to-highest layer order when it picks the final `buildable` value.
- **MAP-9 [C] Regions aren't split when a wall divides one.** `Map.RecalculateRegionsAtTile` (`MapRuntime.Regions.cs`) ends in `// TODO Fix`. Regions go wrong after building walls, which breaks pathing checks, job region filtering and visibility.

### Jobs ([systems/jobs.md](systems/jobs.md))
- **JOB-1 [C] Finishing or cancelling a job re-enters itself.**
  - `Job.Close()` calls `Worker.Jobs.AssignJob(null)`, which calls `JobComponent.ReturnJob()`.
  - `ActiveJob` is still the closing job at that point, which leads to one of two outcomes:
    - If not `Returnable`: it calls `ActiveJob.Close()` again, and the second `JobManager.RemoveJob` throws `KeyNotFoundException` on `jobsByType`.
    - If `Returnable`: `AssignWorker(null)` changes state Returned → Ready, so an already-removed job is reset to Ready.
  - [S] Confirm the exact runtime path.
- **JOB-2 [C] `ActiveJob` isn't cleared for non-returnable jobs.** `JobComponent.ReturnJob` calls `Close()` for them and returns without setting `ActiveJob = null`.
- **JOB-3 [C] `Job.Close()` has no null checks.** It dereferences `JobPreviewObject` and `Worker`. `Worker` is null when an unassigned job is removed.
- **JOB-4 [C] Removed jobs keep ticking.** Every `Job` subscribes to `TimeManager.OnTimeChanged` in its constructor and never unsubscribes, so removed jobs leak and keep running.
- **JOB-5 [C] Required resources are never consumed.**
  - Resources are *reserved*, which moves them into `Inventory.reservedResources`.
  - `Job.OnJobFinished` subtracts them from `Inventory.resources`, where they no longer are, so the call is silently ignored.
  - The reservation is later released back. [S] Net effect: building costs nothing.
- **JOB-6 [C] The region filter in `JobManager.AssignJobs` is wrong.**
  - The condition is `region != && !walkable && All(...)`, which lets walkable jobs in unreachable regions through.
  - `SurroundingTiles` can contain nulls, which causes an NRE at map edges.
- **JOB-7 [C] Professions and priorities aren't applied.** `AssignJobs` has TODOs for profession and priority weighting. `ProfessionsComponent` exists but is never consulted.

### State flow & UI ([systems/core.md](systems/core.md), [systems/ui.md](systems/ui.md))
- **FLOW-1 [C] "Exit to Menu" does nothing.** The pause menu requests `EState.MainMenu`, but `PauseMenuState.ValidNextStates` is `{ Simulation }`, so the transition is rejected with a warning.
- **FLOW-2 [C] Leaving a colony tears nothing down.**
  - `QuitToMenuState` has no transition actions.
  - Tiles, humans, jobs, entities, static registries (RES-5) and event subscriptions all survive.
  - Starting a second colony in the same session is therefore undefined.
- **FLOW-3 [S] Escape probably never opens the pause menu.** `UISimulationPresenter.OnEscapePerformed` only opens it when `uiM.OpenViewCount() == 0`. That count includes UISimulation's own children (UIDateTime, UIActionsPanel), so it is always ≥ 2.
- **FLOW-4 [C] `UIGroup.Close()` only closes the first child.**
  - The loop has a `break`, apparently to avoid modifying the collection mid-iteration.
  - Other children, which are parented to the canvas and not their parent, stay alive.
  - Fix: iterate over a copy.
- **FLOW-5 [C] All UI element components share one RectTransform.** `UIElementComponent.rectTransform` is `static`, so every component gets the first instance's RectTransform.
- **FLOW-6 [C] Missing Addressables keys** (checked against `Assets/AddressableAssetsData/AssetGroups`):
  - `UI/UILoadColony`, `UI/UILoadSave`, `UI/Element/UILoadColonyElement` and `UI/Element/UILoadSaveElement` are missing, so the main-menu Load button fails.
  - `ui_icon_job_EntityBuild` is missing too. Confirmed at runtime 2026-09-28: repeated Quick Start plays all log `InvalidKeyException: No Location found for Key=ui_icon_job_EntityBuild` and no other errors, so this is the only thing surfacing in the console right now (FLOW-7's race was not observed).
- **FLOW-7 [S] QuickStart may race resource loading.** `DebugManager.QuickStart` waits only for `TileManager.TilePrefab`, not for `ResourceManager.StartAsync`. That is where `TileType`, `Biome` and `ResourceVein` data are initialised, after an `await`, so `TileType.GetTileTypeByEnum` may return null during planet creation. It was not seen in the June 2026 log, so this is an ordering hazard rather than a known crash.

---

## P2: Half-implemented features

- **ENT-8 [C] The entity pipeline stops at data.**
  - `EntityProvider` is an empty stub.
  - Material trait blueprints (`Material.Traits` / `IItemTraitBlueprint.Create`) are never instantiated onto entities.
  - `StatService` and `IStatProvider` are never queried.
  - `QualityComponent` has no source of quality.
- **MAT-1 [C] Material id scheme is inconsistent.** Materials reference other materials as `class/id` (e.g. `fuels/anthracite` in `CraftableBlueprint`), but `MaterialRegistry` keys by bare id. `CraftableBlueprint.Create` would throw.
- **MAT-2 [C] Two resource models coexist.**
  - Legacy: `Resource`/`EResource`, from `Assets/Resources/Data/resources.txt`.
  - New: `Material`, from `materials.json`.
  - Inventory, jobs, crafting, trade, farms and UI all still use `Resource`.
  - A migration plan and a bridge are needed.
- **MAT-4 [C] Uncommitted `materials.json` edit replaced every metal's ore ingredient.**
  - Each metal's `Craftable.materialsToCraft` ingredient (`bronze_ore`, `copper_ore`, `gold_ore`, …) was replaced with `fuels/anthracite`.
  - It looks like an accidental bulk edit, possibly by the Materials/Entities JSON editor.
  - Review with `git diff Assets/StreamingAssets/BaseGame/Data/materials.json` before committing, and check the editor for the cause.
  - **Still uncommitted as of 2026-09-28** (deliberately left out of the commits that split up the rest of the working tree, see Resolved GIT-1). Either fix it and commit, or `git checkout -- Assets/StreamingAssets/BaseGame/Data/materials.json` to discard it.
- **MAT-3 [C] `MaterialProvider.BuildRegistry` logs every material.** It calls `Debug.Log` once per material on every launch, which is noise.
- **PERS-1 [C] The new persistence framework has nothing to save or load.**
  - The framework: `PersistenceManager`, `SaveRegistry`, `ISaveable`, `SaveContext`/`LoadContext`.
  - No `ISaveRootProvider` is registered.
  - Only `Map` implements `ISaveable` (`MapSaveable.cs`): `Save` returns an empty DTO, and `Load`/`OnAfterLoad` throw `NotImplementedException`.
  - `LocateOrCreateInstance` needs a parameterless constructor, which `Map` doesn't have.
  - Loaded `Data` arrives as `JObject` and needs converting.
  - Save, Continue and Load are commented out or disabled in the UI (`UIPauseMenuPresenter`, `UIMainMenuPresenter`).
  - The old persistence code is parked in `NonIncludedResources/Snowship_OldPersistenceCode_Temp`.
- **FOG-1 [C] Fog of war isn't wired up.**
  - `ColonistManager.SetInitialRegionVisibility` is never called.
  - `RedrawTiles` is always called with `careAboutColonistVisibility=false`.
  - `Region.SetVisible` exists but has no caller.
- **LIFE-1 [C] Humans ignore their generation data.**
  - The `Life` constructor ignores `LifeData.Gender` and rolls a random gender. The name was picked for a possibly different gender.
  - `HumanData.Skills` is never used.
- **LIFE-2 [C] Humans don't get day/night tinting.** `LifeManager.Life` is never populated, so `OnMapLightingUpdated` does nothing, and `Life.Remove` removes from an empty dictionary.
- **LIFE-3 [C] Death isn't implemented.** When health reaches 0, `IsDead` is set, but `Die()` is never called ("dead state" TODO).
- **LIFE-4 [C] Only two needs exist.** Only Rest and Food are implemented; the other `ENeed` values are commented out. `NeedUtilities` maps only these two, so adding a need to `colonist-needs.txt` would throw.
- **CAR-1 [S] Caravans and trading are untested since the DI refactor.**
  - Boat caravans are a TODO.
  - Traders rely on `TradingPost` and the legacy inventory.
  - `CaravanManager.SetSelectedCaravan` opens `UITradeMenu`.
- **UNI-1 [C] Placeholder managers.**
  - Universe/Ship/Simulation are placeholders: `ShipManager` and `SimulationManager` are empty, and `UniverseManager` holds only a name.
  - `BuildableManager` is also empty.

---

## P3: Tech debt, performance, cleanup

- **CORE-1 [C] Models use a static service locator.** `GameManager.Get<T>()` is called throughout model code (`Tile`, `Job`, `Life`, `Region`, `Inventory`, ...). It hides dependencies and silently returns `default` if something isn't registered.
- **CORE-2 [S] `RegisterEntryPoint` is used on classes with no entry-point interface.**
  - Affected: `EntityRegistry`, `MaterialRegistry`, `ColonyManager`, `UIManager`, `PlanetManager`, `UniverseManager`, `SimulationManager`, `PersistenceManager`, `ResourceLoader`.
  - It is misleading. `ColonyManager` subscribes to events in its constructor, and `ResourceLoader` does all its work there.
- **CORE-3 [C] Early input subscribers can attach to a discarded instance.**
  - The `InputManager.InputSystemActions` getter lazily creates an instance, and `Initialize()` then replaces it.
  - Anything that subscribes before `Initialize` attaches to the discarded instance.
  - `DisableInputSystem` is never used.
- **CORE-4 [C] Settings bugs.**
  - The resolution parse in `SettingsState` compares `refreshRateRatio.value - refreshRate > 0.01` without taking the absolute value.
  - [S] `SetDefaults()` is never called, so on first launch the zero `Resolution` may be applied.
- **CORE-5 [C] Version strings disagree.** `GameManager.GameVersion` is `(3, "2025.1")`, but the README says "Development Version 2026.1".
- **FLOW-8 [C] Presenters are registered in the root container as well.** `UIServiceInstaller` registers every presenter as a root singleton, but `UIConfig.CreatePresenter` registers the presenter in a child scope. The root registrations are unused, and resolving one at root would fail because its view isn't registered there.
- **FLOW-9 [C] Event subscriptions leak.**
  - `UISimulationPresenter` subscribes a lambda to `OnHumanSelected` and never unsubscribes.
  - `NeedsComponent` and `MoodsComponent` never unsubscribe from `OnTimeChanged`.
  - `CameraManager.OnMapSet` re-subscribes to input on every map.
  - `TimeManager` only unsubscribes on `Simulation → QuitToMenu`.
- **INPUT-1 [C] Legacy `UnityEngine.Input` is still used.** Used in `HumanManager`, `CaravanManager`, `SelectionManager` (`Input.mousePosition`) and `UIMainMenuView`. It works only because `activeInputHandler: 2` (Both) is set in `ProjectSettings/ProjectSettings.asset`. Migrate to `InputSystemActions`.
- **RES-1 [C] Inventory overflow math is wrong.** `Inventory.ChangeResourceAmount` computes overflow against *max* capacity rather than *remaining* capacity, and divides by `resource.weight`/`volume`, which throws for zero.
- **RES-2 [C] `Inventory.ContainsResourceAmount` has its comparison inverted** (`<=`).
- **RES-3 [C] Resource totals are recalculated twice per change.** `ResourceManager.CalculateResourceTotals` runs on every inventory change: once directly, and again through `AnyInventoryChanged`. Each time it walks every inventory.
- **RES-4 [C] The `ObjectInstance` constructor assigns to the wrong variable.** It assigns the "closest tile" to the `tile` *parameter*, not the field.
- **RES-5 [C] Static registries are never cleared.**
  - Examples: `Container.containers`, `Farm.farms`, `ObjectInstance.ObjectInstances`, `Plant.smallPlants`, `LightSource.lightSources`, `Bed.Beds`, `TradingPost.tradingPosts`, `CraftingObject.craftingObjectInstances`, `TileType.tileTypes`, `Biome.biomes`, `ResourceVein`, `NeedPrefab`, `SkillPrefab`, `ProfessionPrefab`, `MoodModifierGroup`.
  - They leak between sessions and block re-initialisation.
- **RES-6 [C] Most game data still uses the legacy text format.** Tiles, biomes, veins, plants, objects, resources, needs, moods, skills and professions are parsed from `Assets/Resources/Data/*.txt` by `PersistenceUtilities.GetKeyValuePairsFromLines`. Candidates for JSON, alongside entities and materials.
- **PERF-1 [C] Pathfinding is slow.**
  - `Path.FindPathToTile` re-sorts a `List` with `OrderBy` every step and uses `List.Find` for its closed set.
  - Its cost function calls `RegionBlockDistance`, a nested search, for every neighbour.
  - Fix: use a priority queue and `HashSet`s, and cache region-block distances.
- **PERF-2 [C] Region code uses `List.Contains` inside loops**, in both region generation/merging and `Map.DetermineVisibleRegionBlocks`. Both are quadratic.
- **PERF-3 [C] Map generation redraws every tile at each step.** `MapGenerator.UpdateProgress` redraws all tiles at every generation step.
- **MAP-10 [C] `TileCanBeShadowed` has a redundant expression:** `!blocksLight || (!blocksLight && HasRoof())`. The second half adds nothing. Check what was intended.
- **MAP-11 [C] Maps can't be generated headless.** The `Tile` constructor instantiates a GameObject. The planet map creates GameObjects for every tile and then destroys them in the `PlanetTile` constructor.
- **MAP-12 [S] Drainage basin naming doesn't match the loop.** `DetermineDrainageBasins` iterates tiles in *ascending* height, yet names the start tile `highestTileInRegion` / `HighestTile`. Either the naming or the sort order is wrong.
- **LIFE-5 [C] A spawn loop in `ColonistManager.SpawnColonists` does nothing.** Its minimum-distance loop measures `currentTile` instead of `tile`.
- **ENT-9 [C] Two unrelated `ILocation` interfaces exist:** `Snowship.NUtilities.ILocation` (anything with a `Name`) and `Snowship.NEntity.ILocation` (a location component marker).
- **JOB-8 [C] `JobDefinition<TJob, TJobParams>` can't be registered.** Its constructor takes `(group, subGroup, name, icon)`, but `JobRegistry.CreateJobDefinition` calls `(group, subGroup, name)`, so any subclass would fail to register. It's latent: nothing uses it yet.
- **ED-1 [C] A superseded entity editor is still around.** `Assets/Editor/JsonEditors/old_JsonEntitiesEditor.cs` has been replaced, but its menu item "Snowship/Entity JSON Editor" still sits next to the new "Entities JSON Editor".

---

## Resolved

Format: `- **ID** — fixed YYYY-MM-DD: summary (devlog link)`.

- **BUILD-1** — fixed 2026-09-28: the Unity 6000.6.3f1 upgrade broke VContainer, UniTask and LitMotion (CS0619 on `GetInstanceID`/`TreeView`). The user updated all three git packages to newer upstream commits ([devlog](devlog/2026-09-28-unity-6.6-compile-fix.md)).
- **BUILD-2** — fixed 2026-09-28: the only project code broken by 6.6 was `Assets/Editor/RecentAssetsWindow.cs`, which used `EditorApplication.hierarchyWindowItemOnGUI` and `EditorUtility.EntityIdToObject(int)`. It now uses `hierarchyWindowItemByEntityIdOnGUI` with an `EntityId` parameter. The Editor reports 0 errors and 0 warnings ([devlog](devlog/2026-09-28-unity-6.6-compile-fix.md)).
- **TOOL-1** — fixed 2026-09-28: `com.unity.pipeline` 0.8.0-exp.1 was installed through the Package Manager, and `unity` is now on PATH. `tools/unity.sh status/recompile/command` connect to the live Editor ([devlog](devlog/2026-09-28-unity-6.6-compile-fix.md)).
- **GIT-1** — fixed 2026-09-28: split the working tree into 6 commits (Unity upgrade; entity components/registry/view; job/selection/build-menu wiring; entity JSON editor inspectors; `entities.json` data; docs & tooling). `materials.json` (MAT-4) was deliberately left uncommitted pending review ([devlog](devlog/2026-09-28-commit-split.md)).
