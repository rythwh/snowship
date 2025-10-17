using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Snowship.NEntity
{
	public class BaseRecipeFactory : IJsonComponentFactory
	{
		public string TypeId => "BaseRecipe";

		public IComponent Create(JsonArgs args, IObjectResolver resolver)
		{
			JArray array = args.DataToken as JArray;
			List<Ingredient> ingredients = array != null
				? JsonArgs.ReadIngredientsArray(array)
				: new List<Ingredient>();
			return new CBaseRecipe(ingredients);
		}
	}
}
