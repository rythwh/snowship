# Materials (new JSON resource model)

Meant to replace the legacy `Resource`/`EResource` model ([resources-objects.md](resources-objects.md)).
It loads on boot but **nothing in gameplay consumes it yet** (MAT-2).

## Key files
| File | Role |
|---|---|
| `Assets/Code/Materials/Json/MaterialProvider.cs` | `IAsyncStartable`; reads `Assets/StreamingAssets/BaseGame/Data/materials.json` and builds `Registry`; `IMaterialProvider.WhenReadyAsync()` |
| `Assets/Code/Materials/Json/JsonMaterialLoader.cs` | `JsonMaterial` → `Material` (stat modifiers + trait blueprints through `ItemTraitFactoryRegistry`) |
| `Assets/Code/Materials/Material.cs` | Id, ClassId, Weight, Volume, Value, `Modifiers` (`StatModifier`), `Traits` (`IItemTraitBlueprint`) |
| `Assets/Code/Materials/MaterialRegistry.cs` | Lookup by id and by class (case-insensitive) |
| `Assets/Code/Materials/Stats/` | `EStat`, `EStatOperation`, `StatModifier`, `IStatProvider`, `StatService`, `BaseStatsC`/`BaseStatsFactory` |
| `Assets/Code/Materials/Traits/` | `ETrait` (Fuel, Craftable, Food, Clothing); `Definitions/FuelComponent.cs`, `Definitions/CraftableComponent.cs` (factory → blueprint → component) |
| `Assets/Code/Entity/Items/` | `IItemTraitFactory`, `IItemTraitBlueprint`, `ItemTraitFactoryRegistry` |
| `Assets/Code/Materials/MaterialServiceInstaller.cs` | Registers the trait factories, the registry of factories, and `MaterialProvider` |

## Trait pipeline
The JSON `traits[{type: <int ETrait>, data}]` goes through:
1. `IItemTraitFactory.Parse(JsonArgs)`, which returns an `IItemTraitBlueprint` stored on the `Material`
2. later, `blueprint.Create(IObjectResolver)`, which returns an `IComponent` for an entity. Nothing calls this yet (ENT-8).

Only Fuel and Craftable have factories. Food and Clothing traits in JSON would throw `NotImplementedException`.

## Adding a trait
1. Append to `ETrait`.
2. Write a factory, a blueprint and a component in `Traits/Definitions/`.
3. Register the factory in `MaterialServiceInstaller` with `.AsImplementedInterfaces()`.
4. Update `Assets/Editor/JsonEditors/Editors/MaterialsEditor/JsonMaterialsEditor.cs`.

## Open issues
MAT-1, MAT-2, MAT-3, MAT-4, ENT-2, ENT-8. See [issues.md](../issues.md).
