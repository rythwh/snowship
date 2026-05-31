using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NResource;

namespace Snowship.NEntity
{
	public class PlantFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Plant;

		private readonly IResourceQuery resourceQuery;

		public PlantFactory(IResourceQuery resourceQuery)
		{
			this.resourceQuery = resourceQuery;
		}

		public IComponent Create(JsonArgs args)
		{
			int growthTimeDays = args.GetIntOrDefault("growth_time_days", 0);
			
			string seedResourceId = args.GetStringOrDefault("seed_resource", null);
			Resource seedResource = seedResourceId != null ? resourceQuery.GetResourceByString(seedResourceId) : null;

			List<ResourceAmount> harvestResources = new();
			JArray harvestArray = args.GetArrayOrDefault("harvest_materials");
			if (harvestArray != null) {
				foreach (JToken token in harvestArray) {
					JObject obj = (JObject)token;
					string resourceId = obj["id"]?.Value<string>();
					int amount = obj["amount"]?.Value<int>() ?? 0;
					if (resourceId != null) {
						harvestResources.Add(new ResourceAmount(resourceQuery.GetResourceByString(resourceId), amount));
					}
				}
			}

			return new PlantComponent(growthTimeDays, seedResource, harvestResources);
		}
	}
}
