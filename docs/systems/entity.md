# Entity System (new, component-based world objects)

> **Status: work in progress, not yet connected to gameplay.** It is meant to replace
> `ObjectPrefab`/`ObjectInstance` (see [resources-objects.md](resources-objects.md)). As of 2026-09-28,
> this work is **uncommitted** in the working tree:
> - new files: `RenderingComponent`/`RenderingFactory`, `OccupiedTilesComponent`, `EntityViewManager`, `JsonEntityService`, `EntityBuildJob`, `GenericGroupItem`
> - edits: `EntityView`, `LocationService`, `BuildingComponent`, `JsonEntity` (now `IGroupItem` with `Icon`), `EntityServiceInstaller`, `SelectionManager` and `UIActionsPanelPresenter` (the entity build path), and the Entities JSON editor
>
> Treat failures here as *unfinished*, not regressions.

## Concepts
- **`Entity`** (`Assets/Code/Entity/Entity.cs`): an id plus a type→component dictionary. It also keeps cached lists of `IStatProvider` and `ICostProvider` components.
  - API: `AddComponent`, `TryGet<T>`, `Get<T>`, `Remove<T>`/`Remove(Type)`.
  - Watch out for ENT-1: components are keyed by the *generic* type argument.
- **`IComponent`** (`Assets/Code/Entity/Components/IComponent.cs`): optional `OnAttach(Entity)` and `OnDetach()` default methods.
- **`EntityRegistry`**: the live entity list, with `OnEntityCreated`/`OnEntityRemoved`.
- **Definitions** (`JsonEntity`, loaded from `Assets/StreamingAssets/BaseGame/Data/entities.json` by `JsonEntityService` into `JsonEntityRegistry`):
  - id, name, category and sub-category (these drive the build menu)
  - `is_abstract`
  - `components[{type, data}]`
  - `variations[]` (not applied yet, ENT-5)
- **Factories.**
  - Each `IJsonComponentFactory` maps an `EEntityComponent` id to a component built from `JsonArgs` (a typed accessor over `JObject`).
  - Factories are registered in `EntityServiceInstaller` and collected by `JsonComponentFactoryRegistry`.
  - `JsonEntityLoader.BuildFromDef(registry, def)` creates the entity and adds its components.

## Components (`Assets/Code/Entity/Components/`)
| `EEntityComponent` (int) | Component | Factory data keys |
|---|---|---|
| 0 BaseRecipe | `BaseRecipeComponent` (`ICostProvider`) | `ingredients[{id, amount}]` |
| 1 BaseStats | `BaseStatsC` (in `Assets/Code/Materials/Stats/`) | stat name → value |
| 2 MaterialOptions | `MaterialOptions` | `allowed.{classes,materials}`, `defaults.{class,material}`, `cost.{byClass,byMaterial}.<key>.{mode: ReplaceAll/Delta, recipe/add/remove}` |
| 3 Building | `BuildingComponent` | `width, height, integrity, blocks_light, walkable, walk_speed, flammability`; `GetFootprint(rotation)` |
| 4 Bed | `BedComponent` | `comfort_amount` |
| 5 Container | `ContainerComponent` (legacy `Inventory`) | `weight, volume` |
| 6 Crafting | `CraftingComponent` (empty marker) | — |
| 7 Plant | `PlantComponent` | `growth_time_days, seed_resource, harvest_materials[]` (legacy `Resource` ids) |
| 8 Quality | `QualityComponent` (`IStatProvider`) | `value_multiplier, build_work_multiplier` |
| 9 Rendering | `RenderingComponent` | `sprite_path` (`Resources` path), `pivot_x, pivot_y` |

Components added at runtime, not from JSON:
- `MaterialComponent` (the chosen material; stat and cost provider)
- `RequiredResourcesComponent` (needed and delivered amounts)
- `BlueprintMarkerComponent`
- location components: `WorldLocationComponent` (tile and rotation), `InventoryLocationComponent`, `AttachedToEntityComponent`
- `OccupiedTilesComponent`

`LocationService` guarantees an entity has at most one location component.

## Services
- **`CostService.ComputeFinalCost(entity)`** starts from the base recipe, applies a material override (by material, then by class) if the material has one, then applies any other `ICostProvider`s.
- **`StatService.TryGet(entity, stat)`** returns `(base + Σadd) × Πmul` over the `IStatProvider` modifiers. Nothing calls it yet.
- **`BlueprintBuilder.CreateBlueprint(registry, defId, materialId, tile, rotation)`** does, in order:
  1. build the entity from its definition
  2. check `MaterialOptions.IsAllowed`
  3. add a `MaterialComponent`
  4. add `RequiredResourcesComponent(cost)` and `BlueprintMarkerComponent`
  5. `LocationService.MoveToTile`
- **Rendering.** `EntityViewManager` (MonoBehaviour) creates an `EntityView` per entity. The view positions itself from `WorldLocationComponent` and loads its sprite from `RenderingComponent`. See ENT-4.

## Build path (behind a debug flag)
1. Enable *Snowship → Snowship Debug Settings → "Use Entity System for Buildings"*. The flag is stored in `PlayerPrefs` as `DebugSettings/Use Entity System for Buildings`.
2. `UIActionsPanelPresenter.CreateBuildButtons` groups the non-abstract `JsonEntity`s by category and sub-category.
3. Clicking one calls `SelectionManager.SetSelectedJob<EntityBuildJob, EntityBuildJobDefinition>(new EntityBuildJobParams(def))`.
4. On drag-release, `SelectionManager` creates an `EntityBuildJob` per tile, calls `BlueprintBuilder.CreateBlueprint(...)`, then `JobManager.AddJob`.
5. When the job finishes, `EntityBuildJob.OnJobFinished` removes the `BlueprintMarkerComponent`.

## Adding a component type
1. Append a value to `EEntityComponent`. **Never reorder**: JSON stores the integer.
2. Create the component (`IComponent`) and a factory (`IJsonComponentFactory`) under `Assets/Code/Entity/Components/Definitions/`.
3. Register the factory in `EntityServiceInstaller` (`builder.Register<IJsonComponentFactory, XFactory>(Lifetime.Singleton)`).
4. Update the Entities JSON editor if it needs a custom inspector: `Assets/Editor/JsonEditors/Editors/EntitiesEditor/JsonEntitiesEditorComponents.cs`.

## Open issues
ENT-1 to ENT-9, MAT-1, MAT-2, JOB-5 (resource bridging). See [issues.md](../issues.md).
