using JetBrains.Annotations;
using Snowship.NCaravan;
using Snowship.NHuman;
using Snowship.NJob;

namespace Snowship.NUI.UITab
{
	[UsedImplicitly]
	public class UITradeTab : UITabElement<UITradeTabComponent>
	{
		private readonly Human human;

		public UITradeTab(Human human) {
			this.human = human;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Component.TradeButtonClicked += OnTradeButtonClicked;
		}

		protected override void OnClose() {
			base.OnClose();

			Component.TradeButtonClicked -= OnTradeButtonClicked;
		}

		private void OnTradeButtonClicked() {
			if (human is not Trader trader) {
				return;
			}
			if (GameManager.Get<JobManager>().JobOfTypeExistsAtTile<TradeJob>(trader.Tile) != null) {
				return;
			}
			GameManager.Get<JobManager>().AddJob(new TradeJob(trader));
		}
	}
}
