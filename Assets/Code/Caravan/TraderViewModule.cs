using Snowship.NLife;
using Snowship.NUtilities;

namespace Snowship.NCaravan
{
	public class TraderViewModule : LifeViewModule
	{
		public override void OnBind<TLife, TLifeView>(TLife model, TLifeView view) {
			view.OnNameColourChanged(ColourUtilities.GetColour(ColourUtilities.EColour.LightPurple100));
		}

		public override void OnUnbind() {
		}
	}
}
