using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using Snowship.NResource;
using Snowship.NSelection;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	public interface IJobDefinition : IGroupItem
	{
		public Type JobType { get; }
		public Type JobParamsType { get; }

		// Job Definition Properties
		Func<Tile, int, bool>[] SelectionConditions { get; }
		List<(EResource resource, int amount)> BaseRequiredResources { get; }
		int TimeToWork { get; }
		bool Returnable { get; }
		int Layer { get; }
		SelectionType SelectionType { get; }
		bool HasPreviewObject { get; }

		// Attribute Shortcuts
		IGroupItem Group { get; }
		IGroupItem SubGroup { get; }
	}
}
