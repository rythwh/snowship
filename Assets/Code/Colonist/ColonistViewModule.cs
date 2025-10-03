using Snowship.NLife;
using Snowship.NUtilities;

namespace Snowship.NColonist
{
	public class ColonistViewModule : LifeViewModule
	{
		public override void OnBind<TLife, TLifeView>(TLife model, TLifeView view) {
			view.OnNameColourChanged(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100));
		}

		public override void OnUnbind() {
		}
	}
}
