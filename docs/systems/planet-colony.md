# Planet & Colony

## Key files
| File | Role |
|---|---|
| `Assets/Code/Planet/PlanetManager.cs` | `CreatePlanet(CreatePlanetData)` → `Planet` (a `Map` subclass) + `PlanetTile`s; holds `planet` and `selectedPlanetTile` |
| `Assets/Code/Planet/CreatePlanetData.cs` | UI-editable planet params (name, seed, size index → 10–120, distance → temperature, temperature range, wind) and `CalculatePlanetTemperature` (a simplified black-body formula) |
| `Assets/Code/Planet/PlanetMapDataValues.cs` | Constant `MapData` values for planets (`actualMap=false`, `preventEdgeTouching=true`) |
| `Assets/Code/Planet/PlanetTile.cs` | Wraps a planet `Tile`: equator offset, river flags, neighbour height/river hints; **destroys the tile's GameObject** after capturing its sprite |
| `Assets/Code/Colony/ColonyManager.cs` | `CreateColony(CreateColonyData)`: builds colony `MapData` from the chosen `PlanetTile` and raises `OnColonyCreated`; on `OnMapCreated` spawns 3 colonists |
| `Assets/Code/Colony/CreateColonyData.cs` | Colony name, seed, size index (1–4 → 50–200 tiles) and planet tile |
| `Assets/Code/Colony/ColonyProvider.cs` | `IColonyEvents` |
| `Assets/Code/Colony/Colony.cs` | A name only; also used as an `ILocation` (colonist origin) |
| `Assets/Code/UI/Menu/Modules/PlanetViewModule.cs` | Renders planet tiles into a UI grid; raises `OnPlanetTileClicked` |

## Flow
1. `UICreatePlanetPresenter` edits `CreatePlanetData` and calls `PlanetManager.CreatePlanet` for the preview. Refresh or Randomize regenerates it (MAP-3).
2. "Create Planet" opens `UICreateColony`. The player picks a tile, and `CreateColonyData.CanCreateColony()` requires a valid name and a tile.
3. `ColonyManager.CreateColony` → `MapManager` (see [map.md](map.md)).

The colony's climate comes from the planet tile:
- temperature and precipitation
- `terrainTypeHeights` scaled by the tile's height and precipitation
- neighbour directions (−2 water, 5 stone, 0 ground), which `SmoothHeightWithSurroundingPlanetTiles` uses
- river connections, which `CreateLargeRivers` uses

## Open issues
MAP-1, MAP-2, MAP-3, MAP-11, PERS-1 (no persisted colonies, so the Load Colony UI is unused), FLOW-6. See [issues.md](../issues.md).
