using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class ObjectDefRegistry
	{
		private readonly Dictionary<string, JsonEntityDef> byId = new(StringComparer.OrdinalIgnoreCase);

		public ObjectDefRegistry(IEnumerable<JsonEntityDef> defs)
		{
			foreach (JsonEntityDef def in defs) {
				byId[def.Id] = def;
			}
		}

		public JsonEntityDef Get(string id)
		{
			return byId[id];
		}
	}
}
