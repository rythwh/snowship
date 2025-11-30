using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Snowship.NEntity
{
	public class JsonEntityLoader
	{
		private readonly JsonComponentFactoryRegistry registry;
		private readonly IObjectResolver resolver;

		public JsonEntityLoader(
			JsonComponentFactoryRegistry registry,
			IObjectResolver resolver
		)
		{
			this.registry = registry;
			this.resolver = resolver;
		}

		public JsonEntity Parse(string json)
		{
			return JsonConvert.DeserializeObject<JsonEntity>(json);
		}

		public Entity BuildFromDef(EntityManager manager, JsonEntity jsonEntity)
		{
			Entity entity = manager.Create();

			foreach (JsonComponent jsonComponent in jsonEntity.Components) {
				if (!registry.TryGet(jsonComponent.Type, out IJsonComponentFactory jsonComponentFactory)) {
					throw new KeyNotFoundException($"No factory registered for component type: {jsonComponent.Type}");
				}

				JsonArgs jsonArgs = new JsonArgs((JObject)jsonComponent.Data);
				IComponent component = jsonComponentFactory.Create(jsonArgs);
				entity.AddComponent(component);
			}

			return entity;
		}
	}
}
