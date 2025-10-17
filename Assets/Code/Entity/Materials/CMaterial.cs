using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship
{
	public class CMaterial : IComponent, IStatProvider, ICostProvider
	{
		public MaterialDef Material { get; private set; }

		public CMaterial(MaterialDef material)
		{
			Material = material;
		}

		public void Set(MaterialDef material)
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
