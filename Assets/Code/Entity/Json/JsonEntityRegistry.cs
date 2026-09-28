using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class JsonEntityRegistry
	{
		private readonly Dictionary<string, JsonEntity> idToEntity = new(StringComparer.OrdinalIgnoreCase);

		public IEnumerable<JsonEntity> AllEntities => idToEntity.Values;

		public JsonEntityRegistry(IEnumerable<JsonEntity> jsonEntities)
		{
			foreach (JsonEntity jsonEntity in jsonEntities) {
				idToEntity[jsonEntity.Id] = jsonEntity;
			}
		}

		public JsonEntity Get(string id)
		{
			return idToEntity[id];
		}
	}
}
