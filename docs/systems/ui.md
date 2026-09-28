# UI (custom MVP framework on uGUI + Addressables)

Each screen is described by three classes in the folder `Assets/Code/UI/<Area>/MVPE/<Screen>/`. ("MVPE" stands for Model–View–Presenter–Element.)

| Piece | Base | Responsibility |
|---|---|---|
| **Config** `UIFoo` | `UIConfig<TView, TPresenter>` or `UIConfig<TView, TPresenter, TParams>` | Loads the Addressable prefab `UI/UIFoo` (**key = class name**), instantiates it, and creates a child `LifetimeScope` that registers the view (and params) and resolves the presenter |
| **View** `UIFooView` | `UIView` (MonoBehaviour on the prefab) | Serialized widget references; exposes C# events (`OnXButtonClicked`) and setters. No game logic |
| **Presenter** `UIFooPresenter` | `UIPresenter<TView>` / `UIPresenter<TView, TParams>` | Constructor-injected with the view and any services. `OnCreate()` subscribes, `OnClose()` unsubscribes |
| **Element** `UIBar` + `UIBarComponent` | `UIElement<TComponent>` + `UIElementComponent` | Reusable sub-widgets loaded from `UI/Element/UIBar`; `await element.OpenAsync(parentTransform)` |

## Framework files (`Assets/Code/UI/Generic/`)
- **`UIManager`:** `OpenViewAsync<TConfig>(parent?, params?, showParent, useParentTransform)`, `CloseView`, `GoBack`, `SwitchViewsAsync`, `ReopenView`, `CloseAllViews`, `ToggleAllViews`, `OpenViewCount`.
- **`Classes/UIGroup.cs`:** a tree node (config, view, presenter, scope, children) that handles open and close ordering.
- **`UIEvents`:** a static event for the loading-screen text.
- **`UIServiceInstaller`:** registers `UIManager`. The presenter registrations here are redundant (FLOW-8).

## Screens
| Config | Opened by |
|---|---|
| `UIMainMenu` | `MainMenuState` |
| `UICreatePlanet` → `UICreateColony` | Main menu "New" |
| `UILoadColony` → `UILoadSave` | Main menu "Load" (prefabs missing, FLOW-6; persistence not implemented) |
| `UISettings` | Main or pause menu |
| `UILoadingScreen` | `LoadToSimulationState` |
| `UISimulation` (children: `UIDateTime`, `UIActionsPanel`) | `SimulationState` |
| `UIHumanInfoPanel` (tabs: needs/moods/skills, inventory, clothing, biography, trade) | `OnHumanSelected` |
| `UIPauseMenu` | Escape (FLOW-3) |
| `UITradeMenu` | `CaravanManager.SetSelectedCaravan` |
| `UIDebugConsole` | DebugMenu key (see [debug-and-tools.md](debug-and-tools.md)) |

The **actions panel** (`Simulation/MVPE/UIActionsPanel/`) builds a button tree:
- The top-level buttons come from the `UIActionsButtonsDataSO` ScriptableObject: Build, Terraform, Farm, Remove.
- Children come from `ObjectPrefabGroup`s (legacy build), from `JsonEntity` categories (entity build flag), or from `JobRegistry` groups.

Leftover folders:
- `Assets/Code/UI/Old/` is `#if OLD_CODE` and excluded from compilation.
- `Assets/Code/UI/Elements/` (`CraftableResourceElement`, `PriorityResourceElement`, `PlanetElement`, `UniverseElement`) holds older element classes.

## Adding a screen
1. Create `UIFoo` (config), `UIFooView` and `UIFooPresenter` in a new folder.
2. Build the prefab and put `UIFooView` on its root.
3. Mark the prefab Addressable with the key `UI/UIFoo` (group UI-Prefabs).
4. Open it with `await uiM.OpenViewAsync<UIFoo>(parentPresenter)`.
5. Unsubscribe everything in `OnClose`.

## Open issues
FLOW-3 to FLOW-9. See [issues.md](../issues.md).
