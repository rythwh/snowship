# Life, Humans, Colonists (needs, moods, skills, professions)

## Class hierarchy
```
Life (abstract: position, tile, pathing movement, health, gender, events)      Assets/Code/Life/Life.cs
 └─ Human (inventory, clothing, components, wandering)                          Assets/Code/Human/Human.cs
     ├─ Colonist (job/wander loop, player move, empty inventory)                Assets/Code/Colonist/Colonist.cs
     └─ Trader (walks to trading posts, hands off trades)                       Assets/Code/Caravan/Trader.cs
```
Views mirror the models:
- `LifeView<T>` (`Assets/Code/Life/LifeView.cs`) binds to the model's events and handles movement sprites, the name box and body sections.
- `HumanView` (`Assets/Code/Human/HumanView.cs`) adds clothing.
- `LifeViewModule` components specialise views: `ColonistViewModule`, `TraderViewModule`.

## Creating humans
`HumanManager.CreateHuman<THuman, TViewModule>(tile, HumanData)`:
1. creates the model with `Activator` (constructor `(int id, Tile, HumanData)`)
2. instantiates `Prefabs/Game/Human` and adds `TViewModule`
3. binds the view
4. registers both with `HumanProvider` (`IHumanQuery`/`IHumanWrite`/`IHumanEvents`)

`ColonistProvider` (`IColonistQuery`) is a filtered view over `HumanProvider`.

## Components on `Human`
All live under `Assets/Code/Human/`. Most subscribe to `TimeManager.OnTimeChanged`.

| Component | Behaviour |
|---|---|
| `JobComponent` (`Jobs/`) | Active job + backlog; `AssignJob`/`ForceJob`/`ReturnJob`; reserves required resources and enqueues `CollectResourcesJob`s from containers first. See [jobs.md](jobs.md) |
| `NeedsComponent` (`Needs/`) | `NeedInstance` per `NeedPrefab` (Rest, Food); each minute `NeedUtilities.CalculateNeedValue` and `NeedToReactionFunctionMap` may force a `SleepJob`/`EatJob` |
| `MoodsComponent` (`Moods/`) | Base mood = 100 − weighted needs; mood modifiers (`mood-modifiers.txt`) with timers; `EffectiveMood` eases toward the target |
| `SkillsComponent` (`Skills/`) | `SkillInstance` per `SkillPrefab`; job-type → skill multiplier; XP on job finish |
| `ProfessionsComponent` (`Profession/`) | `ProfessionInstance` per `ProfessionPrefab` with a priority; **not used by job assignment yet** (JOB-7) |
| `TraitsComponent` (`Traits/`) | Empty list; traits not implemented |

## Per-frame loop
`ColonistManager.Tick` → `Colonist.Update()`, only in the Simulation state. It:
1. advances movement along the path (`Life.MoveToTile`); speed = tile walk speed × inventory-weight multiplier
2. wanders when idle

Clicking a human selects it (`HumanManager.SetSelectedHumanFromClick`, legacy `Input`):
- The selection raises `OnHumanSelected`, which opens `UIHumanInfoPanel`.
- Clicking a tile with a colonist selected moves the colonist there (`PlayerMoveToTile`).
- The **F** key focuses the camera on the selected human.

## Adding a need
1. Append to `ENeed`.
2. Add an entry to `colonist-needs.txt`.
3. Add value and reaction functions in `NeedUtilities` (`NeedToReactionFunctionMap`, which is required, and the special value increase).
4. Map any job name in `jobToNeedMap`.

## Open issues
LIFE-1 to LIFE-5, JOB-7, FOG-1, FLOW-9, INPUT-1, RES-5. See [issues.md](../issues.md).
