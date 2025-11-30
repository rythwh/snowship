using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class MaterialC : IComponent, IStatProvider, ICostProvider
	{
		public Material Material { get; private set; }

		public MaterialC(Material material)
		{
			Material = material;
		}

		public void Set(Material material)
		{
			Material = material;
		}

		public void CollectModifiers(List<StatModifier> buffer)
		{
			foreach (StatModifier modifier in Material.Modifiers) {
				buffer.Add(modifier);
			}
		}

		public void ApplyTo(Entity entity, CostDraft draft)
		{
			// Fallback if no per-entity override exists; otherwise CostService will override
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
