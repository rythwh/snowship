# 2026-09-28: Split the working tree into commits; confirmed Quick Start runs

## Goal
Nothing from the 6.6 upgrade or the entity-build WIP was committed (GIT-1). Split it into
separate, logically-scoped commits on `main` so either part can be reverted independently, and
do a first Play-mode check now that the project compiles.

## Changes
- Six commits, in order (each verified to be internally consistent by construction; only the
  final combined state was actually recompiled — see Verification):
  1. `Update to Unity 6000.6.3f1` — engine/package bump, regenerated settings/solution files,
     the `RecentAssetsWindow.cs` API fix.
  2. `Add rendering, location and view components to the entity system` — new components
     (Rendering, OccupiedTiles), EntityView/EntityViewManager, JsonEntity/JsonEntityRegistry/
     JsonEntityService, GenericGroupItem.
  3. `Wire entity build jobs into selection and the build menu` — EntityBuildJob, the
     SelectionManager and UIActionsPanelPresenter changes, the Debug Settings toggle.
  4. `Add editor inspectors for the new entity components` — the Entities JSON Editor changes.
  5. `Flesh out entities.json with the buildable-object set` — the data.
  6. `Add project knowledge base, CLAUDE.md and Unity CLI tooling` — `docs/`, `CLAUDE.md`,
     `tools/`, `.gitattributes`.
- **Left uncommitted, deliberately:** `Assets/StreamingAssets/BaseGame/Data/materials.json`.
  Its diff replaces every metal's ore-crafting ingredient with `fuels/anthracite`, which reads as
  an accidental bulk edit (MAT-4), not intentional data. Not reverted either — left for the user
  to review and decide.
- Updated `CLAUDE.md` per the user's request: commits/PRs must not carry a Claude attribution
  trailer; commit as the git identity already configured (`rythwh`/Ryan White). Also fixed a
  table that had gotten split by an earlier edit.
- Confirmed via `tools/unity.sh command console --level error` that a few live Quick Start runs
  only produced the already-known `InvalidKeyException: No Location found for
  Key=ui_icon_job_EntityBuild` (FLOW-6). No other runtime errors seen.

## Verification
- `tools/unity.sh recompile --json` on the final `HEAD` (after all 6 commits, with
  `materials.json` still dirty in the working tree): `status: up_to_date`, 0 errors, 0 warnings.
- `git status --short` after all commits: only `materials.json` remains modified.
- **Not verified:** each intermediate commit was not individually checked out and recompiled.
  The split was ordered so each commit's file set should compile against everything before it
  (e.g. the entity JSON editor commit only starts referencing `EEntityComponent.Rendering` after
  the commit that adds it), based on reading the diffs, but this wasn't confirmed by actually
  checking out each commit.

## Issues
- Fixed: GIT-1 (moved to Resolved in `../issues.md`)
- Still open: MAT-4 (materials.json), everything else in `../issues.md`

## Follow-ups
- Decide MAT-4: fix the ingredient ids and commit, or discard the file.
- If commit-by-commit bisectability matters later, individually check out and recompile commits
  2-4 (the entity work) to confirm no intermediate breakage.
- Next: plan fixes for the P1 list.
