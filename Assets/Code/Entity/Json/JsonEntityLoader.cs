using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Snowship.NEntity
{
	public class JsonEntityLoader
	{
		private readonly ComponentFactoryRegistry registry;
		private readonly IObjectResolver resolver;

		public JsonEntityLoader(
			ComponentFactoryRegistry registry,
			IObjectResolver resolver
		)
		{
			this.registry = registry;
			this.resolver = resolver;
		}

		public JsonEntityDef Parse(string json)
		{
			return JsonConvert.DeserializeObject<JsonEntityDef>(json);
		}

		public Entity BuildFromDef(EntityManager manager, JsonEntityDef entityDef)
		{
			Entity entity = manager.Create();

			foreach (JsonComponentDef entityDefComponent in entityDef.Components) {
				if (!registry.TryGet(entityDefComponent.Type, out IJsonComponentFactory componentFactory)) {
					throw new KeyNotFoundException($"No factory registered for component type: {entityDefComponent.Type}");
				}

				JsonArgs jsonArgs = new JsonArgs((JObject)entityDefComponent.Data);
				IComponent component = componentFactory.Create(jsonArgs, resolver);
				entity.AddComponent(component);
			}

			return entity;
		}
	}
}
