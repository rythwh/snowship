using System;
using JetBrains.Annotations;
using Snowship.NUI;
using VContainer;

namespace Snowship.NMap.Generation
{
	public class MapGenContext
	{
		public Map Map { get; }
		public MapData Data { get; }

		private Unity.Mathematics.Random random;
		public ref Unity.Mathematics.Random Random => ref random;

		private string StateTitle { get; set; }
		private string SubStateTitle { get; set; }

		public MapGenContext(
			Map map,
			MapData data,
			int seed
		) {
			Map = map;
			Data = data;
			Random = new Unity.Mathematics.Random((uint)Math.Abs(seed == 0 ? 1 : seed));
		}

		public void SetNewState([CanBeNull] string state, [CanBeNull] string substate) {
			if (state != null) {
				StateTitle = state;
			}
			if (substate != null) {
				SubStateTitle = substate;
			}
			UIEvents.UpdateLoadingScreenText(StateTitle, SubStateTitle);
		}
	}
}
