using Snowship.NCaravan;
using Snowship.NTime;
using Snowship.NUI;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Trade", "Trade", "Trade")]
	public class TradeJobDefinition : JobDefinition<TradeJob>
	{
		public TradeJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class TradeJob : Job<TradeJobDefinition>
	{
		private readonly Trader trader;

		public TradeJob(Trader trader) : base(trader.Tile) {
			this.trader = trader;

			SetTimeToWork(0);
		}

		protected override void OnJobWorkerMoving() {
			base.OnJobWorkerMoving();

			if (trader.Tile != Tile) {
				ChangeTile(trader.Tile);
			}
		}

		protected override async void OnJobFinished() {
			base.OnJobFinished();

			GameManager.Get<TimeManager>().TogglePause();
			await GameManager.Get<UIManager>().OpenViewAsync<UITradeMenu>();
		}
	}
}
