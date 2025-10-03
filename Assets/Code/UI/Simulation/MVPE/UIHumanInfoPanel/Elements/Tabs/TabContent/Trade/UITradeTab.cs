using Cysharp.Threading.Tasks;
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
		private readonly HumanView humanView;

		public UITradeTab(Human human, HumanView humanView) {
			this.human = human;
			this.humanView = humanView;
		}

		protected override UniTask OnCreate() {
			Component.TradeButtonClicked += OnTradeButtonClicked;
			return UniTask.CompletedTask;
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
