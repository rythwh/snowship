# Jobs

## Key files
| File | Role |
|---|---|
| `Assets/Code/Job/JobUtilities/JobRegistry/JobRegistry.cs` | At `Start`, reflects over types with `[RegisterJob(group, subGroup, jobName)]`, creates each **definition** (constructor `(IGroupItem group, IGroupItem subGroup, string name)`), builds the `JobGroup` tree and loads the `ui_icon_job_<jobName>` icon |
| `Assets/Code/Job/Jobs/JobDefinition.cs`, `Assets/Code/Job/Jobs/IJobDefinition.cs` | Static per-type config: `SelectionConditions`, `BaseRequiredResources`, `TimeToWork`, `Returnable`, `Layer`, `SelectionType` (Full/Outline/Single), `HasPreviewObject` |
| `Assets/Code/Job/Jobs/Job.cs`, `Assets/Code/Job/Jobs/IJob.cs` | Job instance base class `Job<TJobDefinition>`: tile, state machine, progress, worker, required resources, preview GameObject |
| `Assets/Code/Job/Jobs/IJobParams.cs` | Optional per-selection parameters (e.g. which `ObjectPrefab` to build, rotation, preview sprite) |
| `Assets/Code/Job/JobManager.cs` | Job sets indexed by type and tile; `AssignJobs()` each game minute; `AddJob`/`RemoveJob`; lookups |
| `Assets/Code/Human/Jobs/JobComponent.cs` | The worker side (see [life-humans.md](life-humans.md)) |
| `Assets/Code/Job/EJobState.cs` | Ready → Taken → WorkerMoving → Started → InProgress → Finished; Returned |

## Registered jobs (`Assets/Code/Job/Jobs/`)
| Group / SubGroup | Jobs |
|---|---|
| Build / Build | `BuildJob` (legacy `ObjectPrefab`), `EntityBuildJob` (new entities; registered under group "EntityBuild") |
| Terraform / Terrain | `MineJob`, `DigJob`, `FillJob` |
| Terraform / Plants | `PlantJob`, `ChopPlantJob` |
| Farm | `PlantFarmJob`, `HarvestFarmJob` |
| Remove | `RemoveJob`, `RemoveRoofJob` |
| Task / Craft | `CreateResourceJob` (queued by `CraftingObject`) |
| Hauling | `CollectResourcesJob`, `PickupResourcesJob`, `EmptyInventoryJob`, `TransferResourcesJob` |
| Needs | `SleepJob`, `EatJob`, `CollectFoodJob`, `WearClothesJob` |
| Trade | `TradeJob` |

## Lifecycle
1. **Creation.** `SelectionManager`, a system, or a need creates the job with `new XJob(tile[, params])`, then calls `JobManager.AddJob`.
   - The base constructor looks up its definition, copies `BaseRequiredResources`, subscribes to `OnTimeChanged` and spawns a preview sprite.
2. **Assignment.** Every game minute, `JobManager.AssignJobs` walks colonists that `CanTakeNewJob()`.
   - It filters jobs by region and by resource availability.
   - Each job is scored by distance minus skill, and each colonist gets its best job where it is the cheapest colonist for that job.
3. **Starting.** `JobComponent.AssignJob` reserves resources and queues pickups, then calls `StartJob`: `AssignWorker(human)` puts the job in **Taken**, then **WorkerMoving**, and the worker paths to the tile.
4. **Work.** On arrival (the worker's `TileChanged` event), the job goes to **Started**, then **InProgress**. Each minute, `Progress -= 1`.
   - At 0 the job goes to **Finished**: `OnJobFinished()` (subclass effect, resources consumed, XP), then `Close()`.
5. **Return/cancel.** `ReturnJob` releases reservations. A `Returnable` job goes back to **Ready**; otherwise it is closed.

The finish, return and close paths are currently buggy: JOB-1 to JOB-5.

## Adding a job
1. Create `XJobDefinition : JobDefinition<XJob>` with `[RegisterJob("Group", "SubGroup", "JobName")]` and a constructor `(IGroupItem group, IGroupItem subGroup, string name)`.
   - Override `SelectionConditions`, `Layer` and so on as needed.
2. Create `XJob : Job<XJobDefinition>` with constructor `(Tile tile)` or `(Tile tile, XJobParams p)`. `SelectionManager` creates it with `Activator` using exactly those signatures.
   - Override `OnJobFinished()` (call `base`), and optionally `OnJobStarted` etc.
3. Add the icon Addressable `ui_icon_job_JobName` (group UI-Icons-Jobs).
4. Menu wiring: jobs in the "Terraform", "Farm" and "Remove" groups appear automatically in the actions panel. Other groups need code in `UIActionsPanelPresenter`.

## Open issues
JOB-1 to JOB-8, ENT-7. See [issues.md](../issues.md).
