using System;
using System.Collections.Generic;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NLife;
using UnityEngine;

namespace Snowship.NColonist
{
	public class JobComponent
	{
		private readonly Human human;

		public IJob ActiveJob { get; private set; }
		public readonly Queue<IJob> Backlog = new();

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

		public void SetJob(IJob job) {
			ReturnJob();

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

		public void ReturnJob() {
			if (ActiveJob == null) {
				return;
			}
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