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

		public IJob Job { get; private set; }
		public readonly Queue<IJob> Backlog = new();

		public event Action<IJob> OnJobChanged;

		public JobComponent(Human human) {
			this.human = human;

			human.OnTileChanged += OnHumanTileChanged;
		}

		private void OnHumanTileChanged(Life life, TileManager.Tile tile) {
			if (Job == null) {
				return;
			}
			if (tile != Job.Tile) {
				return;
			}

			HumanArrivedAtJob();
		}

		private void HumanArrivedAtJob() {
			Job.ChangeJobState(EJobState.Started);
		}

		public void SetJob(IJob job) {
			ReturnJob();

			Job = job;

			if (Job != null) {
				Job.AssignWorker(human);
				if (Job.Worker.Tile == Job.Tile) {
					Job.ChangeJobState(EJobState.Started);
				} else {
					bool pathValid = Job.Worker.MoveToTile(Job.Tile, !Job.Tile.walkable);
					if (!pathValid) {
						Debug.LogWarning($"Path invalid when trying to move {Job.Worker.Name} {Job.Worker.Tile.position} to Job {Job.Definition.Name} at {Job.Tile.position}");
						ReturnJob();
					}
				}
			}

			OnJobChanged?.Invoke(Job);
		}

		public void ReturnJob() {
			if (Job == null) {
				return;
			}
			if (!Job.Definition.Returnable) {
				Job.Close();
				return;
			}

			Job.AssignWorker(null);
			Job = null;
		}

		public bool CanTakeNewJob() {
			if (human.dead) {
				return false;
			}
			if (Job != null) {
				return false;
			}
			if (human is Colonist { playerMoved: true }) {
				return false;
			}
			return true;
		}
	}
}