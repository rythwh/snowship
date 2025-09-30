using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NJob
{
	public class JobManager : Manager
	{
		public JobRegistry JobRegistry { get; private set; }
		public HashSet<IJob> Jobs { get; } = new();
		private readonly Dictionary<Type, HashSet<IJob>> jobsByType = new();
		private readonly Dictionary<Tile, HashSet<IJob>> jobsByTile = new();

		private readonly Dictionary<Colonist, SortedSet<JobCostEntry>> colonistJobs = new();

		public event Action<IJob> OnJobAdded;
		public event Action<IJob> OnJobRemoved;

		private TimeManager TimeM => GameManager.Get<TimeManager>();
		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();
		private ResourceManager ResourceM => GameManager.Get<ResourceManager>();

		public override void OnCreate() {
			JobRegistry = new JobRegistry();
			JobRegistry.CreateJobRegistry();
			TimeM.OnTimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(SimulationDateTime _) {
			AssignJobs();
		}

		private void AssignJobs() {

			if (Jobs.Count(j => j.CanBeAssigned()) == 0) {
				return;
			}
			if (ColonistM.Colonists.Count(c => c.Jobs.CanTakeNewJob()) == 0) {
				return;
			}

			foreach (Colonist colonist in ColonistM.Colonists) {
				if (!colonist.Jobs.CanTakeNewJob()) {
					colonistJobs.Remove(colonist);
					continue;
				}

				if (colonistJobs.TryGetValue(colonist, out SortedSet<JobCostEntry> jobs)) {
					jobs.Clear();
				} else {
					colonistJobs.Add(colonist, new SortedSet<JobCostEntry>());
				}

				HashSet<RegionBlock> checkedRegionBlocks = new();
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

					// Check if the resources for the job are available in the world totals, if not, skip
					bool resourcesAvailable = true;
					foreach (ResourceAmount resourceAmount in job.RequiredResources) {
						int amountOnColonist = colonist.Inventory.ContainsResource(resourceAmount.Resource)?.Amount ?? 0;
						int unreservedContainerTotalAmount = resourceAmount.Resource.GetUnreservedContainerTotalAmount();
						resourcesAvailable &= (amountOnColonist + unreservedContainerTotalAmount) >= resourceAmount.Amount;
						if (!resourcesAvailable) {
							break;
						}
					}
					if (!resourcesAvailable) {
						continue;
					}

					checkedRegionBlocks.Add(job.Tile.regionBlock);
					checkedJobTypes.Add(job.Definition);

					float cost = 0;
					//cost += PathManager.RegionBlockDistance(colonist.Tile.regionBlock, job.Tile.regionBlock, true, true, !job.Tile.walkable);
					cost += Vector2.Distance(colonist.Tile.position, job.Tile.position);
					cost -= colonist.Skills.GetSkillFromJobType(job.Name)?.CalculateTotalSkillLevel() * 5f ?? 0;
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

					colonist.Jobs.AssignJob(jobCostEntry.Job);
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

		public IJob JobOfTypeExistsAtTile<TJob>(Tile tile) where TJob : IJob {
			if (!jobsByTile.ContainsKey(tile)) {
				return null;
			}
			if (!jobsByTile.TryGetValue(tile, out HashSet<IJob> jobs)) {
				return null;
			}
			return jobs?.FirstOrDefault(job => job.GetType() == typeof(TJob));
		}

		public IJob JobAtLayerExistsAtTile(Tile tile, int layer) {
			return jobsByTile.GetValueOrDefault(tile)?.FirstOrDefault(j => j.Layer == layer);
		}

		public HashSet<IJob> JobsAtTile(Tile tile) {
			return jobsByTile.GetValueOrDefault(tile);
		}

		public Sprite GetJobSprite(IJobDefinition jobDefinition, IJobParams args) {
			args?.UpdateJobPreviewSprite();
			return args?.JobPreviewSprite ?? jobDefinition?.Icon ?? ResourceM.selectionCornersSprite;
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
