using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Snowship.NEntity
{
	public class JsonEntityLoader
	{
		private readonly JsonComponentFactoryRegistry jsonComponentFactoryRegistry;
		private readonly IObjectResolver resolver;

		public JsonEntityLoader(
			JsonComponentFactoryRegistry jsonComponentFactoryRegistry,
			IObjectResolver resolver
		)
		{
			this.jsonComponentFactoryRegistry = jsonComponentFactoryRegistry;
			this.resolver = resolver;
		}

		public JsonEntity Parse(string json)
		{
			return JsonConvert.DeserializeObject<JsonEntity>(json);
		}

		public Entity BuildFromDef(EntityRegistry registry, JsonEntity jsonEntity)
		{
			Entity entity = registry.Create();

			foreach (JsonEntityComponent jsonComponent in jsonEntity.Components) {
				if (!this.jsonComponentFactoryRegistry.TryGet(jsonComponent.Type, out IJsonComponentFactory jsonComponentFactory)) {
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
