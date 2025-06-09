namespace Snowship.NUI
{
	public class UISimulation : UIConfig<UISimulationView, UISimulationPresenter> {
		public override bool Closeable { get; protected set; } = false;
	}
}
