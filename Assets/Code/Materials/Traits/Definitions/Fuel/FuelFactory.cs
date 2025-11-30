using Snowship.NEntity;
using VContainer;

namespace Snowship.NMaterial
{
	public class FuelFactory : IItemTraitFactory
	{
		public ETrait Id => ETrait.Fuel;

		public IItemTraitBlueprint Parse(JsonArgs args, IObjectResolver resolver)
		{
			float energy = args.TryGetFloat("energyPerUnit", 0f);
			return new FuelBlueprint(energy);
		}
	}
}
