using UnityEngine;

namespace Snowship.NLife
{
	public abstract class LifeViewModule : MonoBehaviour
	{
		public abstract void OnBind<TLife, TLifeView>(TLife model, TLifeView view) where TLife : Life where TLifeView : LifeView<TLife>;
		public abstract void OnUnbind();
	}
}
