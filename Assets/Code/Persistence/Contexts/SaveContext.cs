using System;
using System.Collections.Generic;

namespace Snowship.Persistence
{
	public sealed class SaveContext
	{
		private Dictionary<object, string> objectToId = new();
		private Dictionary<string, object> idToObject = new();

		public void Store(string id, object instance) {
			idToObject.TryAdd(id, instance);
			objectToId.TryAdd(instance, id);
		}

		public string GetId(object instance) {
			if (instance == null) {
				return null;
			}
			if (objectToId.TryGetValue(instance, out string existingId)) {
				return existingId;
			}
			throw new InvalidOperationException($"Object \"{instance}\" not registered with SaveContext.");
		}
	}
}
