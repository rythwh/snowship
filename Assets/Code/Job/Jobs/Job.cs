using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NMap.NTile;
using Snowship.NColonist;
using Snowship.NHuman;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Snowship.NJob
{
	public abstract class Job<TJobDefinition> : IJob
		where TJobDefinition : class, IJobDefinition
	{
		// Job Definition Properties
		public IJobDefinition Definition { get; }
		public int Layer { get; protected set; }
		public string Name => StringUtilities.SplitByCapitals(Definition.Name);
		public Sprite Icon => Definition.Icon;
		public IGroupItem Group => Definition.Group;
		public IGroupItem SubGroup => Definition.SubGroup;
		public IJob ParentJob { get; protected set; }

		// Job Instance Properties
		public Tile Tile { get; private set; }
		public EJobState JobState { get; private set; } = EJobState.Ready;
		public float Progress { get; private set; }
		public Human Worker { get; private set; }
		public string TargetName { get; protected set; } = string.Empty;
		public string Description { get; protected set; } = string.Empty;
		public List<ResourceAmount> RequiredResources { get; } = new();

		public bool ShouldBeCancelled { get; protected set; } = false;
		public GameObject JobPreviewObject { get; protected set; }
		public int Priority { get; private set; } = 1;
		public int Experience { get; protected set; }

		// State Shortcuts
		public bool Started => JobState >= EJobState.Started;
		public bool InProgress => JobState >= EJobState.InProgress;
		public bool Finished => JobState >= EJobState.Finished;

		// Events
		public event Action<Job<TJobDefinition>, Human> OnWorkerAssigned;
		public event Action<Job<TJobDefinition>, EJobState> OnJobStateChanged;
		public event Action<Job<TJobDefinition>, int> OnPriorityChanged;

		protected Job(Tile tile) {
			Definition = GameManager.Get<JobRegistry>().GetJobDefinition(typeof(TJobDefinition)) ?? throw new InvalidOperationException();
			Tile = tile;
			Layer = Definition.Layer;
			Progress = Definition.TimeToWork;
			Experience = Definition.TimeToWork;

			RequiredResources.AddRange(
				Definition.BaseRequiredResources.Select(ra => new ResourceAmount(
						GameManager.Get<IResourceQuery>().GetResourceByEnum(ra.resource),
						ra.amount
					)
				)
			);

			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;

			if (Definition.HasPreviewObject) {
				JobPreviewObject = Object.Instantiate(
					GameManager.Get<TileManager>().TilePrefab, // TODO Create separate Class/Prefab for JobPreviewObject
					Tile.obj.transform,
					false
				);
				JobPreviewObject.GetComponent<SpriteRenderer>().sprite = Icon;
				JobPreviewObject.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.Job + Layer;
			}
		}

		protected Job() {
		}

		protected void SetTimeToWork(float timeToWork) {
			Progress = timeToWork;
		}

		protected void ChangeTile(Tile tile) {
			Tile = tile;
			if (JobPreviewObject) {
				JobPreviewObject.transform.SetParent(Tile.obj.transform, false);
			}
			if (Worker != null) {
				Worker.MoveToTile(Tile, Tile.walkable);
			}
		}

		public void ChangePriority(int amount) {
			Priority += amount;
			OnPriorityChanged?.Invoke(this, Priority);
		}

		public void AssignWorker(Human worker) {
			Worker = worker;
			OnWorkerAssigned?.Invoke(this, Worker);
			ChangeJobState(worker == null ? EJobState.Returned : EJobState.Taken);
			if (worker != null) {
				Debug.Log($"Assigned {worker.Name} to job {Name} at {Tile.PositionGrid}");
			}
		}

		protected virtual void OnJobTaken() {
			ChangeJobState(EJobState.WorkerMoving);
		}

		protected virtual void OnJobWorkerMoving() {
			if (JobState != EJobState.WorkerMoving) {
				return;
			}

			if (Worker.Tile == Tile) {
				ChangeJobState(EJobState.Started);
			}
		}

		protected virtual void OnJobStarted() {
			ChangeJobState(EJobState.InProgress);
		}

		private void OnTimeChanged(SimulationDateTime time) {
			OnJobInProgress();
			OnJobWorkerMoving();
		}

		protected virtual void OnJobInProgress() {
			if (JobState != EJobState.InProgress) {
				return;
			}

			Progress -= 1;
			ProgressJobPreviewObjectAlpha();

			if (Progress <= 0) {
				ChangeJobState(EJobState.Finished);
			}
		}

		private void ProgressJobPreviewObjectAlpha() {
			if (!Definition.HasPreviewObject || JobPreviewObject == null) {
				return;
			}
			JobPreviewObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Progress / Definition.TimeToWork);
		}

		protected virtual void OnJobFinished() {
			foreach (ResourceAmount resourceAmount in RequiredResources) {
				Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
			}

			SkillInstance skill = Worker.Skills.GetSkillFromJobType(Definition.Name); // TODO Skill Not Found
			Debug.Log(skill);
			skill?.AddExperience(Experience);

			Worker.MoveToClosestWalkableTile(true);
		}

		protected virtual void OnJobReturned() {
			if (Definition.Returnable) {
				ChangeJobState(EJobState.Ready);
				return;
			}

			if (JobPreviewObject) {
				Object.Destroy(JobPreviewObject);
			}
		}

		public EJobState ChangeJobState(EJobState newState) {
			Action stateChangedMethod = null;
			switch (newState) {
				case EJobState.Ready:
					break;
				case EJobState.Taken:
					if (JobState != EJobState.Ready) {
						Debug.LogError($"Must progress from Ready -> Taken (Currently {JobState}).");
						return JobState;
					}
					if (Worker == null) {
						Debug.LogError("Worker not assigned to job.");
						return JobState;
					}
					stateChangedMethod = OnJobTaken;
					break;
				case EJobState.WorkerMoving:
					if (JobState != EJobState.Taken) {
						Debug.LogError($"Must progress from Taken -> WorkerMoving (Currently {JobState}).");
						return JobState;
					}
					stateChangedMethod = OnJobWorkerMoving;
					break;
				case EJobState.Started:
					if (JobState != EJobState.WorkerMoving) {
						Debug.LogError($"Must progress from WorkerMoving -> Started (Currently {JobState}).");
						return JobState;
					}
					stateChangedMethod = OnJobStarted;
					break;
				case EJobState.InProgress:
					if (JobState != EJobState.Started) {
						Debug.LogError($"Must progress from Started -> InProgress (Currently {JobState}).");
						return JobState;
					}
					stateChangedMethod = OnJobInProgress;
					break;
				case EJobState.Finished:
					if (JobState != EJobState.InProgress) {
						Debug.LogError($"Must progress from InProgress -> Finished (Currently {JobState}).");
						return JobState;
					}
					stateChangedMethod = OnJobFinished;
					stateChangedMethod += Close;
					break;
				case EJobState.Returned:
					stateChangedMethod = OnJobReturned;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}

			JobState = newState;
			stateChangedMethod?.Invoke();
			OnJobStateChanged?.Invoke(this, JobState);
			return JobState;
		}

		public bool CanBeAssigned() {
			if (JobState != EJobState.Ready) {
				return false;
			}
			if (Worker != null) {
				return false;
			}
			return true;
		}

		[CanBeNull]
		public List<ContainerPickup> CalculateWorkerResourcePickups(Human worker, List<ResourceAmount> resourcesToPickup) {
			List<Container> sortedContainersByDistance = Container.GetContainersInRegion(worker.Tile.region)
				.OrderBy(container => PathManager.RegionBlockDistance(
					worker.Tile.regionBlock,
					container.tile.regionBlock,
					true,
					true,
					false))
				.ToList();

			List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
			if (sortedContainersByDistance.Count <= 0) {
				return null;
			}
			foreach (Container container in sortedContainersByDistance) {
				List<ResourceAmount> resourcesToPickupAtContainer = new();
				IEnumerable<ResourceAmount> matchingResourceInContainer = container.Inventory.resources
					.Where(ra => resourcesToPickup
						.Find(pickupResource => pickupResource.Resource == ra.Resource) != null);
				foreach (ResourceAmount resourceAmount in matchingResourceInContainer) {
					ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.Resource == resourceAmount.Resource);
					if (resourceAmount.Amount >= pickupResource.Amount) {
						resourcesToPickupAtContainer.Add(new ResourceAmount(pickupResource.Resource, pickupResource.Amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.Amount > 0 && resourceAmount.Amount < pickupResource.Amount) {
						resourcesToPickupAtContainer.Add(new ResourceAmount(pickupResource.Resource, resourceAmount.Amount));
						pickupResource.Amount -= resourceAmount.Amount;
						if (pickupResource.Amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					}
				}
				if (resourcesToPickupAtContainer.Count > 0) {
					containersToPickupFrom.Add(new ContainerPickup(container, resourcesToPickupAtContainer));
				}
			}
			if (containersToPickupFrom.Count <= 0) {
				return null;
			}
			if (resourcesToPickup.Count <= 0) {
				return containersToPickupFrom;
			}
			return null;
		}

		public void Close() {
			Object.Destroy(JobPreviewObject.gameObject);
			GameManager.Get<JobManager>().RemoveJob(this);
			Worker.Jobs.AssignJob(null);
		}
	}
}
