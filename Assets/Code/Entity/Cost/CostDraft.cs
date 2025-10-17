using System.Collections.Generic;

namespace Snowship
{
	public class CostDraft
	{
		private readonly Dictionary<string, int> counts = new();

		public void Add(string resourceId, int amount)
		{
			if (counts.TryGetValue(resourceId, out int count)) {
				counts[resourceId] = count + amount;
			} else {
				counts[resourceId] = amount;
			}
		}

		public void AddRange(IEnumerable<Ingredient> ingredients)
		{
			foreach (Ingredient ingredient in ingredients) {
				Add(ingredient.ResourceId, ingredient.Amount);
			}
		}

		public void ReplaceAll(IEnumerable<Ingredient> ingredients)
		{
			Clear();
			AddRange(ingredients);
		}

		public void Remove(string resourceId, int amount)
		{
			if (!counts.TryGetValue(resourceId, out int count)) {
				return;
			}
			int newAmount = count - amount;
			if (newAmount <= 0) {
				counts.Remove(resourceId);
			} else {
				counts[resourceId] = newAmount;
			}
		}

		public void Clear()
		{
			counts.Clear();
		}

		public List<Ingredient> ToList(List<Ingredient> buffer)
		{
			buffer.Clear();
			foreach ((string resourceId, int amount) in counts) {
				buffer.Add(new Ingredient(resourceId, amount));
			}
			return buffer;
		}
	}
}
