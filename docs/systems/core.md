# Core: Composition Root, State, Time, Input, Camera, Settings, Utilities

## Key files
| File | Role |
|---|---|
| `Assets/Code/GameLifetimeScope.cs` | VContainer scope; registers every manager and installer |
| `Assets/Code/GameManager.cs` | `GameManager.Get<T>()` static locator, `GameVersion`, `ServiceLocatorBridge` |
| `Assets/Code/SharedReferences.cs` | Scene references (camera, canvas, parents, preview renderer, sprites) |
| `Assets/Code/State/StateManager.cs` | State machine, `IStateQuery`, `IStateEvents` |
| `Assets/Code/State/States/` | One class per `EState`; each declares legal next states and its transition actions |
| `Assets/Code/Time/TimeManager.cs` | In-game clock; `OnTimeChanged(SimulationDateTime)` every game minute |
| `Assets/Code/Time/SimulationDateTime.cs` | Minute/Hour/Day/Season/Year, pause, `TimeModifier` (0–3), `DeltaTime` |
| `Assets/Code/Input/InputManager.cs` | Owns the generated `InputSystemActions`; `PointerOverUI`, `IsPlayerTyping()` |
| `Assets/Input/InputSystemActions.inputactions` | Action maps: **Simulation** (MoveCamera, ZoomCamera, Escape, DebugMenu, Pause, TimeSpeedUp/SlowDown, Select, Deselect, Rotate) and **UI** |
| `Assets/Code/Camera/CameraManager.cs` (+ `.Input.cs`) | Pan and zoom from input; clamps to the map; initialises on `MapSet` |
| `Assets/Code/Camera/CameraProvider.cs` | Wraps the `Camera`; `ICameraQuery`/`ICameraWrite`/`ICameraEvents`; LitMotion tweens |
| `Assets/Code/Settings/SettingsManager.cs`, `Assets/Code/Settings/SettingsState.cs` | Resolution, fullscreen and canvas scale mode, persisted in `PlayerPrefs` |

## How it works
- **Registration.** Add a manager in its module's `*ServiceInstaller` using one of:
  - `builder.RegisterEntryPoint<T>(Lifetime.Singleton).AsSelf()`, for lifecycle callbacks
  - `builder.Register<T>(...)`, for a plain service

  Mark DI-constructed classes `[UsedImplicitly]`.
- **Adding a state.**
  1. Add a value to `EState`.
  2. Create `States/<Name>State.cs`.
  3. Add it to the `StateManager` constructor dictionary.
  4. Add it to the `ValidNextStates` of every state that should be able to reach it.
- **Transition UI actions.** `ETransitionUIAction.Close` closes all views, `Hide` toggles them, and `Nothing` leaves them alone.
- **Time.**
  - One game minute passes when `timer += deltaTime * TimeModifier * 2 * 2` reaches ≥ 1.
  - A day is 1440 "seconds" (`SimulationDateTime.DayLengthSeconds`), and seasons are 30 days.
  - Hotkeys are bound only on the `LoadToSimulation → Simulation` transition.
  - Pausing sets `TimeModifier = 0`, so `Time.DeltaTime` becomes 0 and everything scaled by it stops.
- **Input.**
  - Subscribe to `inputM.InputSystemActions.Simulation.<Action>.performed`.
  - Guard with `stateQuery.State == EState.Simulation` and `!inputM.IsPlayerTyping()`.
  - Unsubscribe in `OnClose`/`Dispose`.
- **Camera.** Zoom ranges from 3 to 20 (orthographic size). The map sets the camera background colour via `Map.LightingUpdated`.

## Utilities (`Assets/Code/Utilities`)
- **`Random.cs` (`Snowship.Random`):**
  - a static wrapper over `Unity.Mathematics.Random`, seeded from crypto RNG
  - `Seed(uint)` exists but is never called (MAP-1)
  - also provides the extension methods `RandomElement`, `Shuffle`, etc.
  - Several files alias `UnityEngine.Random` instead, so check the `using` lines.
- **`Pathing/Path.cs` (`Snowship.NPath.Path`):**
  - A* over tiles (`FindPathToTile`)
  - `PathExists`
  - region-block distance (`RegionBlockDistance`); slow, see PERF-1
- **Small helpers:**
  - `IdProvider<T>` / `IdUtility`: per-type incrementing ids
  - `IGroupItem` / `GenericGroupItem`: tree items for the action menu
  - `SortingOrder`: sprite sorting bands for Tile, Object, Job, Life, Selection and UI
  - `ColourUtilities`, `StringUtilities`, `Range`, `Priority`
- **MonoBehaviours:** `WorldSpaceNameBox`, `HoverToggleScript`, `HandleClickScript`.

## Stubs
- `SimulationManager` and `ShipManager` are empty.
- `UniverseManager` stores only a name.

## Open issues
GIT-1, FLOW-1, FLOW-2, FLOW-7, CORE-1, CORE-2, CORE-3, CORE-4, CORE-5, FLOW-9, INPUT-1, UNI-1, MAP-1. See [issues.md](../issues.md).
