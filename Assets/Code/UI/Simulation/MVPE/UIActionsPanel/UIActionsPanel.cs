namespace Snowship.NUI
{
	public class UIActionsPanel : UIConfig<UIActionsPanelView, UIActionsPanelPresenter>
	{
		public override bool Closeable { get; protected set; } = false;
	}
}
