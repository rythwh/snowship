# Persistence (save/load)

> **Status: framework only.** No game state is saved or loaded yet (PERS-1).

## Design (`Assets/Code/Persistence/`)
- **`ISaveable`:** `Id`, `Save(SaveContext) → DTO`, `Load(LoadContext, data, typeVersion)`, `OnAfterLoad(LoadContext)` (resolves cross-references).
- **`[Saveable("TypeKey", version)]`** on each saveable class. `SaveRegistry` maps type key ↔ `Type` and holds the version.
- **`ISaveRootProvider.GetAllSaveables()`:** modules hand their objects to `PersistenceManager` (`AddProviders` / `AddProviderAtRuntime`).
- **`PersistenceManager.SaveAsync(path)`:**
  1. collect the saveables and de-duplicate them by reference
  2. assign GUIDs
  3. call `Save` on each
  4. write a `PersistenceFile { Version, Objects: ObjectRecord[] }` as JSON (Newtonsoft)
- **`PersistenceManager.LoadAsync(path)`:**
  1. read the file
  2. `Activator.CreateInstance` each type (requires a parameterless constructor)
  3. call `Load`
  4. once everything is loaded, run `OnAfterLoad` on all of them
- **`PersistenceUtilities`:** also holds the **legacy text-data parser** used by the static data loaders (see [data.md](../data.md)). That parser is unrelated to saves.

## Current state
- Only `Map` is marked saveable (`Assets/Code/Map/MapSaveable.cs`). It is an empty DTO, and its load methods throw.
- No providers are registered, and nothing calls `SaveAsync`/`LoadAsync`. The Save, Continue and Load UI paths are commented out.
- The previous (pre-DI) save system is parked outside the build in `NonIncludedResources/Snowship_OldPersistenceCode_Temp`, with a sample save in `NonIncludedResources/SnowshipSaveExample`.

## Open issues
PERS-1, MAP-11 (tiles can't be created headless, which complicates loading), FLOW-6. See [issues.md](../issues.md).
