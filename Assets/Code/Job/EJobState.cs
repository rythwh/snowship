namespace Snowship.NJob
{
	public enum EJobState
	{
		Ready,
		Taken,
		WorkerMoving,
		Started,
		InProgress,
		Finished,
		Returned
	}
}
