using Snowship.NColonist;
using Snowship.NHuman;

namespace Snowship.NUI
{
	public class UIHumanInfoPanelParameters : IUIParameters {

		public Human Human { get; }

		public UIHumanInfoPanelParameters(Human human) {
			Human = human;
		}

	}
}
