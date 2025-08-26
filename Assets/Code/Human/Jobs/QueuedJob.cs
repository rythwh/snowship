namespace Snowship.NJob
{
	public class QueuedJob
	{
		public IJob ParentJob { get; }
		public IJob Job { get; }

		public QueuedJob(IJob parentJob, IJob job) {
			ParentJob = parentJob;
			Job = job;
		}
	}
}
