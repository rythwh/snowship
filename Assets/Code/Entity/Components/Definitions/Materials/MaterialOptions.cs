using System;
using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public enum MaterialOverrideMode { ReplaceAll, Delta }

	public class MaterialOptions : IComponent
	{
		public IReadOnlyList<string> AllowedClasses { get; }
		public IReadOnlyList<string> AllowedMaterials { get; }
		public string DefaultClassId { get; }
		public string DefaultMaterialId { get; }

		private readonly Dictionary<string, MaterialCostOverride> byClassOverride;
		private readonly Dictionary<string, MaterialCostOverride> byMaterialOverride;

		public MaterialOptions(
			IReadOnlyList<string> allowedClasses,
			IReadOnlyList<string> allowedMaterials,
			string defaultClassId,
			string defaultMaterialId,
			Dictionary<string, MaterialCostOverride> byClassOverride,
			Dictionary<string, MaterialCostOverride> byMaterialOverride
		)
		{
			AllowedClasses = allowedClasses ?? Array.Empty<string>();
			AllowedMaterials = allowedMaterials ?? Array.Empty<string>();
			DefaultClassId = defaultClassId;
			DefaultMaterialId = defaultMaterialId;
			this.byClassOverride = byClassOverride ?? new Dictionary<string, MaterialCostOverride>(StringComparer.OrdinalIgnoreCase);
			this.byMaterialOverride = byMaterialOverride ?? new Dictionary<string, MaterialCostOverride>(StringComparer.OrdinalIgnoreCase);
		}

		public bool TryGetClassOverride(string classId, out MaterialCostOverride classOverride)
		{
			return byClassOverride.TryGetValue(classId, out classOverride);
		}

		public bool TryGetMaterialOverride(string materialId, out MaterialCostOverride materialOverride)
		{
			return byMaterialOverride.TryGetValue(materialId, out materialOverride);
		}

		public bool IsAllowed(Material material)
		{
			foreach (string allowedMaterial in AllowedMaterials) {
				if (string.Equals(allowedMaterial, material.Id, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			foreach (string allowedClass in AllowedClasses) {
				if (string.Equals(allowedClass, material.ClassId, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
