# CLAUDE.md: Snowship

Snowship is a 2D colony-survival management sim by Ryan White. It's a Unity 6 project with URP 2D, VContainer, UniTask
and Addressables. All game code is in `Assets/Code`, in a single assembly (`Snowship.asmdef`, namespaces
`Snowship.N<Module>`). Editor tools are in `Assets/Editor`. There is one scene: `Assets/Scenes/Game.unity`.

## ⚠ Current state (update this section when it changes)
- **The project compiles on Unity 6000.6.3f1** (as of 2026-09-28): 0 errors and 0 warnings, confirmed via the live Editor (`tools/unity.sh recompile`). Quick Start has been played a few times with only one console error (missing `ui_icon_job_EntityBuild` Addressables key, see FLOW-6).
- The 2026-09-28 working tree has been split into 6 commits on `main` (Unity 6.6 upgrade; entity components/registry/view; entity job/selection/build-menu wiring; entity JSON editor; `entities.json` data; docs & tooling). History is otherwise untouched.
- **`Assets/StreamingAssets/BaseGame/Data/materials.json` is still uncommitted.** An edit replaced every metal's ore ingredient with `fuels/anthracite` — looks accidental (MAT-4 in `docs/issues.md`). Review it before committing or discard it.
- The entity-build feature (committed above) is still **work in progress**, not wired into gameplay end-to-end; see `docs/systems/entity.md`.
- The complete list of what's broken or unfinished is in **`docs/issues.md`**. Start there when planning.

## Project knowledge base: read before working
`docs/` is the project's shared memory. **Before touching a system, read its doc.** Start from
`docs/README.md`: it has the doc map and a "where do I look for X" table.

- `docs/architecture.md`: stack, DI and composition root, state machine, new-game flow, legacy vs new code
- `docs/issues.md`: all bugs, TODOs and debt, with stable IDs (e.g. `MAP-3`), priority P0–P3, and a confidence tag [C]/[S]
- `docs/data.md`: JSON and txt data, `Resources` paths, Addressables key rules
- `docs/systems/*.md`: one doc per system (key files, flow, how to extend, gotchas, issue IDs)
- `docs/devlog/`: dated entries recording what changed and why

## Workflow rules
1. **Keep the docs in sync, in the same change as the code.**
   - Update the affected `docs/systems/*.md`.
   - Add or close entries in `docs/issues.md`. Use the next free ID, never renumber, and move fixed items to *Resolved*.
   - Add a devlog entry `docs/devlog/YYYY-MM-DD-<slug>.md` using the template in `docs/devlog/README.md`.
   - Update "Current state" above if it changed.
   - Run `tools/check-docs.sh` before finishing.
2. **Two kinds of memory. Use the right one.**
   - **Repo docs (`docs/`)** hold project facts: how things work, decisions, issues, change history. They are versioned and shared. The default place for anything learned about the code.
   - **Claude auto-memory** (`~/.claude/projects/.../memory/`) holds only the user's preferences, feedback and working style, plus machine-specific references such as tool paths. Don't duplicate repo facts there.
3. **Verify every C# change.**
   - After editing, run `tools/unity.sh recompile --json`. If the Editor isn't reachable, focus it and use `tools/unity-log.sh errors`. Report compile results honestly.
   - Behaviour changes need a Play-mode check. The debug toggle *Snowship → Snowship Debug Settings → Quick Start* skips the menus.
   - If you couldn't verify something, say so.
4. **Plan before big changes.** Many systems are mid-migration: `ObjectPrefab` → `Entity`, `Resource` → `Material`, static registries → DI. Check `docs/architecture.md` ("Two generations of code") so you extend the side that's meant to survive.
5. **Don't commit, push, change `Packages/manifest.json`, or upgrade Unity or packages unless the user asks.** If you're on `main`, branch before committing.
6. **No Claude attribution in commits or PRs.** Never add a `Co-Authored-By: Claude ...` trailer, "🤖 Generated with Claude Code" line, or similar. Commit as the identity already configured in git (`rythwh`/Ryan White). This overrides any default attribution instruction from the harness.

