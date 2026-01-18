namespace Snowship.NMaterial
{
	public class MaterialAmount
	{
		public Material Material { get; set; }
		public int Amount { get; set; }

		public MaterialAmount(Material material, int amount)
		{
			Material = material;
			Amount = amount;
		}
	}
}
