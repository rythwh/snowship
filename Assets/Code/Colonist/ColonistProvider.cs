using System.Collections.Generic;
using Snowship.NHuman;

namespace Snowship.NColonist
{
	public class ColonistProvider : IColonistQuery
	{
		private readonly IHumanQuery humanQuery;

		public ColonistProvider(IHumanQuery humanQuery) {
			this.humanQuery = humanQuery;
		}

		public IEnumerable<Colonist> Colonists => humanQuery.GetHumans<Colonist>();
		public int ColonistCount => humanQuery.CountHumans<Colonist>();
	}

	public interface IColonistQuery
	{
		IEnumerable<Colonist> Colonists { get; }
		int ColonistCount { get; }
	}
}
