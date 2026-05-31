using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Snowship.NEntity
{
	public class BaseRecipeFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.BaseRecipe;

		public IComponent Create(JsonArgs args)
		{
			JArray array = args.GetArrayOrDefault("ingredients");
			List<Ingredient> ingredients = array != null
				? JsonArgs.ReadIngredientsArray(array)
				: new List<Ingredient>();
			return new BaseRecipeComponent(ingredients);
		}
	}
}
