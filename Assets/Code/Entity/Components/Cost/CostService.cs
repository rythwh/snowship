using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class CostService
	{
		private readonly CostDraft draft = new();
		private readonly List<Ingredient> buffer = new();

		public List<Ingredient> ComputeFinalCost(Entity entity)
		{
			draft.Clear();

			if (entity.TryGet(out BaseRecipeC baseRecipe)) {
				baseRecipe.ApplyTo(entity, draft);
			}

			if (entity.TryGet(out MaterialC material)) {
				bool applied = false;

				if (entity.TryGet(out MaterialOptions options)) {
					if (options.TryGetMaterialOverride(material.Material.Id, out MaterialCostOverride ov)
						|| options.TryGetClassOverride(material.Material.ClassId, out ov)
					) {
						ApplyOverride(ov, draft);
						applied = true;
					}
				}

				if (!applied) {
					material.ApplyTo(entity, draft);
				}
			}

			foreach (ICostProvider costProvider in entity.CostProviders) {
				if (costProvider is BaseRecipeC or MaterialC) {
					continue;
				}
				costProvider.ApplyTo(entity, draft);
			}

			return draft.ToList(buffer);
		}

		private static void ApplyOverride(MaterialCostOverride ov, CostDraft draft)
		{
			if (ov.Mode == MaterialOverrideMode.ReplaceAll) {
				draft.ReplaceAll(ov.Recipe);
				return;
			}

			foreach (Ingredient ingredient in ov.Remove) {
				draft.Remove(ingredient.ResourceId, ingredient.Amount);
			}
			draft.AddRange(ov.Add);
		}
	}
}
