# Selection (placing jobs on tiles)

## Key files
| File | Role |
|---|---|
| `Assets/Code/Selection/SelectionManager.cs` | Selected job type, definition and params; mouse-drag area; indicators; creates the jobs on release; rotation |
| `Assets/Code/Selection/SelectionConditions.cs` | Static `Func<Tile, int layer, bool>` predicates (Walkable, Buildable, NoPlant, NoSameLayerObject, NoSameLayerJobs, …) used by job definitions |
| `Assets/Code/Selection/SelectionType.cs` | Full / Outline / Single area shapes |
| `Assets/Code/Selection/ISelectable.cs` | A stack of selectables for Deselect (barely used) |
| `Assets/Code/Job/SelectionModifiers.cs` | Legacy modifier names referenced by `objects.txt` `<SelectionModifiers>` |

## Flow
1. An action-panel button calls `SelectionManager.SetSelectedJob<TJob, TDef>(params?)` or `SetSelectedJob(definition)`.
2. Every tick while a job is selected, `UpdateSelectedJobPreview` puts the preview sprite (`SharedReferences.SelectedJobPreview`) under the mouse. Entity builds apply the `RenderingComponent` pivot.
3. **Select** pressed: record `firstTile`, set `selecting = true`.
4. While `selecting`, the `secondTile` follows the mouse. `GetSelectionArea` filters the rectangle by `SelectionType` and the definition's `SelectionConditions`.
   - Note: `IJobParams.SelectionConditions` is ignored because `UpdateSelectionArea` doesn't pass the params.
5. **Select** released: for each tile, `Activator.CreateInstance(jobType, tile[, params])`, then `JobManager.AddJob`. For `EntityBuildJob`, it also creates the blueprint entity (ENT-3).
6. **Deselect** released: clears the selected job, or pops the selectable stack.
7. **Rotate**: calls `params.SetRotation(rotation + 1)`.

## Open issues
ENT-3, INPUT-1 (`Input.mousePosition`), CORE-3. See [issues.md](../issues.md).
