# Resources, Objects, Plants, Inventory (legacy, still load-bearing)

This is the older data model that current gameplay runs on. Most of its registries are **static** lists
loaded from `Assets/Resources/Data/*.txt` (see [data.md](../data.md)).

## Key files
| File | Role |
|---|---|
| `Assets/Code/Resource/ResourceServiceInstaller.cs` | Registers `ResourceProvider`, `ResourceLoader`, `ResourceManager` (and a stray `MaterialRegistry`, ENT-2) |
| `Assets/Code/Resource/ResourceLoader.cs` | Parses `resources.txt` **in its constructor** → `Resource`/`Food`/`Clothing`, crafting recipes, `ResourceGroup`s |
| `Assets/Code/Resource/ResourceProvider.cs` | `IResourceQuery`: `GetResourceByEnum/ByString`, `GetResourcesInClass` |
| `Assets/Code/Resource/EResource.cs` | Enum of every resource (must match `resources.txt`) |
| `Assets/Code/Resource/ResourceManager.cs` | Boot: loads the Object prefab, parses plants and objects, then calls `TileType`/`Biome`/`ResourceVein` init. Also: `CalculateResourceTotals`, plant growth on time ticks, object bitmasking, location names |
| `Assets/Code/Resource/Models/BaseModels/Resource.cs` | Resource data plus world, colonist, container and available totals |
| `Assets/Code/Resource/Models/Objects/ObjectPrefab.cs` | Object definition: layer, size (`multiTilePositions`), rotation, sprites, variations, job type, costs, instance type |
| `Assets/Code/Resource/Models/Objects/ObjectInstance.cs` | Placed object with GameObject. `CreateObjectInstance` switches on `ObjectInstanceType` → `Container`, `TradingPost`, `Bed`, `LightSource`, `CraftingObject`, `Farm` (in `ObjectInstanceTypes/`), each with a static list |
| `Assets/Code/Resource/Models/Objects/ObjectInstanceTypes/CraftingObject.cs` | Per-frame crafting: queues `CreateResourceJob`s according to `CraftableResourceInstance.CreationMethod` (SingleRun / MaintainStock / ContinuousRun) and the fuel priorities |
| `Assets/Code/Resource/Models/Objects/ObjectInstanceTypes/Farm.cs` | Grows crops by brightness, precipitation and temperature; queues `HarvestFarmJob` |
| `Assets/Code/Resource/Models/Plants/Plant.cs`, `Assets/Code/Resource/Models/Plants/PlantPrefab.cs` | Wild plants (trees, bushes, cacti); small plants grow on time ticks |
| `Assets/Code/Resource/Systems/Inventory.cs` | Resource amounts + reservations (per `Human`), weight/volume limits, transfer helpers, change events |
| `Assets/Code/Resource/Models/*Amount*.cs` | `ResourceAmount`, `ReservedResources`, `TradeResourceAmount`, `ConfirmedTradeResourceAmount`, `ResourceRange`, `PriorityResourceInstance`, `CraftableResourceInstance` |

## Inventory model
- **`resources`:** what is physically present and unreserved.
- **`ReserveResources(list, human)`:** moves amounts from `resources` into a `ReservedResources` entry for that human.
- **`TakeReservedResources`:** transfers a human's reservations into their own inventory.
- **`ReleaseReservedResources`:** returns them to `resources`.
- **Totals:** every change triggers `ResourceManager.CalculateResourceTotals()`, which drives the job availability checks (`GetUnreservedContainerTotalAmount`).

## Open issues
RES-1 to RES-6, JOB-5, MAT-2, CAR-1. See [issues.md](../issues.md).
