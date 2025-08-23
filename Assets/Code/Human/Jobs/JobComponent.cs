using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Tile;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NLife;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NColonist
{
	public class JobComponent
	{
		private readonly Human human;

		public IJob ActiveJob { get; private set; }
		public readonly Queue<IJob> Backlog = new();

		private List<ContainerPickup> currentContainerPickups;

		public event Action<IJob> OnJobChanged;

		public JobComponent(Human human) {
			this.human = human;

			human.OnTileChanged += OnHumanTileChanged;
		}

		private void OnHumanTileChanged(Life life, Tile tile) {
			if (ActiveJob == null) {
				return;
			}
			if (tile != ActiveJob.Tile) {
				return;
			}

			HumanArrivedAtJob();
		}

		private void HumanArrivedAtJob() {
			ActiveJob.ChangeJobState(EJobState.Started);
		}

		public void SetJob(IJob job, bool reserveRequiredResources = true) {
			ReturnJob();

			if (reserveRequiredResources) {
				if (ReserveRequiredResources(job)) {
					foreach (ContainerPickup containerPickup in currentContainerPickups) {
						Backlog.Enqueue(new CollectResourcesJob(containerPickup.container, containerPickup.resourcesToPickup));
					}
					Backlog.Enqueue(job);
					// TODO need to SetJob to the CollectResourcesJob somewhere around here
				} else {
					return;
				}
			}

			ActiveJob = job;

			if (ActiveJob != null) {
				ActiveJob.AssignWorker(human);
				if (ActiveJob.Worker.Tile == ActiveJob.Tile) {
					ActiveJob.ChangeJobState(EJobState.Started);
				} else {
					bool pathValid = ActiveJob.Worker.MoveToTile(ActiveJob.Tile, !ActiveJob.Tile.walkable);
					if (!pathValid) {
						Debug.LogWarning($"Path invalid when trying to move {ActiveJob.Worker.Name} {ActiveJob.Worker.Tile.position} to Job {ActiveJob.Definition.Name} at {ActiveJob.Tile.position}");
						ReturnJob();
					}
				}
			}

			OnJobChanged?.Invoke(ActiveJob);
		}

		private bool ReserveRequiredResources(IJob job) {
			List<ResourceAmount> requiredResources = new();

			// Clone RequiredResources so they can be modified to keep track of how much we still need to reserve
			foreach (ResourceAmount resourceAmount in job.RequiredResources) {
				requiredResources.Add(resourceAmount.Clone());
			}

			List<ResourceAmount> resourcesToReserve = new();

			// Make a shallow copy of requiredResources with ToList() so ew can remove empty entries within the loop
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

			List<ContainerPickup> containerPickups = job.CalculateWorkerResourcePickups(human, requiredResources);
			foreach (ContainerPickup containerPickup in containerPickups) {
				containerPickup.container.Inventory.ReserveResources(containerPickup.resourcesToPickup, human);
			}

			currentContainerPickups = containerPickups;

			// Return true if we have no more required resources to find
			return requiredResources.Count == 0;
		}

		public void ReturnJob() {
			// Release any reserved resources
			human.Inventory.ReleaseReservedResources(human);
			if (currentContainerPickups != null) {
				foreach (ContainerPickup containerPickup in currentContainerPickups) {
					containerPickup.container.Inventory.ReleaseReservedResources(human);
				}
				currentContainerPickups.Clear();
				currentContainerPickups = null;
			}

			if (ActiveJob == null) {
				return;
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
			if (human.dead) {
				return false;
			}
			if (ActiveJob != null) {
				return false;
			}
			if (human is Colonist { playerMoved: true }) {
				return false;
			}
			return true;
		}
	}
}
