using Snowship.NColonist;
using Snowship.NHuman;

namespace Snowship.NUI
{
	public class UIColonistInfoPanelParameters : IUIParameters {

		public Human Human { get; }

		public UIColonistInfoPanelParameters(Human human) {
			Human = human;
		}

	}
}
