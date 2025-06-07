using System.Collections.Generic;

namespace Snowship.NColonist
{
	public class TraitsComponent
	{
		private readonly List<TraitInstance> traits = new();

		public IReadOnlyList<TraitInstance> AsList() {
			return traits;
		}
	}
}