using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NJob
{
	public class JobManager : IManager
	{
		public JobRegistry JobRegistry { get; private set; }
		public HashSet<IJob> Jobs { get; } = new();
		private readonly Dictionary<Type, HashSet<IJob>> jobsByType = new();
		private readonly Dictionary<TileManager.Tile, HashSet<IJob>> jobsByTile = new();

		public event Action<IJob> OnJobAdded;
		public event Action<IJob> OnJobRemoved;

		public void OnCreate() {
			JobRegistry = new JobRegistry();
			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(SimulationDateTime _) {
			AssignJobs();
		}

		private void AssignJobs() {

			if (Colonist.colonists.Count(c => c.JobComponent.CanTakeNewJob()) <= 0) {
				return;
			}

			foreach (IJob job in Jobs) {
				if (!job.CanBeAssigned()) {
					continue;
				}
				Colonist chosenColonist = Colonist.colonists
					.Where(c => c.JobComponent.CanTakeNewJob())
					.Where(c => c.Tile.region == job.Tile.region || (!job.Tile.walkable && job.Tile.horizontalSurroundingTiles.Any(t => c.Tile.region == t.region)))
					.OrderBy(c => Vector2.Distance(c.Tile.position, job.Tile.position))
					.ThenByDescending(c => c.GetSkillFromJobType(job.Name)?.CalculateTotalSkillLevel() * 5f ?? 0)
					.FirstOrDefault();

				chosenColonist?.JobComponent.SetJob(job);

				if (Colonist.colonists.Count(c => c.JobComponent.CanTakeNewJob()) <= 0) {
					break;
				}
			}

			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.dead) {
					continue;
				}
				if (colonist.JobComponent.Job != null) {
					continue;
				}
				if (colonist.playerMoved) {
					continue;
				}

				IJob selectedJob = Jobs
					.Where(j => j.Worker == null)
					.Where(j => j.Tile.region == colonist.Tile.region || (!j.Tile.walkable && j.Tile.surroundingTiles.Any(t => t.region == colonist.Tile.region)))
					//.OrderBy(j => PathManager.RegionBlockDistance(j.Tile.regionBlock, colonist.Tile.regionBlock, true, true, true))
					.OrderBy(j => Vector2.Distance(j.Tile.position, colonist.Tile.position))
					.ThenByDescending(j => colonist.GetSkillFromJobType(j.Name)?.CalculateTotalSkillLevel() * 5f ?? 0)
					.FirstOrDefault();

				if (selectedJob != null) {
					colonist.JobComponent.SetJob(selectedJob);
				}
			}
		}

		public void AddJob(IJob job) {
			Jobs.Add(job);

			if (!jobsByType.TryAdd(job.GetType(), new HashSet<IJob> { job })) {
				jobsByType[job.GetType()].Add(job);
			}

			if (!jobsByTile.TryAdd(job.Tile, new HashSet<IJob> { job })) {
				jobsByTile[job.Tile].Add(job);
			}

			OnJobAdded?.Invoke(job);
			Debug.Log($"Job added: {job.Name} at {job.Tile.obj.name}");
		}

		public void RemoveJob(IJob job) {
			Jobs.Remove(job);

			jobsByType[job.GetType()].Remove(job);
			if (jobsByType[job.GetType()].Count == 0) {
				jobsByType.Remove(job.GetType());
			}

			jobsByTile[job.Tile].Remove(job);
			if (jobsByTile[job.Tile].Count == 0) {
				jobsByTile.Remove(job.Tile);
			}

			OnJobRemoved?.Invoke(job);
		}

		public void ReturnJob(IJob job) {
			job?.AssignWorker(null);
		}

		public IJob JobOfTypeExistsAtTile<TJob>(TileManager.Tile tile) where TJob : IJob {
			return jobsByTile.GetValueOrDefault(tile)?.FirstOrDefault(job => job.GetType() == typeof(TJob));
		}

		public IJob JobAtLayerExistsAtTile(TileManager.Tile tile, int layer) {
			return jobsByTile.GetValueOrDefault(tile)?.FirstOrDefault(j => j.Layer == layer);
		}

		public HashSet<IJob> JobsAtTile(TileManager.Tile tile) {
			return jobsByTile.GetValueOrDefault(tile);
		}

		public Sprite GetJobSprite(IJobDefinition jobDefinition, IJobParams args) {
			return args?.JobPreviewSprite ?? jobDefinition?.Icon ?? GameManager.Get<ResourceManager>().selectionCornersSprite;
		}
	}
}