using System;
using System.Collections.Generic;

namespace Snowship.NJob
{
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