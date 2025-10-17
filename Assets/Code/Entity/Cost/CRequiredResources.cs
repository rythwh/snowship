using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship
{
	public class CRequiredResources : IComponent
	{
		public List<Ingredient> Needed { get; }
		public Dictionary<string, int> Delivered { get; } = new();

		public CRequiredResources(IEnumerable<Ingredient> needed)
		{
			Needed = new List<Ingredient>(needed);
		}

		public bool IsComplete {
			get {
				foreach (Ingredient ingredient in Needed) {
					if (!Delivered.TryGetValue(ingredient.ResourceId, out int have) || have < ingredient.Amount) {
						return false;
					}
				}
				return true;
			}
		}

		public void MarkDelivered(string resourceId, int amount)
		{
			if (Delivered.TryGetValue(resourceId, out int current)) {
				Delivered[resourceId] = current + amount;
			} else {
				Delivered[resourceId] = amount;
			}
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
