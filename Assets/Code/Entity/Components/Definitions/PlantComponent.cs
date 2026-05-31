using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NEntity
{
	public class PlantComponent : IComponent
	{
		public int GrowthTimeDays { get; }
		public Resource SeedResource { get; }
		public List<ResourceAmount> HarvestResources { get; }

		public PlantComponent(int growthTimeDays, Resource seedResource, List<ResourceAmount> harvestResources)
		{
			GrowthTimeDays = growthTimeDays;
			SeedResource = seedResource;
			HarvestResources = harvestResources;
		}
	}
}
