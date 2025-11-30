using System;
using System.Collections.Generic;

namespace Snowship.NMaterial
{
	public class MaterialRegistry
	{
		private readonly Dictionary<string, Material> byId;
		private readonly Dictionary<string, List<Material>> byClass;

		public MaterialRegistry(IEnumerable<Material> materials)
		{
			byId = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
			byClass = new Dictionary<string, List<Material>>(StringComparer.OrdinalIgnoreCase);

			foreach (Material material in materials) {
				byId[material.Id] = material;
				if (!byClass.TryGetValue(material.ClassId, out List<Material> classes)) {
					classes = new List<Material>();
					byClass[material.ClassId] = classes;
				}
				classes.Add(material);
			}
		}

		public Material Get(string id)
		{
			return byId[id];
		}

		public bool TryGet(string id, out Material material)
		{
			return byId.TryGetValue(id, out material);
		}

		public IReadOnlyList<Material> GetByClass(string classId)
		{
			if (byClass.TryGetValue(classId, out List<Material> materials)) {
				return materials;
			}
			return Array.Empty<Material>();
		}
	}
}
