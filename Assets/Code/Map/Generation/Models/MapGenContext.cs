using System;
using JetBrains.Annotations;
using Snowship.NUI;

namespace Snowship.NMap.Generation
{
	public class MapGenContext
	{
		public Map Map { get; }
		public MapData Data { get; }
		public float StartDecimalHour { get; }

		private string stateTitle;
		private string subStateTitle;

		public MapGenContext(
			Map map,
			MapData data,
			float startDecimalHour = 8
		) {
			Map = map;
			Data = data;
			StartDecimalHour = startDecimalHour;
		}

		public void SetNewState([CanBeNull] string state, [CanBeNull] string substate) {
			if (state != null) {
				stateTitle = state;
			}
			if (substate != null) {
				subStateTitle = substate;
			}
			UIEvents.UpdateLoadingScreenText(stateTitle, subStateTitle);
		}
	}
}
