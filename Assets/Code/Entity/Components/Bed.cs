using System;
using Snowship.NHuman;

namespace Snowship.NEntity
{
	public class Bed : IComponent
	{
		public Human Occupant { get; private set; }

		public void SetOccupant(Human occupant)
		{
			if (Occupant != null) {
				throw new InvalidOperationException($"{occupant.Name} tried to occupy Bed with existing occupant {Occupant.Name}");
			}
			Occupant = occupant;
		}

		public void OnAttach(Entity entity) { }

		public void OnDetach()
		{
			if (Occupant != null) {
				throw new InvalidOperationException($"Tried to remove Bed with occupant {Occupant.Name}");
			}
		}
	}
}
