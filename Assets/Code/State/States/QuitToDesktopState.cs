using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Snowship.NState.States
{
	public class QuitToDesktopState : State<EState>
	{
		public override EState Type => EState.QuitToDesktop;
		public override EState[] ValidNextStates => null;
		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition => null;
	}
}
