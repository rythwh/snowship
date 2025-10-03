using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NMap.NTile;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NColonist
{
	public class JobComponent
	{
		private readonly Human human;

		public IJob ActiveJob { get; private set; }
		private readonly List<QueuedJob> backlog = new();

		[CanBeNull] private List<ContainerPickup> currentContainerPickups;

		public event Action<IJob> OnJobChanged;

		public JobComponent(Human human) {
			this.human = human;

			human.TileChanged += OnHumanTileChanged;
		}

		private void OnHumanTileChanged(Tile tile) {
			if (ActiveJob == null) {
				return;
			}
			if (tile != ActiveJob.Tile) {
				return;
			}

			HumanArrivedAtJob();
		}

		public void ForceJob(IJob job) {
			AssignJob(job, true);
		}

		public void AssignJob(IJob job, bool skipBacklog = false) {
			// Return any current job if one exists
			ReturnJob();

			if (job != null) {
				// Create any CollectResource jobs and add them to the backlog
				if (!CreateCollectResourceJobs(job)) {
					return;
				}

				if (skipBacklog) {
					StartJob(job);
					return;
				}
			}

			// If there are jobs in the backlog, start the first one, otherwise start the job directly
			if (backlog.Count > 0) {
				if (job != null && backlog.Find(qj => qj.Job == job) == null) {
					backlog.Add(new QueuedJob(job, job));
				}

				IJob backlogJob = backlog[0].Job;
				backlog.RemoveAt(0);
				StartJob(backlogJob);
			} else {
				StartJob(job);
			}
		}

		private bool CreateCollectResourceJobs(IJob job) {

			if (job.RequiredResources == null || job.RequiredResources.Count == 0) {
				return true;
			}

			bool reservedAllResources = ReserveRequiredResources(job);
			if (reservedAllResources) {
				if (currentContainerPickups != null) {
					foreach (ContainerPickup containerPickup in currentContainerPickups) {
						CollectResourcesJob collectResourcesJob = new(containerPickup.container, containerPickup.resourcesToPickup, job);
						EnqueueJob(collectResourcesJob, job);
					}
				}
				if (currentContainerPickups == null || currentContainerPickups.Count == 0) {
					EnqueueJob(job); // Only necessary if there are collect resource jobs to do before the job itself
				}
			}
			return reservedAllResources;
		}

		private void EnqueueJob(IJob job, IJob parentJob = null) {
			backlog.Add(new QueuedJob(parentJob ?? job, job));
		}

		private void StartJob(IJob job) {
			ActiveJob = job;

			if (ActiveJob != null) {
				ActiveJob.AssignWorker(human);
				if (ActiveJob.Worker.Tile == ActiveJob.Tile) {
					ActiveJob.ChangeJobState(EJobState.Started);
				} else {
					bool pathValid = ActiveJob.Worker.MoveToTile(ActiveJob.Tile, !ActiveJob.Tile.walkable);
					if (!pathValid) {
						Debug.LogWarning($"Path invalid when trying to move {ActiveJob.Worker.Name} {ActiveJob.Worker.Tile.PositionGrid} to Job {ActiveJob.Definition.Name} at {ActiveJob.Tile.PositionGrid}");
						ReturnJob();
					}
				}
			}

			OnJobChanged?.Invoke(ActiveJob);
		}

		private void HumanArrivedAtJob() {
			ActiveJob.ChangeJobState(EJobState.Started);
		}

		private bool ReserveRequiredResources(IJob job) {
			List<ResourceAmount> requiredResources = new();

			if (job.RequiredResources == null || job.RequiredResources.Count == 0) {
				return true;
			}

			// Clone RequiredResources so they can be modified to keep track of how much we still need to reserve
			foreach (ResourceAmount resourceAmount in job.RequiredResources) {
				requiredResources.Add(resourceAmount.Clone());
			}

			List<ResourceAmount> resourcesToReserve = new();

			// Make a shallow copy of requiredResources with ToList() so we can remove empty entries within the loop
			foreach (ResourceAmount requiredResourceAmount in requiredResources.ToList()) {
				ResourceAmount foundResourceAmount = human.Inventory.ContainsResource(requiredResourceAmount.Resource);
				if (foundResourceAmount == null) {
					continue;
				}
				// If the Worker has enough of the resource already, reserve it all
				if (foundResourceAmount.Amount >= requiredResourceAmount.Amount) {
					resourcesToReserve.Add(requiredResourceAmount.Clone());
					requiredResources.Remove(requiredResourceAmount);
				} else { // Otherwise reserve the amount we have, the rest needs to be found in containers(TODO /other colonists)
					resourcesToReserve.Add(foundResourceAmount.Clone());
					requiredResourceAmount.Amount -= foundResourceAmount.Amount;
				}
			}
			human.Inventory.ReserveResources(resourcesToReserve, human);

			// Return early if the Worker had all the required resources in their inventory
			if (requiredResources.Count == 0) {
				return true;
			}

			// Find containers that have any remaining required resources
			List<ContainerPickup> containerPickups = job.CalculateWorkerResourcePickups(human, requiredResources);
			if (containerPickups != null) {
				foreach (ContainerPickup containerPickup in containerPickups) {
					containerPickup.container.Inventory.ReserveResources(containerPickup.resourcesToPickup, human);
				}
			}

			currentContainerPickups = containerPickups;

			// Return true if we have no more required resources to find
			return requiredResources.Count == 0;
		}

		public void ReturnJob() {
			if (ActiveJob == null) {
				return;
			}

			// Release any reserved resources
			human.Inventory.ReleaseReservedResources(human);
			if (currentContainerPickups != null) {
				foreach (ContainerPickup containerPickup in currentContainerPickups) {
					containerPickup.container.Inventory.ReleaseReservedResources(human);
				}
				currentContainerPickups.Clear();
				currentContainerPickups = null;
			}

			// Remove related jobs from the backlog
			foreach (QueuedJob queuedJob in backlog.ToList()) {
				if (queuedJob.ParentJob == ActiveJob.ParentJob || queuedJob.ParentJob == ActiveJob) {
					backlog.Remove(queuedJob);
				}
			}

			// If the job is not returnable, it will simply be closed
			if (!ActiveJob.Definition.Returnable) {
				ActiveJob.Close();
				return;
			}

			ActiveJob.AssignWorker(null);
			ActiveJob = null;
		}

		public bool CanTakeNewJob() {
			if (human.IsDead) {
				return false;
			}
			if (ActiveJob != null) {
				return false;
			}
			if (backlog.Count > 0) {
				return false;
			}
			if (human is Colonist { playerMoved: true }) {
				return false;
			}
			return true;
		}
	}
}
