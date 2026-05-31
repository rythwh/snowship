using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class BaseRecipeComponent : IComponent, ICostProvider
	{
		private readonly List<Ingredient> baseIngredients;

		public BaseRecipeComponent(IEnumerable<Ingredient> ingredients)
		{
			baseIngredients = new List<Ingredient>(ingredients);
		}

		public void ApplyTo(Entity entity, CostDraft draft)
		{
			draft.ReplaceAll(baseIngredients);
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
