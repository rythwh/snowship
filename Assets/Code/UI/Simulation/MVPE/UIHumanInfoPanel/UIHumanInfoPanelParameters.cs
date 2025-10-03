using Snowship.NHuman;

namespace Snowship.NUI
{
	public class UIHumanInfoPanelParameters : IUIParameters
	{
		public readonly Human Human;
		public readonly HumanView HumanView;
		public readonly int OpenedTabIndex;

		public UIHumanInfoPanelParameters(
			Human human,
			HumanView humanView,
			int openedTabIndex
		) {
			Human = human;
			HumanView = humanView;
			OpenedTabIndex = openedTabIndex;
		}

	}
}
