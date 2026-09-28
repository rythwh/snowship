# Map: Generation, Tiles, Regions, Lighting, Rendering

`Map` is used both for the **colony map** (`MapData.actualMap = true`) and the coarse **planet map**
(`actualMap = false`, subclass `Planet`). It is a `partial class` spread over several files.

## Key files
| File | Role |
|---|---|
| `Assets/Code/Map/Map.cs` | Tile lists (`tiles`, `sortedTiles[y][x]`, edge tiles), regions, region blocks, rivers, `GetTileFromPosition` |
| `Assets/Code/Map/MapManager.cs` | Runs a colony map: `OnColonyCreated` → loading state → generate → hook events → `OnMapCreated` → Simulation |
| `Assets/Code/Map/MapProvider.cs` | `IMapQuery` (Map, MapData), `IMapWrite`, `IMapEvents` (`MapSet`, `OnMapCreated`) |
| `Assets/Code/Map/Generation/Models/MapData.cs` | Generation parameters: seed, size, climate, terrain heights, planet-neighbour hints, wind |
| `Assets/Code/Map/Generation/MapGenerator.cs` | The generation pipeline (`Run` → `CreateMap`), with loading-screen progress |
| `Assets/Code/Map/Generation/Steps/MapGenerator.*.cs` | Pipeline steps: Tiles, Heightmap, Regions, Rivers, Biomes, RegionBlocks, Roofs, Resources, Lighting |
| `Assets/Code/Map/Generation/Models/MapGenContext.cs` | Map + data + loading-text updates (via `UIEvents`) |
| `Assets/Code/Map/Tile/Tile.cs` | One tile: GameObject + `SpriteRenderer`, height, `TileType`, biome, plant, object layers, walkability, lighting data, region pointers |
| `Assets/Code/Map/Tile/TileType.cs`, `Assets/Code/Map/Tile/TileTypeGroup.cs` | Static tile-type data from `tile-types.txt`; groups Water/Hole/Ground/Stone |
| `Assets/Code/Map/Tile/TileManager.cs` | Loads the `Prefabs/Game/Tile` Addressable (also reused for plants, job previews and selection indicators) |
| `Assets/Code/Map/Models/Geography/Biome.cs`, `Assets/Code/Map/Models/Geography/ResourceVein.cs`, `Assets/Code/Map/Models/Geography/River.cs`, `Assets/Code/Map/Models/Geography/DrainageBasin.cs` | Geography models (biome/vein data are static, loaded from txt) |
| `Assets/Code/Map/Models/Structure/Region.cs`, `Assets/Code/Map/Models/Structure/RegionBlock.cs` | Connectivity regions; ~10×10 blocks for visibility and coarse pathing |
| `Assets/Code/Map/Runtime/MapRuntime.Lighting.cs` | Day/night colour, per-hour raycast shadows, `RemoveTileBrightnessEffect` |
| `Assets/Code/Map/Runtime/MapRuntime.Rendering.cs` | Bitmask autotiling (`BitSum`, `RedrawTile(s)`, `bitmaskMap` 256→47 sprites) |
| `Assets/Code/Map/Runtime/MapRuntime.Visibility.cs` | Picks visible square region blocks around the camera; only those get lighting updates |
| `Assets/Code/Map/Runtime/MapRuntime.Regions.cs` | Incremental region splitting (unfinished, MAP-9) |
| `Assets/Code/Map/MapSaveable.cs` | `ISaveable` stub (PERS-1) |

## Generation pipeline (`MapGenerator.CreateMap`)
1. **Tiles.** Create `mapSize²` tiles with random heights, link 4-way/8-way neighbours (`SurroundingTiles[EGridConnectivity]`), and collect edge tiles.
   - Neighbour order: 0 up, 1 right, 2 down, 3 left, 4 up-right, 5 down-right, 6 down-left, 7 up-left.
2. **Heightmap.**
   - Coarse diamond-ish subdivision noise (`GenerateTerrain`), then 3 smoothing passes.
   - Each tile's type is set from its height against `terrainTypeHeights` (Water < h < Stone).
   - Optionally `PreventEdgeTouching` (planet) and `SmoothHeightWithSurroundingPlanetTiles` (colony: mountains or water on the sides that border them on the planet).
3. **Regions.** Connected-component labelling (`SetTileRegions`), then `ReduceNoise` merges tiny regions.
4. **Rivers.**
   - Colony only: large rivers crossing the map between planet-river edges.
   - Then drainage basins and small rivers from stone edges down to water.
5. **Biomes.**
   - Temperature comes from latitude (planet) or the average (colony), plus height.
   - Precipitation comes from 8 wind-direction sweeps weighted by the primary wind.
   - `SetBiomes` picks the biome by precipitation/temperature ranges and sets the tile type and plants.
6. **Region blocks.** 10×10 square blocks, plus blocks split by walkability.
7. **Colony-only steps:**
   - roofs (high stone)
   - coastal water
   - resource veins (stone and coast)
   - lighting (shadows plus global colour)
8. **Redraw.** Redraw all tiles (bitmasking), then set `Created = true`.

Each `UpdateProgress` step awaits a frame and redraws every tile (PERF-3). Planet generation skips the progress updates.

## Runtime behaviour
- **Changing a tile.**
  - `Tile.SetTileType(type, setBiomeTileType, bitmask, redetermineRegion)` updates walkable/buildable/light flags.
  - With `bitmask` set, it redraws itself and its neighbours; with `redetermineRegion` set, it updates regions.
- **Objects on tiles.**
  - Stored in `Tile.objectInstances[layer]`: layer 2 is plants, and the rest come from `ObjectPrefab.layer`.
  - `PostChangeObject()` recomputes walkable/buildable/blocksLight and triggers lighting and region recalculation.
- **Lighting.**
  - `TimeManager.OnTimeChanged` → `Map.OnTimeChanged`, which re-colours the visible region blocks every 10 game minutes.
  - Shadows are precomputed for each of the 24 hours (`brightnessAtHour`, `shadowsFrom`/`shadowsTo`).
  - `LightSource` objects add `lightSourceBrightnesses`.
- **Visibility and culling.**
  - Camera move or zoom calls `DetermineVisibleRegionBlocks`, a BFS outward from the centre block.
  - Tile GameObjects stay active; only colour updates are limited to visible blocks.

## Gotchas
- `sortedTiles` is indexed `[y][x]`. `GetTileFromPosition` clamps to the map bounds, so it never returns null.
- `SurroundingTiles` lists contain **nulls** at map edges.
- The static data (`TileType.tileTypes`, `Biome.biomes`, ...) must be loaded before any `Map` is built (FLOW-7).
- The lighting caches in `MapGenerator.Lighting.cs` are static (MAP-4).

## Open issues
MAP-1 to MAP-12, FOG-1, PERF-2, PERF-3, PERS-1. See [issues.md](../issues.md).
