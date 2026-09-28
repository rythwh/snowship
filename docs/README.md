# Snowship Project Knowledge Base

This folder is the project's shared, versioned memory: how the game works, where things live, and what
is broken or unfinished. It sits at the repo root rather than under `Assets/` so that Unity doesn't
import the markdown or generate `.meta` files.

## Map of the docs
| Doc | Read it when… |
|---|---|
| [architecture.md](architecture.md) | You need the big picture: tech stack, DI, state machine, the new-game flow, legacy vs new code |
| [issues.md](issues.md) | You're planning or fixing work. **The single list of bugs, TODOs and debt, with stable IDs** |
| [data.md](data.md) | You're touching game content: JSON, legacy txt, `Resources` paths, Addressables keys |
| [systems/core.md](systems/core.md) | DI registration, states, time, input, camera, settings, utilities |
| [systems/map.md](systems/map.md) | Map generation, tiles, regions, lighting, bitmasking |
| [systems/planet-colony.md](systems/planet-colony.md) | Planet and colony creation |
| [systems/entity.md](systems/entity.md) | The new component-based entity system (WIP) |
| [systems/materials.md](systems/materials.md) | The new JSON material model |
| [systems/resources-objects.md](systems/resources-objects.md) | Legacy resources, objects, plants, inventory |
| [systems/life-humans.md](systems/life-humans.md) | Life/Human/Colonist, needs, moods, skills, professions |
| [systems/jobs.md](systems/jobs.md) | Job registry, lifecycle, assignment, adding a job |
| [systems/selection.md](systems/selection.md) | Placing jobs on tiles with the mouse |
| [systems/ui.md](systems/ui.md) | The MVP UI framework, screens, adding a screen |
| [systems/caravans-trade.md](systems/caravans-trade.md) | Caravans, traders, trading posts |
| [systems/persistence.md](systems/persistence.md) | Save/load framework (skeleton) |
| [systems/debug-and-tools.md](systems/debug-and-tools.md) | Debug console, editor windows, `tools/` scripts |
| [devlog/](devlog/) | What changed, when and why: one dated entry per work session |

## "Where do I look for…?"
| Feature / question | Start here |
|---|---|
| Why doesn't the project compile? | `tools/unity.sh recompile --json` (live Editor) or `tools/unity-log.sh errors` (log) |
| Registering a new manager or service | `Assets/Code/GameLifetimeScope.cs` and the module's `*ServiceInstaller.cs` |
| Game states / menu → game transitions | `Assets/Code/State/StateManager.cs`, `Assets/Code/State/States/` |
| Game clock, pause, speed | `Assets/Code/Time/TimeManager.cs` |
| Key bindings | `Assets/Input/InputSystemActions.inputactions` (regenerate `Assets/Code/Input/InputSystemActions.cs` in the Editor) |
| Terrain, biome, river or vein generation | `Assets/Code/Map/Generation/Steps/` |
| Tile sprites / autotiling | `Assets/Code/Map/Runtime/MapRuntime.Rendering.cs`, `Assets/Code/Map/Tile/TileType.cs` |
| Day/night and shadows | `Assets/Code/Map/Runtime/MapRuntime.Lighting.cs`, `Assets/Code/Map/Generation/Steps/MapGenerator.Lighting.cs` |
| Pathfinding | `Assets/Code/Utilities/Pathing/Path.cs` |
| Colonist spawning | `Assets/Code/Colonist/ColonistManager.cs` |
| Colonist AI: needs → sleep/eat | `Assets/Code/Human/Needs/NeedUtilities.cs`, `Assets/Code/Human/Needs/NeedsComponent.cs` |
| Who gets which job | `Assets/Code/Job/JobManager.cs` (`AssignJobs`) |
| What a job does when finished | `Assets/Code/Job/Jobs/**/<Name>Job.cs` (`OnJobFinished`) |
| Build menu contents | `Assets/Code/UI/Simulation/MVPE/UIActionsPanel/UIActionsPanelPresenter.cs` |
| Legacy buildable objects (walls, beds…) | `Assets/Resources/Data/objects.txt`, `Assets/Code/Resource/Models/Objects/` |
| New entity definitions | `Assets/StreamingAssets/BaseGame/Data/entities.json`, *Snowship → Entities JSON Editor* |
| Materials | `Assets/StreamingAssets/BaseGame/Data/materials.json`, *Snowship → Materials JSON Editor* |
| Resource totals / inventories | `Assets/Code/Resource/ResourceManager.cs`, `Assets/Code/Resource/Systems/Inventory.cs` |
| Crafting | `Assets/Code/Resource/Models/Objects/ObjectInstanceTypes/CraftingObject.cs`, `Assets/Code/Job/Jobs/TaskJobs/CreateResourceJob.cs` |
| Farming | `Assets/Code/Resource/Models/Objects/ObjectInstanceTypes/Farm.cs`, `Assets/Code/Job/Jobs/Farm/` |
| Human info panel | `Assets/Code/UI/Simulation/MVPE/UIHumanInfoPanel/` |
| Debug commands | `Assets/Code/Debug/DebugManager.cs` |
| Debug toggles (Quick Start, entity build) | `Assets/Editor/DebugSettingsWindow.cs` |

## How to maintain this knowledge base
1. **Code and docs change together.** When you change a system, update its `systems/*.md` in the same change: key files, flow, "how to add", gotchas.
2. **Issues live in one place.**
   - Add new problems to [issues.md](issues.md) under the right priority, with the next free ID for that prefix and a `[C]` or `[S]` tag.
   - When one is fixed, move it to *Resolved* with the date and a devlog link.
   - System docs only list IDs.
3. **Log each session.** Add `devlog/YYYY-MM-DD-<slug>.md` using the template in [devlog/README.md](devlog/README.md): what changed, why, what you verified, follow-ups.
4. **Cite files and symbols, not line numbers.** Run `tools/check-docs.sh` before finishing; it flags paths that no longer exist.
5. **Keep facts here, not in chat.** Anything a future session would need to rediscover belongs in these docs.
