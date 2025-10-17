using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class MaterialRegistry
	{
		private readonly Dictionary<string, MaterialDef> byId;
		private readonly Dictionary<string, List<MaterialDef>> byClass;

		public MaterialRegistry(IEnumerable<MaterialDef> materials)
		{
			byId = new Dictionary<string, MaterialDef>(StringComparer.OrdinalIgnoreCase);
			byClass = new Dictionary<string, List<MaterialDef>>(StringComparer.OrdinalIgnoreCase);

			foreach (MaterialDef material in materials) {
				byId[material.Id] = material;
				if (!byClass.TryGetValue(material.ClassId, out List<MaterialDef> classes)) {
					classes = new List<MaterialDef>();
					byClass[material.ClassId] = classes;
				}
				classes.Add(material);
			}
		}

		public MaterialDef Get(string id)
		{
			return byId[id];
		}

		public bool TryGet(string id, out MaterialDef material)
		{
			return byId.TryGetValue(id, out material);
		}

		public IReadOnlyList<MaterialDef> GetByClass(string classId)
		{
			if (byClass.TryGetValue(classId, out List<MaterialDef> materials)) {
				return materials;
			}
			return Array.Empty<MaterialDef>();
		}
	}
}
