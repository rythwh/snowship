using System.Collections.Generic;

namespace Snowship.NPersistence
{
	public sealed class LoadContext
	{
		private readonly Dictionary<string, object> idToObject;
		private readonly List<ISaveable> postLoadList = new List<ISaveable>();

		public LoadContext(Dictionary<string, object> idToObject) {
			this.idToObject = idToObject;
		}

		public T Resolve<T>(string id) where T : class {
			if (string.IsNullOrEmpty(id)) {
				return null;
			}
			if (idToObject.TryGetValue(id, out object instance)) {
				return instance as T;
			}
			return null;
		}

		public void EnqueuePostLoad(ISaveable saveable)
		{
			// Defer OnAfterLoad calls until EVERY object ran Load (so cyclic-dependencies are safe to resolve).
			postLoadList.Add(saveable);
		}

		public void RunPostLoad() {
			foreach (ISaveable saveable in postLoadList) {
				saveable.OnAfterLoad(this);
			}
			postLoadList.Clear();
		}
	}
}
