namespace Snowship
{
	public readonly struct Ingredient
	{
		public string ResourceId { get; }
		public int Amount { get; }

		public Ingredient(string resourceId, int amount)
		{
			ResourceId = resourceId;
			Amount = amount;
		}
	}
}
