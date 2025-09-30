using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NUI;
using Random = Unity.Mathematics.Random;

namespace Snowship.NMap.Generation
{
	public class MapGenContext
	{
		public Map Map { get; }
		public MapData Data { get; }

		private Random random;
		public ref Random Random => ref random;

		private string StateTitle { get; set; }
		private string SubStateTitle { get; set; }

		public MapGenContext(Map map, MapData data, int seed) {
			Map = map;
			Data = data;
			Random = new Random((uint)Math.Abs(seed == 0 ? 1 : seed));
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
