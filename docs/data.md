# Data Files & Asset Keys

Where game content comes from, what format it uses, and who reads it.

## New JSON data: `Assets/StreamingAssets/BaseGame/`
Read with `File.ReadAllText` from `Application.streamingAssetsPath` (desktop only as written).
`BaseGame/` is meant to become one of several mod "sources" (a TODO in `MaterialProvider`).

| File | Loaded by | Model | Notes |
|---|---|---|---|
| `Assets/StreamingAssets/BaseGame/Data/entities.json` | `JsonEntityService` (constructor) → `JsonEntityRegistry` | `JsonEntity` → `Entity` via `JsonEntityLoader` | 27 entity defs. `components[].type` is the **integer** value of `EEntityComponent`. Edited with *Snowship → Entities JSON Editor*. Data problems: ENT-6. |
| `Assets/StreamingAssets/BaseGame/Data/materials.json` | `MaterialProvider.StartAsync` → `MaterialRegistry` | `JsonMaterial` → `Material` | 41 materials in classes fuels/metals/nature/ores/stone/wood. `traits[].type` is the **integer** `ETrait`; stat modifiers use integer `EStat`/`EStatOperation`. Edited with *Snowship → Materials JSON Editor*. |
| `Assets/StreamingAssets/BaseGame/Data/location_names.json` | *nothing yet* | — | Duplicate of `names-locations.txt`, awaiting migration. |
| `Assets/StreamingAssets/BaseGame/Entities/house.png` | *nothing* | — | Can't be loaded with `Resources.Load` (ENT-6). |

> **Enum values are serialised as integers.** Only ever **append** to `EEntityComponent`, `ETrait`
> (`Snowship.NMaterial.ETrait`), `EStat` and `EStatOperation`. Reordering or inserting silently
> corrupts every JSON file.

## Legacy text data: `Assets/Resources/Data/*.txt`
These use an indentation-based `<Key>value` format parsed by `PersistenceUtilities.GetKeyValuePairsFromLines`
(`Assets/Code/Persistence/PersistenceUtilities.cs`) and loaded with `Resources.Load<TextAsset>`.
The results go into **static lists** (RES-5) during boot.

| File | Parsed by | Populates |
|---|---|---|
| `resources.txt` | `ResourceLoader.CreateResources` (constructor) | `ResourceProvider.Resources` (`EResource` → `Resource`/`Food`/`Clothing`), `ResourceGroup.resourceGroups` |
| `objects.txt` | `ResourceManager.CreateObjectPrefabs` | `ObjectPrefabGroup` / `ObjectPrefabSubGroup` / `ObjectPrefab` / `Variation` |
| `plants.txt` | `ResourceManager.CreatePlantPrefabs` | `PlantGroup` / `PlantPrefab` |
| `tile-types.txt` | `TileType.InitializeTileTypes` (called from `ResourceManager.StartAsync`) | `TileType.tileTypes`, `TileTypeGroup.tileTypeGroups` |
| `biomes.txt` | `Biome.InitializeBiomes` | `Biome.biomes` |
| `resource-veins.txt` | `ResourceVein.InitializeResourceVeins` | `ResourceVein` lists |
| `colonist-needs.txt` | `NeedPrefab.CreateColonistNeeds` (`HumanManager.Initialize`) | `NeedPrefab.needPrefabs` |
| `mood-modifiers.txt` | `MoodModifierGroup.CreateMoodModifiers` | mood groups and prefabs |
| `colonist-skills.txt` | `SkillPrefab.CreateColonistSkills` | `SkillPrefab.skillPrefabs` (must load before professions) |
| `colonist-professions.txt` | `ProfessionPrefab.CreateProfessionPrefabs` (`JobManager.Start`) | `ProfessionPrefab.professionPrefabs` |
| `names-male.txt`, `names-female.txt` | `HumanManager.LoadNames` | space-separated name lists |
| `names-locations.txt` | `ResourceManager.LoadLocationNames` | colony, caravan and location names |

The `EResource`, `TileType.TypeEnum`, `ObjectPrefab.ObjectEnum` etc. enums must match the names in these files.

## Sprites & prefabs loaded with `Resources.Load` (`Assets/Resources/`)
- Tile sprites: `Sprites/Map/Tiles/<TileType>/<TileType>-base|-bitmask|-river` (`TileType` constructor)
- Object and plant sprites: under `Sprites/` (see `ObjectPrefab` and `PlantPrefab`)
- Colonist bodies: `Sprites/Colonists/colonists-body-base-<0..2>` (`HumanManager.LoadSprites`)
- UI sprites: `UI/selectionCorners`, `UI/white-square`, `UI/clear-square`, `UI/Backgrounds/SingleMap/*`
- Legacy UI elements: `UI/UIElements/*` (`ResourceManager.SetResourceReferences`)

## Addressables keys (`Assets/AddressableAssetsData/AssetGroups/`)
Groups: *Default Local Group*, *UI-Prefabs*, *UI-Icons-Jobs*, *UI-Icons-Commands*.

| Key pattern | Used by | Rule |
|---|---|---|
| `Prefabs/Game/Tile`, `Prefabs/Game/Object`, `Prefabs/Game/Human`, `Prefabs/Game/SelectionIndicator` | TileManager, ResourceManager, HumanManager, SelectionManager | fixed keys |
| `UI/<ConfigClassName>` | `UIConfig.GetViewPrefab` | **must equal the config class name** (e.g. `UI/UIMainMenu`) |
| `UI/Element/<ElementClassName>` | `UIElement.CreateComponent` | **must equal the element class name** |
| `ui_icon_job_<JobName>` | `JobRegistry.LoadIcon` | `JobName` is the third `[RegisterJob]` argument |
| `UI/PlanetTile` | planet preview | — |

Currently missing: `UI/UILoadColony`, `UI/UILoadSave`, their element prefabs, and `ui_icon_job_EntityBuild` (FLOW-6).
Whenever you add a UI config, UI element or job, add its Addressable entry in the Editor as well.

## Save files
The new format (`PersistenceFile` JSON with `ObjectRecord`s) isn't produced yet (PERS-1). An example of the old
save layout is in `NonIncludedResources/SnowshipSaveExample`.
