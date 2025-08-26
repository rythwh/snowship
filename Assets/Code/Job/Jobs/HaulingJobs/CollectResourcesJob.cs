using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "CollectResources")]
	public class CollectResourcesJobDefinition : JobDefinition<CollectResourcesJob>
	{
		public CollectResourcesJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class CollectResourcesJob : Job<CollectResourcesJobDefinition>
	{
		private Container Container { get; set; }

		public CollectResourcesJob(Container container, List<ResourceAmount> requiredResources, IJob parentJob) : base(container.tile) {
			Container = container;
			RequiredResources.AddRange(requiredResources);
			ParentJob = parentJob;

			Description = "Collecting resources.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Container?.Inventory.TakeReservedResources(Worker);
		}
	}
}
