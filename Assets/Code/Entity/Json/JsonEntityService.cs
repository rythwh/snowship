using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NEntity
{
	public class JsonEntityService
	{
		public JsonEntityRegistry Registry { get; private set; }

		public JsonEntityService()
		{
			Load();
		}

		private void Load()
		{
			string path = Path.Combine(Application.streamingAssetsPath, "BaseGame/Data/entities.json");
			if (File.Exists(path)) {
				string json = File.ReadAllText(path);
				List<JsonEntity> entities = JsonConvert.DeserializeObject<List<JsonEntity>>(json);
				Registry = new JsonEntityRegistry(entities);
			} else {
				Debug.LogWarning($"Could not find entities.json at {path}");
				Registry = new JsonEntityRegistry(new List<JsonEntity>());
			}
		}
	}
}
