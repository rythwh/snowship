# Debug Console, Editor Tools & Agent Tooling

## In-game debug console
- `Assets/Code/Debug/DebugManager.cs` (global namespace) is toggled by the **DebugMenu** input action, which opens `UIDebugConsole`.
- `ParseCommandInput` maps the first word to the `Commands` enum.
- `help` lists everything. Examples:
  - `spawncolonists`, `spawncaravans`, `sethealth`, `setneed`, `setmood`, `changeinvamt`
  - tile inspect/edit (`selecttiles`, `changetiletype`, `changetileobj`, `changetileplant`)
  - `campos`/`camzoom`, `settime`, `pause`, `togglesuncycle`
  - map overlays (`viewregions`, `viewheightmap`, `viewtemperature`, `viewprecipitation`, `viewbiomes`, `viewrivers`, `viewregionblocks`, shadow views, `viewroofs`, `viewhidden`)
  - `listjobs`, `growfarms`, `opentrademenu`
- The **QuickStart** path in `DebugManager.PostStart` is editor only. With the flag on, it auto-creates a planet and a 50×50 colony named "Lumia".

## Editor windows (menu **Snowship/…**, `Assets/Editor/`)
| Menu item | File | Purpose |
|---|---|---|
| Snowship Debug Settings | `Assets/Editor/DebugSettingsWindow.cs` | Toggles stored in `PlayerPrefs` as `DebugSettings/<name>`: **Quick Start**, **Use Entity System for Buildings** |
| Entities JSON Editor | `Assets/Editor/JsonEditors/Editors/EntitiesEditor/` | Edit `entities.json`: component inspectors, material options, variations, validation |
| Materials JSON Editor | `Assets/Editor/JsonEditors/Editors/MaterialsEditor/` | Edit `materials.json` |
| Entity JSON Editor (old) | `Assets/Editor/JsonEditors/old_JsonEntitiesEditor.cs` | Superseded (ED-1) |
| Recent Assets Tracker | `Assets/Editor/RecentAssetsWindow.cs` | Quick access to recently used assets |
| Replace Fonts / Text To TextMeshPro | `Assets/Editor/FontReplacer.cs`, `Assets/Editor/TextToTextMeshPro.cs` | One-off UI migration helpers |

Shared helpers are in `Assets/Editor/JsonEditors/Utilities/` (`JsonIO`, `JsonEditorStyles`, `JsonEditorUtilities`).

## Agent / CLI tooling (`tools/`)
Run these from Git Bash.
| Script | Purpose |
|---|---|
| `tools/unity.sh recompile --json` | Makes the live Editor recompile and return errors and warnings. **Use it after every C# change.** |
| `tools/unity.sh command <name> --json` | Runs any Editor command, e.g. `console_status`, `console --tail 50 --level error`, `editor_play`, `capture_game_view`. List them with `tools/unity.sh command` |
| `tools/unity.sh status --json` / `pipeline list --json` | Connection check, Pipeline package state, Safe Mode |
| `tools/unity-log.sh errors` | Fallback when the Editor isn't reachable: de-duplicated compile errors from `Logs/Editor.log`, split into project vs packages (the log accumulates, so older errors stay in it) |
| `tools/unity-log.sh exceptions` / `tail [N]` / `warnings` | Runtime exceptions, the log tail, project warnings |
| `tools/check-docs.sh` | Fails if any path referenced from `docs/` or `CLAUDE.md` no longer exists |

About `tools/unity.sh`:
- It wraps the `unity` CLI (on PATH, installed in `%LOCALAPPDATA%\Unity\bin`).
- It adds `--project-path` in the Windows backslash form. The CLI matches no Editor for `/c/...` or `C:/...` paths.
- `pipeline` and `job` don't accept `--project-path`, so it passes those through unchanged.

Driving the live Editor needs two things:
- the `com.unity.pipeline` package (installed: 0.8.0-exp.1)
- a project that compiles; otherwise the Editor is in Safe Mode and the package can't load

The package also ships a more detailed `unity-pipeline` agent skill. Mirror it next to the project with `unity skill install <client> --local` (not done yet).