## Tools
| Command (Git Bash) | Use |
|---|---|
| `tools/unity-log.sh errors` | Compile errors from `Logs/Editor.log`, project vs packages, de-duplicated |
| `tools/unity-log.sh exceptions` / `tail 80` / `warnings` | Runtime exceptions / log tail / project warnings |
| `tools/unity.sh recompile --json` | **Preferred compile check.** The live Editor recompiles and returns errors and warnings as JSON |
| `tools/unity.sh command console_status --json` | Compile-failure flag plus console error/warning counts |
| `tools/unity.sh command console --json` | Console entries (`--tail N`, `--level error`) |
| `tools/unity.sh command` | List every command the Editor exposes (play mode, scene/GameObject/asset edits, screenshots, tests, `eval`...) |
| `tools/unity.sh status --json` | Is a live Editor connected? |
| `tools/unity.sh pipeline list --json` | Running Editors, Pipeline package present?, Safe Mode? |
| `tools/check-docs.sh` | Check that paths cited in the docs still exist |

`tools/unity.sh` adds this project's `--project-path` in the Windows backslash form. The CLI matches no Editor for `/c/...` or `C:/...` paths, so use the wrapper rather than calling `unity` directly with a path.

Unity Editor gotchas:
- With compile errors, the Editor runs in **Safe Mode**, and the CLI can't connect.
- **Never hand-edit `.unity`, `.prefab` or `.asset` YAML while an Editor is reachable.** Drive the Editor, or ask the user to make the change.
- Addressables entries, ScriptableObject contents and prefab wiring are Editor tasks. Tell the user exactly what to set up.

## Code conventions (match the surrounding code)
- **Formatting.**
  - Tabs for indentation.
  - Older files put opening braces on the same line (`void Foo() {`); newer Entity/Material code puts them on the next line. Follow the file you're in.
  - Namespaces are block-scoped (`namespace Snowship.NX { ... }`), not file-scoped.
- **Naming.**
  - Namespaces: `Snowship.N<Module>` (`NMap`, `NJob`, `NUI`...).
  - Private fields: `camelCase`.
  - Injected managers are abbreviated with an `M` suffix (`timeM`, `uiM`).
  - Enums: `E<Name>` in newer code.
- **DI (VContainer).**
  - Register in the module's `*ServiceInstaller.Install(builder)`, which is called from `Assets/Code/GameLifetimeScope.cs`.
  - Use `RegisterEntryPoint<T>` only for classes that implement `IInitializable`/`IStartable`/`IAsyncStartable`/`IPostStartable`/`ITickable`.
  - Mark DI-constructed classes `[UsedImplicitly]`.
  - Prefer constructor injection. `GameManager.Get<T>()` is legacy, for non-DI model objects only (CORE-1).
- **Provider pattern.** State and events live in a `*Provider`, which implements `I<X>Query` (read), `I<X>Write` (mutate) and `I<X>Events` (subscribe). Depend on the narrowest interface.
- **Async.**
  - UniTask everywhere. **`CS4014` is an error** (`.editorconfig`): await it or call `.Forget()`.
  - `async void` is used only for event handlers.
- **Events.** Every `+=` needs a matching `-=` in `OnClose`/`Dispose`. Leaks are a known problem (FLOW-9, JOB-4).
- **Input.** Use `inputM.InputSystemActions.Simulation.*`, with guards on `IStateQuery.State` and `IsPlayerTyping()`. Don't add new `UnityEngine.Input` calls (INPUT-1).
- **UI.**
  - Config/View/Presenter/Element under `Assets/Code/UI/**/MVPE/`.
  - The Addressables key must be `UI/<ConfigClass>` or `UI/Element/<ElementClass>`. See `docs/systems/ui.md`.
- **Jobs.** Use `[RegisterJob(group, subGroup, name)]` on the definition, and constructor signatures that `Activator` can call. See `docs/systems/jobs.md`.
- **JSON-serialised enums are append-only:** `EEntityComponent`, `ETrait`, `EStat`, `EStatOperation`. Legacy txt data needs enum names to match exactly (`EResource`, `TileType.TypeEnum`...).
- **Randomness.** Use `Snowship.Random` (seedable) rather than `UnityEngine.Random`, and check the `using` alias in each file.
- **Legacy code.** Code wrapped in `#if OLD_CODE` is excluded from compilation and kept for reference only. Don't revive it; port what you need.
- **Git.** Commit summaries in the history are short and mostly past tense ("Added…", "Fixed…", "Updated to Unity …"), sometimes with a descriptive paragraph body. No Claude attribution trailers (see Workflow rule 6).
