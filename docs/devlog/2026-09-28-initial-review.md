# 2026-09-28: Initial codebase review, docs & tooling

## Goal
The project hadn't been touched in a while and wasn't working. Review the whole codebase, document how
it works, list what's broken or unfinished, and set up CLAUDE.md, the knowledge base and helper tools.
Fixing comes next, as a separate planning step.

## Changes
- Added `CLAUDE.md`, `docs/` (architecture, data, issues, per-system docs, devlog).
- Added `tools/unity-log.sh`, `tools/unity.sh`, `tools/check-docs.sh`.
- **No game code, assets, packages or settings were modified.**

## Findings (headline)
- **BUILD-1:** The working tree has an uncommitted upgrade to Unity 6000.6.3f1. VContainer, UniTask and LitMotion (git packages pinned by hash) fail with CS0619, so no game code compiles. This is the main reason "it isn't working".
- **Uncommitted entity-build work:** `RenderingComponent`, `EntityViewManager`, `EntityBuildJob`, `OccupiedTilesComponent`, and the SelectionManager and ActionsPanel changes. It has several blocking bugs (ENT-1 to ENT-7) but is unfinished rather than regressed.
- **Core-loop bugs** are catalogued as P1 in `docs/issues.md`: job close/return re-entrancy, pause and quit flow, map seeds never applied, and others.
- **Unity CLI:** installed at `%LOCALAPPDATA%\Unity\bin\unity.exe` but not on PATH. It sees the running Editor, but the `com.unity.pipeline` package is missing (TOOL-1).

- **Mixed working tree:** the dirty files mix the upgrade with the entity WIP, so don't revert them all at once. The file split is in BUILD-1.
- **MAT-4:** an uncommitted `materials.json` edit replaced every metal's ore ingredient with `fuels/anthracite`, which looks accidental.
- **Runtime confirmation:** the June 2026 log (`%LOCALAPPDATA%\Unity\Editor\Editor-prev.log`, run on 6000.4.5) confirms ENT-1 and the missing `ui_icon_job_EntityBuild` key, and shows no other game exceptions.

## Verification
- `tools/unity-log.sh errors` against the live `Logs/Editor.log` lists exactly the three package failures and no project errors (project code was never reached).
- `tools/unity.sh pipeline list --json` finds the running Editor with `hasPipelinePackage: false`.
- Findings are tagged [C] (confirmed by reading the code) or [S] (needs a runtime check). Nothing was run in Play mode.

## Follow-ups
1. Decide on BUILD-1: revert to 6000.4.5f1, or update/patch the three packages.
2. After it compiles: install `com.unity.pipeline`, then work through the P1 list in `docs/issues.md`.
