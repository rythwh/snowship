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

		private readonly Dictionary<Colonist, SortedSet<JobCostEntry>> colonistJobs = new();

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

			if (Jobs.Count(j => j.CanBeAssigned()) == 0) {
				return;
			}
			if (Colonist.colonists.Count(c => c.JobComponent.CanTakeNewJob()) == 0) {
				return;
			}

			foreach (Colonist colonist in Colonist.colonists) {
				if (!colonist.JobComponent.CanTakeNewJob()) {
					colonistJobs.Remove(colonist);
					continue;
				}

				if (colonistJobs.TryGetValue(colonist, out SortedSet<JobCostEntry> jobs)) {
					jobs.Clear();
				} else {
					colonistJobs.Add(colonist, new SortedSet<JobCostEntry>());
				}

				HashSet<TileManager.Map.RegionBlock> checkedRegionBlocks = new();
				HashSet<IJobDefinition> checkedJobTypes = new();

				foreach (IJob job in Jobs.OrderBy(j => Vector2.Distance(j.Tile.position, colonist.Tile.position))) {

					// Check that the job is able to be assigned to a colonist
					if (!job.CanBeAssigned()) {
						continue;
					}

					// Check that the job is in the same region as the colonist
					// AND Check that if the job's tile is NOT walkable, then it has at least 1 surrounding tile that IS walkable (for e.g. Mine jobs)
					if (job.Tile.region != colonist.Tile.region && !job.Tile.walkable && job.Tile.horizontalSurroundingTiles.All(t => t.region != colonist.Tile.region)) {
						continue;
					}

					// TODO Check that the colonist will actually do the job according to the assigned Professions

					// Check if a similar job has already been checked in that area (mostly redundant)
					if (checkedRegionBlocks.Contains(job.Tile.regionBlock) && checkedJobTypes.Contains(job.Definition)) {
						continue;
					}

					checkedRegionBlocks.Add(job.Tile.regionBlock);
					checkedJobTypes.Add(job.Definition);

					float cost = 0;
					//cost += PathManager.RegionBlockDistance(colonist.Tile.regionBlock, job.Tile.regionBlock, true, true, !job.Tile.walkable);
					cost += Vector2.Distance(colonist.Tile.position, job.Tile.position);
					cost -= colonist.GetSkillFromJobType(job.Name)?.CalculateTotalSkillLevel() * 5f ?? 0;
					// TODO Check the job type's priority according to the colonist's assigned priorities

					colonistJobs[colonist].Add(new JobCostEntry(job, cost));
				}
			}

			foreach (Colonist colonist in colonistJobs.Keys) {
				foreach (JobCostEntry jobCostEntry in colonistJobs[colonist]) {

					if (!jobCostEntry.Job.CanBeAssigned()) {
						continue;
					}

					bool colonistIsBestChoiceForJob = true;
					foreach (Colonist otherColonist in colonistJobs.Keys) {
						if (colonist == otherColonist) {
							continue;
						}

						if (!colonistJobs[otherColonist].TryGetValue(jobCostEntry, out JobCostEntry otherJobCostEntry)) {
							continue;
						}

						if (!jobCostEntry.Job.CanBeAssigned()) {
							continue;
						}

						if (otherJobCostEntry.Cost >= jobCostEntry.Cost) {
							continue;
						}

						colonistIsBestChoiceForJob = false;
						break;
					}

					if (!colonistIsBestChoiceForJob) {
						continue;
					}

					colonist.JobComponent.SetJob(jobCostEntry.Job);
					break;
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

			foreach ((Colonist _, SortedSet<JobCostEntry> jobs) in colonistJobs) {
				jobs.RemoveWhere(j => j.Job == job);
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

	public readonly struct JobCostEntry : IComparable<JobCostEntry>, IComparer<JobCostEntry>, IEquatable<JobCostEntry>
	{
		public IJob Job { get; }
		public float Cost { get; }

		public JobCostEntry(IJob job, float cost) {
			Job = job;
			Cost = cost;
		}

		public int CompareTo(JobCostEntry other) {
			return Cost.CompareTo(other.Cost);
		}

		public int Compare(JobCostEntry x, JobCostEntry y) {
			return x.CompareTo(y);
		}

		public bool Equals(JobCostEntry other) {
			return Equals(Job, other.Job);
		}

		public override bool Equals(object obj) {
			return obj is JobCostEntry other && Equals(other);
		}

		public override int GetHashCode() {
			return Job != null ? Job.GetHashCode() : 0;
		}

		public static bool operator ==(JobCostEntry left, JobCostEntry right) {
			return left.Equals(right);
		}

		public static bool operator !=(JobCostEntry left, JobCostEntry right) {
			return !left.Equals(right);
		}
	}
}